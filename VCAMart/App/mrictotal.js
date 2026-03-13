//var mtmain = angular.module('mtmain', ["ngStorage"]);


var RXMP_NAME = ''; // tên module mở rộng
var bGirdCreated = false; // kendo ui grid đã tạo
var bLoadDanhmucQuicAdd = true;
var internetOnline = true;
var chid_list = '';//danh sach id cua hang duoc phep dung
var bchid_load = true;// xac nhan da load danh sach ccghi nhanh -
var bsave_close = false;// close sau khi save hoa don 
var rcid = '';
// cac bien dung chung
//var _login_nv_id = '';
var urlroot = '/';
var pageurl = '/';

var bill_chidx = '';
var bill_nvid = '';
var bill_nvid_logined = ''; // lưu id của user logined
var localStorageName_Temp; // ten cua biến storage lưu trên cclient - biến tạm
var localStorageName_Session; // ten cua biến storage lưu trên cclient - biến session

var bill_chidxto = '';
var bill_khachhangid = "KL";
var customer_default = { id: "00000000-0000-0000-0000-000000000000", ma: 'KL', ten: 'Khách lẻ', diachi: '', tel: '' };
var provider_default = { id: "", ma: '', ten: '', diachi: '', tel: '' };

var current_user = { id: "", uid: "", name: "", created: "" };

var mricRunningFunction; // để xử lý hotkeys

var mtmain = angular.module('mtmain', ['ngSanitize']);
var arrPromotionList = [];

var user_permission = {add:1,view:1,edit:1,delete:0,print:1,import:1,export:1,status:1};
 
mtmain.directive('numberInput', function ($filter) {
    // xu ly phan so
    return {
        require: 'ngModel',
        link: function (scope, elem, attrs, ngModelCtrl) {

            ngModelCtrl.$formatters.push(function (modelValue) {
                return setDisplayNumber(modelValue, true);
            });

            ngModelCtrl.$parsers.push(function (viewValue) {
                setDisplayNumber(viewValue);
                return setModelNumber(viewValue);
            });

            elem.bind('keyup focus', function () {
                setDisplayNumber(elem.val());
            });

            function setDisplayNumber(val, formatter) {
                var valStr, displayValue;

                if (typeof val === 'undefined') {
                    return 0;
                }

                valStr = val.toString();
                displayValue = valStr.replace(/,/g, '').replace(/[A-Za-z]/g, '');
                displayValue = parseFloat(displayValue);
                displayValue = (!isNaN(displayValue)) ? displayValue.toString() : '';

                // handle leading character -/0
                if (valStr.length === 1 && valStr[0] === '-') {
                    displayValue = valStr[0];
                } else if (valStr.length === 1 && valStr[0] === '0') {
                    displayValue = '';
                } else {
                    displayValue = $filter('number')(displayValue);
                }

                // handle decimal
                if (!attrs.integer) {
                    if (displayValue.indexOf('.') === -1) {
                        if (valStr.slice(-1) === '.') {
                            displayValue += '.';
                        } else if (valStr.slice(-2) === '.0') {
                            displayValue += '.0';
                        } else if (valStr.slice(-3) === '.00') {
                            displayValue += '.00';
                        }
                    } // handle last character 0 after decimal and another number
                    else {
                        if (valStr.slice(-1) === '0') {
                            displayValue += '0';
                        }
                    }
                }

                if (attrs.positive && displayValue[0] === '-') {
                    displayValue = displayValue.substring(1);
                }

                if (typeof formatter !== 'undefined') {
                    return (displayValue === '') ? 0 : displayValue;
                } else {
                    elem.val((displayValue === '0') ? '' : displayValue);
                }
            }

            function setModelNumber(val) {
                var modelNum = val.toString().replace(/,/g, '').replace(/[A-Za-z]/g, '');
                modelNum = parseFloat(modelNum);
                modelNum = (!isNaN(modelNum)) ? modelNum : 0;
                if (modelNum.toString().indexOf('.') !== -1) {
                    modelNum = Math.round((modelNum + 0.00001) * 100) / 100;
                }
                if (attrs.positive) {
                    modelNum = Math.abs(modelNum);
                }
                return modelNum;
            }
        }
    };
});


var mtclient_product = { name: "mtclient_product", value: [] }
        , mtclient_customer = { name: "mtclient_customer", value: [] }
        , mtclient_provider = { name: "mtclient_provider", value: [] }
        , mtclient_bill20 = { name: "mtclient_bill20", value: [] }
        , mtclient_bill20_ddh = { name: "mtclient_bill20_ddh", value: [] }
        , mtclient_bill21 = { name: "mtclient_bill21", value: [] }
        , mtclient_bill22 = { name: "mtclient_bill22", value: [] }
        , mtclient_bill23 = { name: "mtclient_bill23", value: [] }

