// cac bien dung chung
//var app = angular.module('myApp', []); // angular application
var lstDonThuId = '';
var js_mn_tt = 0;
var _sotien_thapphan = 0;
var _soluong_thapphan = 1;
var recordnumber = 0;
var iPrintColStart = 3; // cot bat dau in cua kendouigrid
var iPrintColNumbers = 3; // so cot in 

var grid;

var exportFileName; // ten file export
var pageid = 0;
var title = '';
var dataTable; // doi tuong jquery datatable
var columnsWidth = []; // dinh nghia do rong cua cac cot
var columnAutoWidth = true; // tu dong xa dinh do rong cua cot

var bLoadDataCommon = true;// biến xác nhận các hàm load data và xử lý chung tron document.ready
var myVersion = 1;
//var bUsingDataTable = true; // su dung jquery datatbale để tổ chức dữ liệu
var pageSize = 10;
var jsonData; // chua du lieu can bind ra bang
var urlobj;
var bAddEdit = true; // true: add, false: edit
var controllerURL= 'Home';
var TienTo = ''; // tên trường tiền tố của 1 bảng
var tblData = 'tblData'; // tên bảng show dữ liệu
var dlgAddEdit= 'EV_Dlg'; // dialog chữa form edit
var _row = {}; // mang cac gia tri cua 1 row
var _EDIT_ID = '';// id cua ban ghi dang edit
var _EDIT_INDEX = 0;
var _COMMONCODE = '';
var reg = /\'/g; // check ky tu '

var msg = '';
var url = '';

 

function _showAjax() {
    var t = $(window).height() / 2 - 25;
    var l = $(window).width() / 2 - 25;
    $(".ajaxloading").css({ top: t, left: l });
    $(".ajaxloading").show();
    $('.ajaxloading_bg').show();
    //var zindex = maxZIndex();
    //$(".ajaxloading").css({ top: t, left: l });
    //$(".ajaxloading").prop('z-index', zindex + 1);
    //$(".ajaxloading").show();

    //$(".ajaxloading_bg").css({ top: t, left: l });
    //$(".ajaxloading_bg").prop('z-index', zindex);
    //$('.ajaxloading_bg').show();
}
function _hideAjax() {
    $('.ajaxloading_bg').hide();
    $(".ajaxloading").hide();
}
 

function IsEmail(email) {
    var filter = /^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$/;
   
    return filter.test(email);
}


//remove all row
function ResetTable(tbl) {
    $('#' + tbl + ' > tbody').empty();
}
function ResetInput(obj) {
    // obj : id cua doi tuong chua input can reset
    _EDIT_ID = '';
    $('#lbthongbao').html('');
    $('#' + obj + ' input').val('');
    $('#' + obj + ' textarea').val('');
    $('#' + obj + ' input').attr('disabled', false);
     
    //ResetAddItem();
}

function DisplayDialog(paramDialog, paramButton, msg) {


    $('#' + paramDialog).kendoDialog({
        width: "450px",
        visible: true,
        actions: [{
            text: "Tắt thông báo", primary: true
            /*action: function (e) {               
                alert("OK action was clicked");            
                return true;
            },*/
        }]
    });
    var dialog = $('#' + paramDialog).data("kendoDialog");
    dialog.content(msg);
    dialog.title("Thông báo lỗi");

    /* setTimeout(function () {
         dialog.close();
     }, 1000);*/

}

function BindDataCombobox(data, obj)
{
    if (data === undefined) return;
   $('#' + obj).empty();  
    for (i=0; i< data.length; i++)
    {
        $('#' + obj).append($('<option>', {
            value: data[i].ma,
            text: data[i].ten
        }));
    }
}
function BindDataCombobox_V2(data, obj, id, text, value) {
   
    if (data === undefined) return;
    $('#' + obj).empty();

    for (i = 0; i < data.length; i++) {

        $('#' + obj).append($('<option>', {
            value: data[i][id],
            text: data[i][text]
        }));
    }
    if (value == '' && data.length > 0) value = data[0][id];
    if (value!='') $('#' + obj).val(value);
}

function loc(array_source, array_destination, combo_source, combo_destination, keydestination) {
   
   
    var nv = [];
    var nv1=[];
    var key = $('#' + combo_source).val();
    nv = array_destination.data;
    if (nv === undefined) return;
    for (var i = 0; i < nv.length; i++) {
        var k = nv[i][keydestination];
        if (k == key) {
            nv1.push(nv[i]);
        }

    }
    $('#' + combo_destination).empty();
    $('#' + combo_destination).append($('<option>', {
        value: '00000000-0000-0000-0000-000000000000',
        text: '--Chọn--'
    }));

    for (var i = 0; i < nv1.length; i++) {
        $('#' + combo_destination).append($('<option>', {
            value: nv1[i]['id'],
            text: nv1[i]['ten']
        }));
    }

  /*  for (var i = 0; i < array_destination.length; i++) {
        var k = array_destination[i][keydestination];
        if ( k== key) {
            nv.push(array_destination[i]);
        }
    }

    $('#' + combo_destination).empty();
    if (nv.length > 0) {
        $('#' + combo_destination).empty();
        for (i = 0; i < nv.length; i++) {

            $('#' + combo_destination).append('<option value=' + nv[i].id + '>' + nv[i].ten + '</option>');
        }
    }*/
    
}

function BindDataCombobox_V3(data, obj, id, text, value) {
    if (data === undefined) return;
    $('#' + obj).empty();

    for (i = 0; i < data.length; i++) {

        $('#' + obj).append('<option value=' + data[i].id + '>' + data[i].ten + '</option>');
    }
    if (value == '' && data.length > 0) value = data[0][id];
    if (value != '') $('#' + obj).val(value);
}


function BindDataCombobox_FromArray(data, obj) {
    if (data === undefined) return;
    $('#' + obj).empty();
    for (i = 0; i < data.length; i++) {

        $('#' + obj).append($('<option>', {
            value: i,
            text: data[i]
        }));
    }
}

 
// load Nhom quyen
function loadComboNhom(obj, url, id  , name, value) {
    //var url = "/Common/GetPermissionGroups";

    $.post(url, {}, function (data) {
        if (data.msg != "100") {
            console.log("Error: " + data.msg);
            return;
        }
        BindDataCombobox_V2(data.data, obj, id, name, value);
         
    });
}
 
 

function CloseDialog()
{
    $('#' + dlgAddEdit).hide();
    $('#' + dlgAddEdit).modal('hide');
    $('#excelImport').hide();
    $('#excelImport').modal('hide');
}
  
