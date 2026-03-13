# QLVB - Quick Start

Huong dan nhanh de clone va chay project tren may Windows.

## Yeu cau

- .NET SDK 9.0+ (khuyen nghi dung 9.0.300 hoac moi hon)
- Git

## Clone source

```powershell
git clone <REPO_URL>
cd <TEN_THU_MUC_REPO>
```

## Build nhanh

```powershell
dotnet restore "QLVB.sln"
dotnet build "QLVB.sln"
```

## Neu gap loi do packages.config (legacy NuGet)

Chay them lenh restore sau, roi build lai:

```powershell
dotnet msbuild "QLVB.sln" -t:Restore -p:RestorePackagesConfig=true
dotnet build "QLVB.sln"
```

## Mo bang Visual Studio

- Mo file `QLVB.sln`
- Chon `Build > Build Solution`

## Ghi chu

- Project chinh: `VCAMart/QLVB.csproj` (.NET Framework 4.8)
- Neu build co warning nhung khong co error, co the tiep tuc chay/debug binh thuong.
