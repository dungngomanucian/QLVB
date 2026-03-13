import json
import sys
import warnings
import os
warnings.filterwarnings(
    "ignore",
    message=r".*urllib3.*doesn't match a supported version.*",
    category=Warning
)

import torch
from transformers import AutoTokenizer, AutoModel


def mean_pool(last_hidden_state, attention_mask):
    mask = attention_mask.unsqueeze(-1).expand(last_hidden_state.size()).float()
    masked = last_hidden_state * mask
    summed = masked.sum(dim=1)
    counts = mask.sum(dim=1).clamp(min=1e-9)
    return summed / counts


def load_request():
    # Ưu tiên đọc từ file nếu có truyền --input <path>
    args = sys.argv[1:]
    in_path = None
    out_path = None
    for i in range(len(args)):
        if args[i] == "--input" and i + 1 < len(args):
            in_path = args[i + 1]
        if args[i] == "--output" and i + 1 < len(args):
            out_path = args[i + 1]

    raw = ""
    if in_path and os.path.exists(in_path):
        # utf-8-sig để tự xử lý BOM từ file JSON do .NET ghi ra
        with open(in_path, "r", encoding="utf-8-sig") as f:
            raw = f.read()
    else:
        raw = sys.stdin.read()

    return raw, out_path


def emit_response(payload, out_path=None):
    content = json.dumps(payload, ensure_ascii=False)
    if out_path:
        with open(out_path, "w", encoding="utf-8") as f:
            f.write(content)
    else:
        print(content)


def main():
    raw, out_path = load_request()
    if not raw:
        emit_response({"embeddings": []}, out_path)
        return

    try:
        req = json.loads(raw)
    except Exception as ex:
        emit_response({"embeddings": [], "error": f"invalid_json: {str(ex)}"}, out_path)
        return
    model_name = req.get("model", "AITeamVN/Vietnamese_Embedding")
    raw_texts = req.get("texts", [])
    if not isinstance(raw_texts, list):
        raw_texts = [raw_texts]

    # Bắt buộc tokenizer nhận list[str]
    texts = []
    for t in raw_texts:
        if t is None:
            texts.append("")
        elif isinstance(t, str):
            texts.append(t)
        elif isinstance(t, (int, float, bool)):
            texts.append(str(t))
        else:
            # dict/list/object -> stringify để tránh TypeError
            try:
                texts.append(json.dumps(t, ensure_ascii=False))
            except Exception:
                texts.append(str(t))

    # Ép cứng toàn bộ input về string để tránh lỗi sentencepiece: "TypeError: not a string"
    texts = ["" if t is None else str(t) for t in texts]

    # use_fast=False để tránh lỗi strict type ở fast tokenizer trên một số input "lạ"
    tokenizer = AutoTokenizer.from_pretrained(model_name, use_fast=False)
    model = AutoModel.from_pretrained(model_name)
    model.eval()
    hidden_size = int(getattr(model.config, "hidden_size", 768))

    all_vecs = []
    fallback_count = 0
    first_error = ""
    with torch.no_grad():
        for t in texts:
            # Cực kỳ chặt để sentencepiece luôn nhận str
            if t is None:
                s = ""
            elif isinstance(t, str):
                s = t
            elif isinstance(t, (dict, list, tuple)):
                try:
                    s = json.dumps(t, ensure_ascii=False)
                except Exception:
                    s = str(t)
            else:
                s = str(t)

            # Ép chắc chắn là built-in str
            s = str(s)
            # Làm sạch ký tự lỗi/ẩn có thể làm sentencepiece lỗi
            s = s.replace("\x00", " ")
            s = s.encode("utf-8", "ignore").decode("utf-8", "ignore")
            if not s.strip():
                s = " "
            try:
                encoded = tokenizer(
                    s,
                    truncation=True,
                    max_length=512,
                    return_tensors="pt"
                )
                output = model(**encoded)
                vec = mean_pool(output.last_hidden_state, encoded["attention_mask"])
                vec = torch.nn.functional.normalize(vec, p=2, dim=1)
                all_vecs.append(vec[0].cpu().tolist())
            except Exception as ex:
                # Không làm fail cả pipeline vì 1 bản ghi lỗi kiểu dữ liệu.
                # Fallback vector 0 để giữ đúng số lượng output.
                fallback_count += 1
                if not first_error:
                    first_error = str(ex)
                all_vecs.append([0.0] * hidden_size)

    emit_response({
        "embeddings": all_vecs,
        "fallback_count": fallback_count,
        "first_error": first_error
    }, out_path)


if __name__ == "__main__":
    main()