function Delete(tbl) {
    // tbl: id cua bang chua cac ban ghi
    if ($('#' + tbl + ' input:checkbox:checked').length === 0) {
        meAlert("Thông báo", "Chọn bản ghi cần xóa.", 400);
        return false;
    }

   
    kendo.confirm("Bạn thực sự muốn xoa những bản ghi đã chọn?").then(function () {
        var listId = ';';

        // chuyển các id vào thành chuỗi
        $('#' + tbl + ' input:checkbox:checked').each(function () {
            listId += $(this).val() + ";";
        });

        var url = controllerURL + "/Delete";
        //  console.log(url);

        $.post(url, { data: listId }, function (data) {
            var dt = data;
            if (dt.msg != "100") {
                meAlert("Thông báo", 'Lỗi: ' + dt.msg, 500);
              
                return;
            }
            // thuc hien xoa row o client
            //for (i=0; i<dt.data.length; i++)
            //{
            //    $('#' + tbl + ' input:checkbox[value=' + dt.data[i] + ']').closest('tr').remove();
            //  //  jsonData.removeItem(dt.data[i]);
            //}
            BindData_V2(tblData, dlgAddEdit, "id");

        });
    }, function () {
        return;
    });

   
}
 

function BindData_V2(tbl, dlgID, idName ) {
// thay doi: lay truong khoa la: id

   // ResetTable(tbl);
    
    var url = controllerURL + "/GetList";

    $.post(url, {pageid:pageid},function (data) {
        jsonData = data.data;
        // console.log(data);
       // $("#" + tbl).dataTable().fnClearTable();
       // $("#" + tbl).dataTable().fnDestroy();
 
        //_show_grid();
     
        //console.log(idName);
        for (var i = 0; i < jsonData.length; i++) {
            var item = jsonData[i];
            var r = "<tr class='odd' role='row'>"
                + "<td>" + (i + 1) + "</td>"
                + "<td><div><input type='checkbox' class='checkbox' value='" + item[idName] + "' /> <a class='btn_edit' href='javascript://' onclick='_edit(this," + i + ");'  data-toggle='modal' data-target='#" + dlgID + "'>Sửa</a></div></td>";
            r += GetRow(item);
            // GetRow: hàm viết trong view
            r += "</tr>";
            $("#" + tbl).append(r);
        }

      //  dataTable = $("#" + tbl).DataTable({ "iDisplayLength": pageSize });
       
        if (columnAutoWidth) {
            dataTable = $("#" + tbl).DataTable({
                 dom: 'Bfrtip',
                "bPaginate": false
                ,buttons: [
                    'print',
                    'excelHtml5',
                    'copyHtml5'
            ]
            });
        }
        else
        {
            dataTable = $("#" + tbl).DataTable({
                "iDisplayLength": pageSize, "bAutoWidth": columnAutoWidth, "aoColumns": columnsWidth, dom: 'Bfrtip',
                "bPaginate": false,
                "bDestroy": true,
                buttons: [
                    'print',
                    'excelHtml5',
                    'copyHtml5'
                //'csvHtml5',
                  //  'pdfHtml5'
                ]
            });
        }
        
      /*
        // xu ly them cac button cua grid
        $('.buttons-print').html('<i class="fa fa-print"></i>   ' + $('.buttons-print').html());
        $('.buttons-excel').html('<i class="fa fa-download"></i>   ' + $('.buttons-excel').html());
        $('.buttons-copy').html('<i class="fa fa-copy"></i>   ' + $('.buttons-copy').html());

        $('.buttons-print').attr('class', $('.buttons-print').attr('class') + ' btn bg-olive btn-flat ');
        $('.buttons-excel').attr('class', $('.buttons-excel').attr('class') + ' btn bg-olive btn-flat ');
        $('.buttons-copy').attr('class', $('.buttons-copy').attr('class') + ' btn bg-olive btn-flat ');
        */
    });

}


function BindData_V2AtClient(tbl, dlgID, idName, row, bAdd) {
    // thay doi: lay truong khoa la: id
    
    if (bAdd)
    {
        var i = jsonData.length;
        var r = "<tr class='odd' role='row'>"
               + "<td>" + (i + 1) + "</td>"
               + "<td><div><input type='checkbox' class='checkbox' value='" + row[idName] + "' /> <a class='btn_edit' href='javascript://' onclick='_edit(this," + i + ");'  data-toggle='modal' data-target='#" + dlgID + "'>Sửa</a></div></td>";
               r += GetRow(row);
                r += "</tr>";
                $("#" + tbl).append(r);
        jsonData.push(row);
    }
    else
    {
        // tim den ban ghi va update
        UpdateRow(row); 
    }
}

function Save() {
   
    if (!IsValid()) return false; // tren view
    
    // ---------------------
    var url = controllerURL;
    if (bAddEdit) {
        url += "/Insert";
    } else {
        url += "/Update";
    }
    $.post(url, { data: JSON.stringify(_row) }, function (data) {
        //var dt = JSON.parse(data);
        var dt =  data;
        if (dt.code/1 != 100)
        {
            $('#lbthongbao').html("Error: " + dt.msg);
            return;
        }  
        CloseDialog();
        //_row.id = data.data;
        BindData_V2(tblData, dlgAddEdit, "id");
    });
    return true;
}

//----------------------------
function GetDatetime1(btime) {
    try {
        var _date = new Date();
        if (!btime)
            return (_date.getDate()) + '/' + (_date.getMonth()+1) + '/' + (_date.getFullYear());
        else
            return (_date.getDate() ) + '/' + (_date.getMonth()+1) + '/' + (_date.getFullYear()) + ' ' +
            + (_date.getHours()) + ':' + (_date.getMinutes()) + ':' + (_date.getSeconds());

    } catch (err) {
        return "";
    }
}



function GetDatetime(date, btime) {
    try {
        var _date = new Date(parseInt(date.substr(6)));
        if (!btime)
            return (_date.getDate() + 1) + '/' + (_date.getMonth()) + '/' + (_date.getFullYear());
        else
            return (_date.getDate() + 1) + '/' + (_date.getMonth()) + '/' + (_date.getFullYear())
            + (_date.getHours() ) + ':' + (_date.getMinutes()) + ':' + (_date.getSeconds());
            
    } catch (err) {
        return "";
    }
}

function BrowseServer(obj) {
    urlobj = obj;
    OpenServerBrowser(
        '/filemanager/',
        screen.width * 0.7,
        screen.height * 0.7);
}

function OpenServerBrowser(url, width, height) {
    var iLeft = (screen.width - width) / 2;
    var iTop = (screen.height - height) / 2;
    var sOptions = "toolbar=no,status=no,resizable=yes,dependent=yes";
    sOptions += ",width=" + width;
    sOptions += ",height=" + height;
    sOptions += ",left=" + iLeft;
    sOptions += ",top=" + iTop;
    var oWindow = window.open(url, "BrowseWindow", sOptions);
}

function SetUrl(url, width, height, alt) {
    document.getElementById(urlobj).value = url;
    oWindow = null;
}

function SetImgPath() {
    var imges = $(".imgPath");
    var img;
    var parent;
    var str = "";
    for (i = 0; i < imges.length; i++) {
        img = imges[i];
        parent = $(img).parent();
        str += "<div class='input-group'>";
        str += img.outerHTML;
        str += "<span class=\"btn input-group-addon\" " +
            "onclick=\"BrowseServer('" + $(img).prop("id") + "');\">...</span>" +
            "</div>";
        $(parent[0]).html('');
        $(parent[0]).append(str);
    }
}