, mtclient_bill24 = { name: "mtclient_bill24", value: [] }
, mtclient_bill25 = { name: "mtclient_bill25", value: [] }
, mtclient_bill26 = { name: "mtclient_bill26", value: [] }
, mtclient_bill27 = { name: "mtclient_bill27", value: [] }
, mtclient_bill28 = { name: "mtclient_bill28", value: [] }

        , mtclient_bill10 = { name: "mtclient_bill10", value: [] }
        , mtclient_bill11 = { name: "mtclient_bill11", value: [] }
        , mtclient_bill12 = { name: "mtclient_bill12", value: [] }
        , mtclient_bill13 = { name: "mtclient_bill13", value: [] }
        , mtclient_bill14 = { name: "mtclient_bill14", value: [] }
        , mtclient_bill15 = { name: "mtclient_bill15", value: [] }
        , mtclient_bill16 = { name: "mtclient_bill16", value: [] }
        , mtclient_bill17 = { name: "mtclient_bill17", value: [] }
        , mtclient_bill18 = { name: "mtclient_bill18", value: [] }

        , mtclient_color = { name: "mtclient_color", value: [] }
        , mtclient_size = { name: "mtclient_size", value: [] }
        , mtclient_area = { name: "mtclient_area", value: [] }
     , mtclient_productgroup = { name: "mtclient_productgroup", value: [] }

     , mtclient_phieuthu = { name: "mtclient_phieuthu", value: [] }
, mtclient_phieuchi = { name: "mtclient_phieuchi", value: [] }
 
     , mtclient_ndtc = { name: "mtclient_area", value: [] }
     , mtclient_chinhanh = { name: "mtclient_chinhanh", value: [] }
     , mtclient_config = { name: "mtclient_config", value: [] }
    , mtclient_nhanvien = { name: "mtclient_nhanvien", value: [] }
  , mtclient_ngoaite = { name: "mtclient_ngoaite", value: [] }
, mtclient_report = { name: "mtclient_report", value: [] }

     ;
var bHasLocalDataStorage = false;
if (typeof Storage !== "undefined") {
    bHasLocalDataStorage = true;
}
else {
    bHasLocalDataStorage = false
}

function setLocalStorage(item,value) 
{
  
    if (!bHasLocalDataStorage) return false;
    item = item + '_' + bill_nvid_logined;
    window.localStorage.setItem(item, JSON.stringify(value));
    return true;
}
function getLocalStorage(item) {
  
    if (!bHasLocalDataStorage) return null;
    item = item + '_' + bill_nvid_logined;
        try {
            return JSON.parse(window.localStorage.getItem(item));
        } catch (e) { }
        return null;
     
}

function removeLocalStorage(item) {
    if (!bHasLocalDataStorage) return null;
    item = item + '_' + bill_nvid_logined;
    try {
        window.localStorage.removeItem(item);
    } catch(e) {}
    return true;
}


function setSessionStorage(item, value) {
   
    if (!bHasLocalDataStorage) return false;
    item = item + '_' + bill_nvid_logined;
    window.sessionStorage.setItem(item, JSON.stringify(value));
    //window.localStorage.setItem(item, JSON.stringify(value));
    return true;
}
function getSessionStorage(item) {
    if (!bHasLocalDataStorage) return null;
    item = item + '_' + bill_nvid_logined;
    try {
        return JSON.parse(window.sessionStorage.getItem(item));
        //return JSON.parse(window.localStorage.getItem(item));
    } catch (e) { }
    return null;
}
function removeSessionStorage(item) {
    if (!bHasLocalDataStorage) return false;
    item = item + '_' + bill_nvid_logined;
    try {
        window.sessionStorage.removeItem(item);
        //window.localStorage.removeItem(item);
    } catch(e) {}
    return true;
}

function LoadChinhanh() {
  
}

$(document).ready(function () {
    setTimeout(checkInternetAvailable, 5000);
  
   
});

 
function checkInternetAvailable() {
    
    // Handle IE and more capable browsers
    /*
    var xhr = new XMLHttpRequest();
    var file = "/images/logo.jpg";
    var r = Math.round(Math.random() * 10000);
    xhr.open('HEAD', file + "?subins=" + r, false);
    try {
        xhr.send();
        if (xhr.status >= 200 && xhr.status < 304) {
            internetOnline =  true;
        } else {
            internetOnline =  false;
        }
    } catch (e) {
        internetOnline = false;
        
    }
    if (!internetOnline) console.clear();
    */
    internetOnline = navigator.onLine;

    $('#liInternetAvailable').prop('class', internetOnline ? 'internet_online' : 'internet_offline');
    $('#liInternetAvailable').find('i:eq(0)').prop('class', internetOnline ? 'fa fa-cloud-upload' : 'fa fa-cloud-download');
    $('#liInternetAvailable').find('i:eq(0)').prop('title', internetOnline ? 'Internet is available.' : 'Internet is not available.');
    setTimeout(checkInternetAvailable, 5000);
    
}


