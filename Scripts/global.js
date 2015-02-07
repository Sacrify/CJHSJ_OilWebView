//<!--网站参数begin
var site = new Object();
site.Name = 'OilWebView';
site.Name2 = 'CJHSJ_OilWebView';
site.Url = 'localhost';
site.Dir = '/';
site.CookieDomain = '';
site.CookiePrev = 'cjhsj';
//-->网站参数end


//写cookies 
function setCookie(name, value) {
    var Days = 30;
    var exp = new Date();
    exp.setTime(exp.getTime() + Days * 24 * 60 * 60 * 1000);
    document.cookie = name + "=" + escape(value) + ";expires=" + exp.toGMTString();
}

//读取cookies 
function getCookie(name) {
    var arr, reg = new RegExp("(^| )" + name + "=([^;]*)(;|$)");
    if (arr = document.cookie.match(reg))
        return unescape(arr[2]);
    else
        return null;
}

//删除cookies 
function delCookie(name) {
    var exp = new Date();
    exp.setTime(exp.getTime() - 1);
    var cval = getCookie(name);
    if (cval != null)
        document.cookie = name + "=" + cval + ";expires=" + exp.toGMTString();
}

function curDateTime() {
    var d = new Date();
    var year = d.getFullYear();
    var month = d.getMonth() + 1;
    var date = d.getDate();
    var day = d.getDay();
    var Hours = d.getHours(); //获取当前小时数(0-23)
    var Minutes = d.getMinutes(); //获取当前分钟数(0-59)
    var Seconds = d.getSeconds(); //获取当前秒数(0-59)
    var curDateTime = year;
    if (month > 9)
        curDateTime = curDateTime + "-" + month;
    else
        curDateTime = curDateTime + "-" + "0" + month;
    if (date > 9)
        curDateTime = curDateTime + "-" + date;
    else
        curDateTime = curDateTime + "-" + "0" + date + " ";
    if (Hours > 9)
        curDateTime = curDateTime + " " + Hours;
    else
        curDateTime = curDateTime + " " + "0" + Hours;
    if (Minutes > 9)
        curDateTime = curDateTime + ":" + Minutes;
    else
        curDateTime = curDateTime + ":" + "0" + Minutes;
    if (Seconds > 9)
        curDateTime = curDateTime + ":" + Seconds;
    else
        curDateTime = curDateTime + ":" + "0" + Seconds;

    return curDateTime;
}


function ensurePositive(num) {

    if (num == null || num == '' || num == undefined) return 0;

    var floatNum = parseFloat(num);
    if (isNaN(floatNum) == true) return 0;
    if (isFinite(floatNum) == false) return 0;
    if (floatNum < 0) return 0;

    return floatNum;
}

function ensureValue(num) {
    if (num == null || num == '' || num == undefined) return 0;
    return num;
}

function ensureNum(num) {

    if (num == null || num == '' || num == undefined) return 0;

    var floatNum = parseFloat(num);
    if (isNaN(floatNum) == true) return 0;
    if (isFinite(floatNum) == false) return 0;

    return floatNum;
}

/* 
* 获得时间差,时间格式为 年-月-日 小时:分钟:秒 或者 年/月/日 小时：分钟：秒 
* 其中，年月日为全格式，例如 ： 2010-10-12 01:00:00 
* 返回精度为：秒，分，小时，天
*/

function GetDateDiff(startTime, endTime, diffType) {
    //将xxxx-xx-xx的时间格式，转换为 xxxx/xx/xx的格式 
    startTime = startTime.replace(/\-/g, "/");
    endTime = endTime.replace(/\-/g, "/");

    //将计算间隔类性字符转换为小写
    diffType = diffType.toLowerCase();
    var sTime = new Date(startTime); //开始时间
    var eTime = new Date(endTime);  //结束时间
    //作为除数的数字
    var divNum = 1;
    switch (diffType) {
        case "second":
            divNum = 1000;
            break;
        case "minute":
            divNum = 1000 * 60;
            break;
        case "hour":
            divNum = 1000 * 3600;
            break;
        case "day":
            divNum = 1000 * 3600 * 24;
            break;
        case "month":
            divNum = 1000 * 3600 * 24 * 30;
            break;
        default:
            break;
    }
    return parseInt((eTime.getTime() - sTime.getTime()) / parseInt(divNum));
}

function ParseDateString(dateString) {
    dateString = dateString.replace(/\-/g, "/");
    return new Date(dateString);
}


function OilHelper_GetOil(si) {
    var total_oil = 0;
    if (si.hasOwnProperty('oil')) {
        if (
        si.oil != undefined &&
        si.oil != null &&
        si.oil != ''
        ) {
            total_oil += parseFloat(si.oil);
        }
    }

    if (si.hasOwnProperty('oil_ex')) {
        if (
        si.oil_ex != undefined &&
        si.oil_ex != null &&
        si.oil_ex != ''
        ) {
            total_oil += parseFloat(si.oil_ex);
        }
    }

    return ensurePositive(total_oil).toFixed(3);
}

function OilHelper_GetOilCost(si) {
    var total_oil_cost = 0;

    if (si.hasOwnProperty('oilcost')) {
        if (
        si.oilcost != undefined &&
        si.oilcost != null &&
        si.oilcost != ''
        ) {
            total_oil_cost += parseFloat(si.oilcost);
        }
    }

    if (si.hasOwnProperty('oilcost_ex')) {
        if (
        si.oilcost_ex != undefined &&
        si.oilcost_ex != null &&
        si.oilcost_ex != ''
        ) {
            total_oil_cost += parseFloat(si.oilcost_ex);
        }
    }

    return ensurePositive(total_oil_cost).toFixed(2);
}

function OilHelper_GetMil(si) {
    var total_dist = 0;
    if (si.hasOwnProperty('mil')) {
        if (
        si.mil != undefined &&
        si.mil != null &&
        si.mil != '') {
            total_dist += parseFloat(si.mil);
        }
    }

    return ensurePositive(total_dist).toFixed(2);
}

function OilHelper_GetSailTime(si) {
    var sail_time = 0;
    if (si.hasOwnProperty('sail_time')) {
        if (
        si.sail_time != undefined &&
        si.sail_time != null &&
        si.sail_time != '') {
            sail_time += parseFloat(si.sail_time);
        }
    }

    return ensurePositive(sail_time).toFixed(1);
}

function OilHelper_GetRunningTime(si) {
    var running_time = 0;
    if (si.hasOwnProperty('running_time')) {
        if (
        si.running_time != undefined &&
        si.running_time != null &&
        si.running_time != '') {
            running_time += parseFloat(si.running_time);
        }
    }

    return ensurePositive(running_time).toFixed(1);
}

function OilHelper_GetPriceBTimeString(res) {
    var btime = 0;
    if (res.hasOwnProperty('PriceBTime')) {
        if (
            res.PriceBTime != undefined &&
            res.PriceBTime != null &&
            res.PriceBTime != '') {
            return res.PriceBTime;
        }
    }

    return btime;
}

function OilHelper_GetPriceType(res) {
    var type = 0;
    if (res.hasOwnProperty('OilType')) {
        if (
            res.OilType != undefined &&
            res.OilType != null &&
            res.OilType != '') {

            type = parseInt(res.OilType);
        }
    }

    return type;
}

function OilHelper_GetOilPrice(res) {
    var price = 0;
    if (res.hasOwnProperty('OilPrice')) {
        if (
            res.OilPrice != undefined &&
            res.OilPrice != null &&
            res.OilPrice != '') {

            price = ensurePositive(res.OilPrice);
        }
    }

    return parseFloat(price).toFixed(2);
}