function SetTinyMCE() {
    tinyMCE.init({
        // General options
        mode: "textareas",
        theme: "modern",
        plugins: 'print preview fullpage powerpaste searchreplace autolink directionality advcode visualblocks visualchars fullscreen image link media template codesample table charmap hr pagebreak nonbreaking anchor toc insertdatetime advlist lists textcolor wordcount tinymcespellchecker a11ychecker imagetools mediaembed  linkchecker contextmenu colorpicker textpattern help',
        toolbar1: 'formatselect | bold italic strikethrough forecolor backcolor | link | alignleft aligncenter alignright alignjustify  | numlist bullist outdent indent  | removeformat',
        image_advtab: true,
        templates: [
            { title: 'Test template 1', content: 'Test 1' },
            { title: 'Test template 2', content: 'Test 2' }
        ],
        // Example content CSS (should be your site CSS)
        content_css: "css/example.css",

        file_browser_callback: function (field_name, url, type, win) {
            var w = window,
                d = document,
                e = d.documentElement,
                g = d.getElementsByTagName('body')[0],
                x = w.innerWidth || e.clientWidth || g.clientWidth,
                y = w.innerHeight || e.clientHeight || g.clientHeight;
            var cmsURL = '/filemanager/?&field_name=' + field_name + '&langCode=' + tinymce.settings.language;
            if (type == 'image') {
                cmsURL = cmsURL + "&type=images";
            }

            tinyMCE.activeEditor.windowManager.open({
                file: cmsURL,
                title: 'Filemanager',
                width: x * 0.8,
                height: y * 0.8,
                resizable: "yes",
                close_previous: "no"
            });
        },

        external_filemanager_path: "/filemanager/",
        filemanager_title: "Responsive Filemanager",
        external_plugins: { "filemanager": "/filemanager/plugin.min.js" }
    });
}

function SetDatetimePicker() {

    $('.form-date').datetimepicker({
        language: '',
        weekStart: 1,
        todayBtn: 1,
        autoclose: 1,
        todayHighlight: 1,
        startView: 2,
        minView: 2,
        forceParse: 0,
        format: "dd/mm/yyyy",
        container: "#EV_dlg .form-date"
    });

    //$(".form-date").datetimepicker("setDate", new Date());

}

function FomatDatimePicker() {
    var dates = $('input[type=datetime]');

    var parent;
    var date;
    for (i = 0; i < dates.length; i++) {
        date = dates[i];
        // console.log(date.outerHTML);

        parent = $(date).parent();
        var str = "<div class='input-group date form-date' data-date='' data-date-format='dd MM yyyy' data-link-format='yyyy-mm-dd'>";
        str += date.outerHTML;
        str += "<span class='input-group-addon'>" +
            "<span class='glyphicon glyphicon-calendar'></span>" +
            "</span>" +
            "</div > ";
        $(parent[0]).html(str);
    }

    SetDatetimePicker();
}


// load Language to combobox
function _load_lang(cboName, idCol, textCol) {
    var url = "Languages/GetList";
    var nv = [];
    $.post(url, {}, function (data) {
        nv = data;
        BindDataCombobox_V2(nv, cbo, idCol, textCol);
    });
}


function printDiv (title, divid)
{
    var contents = "<div class='print'>";
    contents += "<div class='title'>" + title + "</div>";
    
    contents += "<div class='content'><table border=0 cellspacing=0 cellpadding=0>";

    var ths = $("#" + divid + ' th');
    var i, col = ths.length;
    
    contents += '<tr>';
    contents += '<th>TT</th>';
    for (i = 2; i < col; i++)
        contents += '<th>' + $(ths[i]).html() + '</th>';
    contents += '</tr>';
    var trs = $("#" + divid + ' tbody > tr');

    var tds;
    for (j = 0; j < trs.length; j++)
    {
        contents += '<tr>';
        contents += '<td>' + (j+1) + '</td>';

        tds = $(trs[j]).find('td');
        for (i = 2; i < col; i++)
        {
            contents += '<td>' + $(tds[i]).html() + '</td>';
        }
        contents += '</tr>';
    }
    contents += '</table></div></div>';

    var frame1 = $('<iframe />');
    frame1[0].name = "frame1";
    frame1.css({ "position": "absolute", "top": "-1000000px" });
    $("body").append(frame1);
    var frameDoc = frame1[0].contentWindow ? frame1[0].contentWindow : frame1[0].contentDocument.document ? frame1[0].contentDocument.document : frame1[0].contentDocument;
    frameDoc.document.open();
    //Create a new HTML document.
    frameDoc.document.write('<html><head><title></title>');
    frameDoc.document.write('</head><body>');
    //Append the external CSS file.
   // <link rel="stylesheet" href="~/Styles/bootstrap/bootstrap.min.css">
   //<link rel="stylesheet" href="~/Styles/font-awesome/css/font-awesome.min.css">
   //<link rel="stylesheet" href="~/Styles/Ionicons/css/ionicons.min.css">
   //<link rel="stylesheet" href="~/Styles/bootstrap/dataTables.bootstrap.min.css">
    frameDoc.document.write('<link href="/Styles/print.css" rel="stylesheet" type="text/css" />');
    //frameDoc.document.write('<link href="/Styles/bootstrap/bootstrap.min.css" rel="stylesheet" type="text/css" />');
    //frameDoc.document.write('<link href="/Styles/bootstrap/dataTables.bootstrap.min.css" rel="stylesheet" type="text/css" />');
    //Append the DIV contents.
    frameDoc.document.write(contents);
    frameDoc.document.write('</body></html>');
    frameDoc.document.close();
    setTimeout(function () {
        window.frames["frame1"].focus();
        window.frames["frame1"].print();
        frame1.remove();
    }, 500);
}



//--------- KHAI BAO DU LIEU VA XU LY CAC METHODS DE XU LY EXCEL
var exceljsondata = new Array(); // mang du lieu tu excel
var bImportBill = false;// import du lieu cho bill hay danh muc

var sheetindex = 0;
var breaded = false;
var sheet_name_list; // danh sach sheet name
var sheet_name_list_cbb = 'cbbexcelimportsheets'; // id cua commbobox chua sheetname
var exceltableouput = 'tblexcelimportpreview'; // bang hien thi du lieu
var excelfileuploadobject = 'txtexcelfileimport'; // ten doi tuong file upload
var excelMsg = ''; // xau thong bao
var excelColumnSet = [];

