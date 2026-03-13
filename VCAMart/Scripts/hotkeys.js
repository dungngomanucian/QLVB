// huudq@ric.vn - HotKey Capture
var ricHotKey;

Keys = 
{
    F1: 112, F2:113,F3:114, F4:115, F5:116,F6:117,F7:118,F8:119,F9:120,F10:121,F11:122,F12:123,F13:124,F14:125,F15:126,
    Enter : 13,Space : 32, ESC : 27,Backspace:8,Tab:9,Shift:16,Control:17,PageUp:33,PageDown:34,End:35,Home:36,Left:37,Up:38,Right:39,Down:40,Insert:45,Delete:46
    
};

function CheckHotKey(KeyCode)
{
    var kq = "";
    switch (KeyCode)
    {
    case Keys.F1: kq='F1'; break;
    case Keys.F2: kq='F2';break;
    case Keys.F3: kq='F3';break;
    case Keys.F4: kq='F4';break;
    case Keys.F5: kq='F5';break;
    case Keys.F6: kq='F6';break;
    case Keys.F7: kq='F7';break;
    case Keys.F8: kq='F8';break;
    case Keys.F9: kq='F9';break;
    case Keys.F10: kq='F10';break;
    case Keys.F11: kq='F11';break;
    case Keys.Tab: kq='Tab';break;
    case Keys.Shift: kq='Shift';break;
    case Keys.Control: kq='Control';break;
    case Keys.PageUp: kq='PageUp';break;
    case Keys.Left: kq='Left';break;
    
    case Keys.Enter:    kq = "Enter";   break;
    case Keys.ESC:      kq = "ESC";     break;
    case Keys.Space:    kq = "Space";   break;
    }
    
    return "";
}

function hotkeyChungtu(KeyCode)
{
    var kq = "";
    switch (KeyCode)
    {
    // case Keys.F1: kq='F1'; break; /* */
    // case Keys.F2:   $('#' + obj_txt_thanhtoan).focus(); $('#' + obj_txt_thanhtoan).select(); break;/* thanh toan */
    case Keys.F3:      break;  break;
     
    // case Keys.F4:   $('#' + obj_txt_ngay).focus();  break;
     case Keys.F5: kq='F5';
        
     break; /* them moi */
    // case Keys.F7: $('#' + obj_txt_makhachhang).focus();$('#' + obj_txt_makhachhang).select(); break;
    
     case Keys.F8: 
        
        break; /* save */
     case Keys.F9: 
       
     case Keys.F10: 
       
        break; /* in */
     case Keys.F11: //luu tam
        
     break; /* */
    
    // case Keys.Tab: kq='Tab';break;
    // case Keys.Shift: kq='Shift';break;
    // case Keys.Control: kq='Control';break;
    // case Keys.PageUp: kq='PageUp';break;
    // case Keys.Left: kq='Left';break;
    
    // case Keys.Enter:    kq = "Enter";   break;
    case Keys.F2:     break;
    // case Keys.Space:    kq = "Space";   break;

    case Keys.F12:     break;
    
    }
    //alert("You pressed: " + kq);
    return "";
}

function hotkeyChungtu_BaogiaDondathang(KeyCode)
{
    var kq = "";
    switch (KeyCode)
    {
    case Keys.F1: kq='F1'; break; /* */
    
    case Keys.F3:   break;
    case Keys.F4:   $('#' + obj_txt_ngay).focus();  break;
    case Keys.F5: kq='F5';break; /* them moi */
    case Keys.F7: $('#' + obj_txt_makhachhang).focus();$('#' + obj_txt_makhachhang).select(); break;
    
    case Keys.F8: _gr_nx_save('');break; /* save */
    case Keys.F9: _gr_nx_in('print.aspx?pid=' + js_pid ); break; /* huy */
    case Keys.F10: kq='F10';break; /* in */
    case Keys.F11: kq='F11';break; /* */
    
    case Keys.Tab: kq='Tab';break;
    case Keys.Shift: kq='Shift';break;
    case Keys.Control: kq='Control';break;
    case Keys.PageUp: kq='PageUp';break;
    case Keys.Left: kq='Left';break;
    
    case Keys.Enter:    kq = "Enter";   break;
    case Keys.ESC:     $('#' + obj_txt_mavach).val('');  $('#' + obj_txt_mavach).focus();     break;
    case Keys.Space:    kq = "Space";   break;

    case Keys.F12:    _gr_nx_xoa(''); break;
    
    }
    //alert("You pressed: " + kq);
    return "";
}