//editor
var editor_tomtat, editor_noidung;

var _page_size = 10;

// bill
var billIndex = 0;
var bill_phanle_gia = 1;
var bill_phanle_tien = 1;
var current_tab_index = 0; // index cua bill tab dang xu ly


//======== load danh muc
function createSearchTextField(arr, flist) {
    // tao 1 truong tong hop de tim kiem du lieuj = autocomple
    var f = flist.split(',');
    for (i in arr) {
        arr[i].searchtext = '';
        for (j = 0; j < f.length; j++) {
            arr[i].searchtext = arr[i].searchtext + arr[i][f[j]];
        }
    }
} 

//===========end load dnah muc


// create dialog
function meAlert(title1 , content, width) {
      $('#dlgthongbao_mtmain').kendoDialog({
        width: width + "px",
        title: title1,
        closable: false,
        modal: true,
        content: content,//"<p>A new version of <strong>Kendo UI</strong> is available. Would you like to download and install it now?<p>",
        actions: [
            { text: 'Đóng', primary: true }
        ],
        //close: onClose
    });
   // $('#dlgthongbao_mtmain').data("kendoDialog").open();    
}



function printDivReport(idobj) {
    var frame1 = $('<iframe />');
    frame1[0].name = "frame1";
    frame1.css({ "position": "absolute", "top": "-1000000px" });
    $("body").append(frame1);
    var frameDoc = frame1[0].contentWindow ? frame1[0].contentWindow : frame1[0].contentDocument.document ? frame1[0].contentDocument.document : frame1[0].contentDocument;
    frameDoc.document.open();
    frameDoc.document.write('<html><head><title></title>');
    
    frameDoc.document.write('</head><body>');
    frameDoc.document.write($(idobj).html());
    frameDoc.document.write('</body></html>');
    frameDoc.document.close();

    setTimeout(function () {
        window.frames["frame1"].focus();
        window.frames["frame1"].print();
        frame1.remove();
    }, 500);
}

function printContentReport(content) {
    var frame1 = $('<iframe />');
    frame1[0].name = "frame1";
    frame1.css({ "position": "absolute", "top": "-1000000px" });
    $("body").append(frame1);
    var frameDoc = frame1[0].contentWindow ? frame1[0].contentWindow : frame1[0].contentDocument.document ? frame1[0].contentDocument.document : frame1[0].contentDocument;
    frameDoc.document.open();
    frameDoc.document.write('<html><head><title></title>');

    frameDoc.document.write('</head><body>');
    frameDoc.document.write(content);
    frameDoc.document.write('</body></html>');
    frameDoc.document.close();

    setTimeout(function () {
        window.frames["frame1"].focus();
        window.frames["frame1"].print();
        frame1.remove();
    }, 500);
}

function saveOrRemoveLocalStorage(bSave, name, bill, sessionOrLocal) {
    // bSave =true => save / false => remove
    // sessionOrLocal true/false => Session/Local
    // luu du lieu hoa don o local
    

    var mtbilltemp
    if (sessionOrLocal)
        mtbilltemp = getSessionStorage(name)
    else
        mtbilltemp = getLocalStorage(name)

    if (mtbilltemp == null) mtbilltemp = [];

    // check exists => xoa truoc khi inssert
    for (var i = 0; i < mtbilltemp.length; i++) {
        if (mtbilltemp[i].id == bill.id) {
            mtbilltemp.splice(i, 1); // remove
            break;
        }
    }

    if (bSave) mtbilltemp.push(bill); // neu la save ==> add vao
    if (sessionOrLocal)
        setSessionStorage(name, mtbilltemp);
    else
        setLocalStorage(name, mtbilltemp);
}

function readLocalStorage(  name, sessionOrLocal) {
    // sessionOrLocal true/false => Session/Local
    var mtbilltemp
    if (sessionOrLocal)
        mtbilltemp = getSessionStorage(name)
    else
        mtbilltemp = getLocalStorage(name)
    return mtbilltemp;
}


function promotionLowerID()
{
    for (i=0; i<arrPromotionList.length; i++)
    {
        arrPromotionList[i].noidung = JSON.parse( arrPromotionList[i].noidung);
    }
}