function excelExportToTable() {
    var regex = /(.xlsx|.xls)$/;
    /*Checks whether the file is a valid excel file*/
 
    if (regex.test($("#" + excelfileuploadobject).val().toLowerCase())) {
        var xlsxflag = false; /*Flag for checking whether excel is .xls format or .xlsx format*/
        if ($("#" + excelfileuploadobject).val().toLowerCase().indexOf(".xlsx") > 0) {
            xlsxflag = true;
        }
        /*Checks whether the browser supports HTML5*/
        if (typeof (FileReader) != "undefined") {
            var reader = new FileReader();


            //READ FILE-----------------------------------
            if (xlsxflag) {/*If excel file is .xlsx extension than creates a Array Buffer from excel*/
                reader.readAsArrayBuffer($("#" + excelfileuploadobject)[0].files[0]);
            }
            else {
                reader.readAsBinaryString($("#" + excelfileuploadobject)[0].files[0]);
            }
            //END-----------------------------------


            //----- EVENT LOAD DU LIEU
            reader.onload = function (e) {
                var data = e.target.result;
                /*Converts the excel data in to object*/
                if (xlsxflag) {
                    var workbook = XLSX.read(data, { type: 'binary' });
                }
                else {
                    var workbook = XLS.read(data, { type: 'binary' });
                }
                /*Gets all the sheetnames of excel in to a variable*/
                sheet_name_list = workbook.SheetNames;

                BindDataCombobox_FromArray(sheet_name_list, sheet_name_list_cbb);

                sheet_name_list.forEach(function (y) { /*Iterate through all sheets*/
                    /*Convert the cell value to Json*/
                    if (xlsxflag) {
                        exceljsondata[sheetindex] = XLSX.utils.sheet_to_json(workbook.Sheets[y], { range: 1 });
                    }
                    else {
                        exceljsondata[sheetindex] = XLS.utils.sheet_to_row_object_array(workbook.Sheets[y], { range: 1 });
                    }
                    sheetindex += 1;

                    breaded = true;

                });
                excelShowdata();
            }
            //end  //----- EVENT LOAD DU LIEU

            reader.onloadend = function () {
                // show dialog

            }
        }
        else {

            alert("Sorry! Your browser does not support HTML5!");
        }
    }
    else {
        alert("Please upload a valid Excel file!");
    }
}

function excelShowdata() {
    var s = $('#' + sheet_name_list_cbb).val();
    $('#' + exceltableouput).empty();
    excelBindTable(exceljsondata[s], '#' + exceltableouput);
    $('#' + exceltableouput).show();

}

function excelBindTable(jsondata, tableid) {/*Function used to convert the JSON array to Html Table*/
    var columns = excelBindTableHeader(jsondata, tableid); /*Gets all the column headings of Excel*/
    for (var i = 0; i < jsondata.length; i++) {
        var row$ = $('<tr/>');
        for (var colIndex = 0; colIndex < columns.length; colIndex++) {
            var cellValue = jsondata[i][columns[colIndex]];
            if (cellValue == null)
                cellValue = "";
            row$.append($('<td/>').html(cellValue));
        }
        $(tableid).append(row$);
    }
}

function excelBindTableHeader(jsondata, tableid) {/*Function used to get all column names from JSON and bind the html table header*/
    excelColumnSet = [];

    var headerTr$ = $('<tr/>');
    for (var i = 0; i < jsondata.length; i++) {
        var rowHash = jsondata[i];
        for (var key in rowHash) {
            if (rowHash.hasOwnProperty(key)) {
                if ($.inArray(key, excelColumnSet) == -1) {/*Adding each unique column names to a variable array*/
                    excelColumnSet.push(key);
                    headerTr$.append($('<th/>').html(key));
                }
            }
        }
    }
    $(tableid).append(headerTr$);
    return excelColumnSet;
}

/*
function excelRunImport_1()
{ // thuc hien import: khi ko dung voi kendo grid
    var s = $('#' + sheet_name_list_cbb).val();  
 
    // thuc hien import
    var url = controllerURL + '/Import';
    $.post(url, { data: JSON.stringify(exceljsondata[s]), cols: JSON.stringify(excelColumnSet) }, function (data) {
        var dt = data;
        if (dt.code / 1 != 100) {
            alert("Error: " + dt.msg);
            return;
        }
        else
        {
            CloseDialog();
            BindData_V2(tblData, dlgAddEdit, "id");
        }
    });
}
*/
//Hien thi cua so thong bao loi

function DisplayDialog(paramDialog, paramButton, msg) {
   
    $('#' + paramDialog).kendoDialog({
        width: "450px",
        visible: true,
        actions: [{
            text: "Tắt thông báo", primary: true
            /*action: function (e) {               
                alert("OK action was clicked");            
                return true;
            },*/
        }]
    });
    var dialog = $('#' + paramDialog).data("kendoDialog");
    dialog.content(msg);
    dialog.title("Thông báo lỗi");
    
   /* setTimeout(function () {
        dialog.close();
    }, 1000);*/


   
}
function ShowDetail(paramDialog, paramButton, msg) {

    $('#' + paramDialog).kendoDialog({
        width: "80%",
        visible: true,
        actions: [{
            text: "", primary: true
            /*action: function (e) {               
                alert("OK action was clicked");            
                return true;
            },*/
        }]
    });
    var dialog = $('#' + paramDialog).data("kendoDialog");
    dialog.content(msg);
    dialog.title("");

    /* setTimeout(function () {
         dialog.close();
     }, 1000);*/



}
//--------- end CAC BIEN CHO IMPORT DU LIEU


function _activeDeActiveItems(tbl) {
    // tbl: id cua bang chua cac ban ghi

    if ($('#' + tbl + ' input:checkbox:checked:not("#ckcheckall")').length === 0) {
        meAlert("Thông báo", "Chọn bản ghi cần xử lý.", 400);
        return false;
    }

    if (!confirm("Các bản ghi đang Active sẽ chuyển thành DeActive và ngược lại.\nBạn thực sự muốn thực hiện chức năng này?")) return false;

    var listId = ';', listRow = '';

    // chuyển các id vào thành chuỗi
    var arrid = [];
    $('#' + tbl + ' input:checkbox:checked:not("#ckcheckall")').each(function () {

        if ($.trim($(this).val()).length < 32) return;
        listId += $(this).val() + ";";
        arrid.push($(this).val());
    });

    var url = controllerURL + "/ActiveDeactive";
    //  console.log(url);

    $.post(url, { data: listId }, function (data) {
        var dt = data;
        if (dt.msg != "100") {
            meAlert("Thông báo", 'Lỗi: ' + dt.msg, 500);
            return;
        }
        // update trang thai cac bản ghi

        for (var i = 0; i < arrid.length; i++) {
            var a = grid.dataSource.get(arrid[i]);
            a.set("active", a.active / 1 == 1 ? 0 : 1);
        }
        grid.refresh();
    });
}


// gui yeu cau ve may chu

function SaveYeucau() {

    var url = "/Common/AddRequest";
    _row.note = $('#txtnoidungyeucau').val().replace(reg, '');;
    _row.type = "THEM-CHI-NHANH";
    _row.lakho = ($("#cklakho").is(':checked') ? 1 : 0);

    $.post(url, { data: JSON.stringify(_row) }, function (data) {

        var dt = data;
        if (dt.code / 1 != 100) {
            $('#lbthongbao').html("Error: " + dt.msg);
            return;
        } else {
            meAlert("Thông báo", "Yêu cầu của bạn đã được gửi đến RIC.<br>Chúng tôi sẽ liên hệ với bạn sớm nhất.<br>Trân trọng cảm ơn.", 500);
            
            $('#divAddRequest').hide();
            $('#divAddRequest').modal('hide');
        }
    });
    return true;
}


