
var log_max_id = 0;

function InitDefault() {
    bLoadDataCommon = false;
}

function _LoadHome() {
    var chinhanh = $('#cbbmain_chinhanh').val();

    if (chinhanh == '' || chinhanh == null) {
        window.setTimeout('_LoadHome()', 1000);
        return;
    }
    _LoadHistory();

    _row = { "chid": chinhanh };
    var url1 = '/Home/LoadDashboard';
    $.post(url1, { data: JSON.stringify(_row) }, function (data) {


        if (data.code == 100) {
            var row = JSON.parse(data.data[0].giatri);

            $("#lbsohoadon").html(formatSo0(row.so));
            $("#lbdoanhthu").html(formatSo0(row.so_thang));
            $("#lbthucthu").html(formatSo0(row.so_quy));
            $("#lblailo").html(formatSo0(row.so_nam));

          
          //  if (row != null) createChart_DoanhthuNhanvien(row);
            
            var cate = JSON.parse(data.data[1].giatri); // category
            row = JSON.parse(data.data[2].giatri);
          
            var row1 = [{
                "name": "Số Download", "data": row
            }];
            
           
            if (row != null && cate != null) createChart_DoanhthuLoinhuan(row1, cate);
 
            
        } else {

        }
    });
}

function createChart_DoanhthuLoinhuan(_data, _category) {
    $("#chartdoanhthu_loinhuan").kendoChart({
        title: {
            position: "bottom",
            text: "DOWNLOAD - HỢP ĐỒNG"
        },
        legend: {
            position: "top"
        },
        chartArea: {
            background: ""
        },
        seriesDefaults: {
            type: "line",
            style: "smooth"
        },
        series: _data,
        valueAxis: {
            labels: {
                format: "{0}"
            },
            line: {
                visible: false
            },
            axisCrossingValue: -1
        },
       
        categoryAxis: {
            categories: _category,// [2002, 2003, 2004, 2005, 2006, 2007, 2008, 2009, 2010, 2011],
            majorGridLines: {
                visible: false
            },
            labels: {
                rotation: "auto"
            }
        }, 
        tooltip: {
            visible: true,
            format: "{0}",
            template: "#= series.name #: #= value #"
        }
    });
}

function createChart_DoanhthuNhanvien(_data) {
    $("#chartdoanhthu_nv").kendoChart({
        title: {
            position: "bottom",
            text: "DOANH THU THEO NHÂN VIÊN"
        },
        legend: {
            visible: false
        },
        chartArea: {
            background: ""
        },
        seriesDefaults: {
            labels: {
                visible: true,
                background: "transparent",
                template: "#= category # vnđ \n #= value#%"
            }
        },
        series: [{
            type: "pie",
            startAngle: 150,
            data: _data
        }],
        tooltip: {
            visible: true,
            format: "{0}%"
        }
    });
}

function selectStyleThaoTac(loai) {
   return   loai / 1 == 1 ? 'thaotac_them' : loai / 1 == 2 ? 'thaotac_sua' : loai / 1 == 3 ? 'thaotac_xoa' : 'thaotac';
}

function customizeNoidung(r) {
    var noidung = '';
    var ma = r.noidung;
    var k = ma.indexOf('[');
    if (k > 0 && r.loai / 1 != 3) { // tao link
        ma = ma.substring(k, ma.indexOf(']'));
        noidung = '<a class=noidung_link data-billid="' + r.billid + '" data-billtype="' + r.billtype + '" href="javascript://">' + ma + '</a>';
        noidung = r.noidung.replace(ma, noidung);
        noidung = noidung.replace('[', '').replace(']', '');
    }
    else
        noidung = r.noidung;
    return noidung;
}

function _logBind(obj, data) {
    var s = '', r, row, thaotac, ngay;
    var o = $('#' + obj);

    o.html('<table class=home_history><tr><td  class=title colspan=4>LỊCH SỬ THAO TÁC <div style="float:right"><a href="/Log"><i class="fa fa-angle-double-right"></i></a></div></td></tr></table>');
    o = $('#' + obj + ' table');
    for (i = 0; i < data.length; i++) {
        r = data[i];
        thaotac = selectStyleThaoTac(r.loai);

        noidung = customizeNoidung(r);
        ngay = new Date(r.ngay);

        row = '<tr>'
            + '<td class=ngay>' + formatDateTime(ngay, 'hh:mmm') + '</td>'
            + '<td class=uid>' + r.uid + '</td>'
            + '<td class=' + thaotac + '>' + r.thaotac + '</td>'
            + '<td class=noidung>' + noidung + '</td>'
            + '</tr>'

        o.append(row);
    }
    $('.noidung_link').click(function () {
        var id = $(this).data('billid');
        var type = $(this).data('billtype');
        _logShowBillDetail(id, type);
    })
}