function hotkeyThuchi(KeyCode)
{
    var kq = "";
    switch (KeyCode)
    {
    case Keys.F1: kq='F1'; break; /* */
    
    case Keys.F3:   break;
    case Keys.F4:   $('#' + obj_txt_ngay).focus();  break;
    case Keys.F5: kq='F5';break; /* them moi */
    case Keys.F7: $('#' + obj_txt_makhachhang).focus();$('#' + obj_txt_makhachhang).select(); break;
    
    case Keys.F8: _tc_save('');break; /* save */
    case Keys.F9: _tc_in('print.aspx?pid=' + js_pid ); break; /* huy */
    case Keys.F10: kq='F10';break; /* in */
    case Keys.F11: kq='F11';break; /* */
    
    case Keys.Tab: kq='Tab';break;
    case Keys.Shift: kq='Shift';break;
    case Keys.Control: kq='Control';break;
    case Keys.PageUp: kq='PageUp';break;
    case Keys.Left: kq='Left';break;
    
    case Keys.Enter:    kq = "Enter";   break;
    case Keys.ESC:   $('#' + obj_txt_sotien).focus();$('#' + obj_txt_sotien).select(); break;
    case Keys.Space:    kq = "Space";   break;

    case Keys.F12:    _tc_xoa(''); break;
    
    }
    //alert("You pressed: " + kq);
    return "";
}

function hotkeyHethong_Danhmuc(KeyCode)
{
    var kq = "";
    switch (KeyCode)
    {
    case Keys.F1: kq='F1'; break; /* */
    
    case Keys.F3:   break;
    case Keys.F4:   $('#' + obj_txt_ngay).focus();   $('#' + obj_txt_ngay).select();  break;
    case Keys.F5: kq='F5';break; /* them moi */
    case Keys.F7:  break;
    
    case Keys.F8: __save('Default.aspx?pid=' + js_pid +'&act=Save');break; /* save */
    case Keys.F9:  break; /* huy */
    case Keys.F10: kq='F10';break; /* in */
    case Keys.F11: kq='F11';break; /* */
    
    case Keys.Tab: kq='Tab';break;
    case Keys.Shift: kq='Shift';break;
    case Keys.Control: kq='Control';break;
    case Keys.PageUp: kq='PageUp';break;
    case Keys.Left: kq='Left';break;
    
    case Keys.Enter:    kq = "Enter";   break;
    case Keys.ESC: break;
    case Keys.Space:    kq = "Space";   break;

    case Keys.F12: break;
    
    }
    
    return "";
}


function handler(e) 
{


    ricHotKey = null;

    if (!e) var e = window.event;
    var code;
    if (e.keyCode) 
        code = e.keyCode;
    else if (e.which) 
        code = e.which;
    else return;

//    
//    if (e.cancelBubble != null) e.cancelBubble = true;
//    if (e.returnValue != null) e.returnValue = false;
//    if (e.stopPropagation) e.stopPropagation();
//    if (e.preventDefault) e.preventDefault();
  
    switch (mricRunningFunction.toUpperCase()){
        case "XUATBANBUON": 
        case "XUATNOIBO":
        case "NHAPTUNHACUNGCAP": 
        case "NHAPKHACHTRALAI":
        case "XUATTRANHACUNGCAP":
        case "NHAPTONDAUKY":
        case "XUATBANLE":
          
        case "BAOGIA":
        case "DONDATHANG":
             hotkeyChungtu(code);
            break;
        case "PHIEUTHU":
        case "PHIEUCHI":
             hotkeyThuchi(code);
            break;
        case "THONGTINSUDUNG":
         hotkeyHethong_Danhmuc(code);
          break;
          default:
          CheckHotKey(code);
          break;
    }
   ricHotKey = code;
  
} // end function

  if (!document.all) 
  {
    window.captureEvents(Event.KEYDOWN);
    window.onkeydown=handler;
  } 
  else 
  {
    document.onkeydown = handler;
  }