//========================= CAC HAM XU LY CODE VOI KENDOUI
//------------------ import du lieu tu excel len
function excelRunImport() { // thuc hien import
    var s = $('#' + sheet_name_list_cbb).val();

    // thuc hien import
    var url = controllerURL + '/Import';
    CloseDialog()
    $.post(url, { data: JSON.stringify(exceljsondata[s]), cols: JSON.stringify(excelColumnSet) }, function (data) {
        var dt = data;
        if (dt.code / 1 != 100) {
            meAlert("Thông báo", 'Lỗi: ' + dt.msg, 500);

            return;
        }
        else {
            // reload data
            grid.dataSource.read();
            grid.refresh();
        }
    });
}


//------------------ END import du lieu tu excel len

//------------------ xu ly 1 so ham dung chung voi grid va view
function _checkAll() {
    
    if ($('#ckcheckall').is(":checked")) {
        $('#grid_noidung input[type=checkbox]').attr('checked', 'checked');
    } else {
        $('#grid_noidung input[type=checkbox]').attr('checked', false);
    }
}

//--- goi form addnew
function _addItem() {
    bAddEdit = true;
    resetAddItem(); // dinh nghĩa trong tưng view
    InitDefaultValue(); // dinh nghĩa trong tưng view
}

//--- add 1 row vao kendo ui grid sau khi add len may chu
function _gridAddItem(obj, rowindex) {
    var index = 0; //random hard-coded index
    //var grid = $("#grid_noidung").data("kendoGrid");
    var dataSource = grid.dataSource;
    var newItem = dataSource.insert(index, _row);
}


 
function _saveItem() {
  
    if (!IsValid()) return false; // tren view

    var url = controllerURL + (bAddEdit ? "/Insert/" + js_mn_tt : "/Update" + "/" + js_mn_tt);
    
    // _row: dinh nghia trong view
    $.post(url, { data: JSON.stringify(_row) }, function (data) {
        //var dt = JSON.parse(data);
        var dt = data;
        if (dt.code / 1 != 100) {
            //$('#lbthongbao').html("Error: " + dt.msg);
            DisplayDialog('dialog', 'cmdsave_cate', dt.msg);
            return false;
        }
        CloseDialog();
        try {
            _row.id = dt.data.key;
       

        } catch(e){}
         
        if (bAddEdit) {
            try {
                _row.ma = dt.data.code;
                } catch(e){}
           
            _gridAddItem();
            if (typeof _gridAddItemParent == 'function') {
                _gridAddItemParent(); // nhóm hàng hóa ==> có add parent vào
            }
        }
        else {
            _gridUpdateItem(); // dinh nghia trong view rieng
        }
        if (typeof _addSubProductToGridAfterInsertUpdate == 'function') {
            _addSubProductToGridAfterInsertUpdate(); // add subproduct
        }

        grid.clearSelection();
    });

    return true; // de ko submit len may chu
}

// xoa du lieu
function _deleteItems(tbl) {
    // tbl: id cua bang chua cac ban ghi
    
    if ($('#' + tbl + ' input:checkbox:checked:not("#ckcheckall")').length === 0) {
        meAlert("Thông báo", "Chọn bản ghi cần xóa.", 400);
        return false;
    }

   // if (!kendo.confirm("Bạn thực sự muốn xóa những bản ghi đã chọn")) return false;

    kendo.confirm("Bạn thực sự muốn xoa những bản ghi đã chọn?").then(function () {
        var listId = ';', listRow = '';

        // chuyển các id vào thành chuỗi
        $('#' + tbl + ' input:checkbox:checked:not("#ckcheckall")').each(function () {
            if ($.trim($(this).val()).length < 32) return;
            listId += $(this).val() + ";";
            //     grid.removeRow($(this).parent());
            // REMOVE FROM LOCAL store

        });
        
        var url = controllerURL + "/Delete/" + js_mn_tt;
        //  console.log(url);

        $.post(url, { data: listId }, function (data) {
            var dt = data;
            
            if (dt.msg != "100") {
                meAlert("Thông báo", 'Lỗi: ' + dt.msg, 500);

                return;
            }
            try {
                $('#' + tbl + ' input:checkbox:checked:not("#ckcheckall")').each(function () {
                    if ($.trim($(this).val()).length < 32) return;
                    grid.removeRow($(this).parent());
                });
            } catch(e) {}
            // REMOVE FROM LOCAL store

            if (typeof (removeLocalStoreFunction) != 'undefined') removeLocalStoreFunction(listId); // duoc dinh nghia rieng trong tung form
            meAlert("Thông báo", "Lưu ý: các bản ghi đang sử dụng sẽ không được xóa.", 500);
             
            grid.refresh();
        });
    }, function () {
        return;
    });

    
}


function printDivGrid(title, divid, colStart , colPrint) {
   
    var contents = "<div class='print'>";
    contents += "<div class='title'>" + title + "</div>";

    contents += "<div class='content'><table border=0 cellspacing=0 cellpadding=0>";

    var ths = $("#" + divid + ' th');
    var i, col = ths.length;

    contents += '<tr>';
    contents += '<th>TT</th>';
    for (i = colStart; i < colPrint + colStart; i++)
        contents += '<th>' + $(ths[i]).text() + '</th>';
//    contents += '<th>' + $(ths[i]).html() + '</th>';

    contents += '</tr>';
    var trs = $("#" + divid + ' tbody > tr');

    var tds;
    for (j = 0; j < trs.length; j++) {
        contents += '<tr>';
        contents += '<td>' + (j + 1) + '</td>';

        tds = $(trs[j]).find('td');
        for (i = colStart; i < colPrint + colStart; i++) {
            contents += '<td>' + $(tds[i]).html() + '</td>';
        }
        contents += '</tr>';
    }
    contents += '</table></div></div>';

    var frame1 = $('<iframe />');
    frame1[0].name = "frame1";
    frame1.css({ "position": "absolute", "top": "-1000000px" });
    $("body").append(frame1);
    var frameDoc = frame1[0].contentWindow ? frame1[0].contentWindow : frame1[0].contentDocument.document ? frame1[0].contentDocument.document : frame1[0].contentDocument;
    frameDoc.document.open();
    //Create a new HTML document.
    frameDoc.document.write('<html><head><title></title>');
    frameDoc.document.write('</head><body>');
    frameDoc.document.write('<link href="/Styles/print.css?v=2" rel="stylesheet" type="text/css" />');
    frameDoc.document.write(contents);
    frameDoc.document.write('</body></html>');
    frameDoc.document.close();
    setTimeout(function () {
        window.frames["frame1"].focus();
        window.frames["frame1"].print();
        frame1.remove();
    }, 500);
    
}

 

