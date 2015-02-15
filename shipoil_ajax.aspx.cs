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

        /// <summary>
        /// Check Form URL
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Return Query Parameter
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public string QueryString(string s) 
        {
            if (HttpContext.Current.Request.QueryString[s] != null && HttpContext.Current.Request.QueryString[s] != "")
            {
                return HttpContext.Current.Request.QueryString[s].ToString();
            }
            return string.Empty;
        }

        /// <summary>
        /// Return Post Parameter
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public string PostString(string s)
        {
            if (HttpContext.Current.Request.Form[s] != null && HttpContext.Current.Request.Form[s] != "")
            {
                return HttpContext.Current.Request.Form[s].ToString();
            }
            return string.Empty;
        }

        /// <summary>
        /// 输出json结果
        /// </summary>
        /// <param name="success">是否操作成功,0表示失败;1表示成功</param>
        /// <param name="str">输出字符串</param>
        /// <returns></returns>
        public string JsonResult(int success, string str)
        {
            return "{\"result\" :\"" + success.ToString() + "\",\"returnval\" :\"" + str + "\"}";

        }

        /// <summary>
        /// Page Load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

                case "getShipConfig":
                    getShipOilInfo();
                    getShipConfig();
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

                this.companyID = dt.Rows[0]["company_id"].ToString();
            }

            {
                dohOil.Reset();
                sqlCmd =
                    "SELECT TOP 1 * " +
                    "FROM hsj_oil_density " +
                    "WHERE CompanyID = " + this.companyID + " " +
                    "AND OilTypeID = " + oilType.ToString();
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

        private void getShipConfig()
        {
            ShipConfig config = null;

            //调用油耗数据操作类
            dohOil.Reset();

            if (mmsi != null && mmsi.Length > 0)
            {
                string sqlCmd =
                    "SELECT "
                    + "TOP 1 "
                    + "a.mmsi, "
                    + "a.warning_llun, "
                    + "a.warning_loil, "
                    + "a.warning_speed, "
                    + "a.warning_moil, "
                    + "a.warning_rlun, "
                    + "a.warning_roil "
                    + "FROM ship_config a "
                    + "WHERE "
                    + "a.mmsi='" + mmsi + "'";

                dohOil.SqlCmd = sqlCmd;
                DataTable dt = dohOil.GetDataTable();

                //绑定对象
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    if (config == null) config = new ShipConfig();
                    if (dt.Rows[0]["mmsi"] != DBNull.Value) config.mmsi = mmsi;
                    if (dt.Rows[0]["warning_llun"] != DBNull.Value) config.warning_llun = dt.Rows[0]["warning_llun"].ToString();
                    if (dt.Rows[0]["warning_loil"] != DBNull.Value) config.warning_loil = dt.Rows[0]["warning_loil"].ToString();
                    if (dt.Rows[0]["warning_speed"] != DBNull.Value) config.warning_speed = dt.Rows[0]["warning_speed"].ToString();
                    if (dt.Rows[0]["warning_moil"] != DBNull.Value) config.warning_moil = dt.Rows[0]["warning_moil"].ToString();
                    if (dt.Rows[0]["warning_rlun"] != DBNull.Value) config.warning_rlun = dt.Rows[0]["warning_rlun"].ToString();
                    if (dt.Rows[0]["warning_roil"] != DBNull.Value) config.warning_roil = dt.Rows[0]["warning_roil"].ToString();
                    config.oil_type = oilType.ToString();
                    config.oil_density_summer = oilDensitySummer.ToString();
                    config.oil_density_winter = oilDensityWinter.ToString();
                }

                //输出返回
                if (config != null)
                {
                    dtHelp.ResutJsonStr((Object)config);
                    //WriteLocalLog(Cxw.Utils.dtHelp.ToJson((Object)config));
                }
                else
                {
                    this._response = JsonResult(0, "未查找到数据");
                }
            }
        }

        private void setShipConfig()
        {
            if (string.IsNullOrEmpty(mmsi) == true)
            {
                return;
            }

            //调用油耗数据操作类
            dohOil.Reset();
            bool hasRecord = false;

            string sqlCmd =
                "SELECT "
                + "TOP 1 * "
                + "FROM ship_config "
                + "WHERE "
                + "mmsi='" + mmsi + "'";

            dohOil.SqlCmd = sqlCmd;
            DataTable dt = dohOil.GetDataTable();

            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                hasRecord = true;
            }

            //WriteLocalLog("SELECT ship_config: " + hasRecord.ToString());

            dohOil.Reset();
            sqlCmd = null;

            if (hasRecord)
            {
                sqlCmd =
                    "UPDATE ship_config " +
                    "SET " +
                    "warning_llun = " + QueryString("llun") + ", " +
                    "warning_loil = " + QueryString("loil") + ", " +
                    "warning_speed = " + QueryString("speed") + ", " +
                    "warning_moil = " + QueryString("moil") + ", " +
                    "warning_rlun = " + QueryString("rlun") + ", " +
                    "warning_roil = " + QueryString("roil") + " " +
                    "WHERE mmsi = '" + mmsi + "' ";
            }
            else
            {
                sqlCmd =
                    "INSERT INTO ship_config " +
                    "(mmsi, " +
                    "warning_llun, " +
                    "warning_loil, " +
                    "warning_speed, " +
                    "warning_moil, " +
                    "warning_rlun, " +
                    "warning_roil) " +
                    "VALUES " +
                    "( '" + mmsi + "', " +
                    QueryString("llun") + ", " +
                    QueryString("loil") + ", " +
                    QueryString("speed") + ", " +
                    QueryString("moil") + ", " +
                    QueryString("rlun") + ", " +
                    QueryString("roil") + ") ";
            }

            dohOil.SqlCmd = sqlCmd;
            int result = dohOil.ExecuteSqlNonQuery();

            //WriteLocalLog("save ship_config:" + result.ToString());

            dohOil.Reset();
            hasRecord = false;

            sqlCmd =
                    "SELECT TOP 1 * " +
                    "FROM hsj_oil_density " +
                    "WHERE CompanyID = " + this.companyID + " " +
                    "AND OilTypeID = " + QueryString("oil_type");
            dohOil.SqlCmd = sqlCmd;
            dt = dohOil.GetDataTable();
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                hasRecord = true;
            }

            //WriteLocalLog("SELECT hsj_oil_density: " + hasRecord.ToString());

            dohOil.Reset();
            if (hasRecord)
            {
                sqlCmd =
                    "UPDATE hsj_oil_density " +
                    "SET " +
                    "OilDensitySummer = " + QueryString("oil_density_summer") + ", " +
                    "OilDensityWinter = " + QueryString("oil_density_winter") + " " +
                    "WHERE " +
                    "CompanyID = " + this.companyID + " " +
                    "AND OilTypeID = " + QueryString("oil_type");
            }
            else
            {
                sqlCmd =
                    "INSERT INTO hsj_oil_density " +
                    "(CompanyID, " +
                    "OilTypeID, " +
                    "OilDensitySummer, " +
                    "OilDensityWinter) " +
                    "VALUES " +
                    "( " + this.companyID + ", " +
                    QueryString("oil_type") + ", " +
                    QueryString("oil_density_summer") + ", " +
                    QueryString("oil_density_winer") + ")";
            }

            dohOil.SqlCmd = sqlCmd;
            result = dohOil.ExecuteSqlNonQuery();

            //WriteLocalLog("save hsj_oil_density:" + result.ToString());

            this._response = JsonResult(1, "保存成功");
        }

        private void DefaultResponse()
        {
            this._response = JsonResult(0, "未知操作");
        }
    }
}