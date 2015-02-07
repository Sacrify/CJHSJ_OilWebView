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
        private string companyID = "0";

        private const int SEASON_SUMMER = 5;
        private const int SEASON_WINTER = 11;
        private const double OIL_DENSITY_SUMMER = 0.84;
        private const double OIL_DENSITY_WINTER = 0.86;
        private double oilDensitySummer = OIL_DENSITY_SUMMER;
        private double oilDensityWinter = OIL_DENSITY_WINTER;
        private double oilExKgPerHour = 3.0;

        private enum DURATION_TYPE
        {
            NONE = 0,
            SAIL_TIME,
            RUNNING_TIME_LEFT,
            RUNNING_TIME_RIGHT,
            RUNNING_TIME
        }

        public DbOperHandler dohOil;

        override protected void OnInit(EventArgs e)
        {
            Server.ScriptTimeout = 90;//默认脚本过期时间
            base.OnInit(e);

            //油耗数据操作类
            if (dohOil == null)
            {
                System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.AppSettings["ConnectionString"].ToString());
                dohOil = new SqlDbOperHandler(conn);
            }
        }

        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);
            if (dohOil != null) 
            {
                dohOil.Dispose();
                dohOil = null;
            }
        }

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
                case "getShipInfo":
                    getShipInfo();
                    break;

                case "getRealtimeStat":
                    getShipOilInfo();
                    getRealtimeStat();
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// getShipInfoByMMSI
        /// </summary>
        /// <returns>ShipInfo</returns>
        private ShipInfo getShipInfoByMMSI()
        {
            dohOil.Reset();
            dohOil.SqlCmd = "SELECT TOP 1 * " +
                "FROM " + "ship_reginfo " +
                "WHERE " + "mmsi=" + "'" + mmsi + "'";

            DataTable dt = dohOil.GetDataTable();
            if (dt.Rows.Count > 0)
            {
                ShipInfo si = new ShipInfo();
                si.mmsi = dt.Rows[0]["mmsi"].ToString();
                si.shipname = dt.Rows[0]["ship_name"].ToString();
                si.speed = dt.Rows[0]["speed"].ToString();
                si.load_weight = dt.Rows[0]["load_weight"].ToString();
                si.draft = dt.Rows[0]["draft"].ToString();
                si.llunsp = dt.Rows[0]["llun_speed"].ToString();
                si.rlunsp = dt.Rows[0]["rlun_speed"].ToString();
                return si;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// getShipInfo
        /// </summary>
        private void getShipInfo()
        {
            List<ShipInfo> items = new List<ShipInfo>();
            ShipInfo si = getShipInfoByMMSI();

            if (si != null)
            {
                items.Add(si);
            }

            //输出返回
            dtHelp.ResutJsonStr((Object)items);
        }

        /// <summary>
        /// getShipOilInfo
        /// </summary>
        private void getShipOilInfo()
        {
            if (string.IsNullOrEmpty(mmsi))
            {
                return;
            }

            dohOil.Reset();
            string sqlCmd =
                "SELECT TOP 1 * " +
                "FROM ship_reginfo " +
                "WHERE mmsi = '" + mmsi + "'";
            dohOil.SqlCmd = sqlCmd;
            DataTable dt = dohOil.GetDataTable();
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                if (int.TryParse(dt.Rows[0]["oil_type"].ToString(), out oilType) == false)
                {
                    oilType = 0;
                }
            }

            {
                dohOil.Reset();
                sqlCmd =
                    "SELECT TOP 1 * " +
                    "FROM hsj_oil_density " +
                    "WHERE OilTypeID = " + oilType.ToString();
                dohOil.SqlCmd = sqlCmd;
                dt = dohOil.GetDataTable();
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    if (double.TryParse(dt.Rows[0]["OilDensitySummer"].ToString(), out oilDensitySummer) == false)
                    {
                        oilDensitySummer = OIL_DENSITY_SUMMER;
                    }

                    if (double.TryParse(dt.Rows[0]["OilDensityWinter"].ToString(), out oilDensityWinter) == false)
                    {
                        oilDensityWinter = OIL_DENSITY_WINTER;
                    }
                }
            }
        }

        /// <summary>
        /// getRealtimeStat
        /// </summary>
        private void getRealtimeStat()
        {
            List<RealtimeInfo> items = new List<RealtimeInfo>();
            RealtimeInfo ri = null;
            //DateTime datetime=DateTime.Now;
            //string endTime = datetime.AddMinutes(Const.LIMIT).ToString("yyyy-MM-dd HH:mm:ss");
            //string startTime = datetime.AddMinutes(-Const.MINUTES).ToString("yyyy-MM-dd HH:mm:ss");

            //调用油耗数据操作类
            dohOil.Reset();
            dohOil.SqlCmd =

                "SELECT * FROM ( " +
                "SELECT " +
                "MAX(stime) as stime, " +
                "AVG(speed) as speed, " +
                "MAX(mmsi) as mmsi," +
                "AVG(pos_x) as pos_x, " +
                "AVG(pos_y) as pos_y, " +
                "AVG(llun_rps) as llun_rps, " +
                "AVG(rlun_rps) as rlun_rps, " +
                "AVG(lmain_oil_gps) as lmain_oil_gps, " +
                "AVG(lasist_oil_gps) as lasist_oil_gps, " +
                "AVG(rmain_oil_gps) as rmain_oil_gps, " +
                "AVG(rasist_oil_gps) as rasist_oil_gps, " +
                "DATEDIFF(MINUTE, MAX(stime), GETDATE()) as intervalMinute, " +
                "AVG(drift_interval) as drift_interval " +
                "FROM " +
                "( " +
                "SELECT TOP 3 * " +
                "FROM ship_dynamicinfo " +
                "WHERE mmsi='" + mmsi + "' " +
                "ORDER BY stime DESC " +
                ")  AS dynamic_top3 " +
                ") AS result_left " +
                "INNER JOIN " +
                "( " +
                "SELECT " +
                "TOP 1 " +
                "stime as t1stime, " +
                "lmpg_status, " +
                "rmpg_status " +
                "FROM ship_dynamicinfo " +
                "WHERE mmsi='" + mmsi + "' " +
                "ORDER BY stime DESC " +
                ") AS dynamic_top1 " +
                "ON result_left.stime = dynamic_top1.t1stime";

            DataTable dt = dohOil.GetDataTable();
            //绑定对象
            if (dt != null && dt.Rows.Count > 0)
            {
                ri = new RealtimeInfo();
                ri.mmsi = dt.Rows[0]["mmsi"].ToString();
                ri.stime = dt.Rows[0]["stime"].ToString();
                ri.pos_x = dt.Rows[0]["pos_x"].ToString();
                ri.pos_y = dt.Rows[0]["pos_y"].ToString();
                ri.speed = dt.Rows[0]["speed"].ToString();
                ri.llunrps = dt.Rows[0]["llun_rps"].ToString();
                ri.rlunrps = dt.Rows[0]["rlun_rps"].ToString();
                try
                {
                    if (string.IsNullOrEmpty(ri.stime) == false)
                    {
                        DateTime stime = Convert.ToDateTime(ri.stime);
                        double oilDensity = getOilDensity(stime);

                        if (dt.Rows[0]["lmain_oil_gps"] != DBNull.Value)
                            ri.lmain_gps = (Convert.ToDouble(dt.Rows[0]["lmain_oil_gps"].ToString()) * oilDensity).ToString();
                        if (dt.Rows[0]["rmain_oil_gps"] != DBNull.Value)
                            ri.rmain_gps = (Convert.ToDouble(dt.Rows[0]["rmain_oil_gps"].ToString()) * oilDensity).ToString();
                        if (dt.Rows[0]["lasist_oil_gps"] != DBNull.Value)
                            ri.lasist_gps = (Convert.ToDouble(dt.Rows[0]["lasist_oil_gps"].ToString()) * oilDensity).ToString();
                        if (dt.Rows[0]["rasist_oil_gps"] != DBNull.Value)
                            ri.rasist_gps = (Convert.ToDouble(dt.Rows[0]["rasist_oil_gps"].ToString()) * oilDensity).ToString();
                    }
                }
                catch (Exception e)
                {

                }

                ri.interval_minute = dt.Rows[0]["intervalMinute"].ToString();
                ri.drift_interval = dt.Rows[0]["drift_interval"].ToString();
                ri.lmpg_status = dt.Rows[0]["lmpg_status"].ToString();
                ri.rmpg_status = dt.Rows[0]["rmpg_status"].ToString();
                ri.lrun_time = getLatestDuration(DURATION_TYPE.RUNNING_TIME_LEFT).ToString();
                ri.rrun_time = getLatestDuration(DURATION_TYPE.RUNNING_TIME_RIGHT).ToString();
                ri.sail_time = getLatestDuration(DURATION_TYPE.SAIL_TIME).ToString();
                items.Add(ri);
            }

            dtHelp.ResutJsonStr((Object)items);
        }

        /// <summary>
        /// getOilDensity
        /// </summary>
        /// <param name="dtBTime">season time</param>
        /// <returns></returns>
        private double getOilDensity(DateTime dtBTime)
        {
            if (SEASON_SUMMER <= dtBTime.Month && dtBTime.Month < SEASON_WINTER)
            {
                return oilDensitySummer;
            }
            else
            {
                return oilDensityWinter;
            }
        }

        /// <summary>
        /// getLatestDuration
        /// </summary>
        /// <param name="type">Duration Type</param>
        /// <returns></returns>
        private double getLatestDuration(DURATION_TYPE type)
        {
            TimeSpan sailingTimeSpan = TimeSpan.Zero;
            dohOil.Reset();

            string durationTable = getDurationTable(type);
            string sqlCmd =
                "SELECT TOP 1 * " +
                "FROM " + durationTable + " " +
                "ORDER BY startTime DESC";
            dohOil.SqlCmd = sqlCmd;
            DataTable dt = dohOil.GetDataTable();

            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                if (dt.Rows[0]["endTime"] == DBNull.Value)
                {
                    DateTime recordBTime = Convert.ToDateTime(dt.Rows[0]["startTime"].ToString());
                    DateTime recordETime = DateTime.Now;

                    sailingTimeSpan = recordETime - recordBTime;
                }
            }

            return sailingTimeSpan.TotalHours;
        }

        /// <summary>
        /// Helper Function: getDurationTable
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private string getDurationTable(DURATION_TYPE type)
        {
            string durationTable = "dbo.Ship_Voyeges3";
            switch (type)
            {
                case DURATION_TYPE.RUNNING_TIME_LEFT:
                    durationTable = "dbo.ship_voyeges_llun";
                    break;

                case DURATION_TYPE.RUNNING_TIME_RIGHT:
                    durationTable = "dbo.ship_voyeges_rlun";
                    break;

                case DURATION_TYPE.RUNNING_TIME:
                    durationTable = "dbo.Ship_Voyeges2";
                    break;

                case DURATION_TYPE.SAIL_TIME:
                default:
                    break;
            }

            return durationTable;
        }
    }
}