// xu ly phan goi du lieu khi ap dung kendo ui
$(document).ready(function () {
    $(document).ajaxStart(function () {
        _showAjax();
    }).ajaxStop(function () {
        _hideAjax();
    });
    $('.ajaxloading_bg').click(function () {
        _hideAjax();
    });



    if (typeof InitDefault == 'function') {
        InitDefault(); // hàm init các giá trị mặc định, trên form
    }
    if (typeof _showGrid == 'function') {
        _showGrid(); //// dinh nghia rieng 
    }


    $('.btn_fullscreen').click(function () {
        // $('#divcontent').toggleClass('fullscreen');
        $('#divcontent').attr('class', 'box fullscreen');
        $('.btn_fullscreen').hide();
        $('.btn_endfullscreen').show();
        $('#lbcontent_title').show();

        // goi fullscreen cua trinh duyet
        $('body').fullscreen();
    });
    $('.btn_endfullscreen').click(function () {
        $('#divcontent').attr('class', 'box');
        $('.btn_endfullscreen').hide();
        $('.btn_fullscreen').show();
        $('#lbcontent_title').hide();
        $.fullscreen.exit();
    });
    // console.log("load data");
    // dieu khien phan hien ajax spin
    

    if (bLoadDataCommon) {
        BindData_V2(tblData, dlgAddEdit, "id");
    }

    $("#ck_all").click(function () {
        if ($(this).is(":checked")) {
            $('#' + tblData + ' input[type=checkbox]').attr('checked', 'checked');
        } else {
            $('#' + tblData + ' input[type=checkbox]').attr('checked', false);
        }

    });

    // clict nut thêm mới
    $('#cmdadd').click(function () {
        ResetInput(dlgAddEdit);
        InitDefaultValue();// init gia tri mac dinh tren form
        bAddEdit = true;
    });

    // click nut del
    $('#cmddelete').click(function () { Delete(tblData); return false; });
    // click nut save
    $('#cmdsave').click(function () { Save(); return false; });

    // nut in 
    $('#cmdprint').click(function () { printDiv(title, tblData); return false; });

    $('#cmdexcelimportopen').click(function () {
        excelExportToTable();
    });

    $('#cmdexcelimportrun').click(function () {
        excelRunImport();
    });
    $('#cmdexcelimportrun_bill').click(function () {
        excelRunImport_bill();
    });

    $('#' + sheet_name_list_cbb).change(function () { excelShowdata(); })

    // goi them chi nhanh
    $('#cmdyeucauchinhanh').click(function () {
        $.ajax({
            type: "POST",
            url: '/Common/RequestMoreBranch',
            contentType: false, // NEEDED, DON'T OMIT THIS (requires jQuery 1.6+)
            processData: false,
            success: function (response) {

            },
            error: function (response) {
                meAlert("Thông báo", "Lỗi", 400);
             
            }
        });
    });

    $('#cmdexport2excel').click(function () {
        $("#grid_noidung").getKendoGrid().saveAsExcel();

        

    });
    $('#cmdexport2pdf').click(function () {
        $("#grid_noidung").getKendoGrid().saveAsPDF();
    });
    $('#cmdactive').click(function () {
        _activeDeActiveItems('grid_noidung');
    });

    $('#cmdsave_cate').click(function () {
        _saveItem();
    });
    $('#cmdadd_cate').click(function () {
        _addItem();
       

       
    });

    $('#cmdadd_donthu').click(function () {
        _addItem();
        var obj = $('#tblnoidung').get(0);
        var scope = angular.element(obj).scope();
       
        scope.$apply(function () {
            scope.setComboboxValue();
        });

    });

    $('#cmddelete_cate').click(function () {
        _deleteItems('grid_noidung');
    });
    $('#cmdprint_cate').click(function () {
        printDivGrid(title, "grid_noidung", iPrintColStart, iPrintColNumbers); return false;
    });
    

});
//------------------END xu ly 1 so ham dung chung voi grid va view


function _getComboboxSelectedText(cbb) {
    // lay danh sách text cua combobox select multi
    var arr = new Array();
    $("#" + cbb + " option:selected").each(function () {
        arr.push($(this).text());
    });
    return arr;
}

