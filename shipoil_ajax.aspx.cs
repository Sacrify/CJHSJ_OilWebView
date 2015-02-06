using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Runtime.Serialization;
using System.Web.Script.Serialization;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;

using CJHSJ_OilWebView.Utils;
using CJHSJ_OilWebView.Entity;


namespace CJHSJ_OilWebView
{
    public partial class shipoil_ajax : System.Web.UI.Page
    {
        private string _operType = string.Empty;
        private string _response = string.Empty;
        private string mmsi;
        private string shipname;
        private string unit;
        private string btime;
        private string etime;
        private int pageSize = 0;
        private int oilType = 0;

        private const int SEASON_SUMMER = 5;
        private const int SEASON_WINTER = 11;
        private double oilDensitySummer = 0.84;
        private double oilDensityWinter = 0.86;
        private double oilExKgPerHour = 3.0;

        public bool CheckFormUrl()
        {
            if (HttpContext.Current.Request.UrlReferrer == null)
            {
                return false;
            }
            if ((HttpContext.Current.Request.UrlReferrer.Host) != (HttpContext.Current.Request.Url.Host))
            {
                return false;
            }
            return true;
        }

        public string QueryString(string s) 
        {
            if (HttpContext.Current.Request.QueryString[s] != null && HttpContext.Current.Request.QueryString[s] != "")
            {
                return HttpContext.Current.Request.QueryString[s].ToString();
            }
            return string.Empty;
        }

        public string PostString(string s)
        {
            if (HttpContext.Current.Request.Form[s] != null && HttpContext.Current.Request.Form[s] != "")
            {
                return HttpContext.Current.Request.Form[s].ToString();
            }
            return string.Empty;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!CheckFormUrl())
            {
                Response.End();
            }

            this._operType = QueryString("oper");
            mmsi = QueryString("mmsi");

            bool bWriteResponse = true;

            // 获取top数
            string strSize = QueryString("pageSize");
            if (!string.IsNullOrEmpty(strSize))
            {
                if (int.TryParse(strSize, out pageSize) == false)
                {
                    pageSize = 0;
                }
            }

            switch (this._operType) 
            {
                default:
                    break;
            }
        }

        private void getShipOilInfo()
        {
            if (string.IsNullOrEmpty(mmsi))
            {
                return;
            }

            //dohOil.Reset();
            //string sqlCmd =
            //    "SELECT TOP 1 * " +
            //    "FROM ship_reginfo " +
            //    "WHERE mmsi = '" + mmsi + "'";
            //dohOil.SqlCmd = sqlCmd;
            //DataTable dt = dohOil.GetDataTable();
            //if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            //{
            //    if (int.TryParse(dt.Rows[0]["oil_type"].ToString(), out oilType) == false)
            //    {
            //        oilType = 0;
            //    }
            //}

            //{
            //    dohOil.Reset();
            //    sqlCmd =
            //        "SELECT TOP 1 * " +
            //        "FROM hsj_oil_density " +
            //        "WHERE CompanyID = " + this.ComId + " " +
            //        "AND OilTypeID = " + oilType.ToString();
            //    dohOil.SqlCmd = sqlCmd;
            //    dt = dohOil.GetDataTable();
            //    if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            //    {
            //        if (double.TryParse(dt.Rows[0]["OilDensitySummer"].ToString(), out oilDensitySummer) == false)
            //        {
            //            oilDensitySummer = 0.84;
            //        }

            //        if (double.TryParse(dt.Rows[0]["OilDensityWinter"].ToString(), out oilDensityWinter) == false)
            //        {
            //            oilDensityWinter = 0.86;
            //        }
            //    }
            //}
        }
    }
}