function _logShowBillDetail(id, type) {
    var scope = angular.element($('#div_home_detail').get(0)).scope();

    scope.$apply(function () {
        scope.billsDetailShow(id, type);
    });
}

function _LoadHistory() {
    var data = {
        chid: $('#cbbmain_chinhanh').val(), id: log_max_id
    };
    var url = '/Log/LoadHome';
    $.post(url, { data: JSON.stringify(data) }, function (data) {

        if (data.code / 1 == 100 && data.data.length > 0) {
            // data.data = JSON.parse(data.data);
            if (data.data.length > 0) {
                log_max_id = data.data[0].id;
                _logBind('divlog', data.data);
            }
        }

    });

}

$(document).ready(function () {
    _LoadHome();
   
    // _LoadHistory();

    //$(document).ready(createChart);
    // $(document).bind("kendo:skinChange", createChart);

    $('#cbbmain_chinhanh').change(function () {
        _LoadHome();
         
        _LoadHistory();
    });

    $('#windowBillsShow_NX').kendoWindow({
        width: "900px", title: "THÔNG TIN HÓA ĐƠN", visible: false,
        actions: ["Close" /*/, "Pin",// "Minimize",// "Maximize"*/],
    }).data("kendoWindow");

    $('#windowBillsShow_TC').kendoWindow({
        width: "600px", title: "THÔNG TIN PHIẾU THU/CHI", visible: false,
        actions: ["Close" /*/, "Pin",// "Minimize",// "Maximize"*/],
    }).data("kendoWindow");
});


localStorageName_Session = 'mtbill_home_detail';

mtmain.controller('mtbill_home_detail', function ($scope) {
    $scope.billCurrent = {};
    
    function checkBillExistsAtClient(name, billid, sessionOrLocal) {
        // sessionOrLocal true/false => Session/Local
        
        var mtbilltemp
        if (sessionOrLocal)
            mtbilltemp = getSessionStorage(name)
        else
            mtbilltemp = getLocalStorage(name)

        if (mtbilltemp == null) mtbilltemp = [];

        // check exists => xoa truoc khi inssert
        for (var i = 0; i < mtbilltemp.length; i++) {
            if (mtbilltemp[i].id == billid) {
                $scope.billCurrent = mtbilltemp[i];
                return true;
            }
        }
        return false;
    }

    $scope.billsDetailShow = function (id, type) {
        type = type / 1;
        id = id.toLowerCase();
         
        if (checkBillExistsAtClient(localStorageName_Session  , id, true)) {
            if (type >= 10) {
                $("#windowBillsShow_NX").data("kendoWindow").center().open();
            }
            else {
                $("#windowBillsShow_TC").data("kendoWindow").center().open();
            }
            return true;
        }

        var url1;
        if (type >= 10) {
            // nhap xuat
            url1 = '/Bill/20/LoadBillDetail';
            $.post(url1, { billid: id }, function (data) {
                if (data.code / 1 == 100) {
                    var rows = JSON.parse(data.data);
                    var khachhang = JSON.parse(rows[0].khachhang);
                    var hanghoa = JSON.parse(rows[0].products);
                    $scope.billCurrent = rows[0]
                    $scope.billCurrent.khachhang = khachhang;
                    $scope.billCurrent.products = hanghoa;
                    var sltong = 0;
                    for (var i = 0; i < $scope.billCurrent.products.length; i++) sltong += $scope.billCurrent.products[i].soluong / 1;
                    $scope.soluong = sltong;
                    $scope.tienchu = SotienBangChu.read($scope.billCurrent.phaitra);
                    $scope.nomoi = $scope.billCurrent.nocu + $scope.billCurrent.phaitra - $scope.billCurrent.thanhtoan;

                    saveOrRemoveLocalStorage(true, localStorageName_Session  , $scope.billCurrent, true);

                    $scope.$apply();
                    // show
                    $("#windowBillsShow_NX").data("kendoWindow").center().open();
                    return;
                }
                meAlert("Thông báo",   data.msg, 400);
                
            });

        }
        else {
            // thu chi
            url1 = '/Bill/Pay/LoadBillDetail';
            $.post(url1, { data: id }, function (data) {
                if (data.code / 1 == 100) {
                    var rows = JSON.parse(data.data);
                    $scope.billCurrent = rows[0];
                    $scope.billCurrent.sotien_chu = SotienBangChu.read($scope.billCurrent.sotien);
                    saveOrRemoveLocalStorage(true, localStorageName_Session, $scope.billCurrent, true);
                    $scope.$apply();
                    // show
                    $("#windowBillsShow_TC").data("kendoWindow").center().open();
                    return;
                }
                meAlert("Thông báo", data.msg, 400);
            });
        }
    }//end function
}); // end controller