function _tiengVietKhongDau(alias) {
    var str = alias;
    str = str.toLowerCase();
    str = str.replace(/à|á|ạ|ả|ã|â|ầ|ấ|ậ|ẩ|ẫ|ă|ằ|ắ|ặ|ẳ|ẵ/g,"a"); 
    str = str.replace(/è|é|ẹ|ẻ|ẽ|ê|ề|ế|ệ|ể|ễ/g,"e"); 
    str = str.replace(/ì|í|ị|ỉ|ĩ/g,"i"); 
    str = str.replace(/ò|ó|ọ|ỏ|õ|ô|ồ|ố|ộ|ổ|ỗ|ơ|ờ|ớ|ợ|ở|ỡ/g,"o"); 
    str = str.replace(/ù|ú|ụ|ủ|ũ|ư|ừ|ứ|ự|ử|ữ/g,"u"); 
    str = str.replace(/ỳ|ý|ỵ|ỷ|ỹ/g,"y"); 
    str = str.replace(/đ/g,"d");
    str = str.replace(/!|@|%|\^|\*|\(|\)|\+|\=|\<|\>|\?|\/|,|\.|\:|\;|\'|\"|\&|\#|\[|\]|~|\$|_|`|-|{|}|\||\\/g," ");
    str = str.replace(/ + /g," ");
    str = str.trim(); 
    return str;
}
function _createURLFromText(text)
{
    var url = _tiengVietKhongDau(text);
    url = url.replace(/ /g, '-');
    while (url.indexOf('--') >= 0)
    {
        url = url.replace(/--/g, '-');
    }

    return url;
}



function createEditor(languageCode, id, bTomtat) {
    return CKEDITOR.replace(id, {
      language: languageCode,
        toolbar: (bTomtat ? "Tomtat" : "Basic"),
        height: (bTomtat ? 70 : 200)
        , removePlugins: (bTomtat ? 'elementspath' : '')
        , resize_enabled: (bTomtat ? false : true)

    });
}
//---------File upload

function _chonAnh(imgshow, multi) {
    var finder = new CKFinder();
    finder.resourceType = 'Images';
    finder.selectActionFunction = function (fileUrl) {
        var i = $('#' + imgshow).find('img').length;
        var imgs;
        if (multi) {
            imgs = $('#' + imgshow).html() + "<div><div><img src='" + fileUrl + "' onclick='_showImage(this);' data-toggle='modal' data-target='#dlgproduct_detail'/></div><div style='margin-left:-30px;'><a href='javascript://' onclick='_removeImgFromList(this)'   >X</a></div></div>";
            $('#' + imgshow).html(imgs);
        } else {
            imgs = "<div><img src='" + fileUrl + "' onclick='_showImage(this);' data-toggle='modal' data-target='#dlgproduct_detail' /></div>  ";
            $('#' + imgshow).html(imgs);
        }
    };
    finder.popup();
    return false;
}


function _chonFile(imgshow, multi) {
    var finder = new CKFinder();
    finder.resourceType = 'Images';
    finder.selectActionFunction = function (fileUrl) {
        var i = $('#' + imgshow).find('span').length;
        var fna = fileUrl.split('/');
        var fn = fna[fna.length-1];
        var imgs;
        if (multi) {
            imgs = $('#' + imgshow).html() + "<div style='float:left;padding:0px 10px'><div><span> <a class='file' target=_blank href='" + fileUrl + "'><i class='fa fa-paperclip' style='font-size:16pt;'></i>" + fn +"</a></span></div><div style='margin-left:0px;'><a href='javascript://' onclick='_removeImgFileList(this)'   >X</a></div></div>";
            $('#' + imgshow).html(imgs);
        } else {
            imgs = "<div><img src='" + fileUrl + "' onclick='_showImage(this);' data-toggle='modal' data-target='#dlgproduct_detail' /></div>  ";
            $('#' + imgshow).html(imgs);
        }
    };
    finder.popup();
    return false;
}


function _showImage(obj) {
    $('#dlgproduct_detail').find('.modal-title:eq(0)').html('');
    var w =  $('.modal-content').width()-30;
    var h = $(window).height()-170;
    var img = '<img src="' + $(obj).attr('src') +'" style="max-height:' + h +'px;max-width:' + w +'px">';
    $('#dlgproduct_detail').find('.modal-body:eq(0)').html(img);
}
function _removeImgFileList(obj) {
    $(obj).parent().parent().remove();
}

function _removeImgFromList(obj)
{
    $(obj).parent().parent().remove();
}

function isNumeric(n) {
    return !isNaN(parseFloat(n)) && isFinite(n);
}

function getBoolean(str) {
    if ("true".startsWith(str)) {
        return true;
    } else if ("false".startsWith(str)) {
        return false;
    } else {
        return null;
    }
}

function isGuid(stringToTest) {
    if (stringToTest[0] === "{") {
        stringToTest = stringToTest.substring(1, stringToTest.length - 1);
    }
    var regexGuid = /^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$/gi;
    return regexGuid.test(stringToTest);
}

var congty_thongtin = {
    ten: "PHAN MEM RIC",
    chinhanh: "CHI NHANH 1 - HOANG QUOC VIET",
    diachi: "CAU GIAY - HA NOI",
    tel: "097 247 8383"
};


function getChinhanhInfo() {
    congty_thongtin.chinhanh = $('#cbbmain_chinhanh option:selected').text();
    var id = $('#cbbmain_chinhanh').val();
    for (var i = 0; i < mtclient_chinhanh.value.length; i++) {
        if (mtclient_chinhanh.value[i].chid != id) continue;

        congty_thongtin.chinhanh = mtclient_chinhanh.value[i].chten;
        if ( mtclient_chinhanh.value[i].tel != '') congty_thongtin.tel = mtclient_chinhanh.value[i].tel;
        if ( mtclient_chinhanh.value[i].diachi!='') congty_thongtin.diachi = mtclient_chinhanh.value[i].diachi;

        break;
    }
  //  console.log(congty_thongtin);
}

function getChinhanhInfoById(id) {
    
    for (var i = 0; i < mtclient_chinhanh.value.length; i++) {
        if (mtclient_chinhanh.value[i].chid != id) continue;
        congty_thongtin.chinhanh = mtclient_chinhanh.value[i].chten;
        if (mtclient_chinhanh.value[i].tel != '') congty_thongtin.tel = mtclient_chinhanh.value[i].tel;
        if (mtclient_chinhanh.value[i].diachi != '') congty_thongtin.diachi = mtclient_chinhanh.value[i].diachi;

        break;
    }
}

var ChuSo=new Array(" không "," một "," hai "," ba "," bốn "," năm "," sáu "," bảy "," tám "," chín ");
var Tien=new Array( "", " nghìn", " triệu", " tỷ", " nghìn tỷ", " triệu tỷ");

//1. Hàm đọc số có ba chữ số;
function DocSo3ChuSo(baso)
{
    var tram;
    var chuc;
    var donvi;
    var KetQua="";
    tram=parseInt(baso/100);
    chuc=parseInt((baso%100)/10);
    donvi=baso%10;
    if(tram==0 && chuc==0 && donvi==0) return "";
    if(tram!=0)
    {
        KetQua += ChuSo[tram] + " trăm ";
        if ((chuc == 0) && (donvi != 0)) KetQua += " linh ";
    }
    if ((chuc != 0) && (chuc != 1))
    {
        KetQua += ChuSo[chuc] + " mươi";
        if ((chuc == 0) && (donvi != 0)) KetQua = KetQua + " linh ";
    }
    if (chuc == 1) KetQua += " mười ";
    switch (donvi)
    {
        case 1:
            if ((chuc != 0) && (chuc != 1))
            {
                KetQua += " mốt ";
            }
            else
            {
                KetQua += ChuSo[donvi];
            }
            break;
        case 5:
            if (chuc == 0)
            {
                KetQua += ChuSo[donvi];
            }
            else
            {
                KetQua += " lăm ";
            }
            break;
        default:
            if (donvi != 0)
            {
                KetQua += ChuSo[donvi];
            }
            break;
    }
    return KetQua;
}

//2. Hàm đọc số thành chữ (Sử dụng hàm đọc số có ba chữ số)

function DocTienBangChu(SoTien)
{
    var lan=0;
    var i=0;
    var so=0;
    var KetQua="";
    var tmp="";
    var ViTri = new Array();
    if(SoTien<0) return "Số tiền âm !";
    if(SoTien==0) return "Không đồng !";
    if(SoTien>0)
    {
        so=SoTien;
    }
    else
    {
        so = -SoTien;
    }
    if (SoTien > 8999999999999999)
    {
        //SoTien = 0;
        return "Số quá lớn!";
    }
    ViTri[5] = Math.floor(so / 1000000000000000);
    if(isNaN(ViTri[5]))
        ViTri[5] = "0";
    so = so - parseFloat(ViTri[5].toString()) * 1000000000000000;
    ViTri[4] = Math.floor(so / 1000000000000);
    if(isNaN(ViTri[4]))
        ViTri[4] = "0";
    so = so - parseFloat(ViTri[4].toString()) * 1000000000000;
    ViTri[3] = Math.floor(so / 1000000000);
    if(isNaN(ViTri[3]))
        ViTri[3] = "0";
    so = so - parseFloat(ViTri[3].toString()) * 1000000000;
    ViTri[2] = parseInt(so / 1000000);
    if(isNaN(ViTri[2]))
        ViTri[2] = "0";
    ViTri[1] = parseInt((so % 1000000) / 1000);
    if(isNaN(ViTri[1]))
        ViTri[1] = "0";
    ViTri[0] = parseInt(so % 1000);
    if(isNaN(ViTri[0]))
        ViTri[0] = "0";
    if (ViTri[5] > 0)
    {
        lan = 5;
    }
    else if (ViTri[4] > 0)
    {
        lan = 4;
    }
    else if (ViTri[3] > 0)
    {
        lan = 3;
    }
    else if (ViTri[2] > 0)
    {
        lan = 2;
    }
    else if (ViTri[1] > 0)
    {
        lan = 1;
    }
    else
    {
        lan = 0;
    }
    for (i = lan; i >= 0; i--)
    {
        tmp = DocSo3ChuSo(ViTri[i]);
        KetQua += tmp;
        if (ViTri[i] > 0) KetQua += Tien[i];
        if ((i > 0) && (tmp.length > 0)) KetQua += ',';//&& (!string.IsNullOrEmpty(tmp))
    }
    if (KetQua.substring(KetQua.length - 1) == ',')
    {
        KetQua = KetQua.substring(0, KetQua.length - 1);
    }
    KetQua = KetQua.substring(1,2).toUpperCase()+ KetQua.substring(2);
    return KetQua +' đồng';//.substring(0, 1);//.toUpperCase();// + KetQua.substring(1);
}

function formatNumber(num) {
    return num.toString().replace(/(\d)(?=(\d{3})+(?!\d))/g, "$1,")
}

function formatSo0(so) { /* dinh dang so */

    nStr = so + '';
    x = nStr.split('.');
    x1 = x[0];
    x2 = x.length > 1 ? '.' + x[1] : '';
    var rgx = /(\d+)(\d{3})/;
    while (rgx.test(x1)) {
        x1 = x1.replace(rgx, '$1' + ',' + '$2');
    }
    x2 = "" + (x2 / 1).toFixed(0);
    x2 = x2.substring(1, 10);

    return x1 + x2;
}
function formatSo(so,n) { /* dinh dang so */

    nStr = so + '';
    x = nStr.split('.');
    x1 = x[0];
    x2 = x.length > 1 ? '.' + x[1] : '';
    var rgx = /(\d+)(\d{3})/;
    while (rgx.test(x1)) {
        x1 = x1.replace(rgx, '$1' + ',' + '$2');
    }
    x2 = "" + (x2 / 1).toFixed(n);
    x2 = x2.substring(1, 10);

    return x1 + x2;
}


function formatDateTime(ngay, format) { /* lay ngay  */
    
    var a = ngay;
    so = a.getMinutes();
    format = format.replace(/mmm/g, so < 10 ? '0' + so : so);
    so = a.getHours();
    format = format.replace(/hh/g, so < 10 ? '0' + so : so);
    so = a.getSeconds();
    format = format.replace(/ss/g, so < 10 ? '0' + so : so);

    so = a.getDate();
    format = format.replace(/dd/g, so< 10 ? '0' + so : so);
    so = a.getMonth();
    format = format.replace(/MM/g, so < 9 ? '0' + (so + 1) : so + 1);
    so = a.getFullYear();
    format = format.replace(/yyyy/g, so);

    return format;
}

var SotienBangChu = function () {
    var t = ["không", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín"],
        r = function (r, n) {
            var o = "",
                a = Math.floor(r / 10),
                e = r % 10;
            return a > 1 ? (o = " " + t[a] + " mươi", 1 == e && (o += " mốt")) : 1 == a ? (o = " mười", 1 == e && (o += " một")) : n && e > 0 && (o = " lẻ"), 5 == e && a >= 1 ? o += " lăm" : 4 == e && a >= 1 ? o += " tư" : (e > 1 || 1 == e && 0 == a) && (o += " " + t[e]), o
        },
        n = function (n, o) {
            var a = "",
                e = Math.floor(n / 100),
                n = n % 100;
            return o || e > 0 ? (a = " " + t[e] + " trăm", a += r(n, !0)) : a = r(n, !1), a
        },
        o = function (t, r) {
            var o = "",
                a = Math.floor(t / 1e6),
                t = t % 1e6;
            a > 0 && (o = n(a, r) + " triệu", r = !0);
            var e = Math.floor(t / 1e3),
                t = t % 1e3;
            return e > 0 && (o += n(e, r) + " ngàn", r = !0), t > 0 && (o += n(t, r)), o
        };
    return {
        read: function (r) {
            if (0 == r) return t[0];
            var n = "",
                a = "";
            do ty = r % 1e9, r = Math.floor(r / 1e9), n = r > 0 ? o(ty, !0) + a + n : o(ty, !1) + a + n, a = " tỷ"; while (r > 0);
            n = n.trim();
            n = n.charAt(0).toUpperCase() + n.slice(1) + '';
            return n  + ' đồng./.';
        }
    }
}();

function xoa_dau(str) {
    str = str.replace(/à|á|ạ|ả|ã|â|ầ|ấ|ậ|ẩ|ẫ|ă|ằ|ắ|ặ|ẳ|ẵ/g, "a");
    str = str.replace(/è|é|ẹ|ẻ|ẽ|ê|ề|ế|ệ|ể|ễ/g, "e");
    str = str.replace(/ì|í|ị|ỉ|ĩ/g, "i");
    str = str.replace(/ò|ó|ọ|ỏ|õ|ô|ồ|ố|ộ|ổ|ỗ|ơ|ờ|ớ|ợ|ở|ỡ/g, "o");
    str = str.replace(/ù|ú|ụ|ủ|ũ|ư|ừ|ứ|ự|ử|ữ/g, "u");
    str = str.replace(/ỳ|ý|ỵ|ỷ|ỹ/g, "y");
    str = str.replace(/đ/g, "d");
    str = str.replace(/À|Á|Ạ|Ả|Ã|Â|Ầ|Ấ|Ậ|Ẩ|Ẫ|Ă|Ằ|Ắ|Ặ|Ẳ|Ẵ/g, "A");
    str = str.replace(/È|É|Ẹ|Ẻ|Ẽ|Ê|Ề|Ế|Ệ|Ể|Ễ/g, "E");
    str = str.replace(/Ì|Í|Ị|Ỉ|Ĩ/g, "I");
    str = str.replace(/Ò|Ó|Ọ|Ỏ|Õ|Ô|Ồ|Ố|Ộ|Ổ|Ỗ|Ơ|Ờ|Ớ|Ợ|Ở|Ỡ/g, "O");
    str = str.replace(/Ù|Ú|Ụ|Ủ|Ũ|Ư|Ừ|Ứ|Ự|Ử|Ữ/g, "U");
    str = str.replace(/Ỳ|Ý|Ỵ|Ỷ|Ỹ/g, "Y");
    str = str.replace(/Đ/g, "D");
    return str;
}
/* kendo grid filter -remove combobox */
function filterRemoveCombobox(element) {
    var form = element.closest("form");  
    form.find(".k-filter-help-text:first").remove();//text("Select an item from the list:");
    form.find("input:first").attr("placeholder"," Gõ từ cần tìm");
    form.find("select").remove();
}

function closeWin(id) {
    var dialog = $("#" + id).data("kendoWindow");
    dialog.close();
}
function createAndShowPopup(id, title) {
    $('#'+id).kendoWindow({
        width: "85%",
        modal: true,
      //  height: '120px',
        iframe: true,
        resizable: false,
        title: title,
        content: url,
       
        visible: false
    });

    var popup = $("#"+id).data('kendoWindow');
    popup.open();
    popup.center();
}
//Tính số ngày, trừ ngày nghỉ
function addWeekdays(date, weekdays) {
    var newDate = new Date(date.getTime());
    var i = 0;
    while (i < weekdays) {
        newDate.setDate(newDate.getDate() + 1);
        var day = newDate.getDay();
        if (day !=0 && day !=6) {
            i++;
        }
    }
    return newDate;
}

function DateTimeVN(date) {
    var now = new Date(date);
    var day = ("0" + now.getDate()).slice(-2);
    var month = ("0" + (now.getMonth() + 1)).slice(-2);
    var today = now.getFullYear() + "-" + (month) + "-" + (day);
    return today;
}

function FormatDateTimeVN(date) {
    var now = new Date(date);
    var day = ("0" + now.getDate()).slice(-2);
    var month = ("0" + (now.getMonth() + 1)).slice(-2);
    var today = (day) + "-" + (month) + "-" + now.getFullYear();
    return today;
}