function promotionCheckHaveTangHang(billProducts)
{
    //hhid: hang hoa hien tại
    //soluong: so luong hien tai dang xu ly => de xet chuong trinh tang hang
    var chid = $('#cbbmain_chinhanh').val() ;
    var loai=0, ck=0, tang=0, r,rs, co = 0, id, ten, sl1=0;
    var km = [];

    for (j=0; j<arrPromotionList.length; j++)
    {  // chuong trinh khuyen mai
        rs = arrPromotionList[j].noidung;
        loai = arrPromotionList[j].loai;
        ch = arrPromotionList[j].chid;
        id = arrPromotionList[j].id;
        ten = arrPromotionList[j].ten;
        if (ch == '00000000-0000-0000-0000-000000000000' || ch  !=  chid) continue;
        if(loai/1==1) continue;// bo qua loai khuyen mai bang chiet khau

        for (i=0; i<rs.length; i++)
        { // lap lai cac item cua khuyen mai
            r = rs[i];
            // duyet san pham trong hoa don
            for (k =0; k<billProducts.length; k++) 
            {
                var sp = billProducts[k];
                if (r.hhid == sp.hhid && r.sl/1 <= sp.soluong/1)
                {
                    sl1 = Math.floor(sp.soluong / r.sl) * r.sl_tang;
                    km.push({
                        id: id,kmten: ten, 
                        ma: r.ma, ten: r.ten, ma_tang: r.ma_tang, ten_tang: r.ten_tang, sl: r.sl, sl_tang:r.sl_tang,
                        hhid_tang: r.hhid_tang, sl1: sl1
                    });
                    break;
                }
            }
            
        } // end for noidung
    } //end for ngoai
     
    if (km.length > 0)
    {
        return { co: 1, km: km };
    }
    return {co :0};
}


function promotionCheckHaveChietkhau(nid)
{// check xem co chuong trinh khuyen mai chiet khau hay ko?
    //hhid: hang hoa hien tại
    //soluong: so luong hien tai dang xu ly => de xet chuong trinh tang hang
    var chid = $('#cbbmain_chinhanh').val() ;
    var r,rs, id, loai=0;
   
    for (j=0; j<arrPromotionList.length; j++)
    { 
        rs = arrPromotionList[j].noidung;
        loai = arrPromotionList[j].loai;
        ch = arrPromotionList[j].chid;
        id = arrPromotionList[j].id;
        
        if (ch == '00000000-0000-0000-0000-000000000000' || ch  !=  chid) continue;
        if (loai/1 != 1) continue;// khong kiem tra chuonng trinh khuyen mai khac   ngoai chieu khau
        for (i=0; i<rs.length; i++)
        { // lap lai cac item cua khuyen mai
            r = rs[i];
            if (r.id  == nid)
            { // tim thay
                return { co :1, pid:id, loai:1, ck:r.ck};
            }
        } // end for noidung
    } //end for ngoai
    return {co :0};
}

function _get_permission()
{
    var url = '/RXMP/Run';
    var p = window.location.pathname.split('/');
    var func = p.length ==0 ? '': p.length>=2? p[1] + '/' + p[2]:p[1];
    if (func=='') return;
    var data = {
        from: '1/6/2008', to: '1/6/3008', name:'GetUserPermisionFunction', p1: func
    };
   
    $.ajax({
        async: false,type: "POST",url: url,
        data: {'data':JSON.stringify(data)},
        //contentType: "application/json; charset=utf-8",
       // dataType: "json",
        success: function (data1) {
            if (data1.code/1==100){
                user_permission = JSON.parse(data1.data)[0];
                //$scope.user_per = user_permission;
            }
        }
    });
}
mtmain.controller('permission', function ($scope) {
    $scope.init = function()
    {
        _get_permission();
        $scope.user_per = user_permission;
    
        // // load permission
        // var url = '/RXMP/Run';
        // var p = window.location.pathname.split('/');
        // var func = p.length ==0 ? '': p.length>=2? p[1] + '/' + p[2]:p[1];
        // if (func=='') return;
        // var data = {
        //     from: '1/6/2008', to: '1/6/3008', name:'GetUserPermisionFunction', p1: func
        // };
       
        // $.ajax({
        //     async: false,type: "POST",url: url,
        //     data: {'data':JSON.stringify(data)},
        //     //contentType: "application/json; charset=utf-8",
        //    // dataType: "json",
        //     success: function (data1) {
        //         if (data1.code/1==100){
        //             user_permission = JSON.parse(data1.data)[0];
        //             $scope.user_per = user_permission;
        //         }
        //     }
        // });
        
    
    }
});
 