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

        private static bool bLog = false;
        private static volatile object lockLog = new object();
        private void WriteLocalLog(string logContent)
        {
            if (bLog == true)
            {
                lock (lockLog)
                {
                    using (System.IO.StreamWriter sw = System.IO.File.AppendText("C:\\Temp\\log.txt"))
                    {
                        sw.WriteLine(logContent);
                        sw.Flush();
                    }
                }
            }
        }

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
        /// Default Response
        /// </summary>
        private void DefaultResponse()
        {
            this._response = JsonResult(0, "未知操作");
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

                case "setShipConfig":
                    setShipConfig();
                    break;

                case "getOilTimingStatistics":
                    getShipOilInfo();
                    getOilTimingStatistics();
                    break;

                case "getOilHistoryStatistics":
                    getShipOilInfo();
                    getOilHistoryStatistics();
                    break;

                case "getOilHistListNoPaging":
                    getShipOilInfo();
                    getOilHistListNoPaging();
                    break;

                case "exportHistoryStatusToExcel":
                    {
                        bWriteResponse = false;
                        getShipOilInfo();
                        ExportHistoryStatusToExcel();
                    }
                    break;

                case "getOilSaveYearRecord":
                    getOilSaveYearRecord();
                    break;
                case "getOilFillYearRecords":
                    getOilFillYearRecords();
                    break;
                case "getOilFillMonthRecord":
                    getOilFillMonthRecord();
                    break;
                case "getOilFillRecords":
                    getOilFillRecords();
                    break;
                case "updateOilFillRecords":
                    updateOilFillRecords();
                    break;

                default:
                    DefaultResponse();
                    break;
            }

            if (bWriteResponse)
            {
                Response.Write(this._response);
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
        /// return years' season's time
        /// </summary>
        /// <param name="dtBTime"></param>
        /// <param name="dtETime"></param>
        /// <returns></returns>
        private List<DateTime> getSeasonTime(DateTime dtBTime, DateTime dtETime)
        {
            List<DateTime> seasonTimes = new List<DateTime>();

            for (int i = dtBTime.Year; i <= dtETime.Year; i++)
            {
                DateTime seasonSummer = new DateTime(i, SEASON_SUMMER, 1);
                if (dtBTime < seasonSummer && seasonSummer < dtETime)
                {
                    seasonTimes.Add(seasonSummer);
                }

                DateTime seasonWinter = new DateTime(i, SEASON_WINTER, 1);
                if (dtBTime < seasonWinter && seasonWinter < dtETime)
                {
                    seasonTimes.Add(seasonWinter);
                }
            }

            return seasonTimes;
        }

        /// <summary>
        /// Sort Date Time List
        /// </summary>
        /// <param name="listDateTime"></param>
        private void SortDateTimeList(ref List<DateTime> listDateTime)
        {
            listDateTime.Sort(delegate(DateTime dtL, DateTime dtR)
            {
                if (dtL == null && dtR == null) return 0;
                else if (dtL == null) return -1;
                else if (dtR == null) return 1;
                else
                {
                    if (dtL > dtR) return 1;
                    else if (dtL == dtR) return 0;
                    else return -1;
                }
            });
        }

        /// <summary>
        /// Get price change DateTime from dtBTime to dtETime
        /// </summary>
        /// <param name="oilType">Oil Type</param>
        /// <param name="dtBTime">Begin Time</param>
        /// <param name="dtETime">End Time</param>
        /// <returns></returns>
        private List<DateTime> getPriceTime(int oilType, DateTime dtBTime, DateTime dtETime)
        {
            List<DateTime> priceTimes = new List<DateTime>();

            dohOil.Reset();

            string sqlCmd =
                "SELECT stime " +
                "FROM hsj_oil_price " +
                "WHERE " +
                "(stime > '" + dtBTime.ToString("yyyy-MM-dd") + "'" + " AND stime < '" + dtETime.ToString("yyyy-MM-dd") + "') " +
                "AND " +
                "OilTypeID = " + oilType.ToString();

            dohOil.SqlCmd = sqlCmd;
            DataTable dt = dohOil.GetDataTable();

            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    try
                    {
                        if (dt.Rows[i]["stime"] != DBNull.Value)
                        {
                            DateTime priceTime = Convert.ToDateTime(dt.Rows[i]["stime"].ToString());

                            if (priceTime > dtBTime && priceTime < dtETime)
                            {
                                priceTimes.Add(priceTime);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
            }

            SortDateTimeList(ref priceTimes);
            return priceTimes;
        }

        /// <summary>
        /// return oil price by given dtBTime
        /// </summary>
        /// <param name="oilType"></param>
        /// <param name="dtBTime"></param>
        /// <returns></returns>
        private double getOilPrice(int oilType, DateTime dtBTime)
        {
            double oilPrice = 0;

            dohOil.Reset();
            string sqlCmd =
                "SELECT TOP 1 price " +
                "FROM hsj_oil_price " +
                "WHERE stime <= '" + dtBTime.ToString("yyyy-MM-dd") + "'" +
                "AND OilTypeID = " + oilType.ToString() +
                "ORDER BY stime DESC";

            dohOil.SqlCmd = sqlCmd;
            DataTable dt = dohOil.GetDataTable();

            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                try
                {
                    oilPrice = double.Parse(dt.Rows[0]["price"].ToString());
                }
                catch (Exception e)
                {
                    oilPrice = 0;
                    Console.WriteLine(e.ToString());
                }
            }

            return oilPrice;
        }

        /// <summary>
        /// return oil price list
        /// </summary>
        private void getOilPrices()
        {
            List<HSJ_OilPrice> listPrice = new List<HSJ_OilPrice>();

            dohOil.Reset();
            dohOil.SqlCmd =
                "SELECT * " +
                "FROM hsj_oil_price " +
                "WHERE OilTypeID = " + QueryString("oilType") +
                "ORDER BY stime DESC";
            DataTable dt = dohOil.GetDataTable();

            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    HSJ_OilPrice oilPrice = new HSJ_OilPrice();

                    oilPrice.PriceID = dt.Rows[i]["OilPriceID"].ToString();
                    oilPrice.PriceBTime = dt.Rows[i]["stime"].ToString();
                    oilPrice.OilPrice = dt.Rows[i]["price"].ToString();
                    oilPrice.OilType = dt.Rows[i]["OilTypeID"].ToString();
                    listPrice.Add(oilPrice);
                }
            }

            //输出返回
            dtHelp.ResutJsonStr((Object)listPrice);
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
        /// return ship dynamic info list
        /// </summary>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <param name="topCount"></param>
        /// <param name="isOrder"></param>
        /// <returns></returns>
        private DataTable getShipDynamicInfoList(string beginTime, string endTime, int topCount, bool isOrder)
        {
            //调用油耗数据操作类
            dohOil.Reset();
            string sqlName = "";
            //如果船名不为空则加上
            if (!string.IsNullOrEmpty(shipname))
            {
                sqlName = "'" + shipname + "' as name,";
            }
            else
            {
                sqlName = "'' as name,";
            }
            //查找结果
            string strTable = "SELECT substring(CONVERT(varchar(100),a.stime,25),1,16) as stime,a.mmsi," + sqlName
            + "ROUND(ISNULL(a.pos_x, 0),6) AS pos_x,"
            + "ROUND(ISNULL(a.pos_y, 0),6) AS pos_y,"
            + "ROUND(ISNULL(a.speed, 0),2) AS speed,"
            + "ROUND(ISNULL(a.llun_rps, 0),2) AS llun_rps,"
            + "ROUND(ISNULL(a.rlun_rps, 0),2) AS rlun_rps,"
            + "ROUND(ISNULL(a.lmain_oil_gps, 0),3) AS lmain_oil_gps,"
            + "ROUND(ISNULL(a.lasist_oil_gps, 0),3) AS lasist_oil_gps,"
            + "ROUND(ISNULL(a.rmain_oil_gps, 0),3) AS rmain_oil_gps,"
            + "ROUND(ISNULL(a.rasist_oil_gps, 0),3) AS rasist_oil_gps,"
                //+ "ROUND(AVG((((a.lmain_oil_gps + a.lasist_oil_gps) + a.rmain_oil_gps) + a.rasist_oil_gps)),2) AS total_oil_gps,"
                // PPENG: + "ROUND(AVG(a.drift_interval),2) AS drift_interval FROM ship_dynamicinfo a WHERE a.mmsi ='" + mmsi + "' AND a.stime >= '" + beginTime + "' AND a.stime <= '" + endTime + "'"
            + "ROUND(ISNULL(a.drift_interval, 0),2) AS drift_interval FROM ship_dynamicinfo a WHERE a.mmsi ='" + mmsi + "' AND a.stime > '" + beginTime + "' AND a.stime <= '" + endTime + "'"
                //+ " GROUP BY substring(CONVERT(varchar(100),a.stime,25),1,16),a.mmsi";
            + " ";

            //设置top值
            string strTop = "";
            if (topCount > 0)
            {
                strTop = " top " + topCount;
            }

            string sqlCmd = "SELECT " + strTop + " stime,mmsi,name,pos_x,pos_y,speed,llun_rps,rlun_rps,lmain_oil_gps"
            + ",rmain_oil_gps,lasist_oil_gps,rasist_oil_gps,drift_interval from (" + strTable + ") a ";

            //判断是否排序
            if (isOrder)
            {
                sqlCmd += " order by stime desc";
            }
            dohOil.SqlCmd = sqlCmd;
            return dohOil.GetDataTable();
        }

        /// <summary>
        /// return ship accumulation info list
        /// </summary>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <param name="topCount"></param>
        /// <param name="isOrder"></param>
        /// <returns></returns>
        private DataTable getShipAccInfoList(string beginTime, string endTime, int topCount, bool isOrder)
        {
            //调用油耗数据操作类
            dohOil.Reset();
            //查找结果
            //string strTable = "SELECT substring(CONVERT(varchar(100),a.stime,25),1,16) as stime,a.mmsi,"
            //+ "a.total_lmain_oil,a.total_lasist_oil,a.total_rmain_oil,a.total_rasist_oil,"
            //+ "ROUND(a.total_lmain_oil + a.total_lasist_oil + a.total_rmain_oil + a.total_rasist_oil, 2) total_oil, a.sum_distance as sum_distance, a.total_distance as total_dist"
            //+ " FROM ship_accumulation a WHERE a.mmsi ='" + mmsi + "' AND a.stime >= '" + beginTime + "' AND a.stime <= '" + endTime + "'";

            // PPENG: In my opinion, beginTime should not be involved.
            string strTable =
                "SELECT substring(CONVERT(varchar(100),a.stime,25),1,16) as stime,a.mmsi,"
                + "a.total_lmain_oil,a.total_lasist_oil,a.total_rmain_oil,a.total_rasist_oil,"
                + "ROUND(ISNULL(a.total_lmain_oil, 0) + ISNULL(a.total_lasist_oil, 0) + ISNULL(a.total_rmain_oil, 0) + ISNULL(a.total_rasist_oil, 0), 3) as total_oil, "
                + "ROUND(ISNULL(a.lmain_oil, 0) + ISNULL(a.lasist_oil, 0) + ISNULL(a.rmain_oil, 0) + ISNULL(a.rasist_oil, 0), 3) as sum_oil, "
                + "a.sum_distance as sum_distance, a.total_distance as total_dist "
                + " FROM ship_accumulation a WHERE a.mmsi ='" + mmsi + "' AND a.stime > '" + beginTime + "' AND a.stime <= '" + endTime + "'";

            //设置top值
            string strTop = "";
            if (topCount > 0)
            {
                strTop = " top " + topCount;
            }
            //string sqlcmd = "select " + strtop + " a.stime,a.mmsi,a.total_oil,sum(b.total_oil) as total_accoil,a.total_distance as total_dist,sum(b.total_distance) as total_accdist,"
            //+ "a.total_lmain_oil,sum(b.total_lmain_oil) as total_lmain_accoil"
            //+ ",a.total_rmain_oil,sum(b.total_rmain_oil) as total_rmain_accoil,a.total_lasist_oil,sum(b.total_lasist_oil) as total_lasist_accoil,"
            //+ "a.total_rasist_oil,sum(b.total_rasist_oil) as total_rasist_accoil"
            //+ " from (" + strtable + ") a inner join (" + strtable + ") b on b.stime <=a.stime group by a.stime,a.mmsi,a.total_oil,a.total_distance,a.total_lmain_oil,a.total_rmain_oil,a.total_lasist_oil,a.total_rasist_oil";

            string sqlCmd = "SELECT " + strTop + " * from (" + strTable + ") a";
            //判断是否排序
            if (isOrder)
            {
                sqlCmd += " order by a.stime desc";
            }
            else
            {
                sqlCmd += " order by a.stime asc";
            }

            dohOil.SqlCmd = sqlCmd;
            return dohOil.GetDataTable();
        }

        /// <summary>
        /// return ship accumulatioon info
        /// </summary>
        /// <param name="time"></param>
        /// <param name="beginEndTime"></param>
        /// <param name="excludeEnd"></param>
        /// <returns></returns>
        private DataTable getShipAccInfo(string time, bool beginEndTime, bool excludeEnd = true)
        {
            //调用油耗数据操作类
            dohOil.Reset();

            string sqlCmd =
                "SELECT TOP 1 substring(CONVERT(varchar(100),a.stime,25),1,16) as stime,a.mmsi,"
                + "a.total_lmain_oil,a.total_lasist_oil,a.total_rmain_oil,a.total_rasist_oil,"
                + "ROUND(ISNULL(a.total_lmain_oil, 0) + ISNULL(a.total_lasist_oil, 0) + ISNULL(a.total_rmain_oil, 0) + ISNULL(a.total_rasist_oil, 0), 3) as total_oil, "
                + "ROUND(ISNULL(a.lmain_oil, 0) + ISNULL(a.lasist_oil, 0) + ISNULL(a.rmain_oil, 0) + ISNULL(a.rasist_oil, 0), 3) as sum_oil, "
                + "a.sum_distance as sum_distance, a.total_distance as total_dist "
                + " FROM ship_accumulation a WHERE a.mmsi ='" + mmsi + "' ";

            if (beginEndTime == true) // beginTime
            {
                // !!! Here we MAY NEED TO find a last record from stime, for we cannot use SUM_OIL.
                //sqlCmd += "AND a.stime <= '" + time + "' ORDER BY a.stime DESC";
                sqlCmd += "AND a.stime >= '" + time + "' ORDER BY a.stime ASC";
            }
            else
            {
                sqlCmd += "AND a.stime " + (excludeEnd == true ? "< " : "<= ") +
                    "'" + time + "' ORDER BY a.stime DESC";
            }

            dohOil.SqlCmd = sqlCmd;
            return dohOil.GetDataTable();
        }

        /// <summary>
        /// return ship accumulation total
        /// </summary>
        /// <param name="btime"></param>
        /// <param name="etime"></param>
        /// <param name="excludeEnd"></param>
        /// <param name="ignoreOilPrice"></param>
        /// <returns></returns>
        OilHis getShipAccTotal(string btime, string etime, bool excludeEnd = true, bool ignoreOilPrice = false)
        {
            string eEndTime = string.Empty;
            return getShipAccTotal(btime, etime, out eEndTime, excludeEnd, ignoreOilPrice);
        }

        /// <summary>
        /// return ship accumulation total
        /// </summary>
        /// <param name="btime"></param>
        /// <param name="etime"></param>
        /// <param name="eEndTime"></param>
        /// <param name="excludeEnd"></param>
        /// <param name="ignoreOilPrice"></param>
        /// <returns></returns>
        Entity.OilHis getShipAccTotal(string btime, string etime, out string eEndTime, bool excludeEnd = true, bool ignoreOilPrice = false)
        {
            WriteLocalLog("--> getShipAccTotal " + btime + " to " + etime);
            eEndTime = string.Empty;

            if ((mmsi == null) ||
                (mmsi != null && mmsi.Length <= 0))
            {
                return null;
            }

            OilHis si = new OilHis();
            double si_oil = 0, si_mil = 0, si_oilcost = 0;
            double si_oil_ex = 0, si_oilcost_ex = 0;

            DateTime bDateTime = DateTime.Now;
            DateTime eDateTime = DateTime.Now;

            try
            {
                bDateTime = Convert.ToDateTime(btime);
                eDateTime = Convert.ToDateTime(etime);
            }
            catch (Exception e)
            {
                return null;
            }

            List<DateTime> accDateTimes = new List<DateTime>();
            accDateTimes.Add(bDateTime);
            accDateTimes.Add(eDateTime);

            if (ignoreOilPrice == false)
            {
                List<DateTime> priceDateTimes = getPriceTime(oilType, bDateTime, eDateTime);
                if (priceDateTimes != null) accDateTimes.AddRange(priceDateTimes);
            }

            {
                List<DateTime> seasonDateTimes = getSeasonTime(bDateTime, eDateTime);
                if (seasonDateTimes != null) accDateTimes.AddRange(seasonDateTimes);
            }

            SortDateTimeList(ref accDateTimes);

            WriteLocalLog("AccDateTimes: " + accDateTimes.Count.ToString() + " : " + accDateTimes.ToString());

            for (int i = 0; i < accDateTimes.Count - 1; i++)
            {
                // Calc Extra Oil
                double running_time = getRunningTime(accDateTimes[i], accDateTimes[i + 1]);
                double oilPrice = ignoreOilPrice ? 0 : getOilPrice(oilType, accDateTimes[i]);

                si_oil_ex += running_time * oilExKgPerHour;
                WriteLocalLog("oil_ex" + si_oil_ex.ToString());

                if (ignoreOilPrice == false) si_oilcost_ex += running_time * oilExKgPerHour * oilPrice;

                // Calc Main Oil
                double total_oil = 0;
                double total_dis = 0;

                DataTable dtB = getShipAccInfo(accDateTimes[i].ToString("yyyy-MM-dd HH:mm:ss"), true);
                DataTable dtE = getShipAccInfo(accDateTimes[i + 1].ToString("yyyy-MM-dd HH:mm:ss"), false, excludeEnd);

                WriteLocalLog(accDateTimes[i].ToString("yyyy-MM-dd HH:mm:ss") + " TO " +
                    accDateTimes[i + 1].ToString("yyyy-MM-dd HH:mm:ss"));


                DateTime dtBtime;
                DateTime dtEtime;

                if (dtB.Rows.Count <= 0 || dtE.Rows.Count <= 0)
                {
                    WriteLocalLog("no records");
                    continue;
                }

                if (dtB.Rows[0]["stime"] == DBNull.Value || dtE.Rows[0]["stime"] == DBNull.Value)
                {
                    WriteLocalLog("no record");
                    continue;
                }

                try
                {
                    dtBtime = Convert.ToDateTime(dtB.Rows[0]["stime"].ToString());
                    dtEtime = Convert.ToDateTime(dtE.Rows[0]["stime"].ToString());

                    if (dtBtime <= dtEtime) eEndTime = dtE.Rows[0]["stime"].ToString();
                }
                catch (Exception e)
                {
                    WriteLocalLog(e.ToString());
                    continue;
                }

                if (dtBtime > dtEtime)
                {
                    WriteLocalLog(
                        "dtBtime" +
                        dtBtime.ToString("yyyy-MM-dd HH:mm:ss") +
                        " > dtEtime" +
                        dtEtime.ToString("yyyy-MM-dd HH:mm:ss"));
                    continue;
                }
                else if (dtBtime == dtEtime)
                {
                    WriteLocalLog("dtBtime == dtEtime" + dtBtime.ToString("yyyy-MM-dd HH:mm:ss"));

                    // For fix bug: big value may appear in lmain_oil, so cancel this line
                    //if (dtE.Rows[0]["sum_oil"] != DBNull.Value)
                    //{
                    //    total_oil = Convert.ToDouble(dtE.Rows[0]["sum_oil"]);
                    //}
                    //else
                    //{
                    total_oil = 0;
                    //}

                    WriteLocalLog("Total_Oil:" + total_oil.ToString());

                    // For fix bug: big value may appear in lmain_oil, so cancel this line
                    //if (dtE.Rows[0]["sum_distance"] != DBNull.Value)
                    //{
                    //    total_dis = Convert.ToDouble(dtE.Rows[0]["sum_distance"]);
                    //}
                    //else
                    //{
                    total_dis = 0;
                    //}

                    WriteLocalLog("Total_Dis" + total_dis.ToString());
                }
                else
                {
                    WriteLocalLog(
                        "dtBtime" +
                        dtBtime.ToString("yyyy-MM-dd HH:mm:ss") +
                        ", dtEtime" +
                        dtEtime.ToString("yyyy-MM-dd HH:mm:ss"));

                    if (dtE.Rows[0]["total_oil"] != DBNull.Value &&
                        dtB.Rows[0]["total_oil"] != DBNull.Value)
                    {
                        total_oil = Convert.ToDouble(dtE.Rows[0]["total_oil"]) - Convert.ToDouble(dtB.Rows[0]["total_oil"]);

                        // For fix bug: big value may appear in lmain_oil, so cancel this line
                        //if (dtB.Rows[0]["sum_oil"] != DBNull.Value)
                        //{
                        //    total_oil += Convert.ToDouble(dtB.Rows[0]["sum_oil"]);
                        //}
                    }
                    else
                    {
                        total_oil = 0;
                    }

                    WriteLocalLog("N Total_Oil:" + total_oil.ToString());

                    if (dtE.Rows[0]["total_dist"] != DBNull.Value &&
                        dtB.Rows[0]["total_dist"] != DBNull.Value)
                    {
                        total_dis = Convert.ToDouble(dtE.Rows[0]["total_dist"]) - Convert.ToDouble(dtB.Rows[0]["total_dist"]);

                        // For fix bug: big value may appear in lmain_oil, so cancel this line
                        //if (dtB.Rows[0]["sum_distance"] != DBNull.Value)
                        //{
                        //    total_dis += Convert.ToDouble(dtB.Rows[0]["sum_distance"]);
                        //}
                    }
                    else
                    {
                        total_dis = 0;
                    }

                    WriteLocalLog("Total_Dis" + total_dis.ToString());
                }

                si_oil += total_oil * getOilDensity(dtBtime);
                WriteLocalLog("si_oil" + si_oil.ToString());

                si_mil += total_dis;
                WriteLocalLog("si_mil" + si_mil.ToString());

                if (ignoreOilPrice == false) si_oilcost += total_oil * getOilDensity(dtBtime) * getOilPrice(oilType, dtBtime);
            }

            si.mmsi = mmsi;
            si.sail_time = getSailTime(bDateTime, eDateTime).ToString();
            si.running_time = getRunningTime(bDateTime, eDateTime).ToString();
            si.oil = si_oil.ToString();
            si.oilcost = si_oilcost.ToString();
            si.oil_ex = si_oil_ex.ToString();
            si.oilcost_ex = si_oilcost_ex.ToString();
            si.mil = si_mil.ToString();

            return si;
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

        /// <summary>
        /// return sail time
        /// </summary>
        private void getSailTime()
        {
            DateTime dtBTime = Convert.ToDateTime(QueryString("btime"));
            DateTime dtETime = Convert.ToDateTime(QueryString("etime"));
            ShipSailTime sst = null;

            double sailTime = getSailTime(dtBTime, dtETime);

            if (sailTime > 0)
            {
                sst = new ShipSailTime();

                sst.mmsi = mmsi;
                sst.begin_time = QueryString("btime");
                sst.end_time = QueryString("etime");
                sst.sail_time = sailTime.ToString();

                dtHelp.ResutJsonStr((Object)sst);
            }
            else
            {
                this._response = JsonResult(0, "未查找到数据");
            }
        }

        private double getSailTime(string btime, string etime)
        {
            if (string.IsNullOrEmpty(btime) == false &&
                string.IsNullOrEmpty(etime) == false)
            {
                DateTime dtBTime = Convert.ToDateTime(btime);
                DateTime dtETime = Convert.ToDateTime(etime);

                return getSailTime(dtBTime, dtETime);
            }

            return 0;
        }

        /// <summary>
        /// return sail time
        /// </summary>
        /// <param name="dtBTime"></param>
        /// <param name="dtETime"></param>
        /// <returns></returns>
        private double getSailTime(DateTime dtBTime, DateTime dtETime)
        {
            return getDuration(DURATION_TYPE.SAIL_TIME, dtBTime, dtETime);
        }

        /// <summary>
        /// return running time
        /// </summary>
        /// <param name="btime"></param>
        /// <param name="etime"></param>
        /// <returns></returns>
        private double getRunningTime(string btime, string etime)
        {
            if (string.IsNullOrEmpty(btime) == false &&
                string.IsNullOrEmpty(etime) == false)
            {
                DateTime dtBTime = Convert.ToDateTime(btime);
                DateTime dtETime = Convert.ToDateTime(etime);

                return getRunningTime(dtBTime, dtETime);
            }

            return 0;
        }

        /// <summary>
        /// return running time
        /// </summary>
        /// <param name="dtBTime"></param>
        /// <param name="dtETime"></param>
        /// <returns></returns>
        private double getRunningTime(DateTime dtBTime, DateTime dtETime)
        {
            return getDuration(DURATION_TYPE.RUNNING_TIME, dtBTime, dtETime);
        }

        /// <summary>
        /// helper function for get time
        /// </summary>
        /// <param name="type"></param>
        /// <param name="dtBTime"></param>
        /// <param name="dtETime"></param>
        /// <returns></returns>
        private double getDuration(DURATION_TYPE type, DateTime dtBTime, DateTime dtETime)
        {
            TimeSpan sailingTimeSpan = TimeSpan.Zero;
            dohOil.Reset();

            string durationTable = getDurationTable(type);
            string sqlCmd =

                "SELECT a.MMSI, substring(CONVERT(varchar(100), a.startTime, 25), 1, 16) AS startTime, "
                + "substring(CONVERT(varchar(100), a.endTime, 25), 1, 16) AS endTime "
                + "FROM "
                + durationTable + " "
                + "AS a "
                + "WHERE a.MMSI = '" + mmsi + "' "
                + "AND ( "
                + "( NOT ( a.startTime >= '" + dtETime.ToString("yyyy-MM-dd HH:mm:ss") + "' " + "OR a.endTime <= '" + dtBTime.ToString("yyyy-MM-dd HH:mm:ss") + "') ) OR "
                + "( "
                + "(a.endTime = NULL) AND "
                + "(a.startTime < '" + dtETime.ToString("yyyy-MM-dd HH:mm:ss") + "') "
                + ") "
                + ")";

            dohOil.SqlCmd = sqlCmd;
            DataTable dt = dohOil.GetDataTable();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (dt.Rows[i]["startTime"] == DBNull.Value)
                {
                    continue;
                }

                DateTime recordBTime = Convert.ToDateTime(dt.Rows[i]["startTime"].ToString());
                DateTime recordETime = DateTime.Now;

                if (dt.Rows[i]["endTime"] != DBNull.Value)
                {
                    recordETime = Convert.ToDateTime(dt.Rows[i]["endTime"].ToString());
                }

                if (recordBTime <= dtBTime && recordETime <= dtETime)
                {
                    sailingTimeSpan += recordETime - dtBTime;
                }
                else if (dtBTime <= recordBTime && dtETime <= recordETime)
                {
                    sailingTimeSpan += dtETime - recordBTime;
                }
                else if (dtBTime <= recordBTime && recordETime <= dtETime)
                {
                    sailingTimeSpan += recordETime - recordBTime;
                }
                else if (recordBTime <= dtBTime && dtETime <= recordETime)
                {
                    sailingTimeSpan += dtETime - dtBTime;
                }
            }

            return sailingTimeSpan.TotalHours;
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
        /// return ship config
        /// </summary>
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
                    //WriteLocalLog(dtHelp.ToJson((Object)config));
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
                    QueryString("oil_density_winter") + ")";
            }

            dohOil.SqlCmd = sqlCmd;

            //WriteLocalLog("save hsj_oil_density sqlcmd:" + sqlCmd);

            result = dohOil.ExecuteSqlNonQuery();

            //WriteLocalLog("save hsj_oil_density:" + result.ToString());

            this._response = JsonResult(1, "保存成功");
        }

        private void getOilTimingStatistics()
        {
            TimingStatistics si = new TimingStatistics();

            btime = QueryString("btime");
            etime = QueryString("etime");

            string btimeSaved = new string(btime.ToCharArray());
            string eEndTime = string.Empty;

            if (mmsi != null && mmsi.Length > 0)
            {
                OilHis his = getShipAccTotal(btime, etime, out eEndTime);
                si.oil_accu = his.oil;
                si.oilcost_accu = his.oilcost;
                si.mil_accu = his.mil;
                //si.oil_ex = his.oil_ex;
                //si.oilcost_ex = his.oilcost_ex;

                double running_time_accu = 0;
                double.TryParse(his.running_time, out running_time_accu);

                double oilcost_ex_accu = 0;
                double.TryParse(his.oilcost_ex, out oilcost_ex_accu);

                if (eEndTime != string.Empty)
                    btime = eEndTime;

                DataTable dt_dyna = getShipDynamicInfoList(btime, etime, 0, true);
                double total_oil_dyna = 0;
                double total_oil_cost_dyna = 0;
                double total_dis_dyna = 0;

                //绑定对象
                for (int i = 0; i < dt_dyna.Rows.Count; i++)
                {
                    if (dt_dyna.Rows[i]["lmain_oil_gps"] != DBNull.Value)
                        total_oil_dyna += Convert.ToDouble(dt_dyna.Rows[i]["lmain_oil_gps"]);
                    if (dt_dyna.Rows[i]["lasist_oil_gps"] != DBNull.Value)
                        total_oil_dyna += Convert.ToDouble(dt_dyna.Rows[i]["lasist_oil_gps"]);
                    if (dt_dyna.Rows[i]["rmain_oil_gps"] != DBNull.Value)
                        total_oil_dyna += Convert.ToDouble(dt_dyna.Rows[i]["rmain_oil_gps"]);
                    if (dt_dyna.Rows[i]["rasist_oil_gps"] != DBNull.Value)
                        total_oil_dyna += Convert.ToDouble(dt_dyna.Rows[i]["rasist_oil_gps"]);
                    if (dt_dyna.Rows[i]["drift_interval"] != DBNull.Value)
                        total_dis_dyna += Convert.ToDouble(dt_dyna.Rows[i]["drift_interval"]);
                }

                double oilPrice_dync = 0;
                DateTime bDateTime = DateTime.Now;
                if (DateTime.TryParse(btime, out bDateTime))
                {
                    oilPrice_dync = getOilPrice(oilType, bDateTime);
                    total_oil_cost_dyna = total_oil_dyna * getOilDensity(bDateTime) * oilPrice_dync;
                }

                si.oil_dyna = (total_oil_dyna * getOilDensity(bDateTime)).ToString();
                si.mil_dyna = total_dis_dyna.ToString();
                si.oilcost_dyna = total_oil_cost_dyna.ToString();

                //输出返回
                si.mmsi = mmsi;
                si.datetiming = btimeSaved;
                si.datetiming_dyna = btime;
                si.sail_time = getSailTime(btimeSaved, etime).ToString();
                double running_time = getRunningTime(btimeSaved, etime);
                si.running_time = running_time.ToString();
                si.oil_ex = (running_time * oilExKgPerHour).ToString();

                double running_time_dyna = running_time - running_time_accu;
                si.oilcost_ex = (oilcost_ex_accu + running_time_dyna * oilExKgPerHour * oilPrice_dync).ToString();

                dtHelp.ResutJsonStr((Object)si);
            }
            else
            {
                this._response = JsonResult(0, "未查找到数据");
            }
        }

        /// <summary>
        /// return oil history statistics
        /// </summary>
        private void getOilHistoryStatistics()
        {
            btime = QueryString("btime");
            etime = QueryString("etime");

            OilHis si = getShipAccTotal(btime, etime);

            //输出返回
            if (si != null)
            {
               dtHelp.ResutJsonStr((Object)si);
                //WriteLocalLog("getOilHistoryStatistics" + dtHelp.ToJson((Object)si));
            }
            else
            {
                this._response = JsonResult(0, "未查找到数据");
            }
        }

        /// <summary>
        /// return oil history list without paging
        /// </summary>
        private void getOilHistListNoPaging()
        {
            btime = QueryString("btime");
            etime = QueryString("etime");

            DateTime bDateTime = Convert.ToDateTime(btime);
            DateTime eDateTime = Convert.ToDateTime(etime);

            DateTime dtBtime = new DateTime(bDateTime.Year, bDateTime.Month, bDateTime.Day);
            DateTime dtEtime = new DateTime(eDateTime.Year, eDateTime.Month, eDateTime.Day);

            if (dtEtime < dtBtime.AddDays(1))
            {
                this._response = JsonResult(0, "未查找到数据");
                return;
            }

            DateTime dtTime = new DateTime(bDateTime.Year, bDateTime.Month, bDateTime.Day);

            List<OilHis> items = new List<OilHis>();
            OilHis si = null;

            while (dtTime < dtEtime)
            {
                si = getShipAccTotal(dtTime.ToString("yyyy-MM-dd"), dtTime.AddDays(1).ToString("yyyy-MM-dd"));
                if (si != null)
                {
                    si.stime = dtTime.ToString("yyyy-MM-dd");
                    items.Add(si);
                }

                dtTime = dtTime.AddDays(1);
            }

            // WriteLocalLog(dtHelp.ToJson((Object)items));

            //输出返回
            if (items.Count > 0)
            {
                items.Reverse();
                dtHelp.ResutJsonStr((Object)items);
            }
            else
            {
                this._response = JsonResult(0, "未查找到数据");
            }
        }

        public enum EXPORT_TYPE
        {
            NONE = 0,
            YEAR,
            MONTH,
            CUR,
            COUNT
        }

        public enum EXPORT_BY
        {
            NONE = 0,
            DAY,
            MONTH,
            COUNT
        }

        protected double EnsurePositive(double num)
        {
            return num < 0 ? 0 : num;
        }

        private static volatile object lockFile = new object();
        public void ExportHistoryStatusToExcel()
        {
            string exportType = QueryString("exportType");
            EXPORT_TYPE type = EXPORT_TYPE.NONE;

            if (string.Compare(exportType, "year", true) == 0)
            {
                type = EXPORT_TYPE.YEAR;
            }
            else if (string.Compare(exportType, "month", true) == 0)
            {
                type = EXPORT_TYPE.MONTH;
            }
            else if (string.Compare(exportType, "cur", true) == 0)
            {
                type = EXPORT_TYPE.CUR;
            }

            if (type == EXPORT_TYPE.NONE) return;

            DateTime bTime = DateTime.Now;
            DateTime eTime = DateTime.Now;

            try
            {
                bTime = Convert.ToDateTime(QueryString("btime"));
            }
            catch (Exception e)
            {
                Console.WriteLine("'{0}' is not in the proper format.", QueryString("btime"));
                if (type == EXPORT_TYPE.YEAR || type == EXPORT_TYPE.MONTH || type == EXPORT_TYPE.CUR)
                {
                    return;
                }
            }

            try
            {
                eTime = Convert.ToDateTime(QueryString("etime"));
            }
            catch (Exception e)
            {
                if (type == EXPORT_TYPE.CUR)
                {
                    return;
                }
                Console.WriteLine("'{0}' is not in the proper format.", QueryString("etime"));
            }

            bool bFileCache = false;

            switch (type)
            {
                case EXPORT_TYPE.YEAR:
                    {
                        bFileCache =
                            (bTime.Year < DateTime.Now.Year) ||
                            (bTime.Year == DateTime.Now.Year && bTime.Month < DateTime.Now.Month);
                        break;
                    }

                case EXPORT_TYPE.MONTH:
                    {
                        bFileCache =
                            (bTime.Year < DateTime.Now.Year) ||
                            (bTime.Year == DateTime.Now.Year && bTime.Month < DateTime.Now.Month);
                        break;
                    }

                case EXPORT_TYPE.CUR:
                    {
                        bFileCache = false;
                        break;
                    }
            }

            // Temporarily disable file cache for oil density could be modified and the excel is not valid.
            bFileCache = false;

            if (bFileCache)
            {
                string timeMark = string.Empty;
                if (type == EXPORT_TYPE.YEAR) timeMark = "year";
                else if (type == EXPORT_TYPE.MONTH) timeMark = "month";

                // excel name
                string filepath = "C:\\Temp\\";
                string filename =
                    filepath +
                    "ship_" + mmsi + "_" + timeMark + "_" +
                    bTime.ToString("yyyy-MM") + ".xls";

                if (File.Exists(filename))
                {
                    DownloadExcel(filename);
                }
                else
                {
                    lock (lockFile)
                    {
                        if (File.Exists(filename))
                        {
                            DownloadExcel(filename);
                            return;
                        }
                        else
                        {
                            string content = ExportHistoryStatusContent(type, bTime, eTime);

                            try
                            {
                                StreamWriter sw = new StreamWriter(filename, false, Encoding.UTF8);
                                if (sw != null)
                                {
                                    sw.Write(content);
                                    sw.Flush();
                                    sw.Close();
                                }

                                DownloadExcel(filename);
                            }
                            catch (Exception e)
                            {
                                //this._response = JsonResult(0, "未查找到数据");
                                return;
                            }
                        }
                    }
                }
            }
            else
            {
                string content = string.Empty;
                string excelName = string.Empty;

                switch (type)
                {
                    case EXPORT_TYPE.YEAR:
                        {
                            content = ExportHistoryStatusContent(
                                        type,
                                        bTime);

                            excelName = "ship_" + mmsi + "_year_" +
                                bTime.ToString("yyyy-MM") + ".xls";
                            break;
                        }

                    case EXPORT_TYPE.MONTH:
                        {
                            content = ExportHistoryStatusContent(
                                        type,
                                        bTime);

                            excelName = "ship_" + mmsi + "_month_" +
                                bTime.ToString("yyyy-MM") + ".xls";
                            break;
                        }

                    case EXPORT_TYPE.CUR:
                        {
                            content = ExportHistoryStatusContent(
                                type,
                                bTime,
                                eTime);

                            excelName =
                                "ship_" + mmsi +
                                "_" + bTime.ToString("MM-dd") +
                                "_" + eTime.ToString("MM-dd") + ".xls";
                            break;
                        }

                }

                if (string.IsNullOrEmpty(content) == false &&
                    string.IsNullOrEmpty(excelName) == false)
                {
                    DownloadExcelContent(content, excelName);
                }

                return;
            }
        }

        private string ExportHistoryStatusContent(EXPORT_TYPE type, DateTime btime, DateTime etime = new DateTime())
        {
            EXPORT_BY by = type == EXPORT_TYPE.YEAR ? EXPORT_BY.MONTH : EXPORT_BY.DAY;
            ShipInfo shipinfo = getShipInfoByMMSI();

            StringBuilder sb = new StringBuilder();

            sb.Append("<table style='width:649px; height:978px; text-align:center; font-size:14px; font-family:@微软雅黑; word-wrap: break-word;'> <!-- A4 size -->");

            sb.Append("<tr class='oiltrMenu'>");
            sb.Append("<td colspan='11' style='text-align:center; vertical-align:middle; font-size:18px;'>");
            sb.Append((char)10 + "武汉海事局船艇燃料消耗统计表" + (char)10);
            sb.Append("</td>");
            sb.Append("</tr>");

            sb.Append("<tr class='oiltrMenu' style='text-align:left;' >");
            sb.Append("<td colspan='3'>");
            sb.Append("编制单位：");
            sb.Append("</td>");

            sb.Append("<td colspan='2'>");
            sb.Append("船名：" + (shipinfo == null ? string.Empty : shipinfo.shipname));
            sb.Append("</td>");

            sb.Append("<td colspan='2'>");
            sb.Append("船舶类型：");
            sb.Append("</td>");

            sb.Append("<td colspan='4'>");
            sb.Append("主机功率(KW)：");
            sb.Append("</td>");
            sb.Append("</tr>");

            sb.Append("<tr class='oiltrMenu'>");
            sb.Append("<td rowspan='2' class='oiltdBorder' style='width:35px'>序号</td>");
            sb.Append("<td rowspan='2' class='oiltdBorderNoLeft' style='width:77px'>月份</td>");
            sb.Append("<td rowspan='2' class='oiltdBorderNoLeft' style='width:70px'>船舶巡航" + (char)10 + "时间(H)</td>");
            sb.Append("<td rowspan='2' class='oiltdBorderNoLeft' style='width:70px'>主机运转" + (char)10 + "时间(H)</td>");
            sb.Append("<td rowspan='2' class='oiltdBorderNoLeft' style='width:70px'>柴油消耗" + (char)10 + "(KG)</td>");
            sb.Append("<td rowspan='2' class='oiltdBorderNoLeft' style='width:81px'>柴油市场价" + (char)10 + "格(元/吨)</td>");
            sb.Append("<td colspan='4' class='oiltdBorderNoLeft' style='width:162px'>当月加油记录</td>");
            sb.Append("<td rowspan='2' class='oiltdBorderNoLeft' style='width:81px'>当月加油" + (char)10 + "时间</td>");
            sb.Append("</tr>");

            sb.Append("<tr class='oiltrMenu'>");
            sb.Append("<td class='oiltdBorderNoLeftTop' style='width:40.5px'>上月结存(KG)</td>");
            sb.Append("<td class='oiltdBorderNoLeftTop' style='width:40.5px'>本月加油(KG)</td>");
            sb.Append("<td class='oiltdBorderNoLeftTop' style='width:40.5px'>本月使用(KG)</td>");
            sb.Append("<td class='oiltdBorderNoLeftTop' style='width:40.5px'>本月结存(KG)</td>");
            sb.Append("</tr>");

            int count = 0;

            if (type == EXPORT_TYPE.YEAR) count = 12;
            if (type == EXPORT_TYPE.MONTH)
            {
                count = DateTime.DaysInMonth(btime.Year, btime.Month);
            }
            if (type == EXPORT_TYPE.CUR)
            {
                TimeSpan ts = etime - btime;
                count = ts.Days;
            }

            List<OilMonthSaveRecord> oilSaveRecords = null;
            if (type == EXPORT_TYPE.YEAR)
            {
                DateTime oilSaveSTime = btime.AddMonths(-count + 1);
                DateTime oilSaveETime = btime;
                oilSaveRecords = GetOilSaveRecordsMonthCalced(oilSaveSTime, oilSaveETime);
            }


            for (int i = 0; i <= count; i++)
            {
                bool bSum = (i == count);

                OilHis si = null;
                DateTime bDateTime = DateTime.Now;

                switch (type)
                {
                    case EXPORT_TYPE.YEAR:
                        {
                            DateTime searchMonth = new DateTime(btime.Year, btime.Month, 1);

                            if (bSum == false)
                            {
                                bDateTime = searchMonth.AddMonths(-count + i + 1);
                                string bTime = bDateTime.ToString("yyyy-MM-dd");
                                string eTime = bDateTime.AddMonths(1).ToString("yyyy-MM-dd");
                                si = getShipAccTotal(bTime, eTime);
                            }
                            else
                            {
                                bDateTime = searchMonth.AddMonths(-count + 1);
                                string bTime = bDateTime.ToString("yyyy-MM-dd");
                                string eTime = bDateTime.AddMonths(count).ToString("yyyy-MM-dd");
                                si = getShipAccTotal(bTime, eTime);
                            }

                            break;
                        }

                    case EXPORT_TYPE.MONTH:
                        {
                            DateTime searchMonth = new DateTime(btime.Year, btime.Month, 1);

                            if (bSum == false)
                            {
                                bDateTime = searchMonth.AddDays(i);
                                string bTime = bDateTime.ToString("yyyy-MM-dd");
                                string eTime = bDateTime.AddDays(1).ToString("yyyy-MM-dd");
                                si = getShipAccTotal(bTime, eTime);
                            }
                            else
                            {
                                bDateTime = searchMonth;
                                string bTime = bDateTime.ToString("yyyy-MM-dd");
                                string eTime = bDateTime.AddDays(count).ToString("yyyy-MM-dd");
                                si = getShipAccTotal(bTime, eTime);
                            }

                            break;
                        }

                    case EXPORT_TYPE.CUR:
                        {
                            if (bSum == false)
                            {
                                bDateTime = btime.AddDays(i);
                                string bTime = bDateTime.ToString("yyyy-MM-dd");
                                string eTime = bDateTime.AddDays(1).ToString("yyyy-MM-dd");
                                si = getShipAccTotal(bTime, eTime);
                            }
                            else
                            {
                                bDateTime = btime;
                                string bTime = bDateTime.ToString("yyyy-MM-dd");
                                string eTime = bDateTime.AddDays(count).ToString("yyyy-MM-dd");
                                si = getShipAccTotal(bTime, eTime);
                            }
                            break;
                        }
                }

                sb.Append("<tr class='oiltr'>");
                if (bSum == false)
                {
                    sb.Append("<td class='oiltdBorderNoTop'>" + (i + 1).ToString() + "</td>");
                    sb.Append("<td class='oiltdBorderNoLeftTop'>" +
                        (by == EXPORT_BY.MONTH ? bDateTime.Year.ToString() + "年" : string.Empty) +
                        bDateTime.Month.ToString() + "月" +
                        (by == EXPORT_BY.DAY ? bDateTime.Day.ToString() + "日" : string.Empty) +
                        "</td>");
                }
                else
                {
                    sb.Append("<td class='oiltdBorderNoTop' colspan='2'>合计</td>");
                }

                if (si == null)
                {
                    sb.Append("<td class='oiltdBorderNoLeftTop'>0</td>");
                    sb.Append("<td class='oiltdBorderNoLeftTop'>0</td>");
                    sb.Append("<td class='oiltdBorderNoLeftTop'>0</td>");
                    sb.Append("<td class='oiltdBorderNoLeftTop'>0</td>");
                }
                else
                {
                    sb.Append("<td class='oiltdBorderNoLeftTop'>" + Math.Round(EnsurePositive(Convert.ToDouble(si.sail_time)), 1).ToString() + "</td>");
                    sb.Append("<td class='oiltdBorderNoLeftTop'>" + Math.Round(EnsurePositive(Convert.ToDouble(si.running_time)), 1).ToString() + "</td>");

                    double oilTotal =
                        Convert.ToDouble(si.oil) +
                        Convert.ToDouble(si.oil_ex);

                    sb.Append("<td class='oiltdBorderNoLeftTop'>" + Math.Round(EnsurePositive(oilTotal), 3).ToString() + "</td>");

                    double oilcostTotal =
                        Convert.ToDouble(si.oilcost) +
                        Convert.ToDouble(si.oilcost_ex);

                    sb.Append("<td class='oiltdBorderNoLeftTop'>" + Math.Round(EnsurePositive(oilcostTotal), 2).ToString() + "</td>");
                }

                OilMonthSaveRecord oilSaveRecord = null;
                for (int k = 0; (oilSaveRecords != null && k < oilSaveRecords.Count); k++)
                {
                    OilMonthSaveRecord record = oilSaveRecords[k];
                    if (record != null)
                    {
                        if (record.saveDate.Year == bDateTime.Year && record.saveDate.Month == bDateTime.Month)
                        {
                            oilSaveRecord = record;
                            break;
                        }
                    }
                }

                sb.Append("<td class='oiltdBorderNoLeftTop'>" +
                    (by == EXPORT_BY.MONTH && bSum == false ?
                    (oilSaveRecord == null ? "0" : Math.Round(oilSaveRecord.lastMonthOilSave, 3).ToString())
                    : string.Empty) + "</td>");
                sb.Append("<td class='oiltdBorderNoLeftTop'>" +
                    (by == EXPORT_BY.MONTH && bSum == false ?
                    (oilSaveRecord == null ? "0" : Math.Round(oilSaveRecord.oilFillAmount, 3).ToString())
                    : string.Empty) + "</td>");
                sb.Append("<td class='oiltdBorderNoLeftTop'>" +
                    (by == EXPORT_BY.MONTH && bSum == false ?
                    (oilSaveRecord == null ? "0" : Math.Round(oilSaveRecord.oilConsumeAmount, 3).ToString())
                    : string.Empty) + "</td>");
                sb.Append("<td class='oiltdBorderNoLeftTop'>" +
                    (by == EXPORT_BY.MONTH && bSum == false ?
                    (oilSaveRecord == null ? "0" : Math.Round(oilSaveRecord.saveAmount, 3).ToString())
                    : string.Empty) + "</td>");
                sb.Append("<td class='oiltdBorderNoLeftTop'>" +
                    (by == EXPORT_BY.MONTH && bSum == false ?
                    (oilSaveRecord == null ? "0" : oilSaveRecord.getOilFillDates())
                    : string.Empty) + "</td>");

                sb.Append("</tr>");
            }

            sb.Append("<tr class='oiltr' style='text-align:left;' >");
            sb.Append("<td colspan='4'>");
            sb.Append("装备部门负责人:");
            sb.Append("</td>");

            sb.Append("<td colspan='4'>");
            sb.Append("财务负责人:");
            sb.Append("</td>");

            sb.Append("<td colspan='3'>");
            sb.Append("填报人:");
            sb.Append("</td>");
            sb.Append("</tr>");

            sb.Append("</table>");

            StringBuilder cssBuilder = new StringBuilder();

            cssBuilder.Append(".oiltr");
            cssBuilder.Append("{");
            cssBuilder.Append("text-align:right;");
            cssBuilder.Append("vertical-align:middle;");
            cssBuilder.Append("}");

            cssBuilder.Append(".oiltrMenu");
            cssBuilder.Append("{");
            cssBuilder.Append("text-align:center;");
            cssBuilder.Append("vertical-align:middle;");
            cssBuilder.Append("}");

            cssBuilder.Append(".oiltdBorderTop");
            cssBuilder.Append("{");
            cssBuilder.Append("border-top:.5pt solid black;");
            cssBuilder.Append("}");

            cssBuilder.Append(".oiltdBorderLeft");
            cssBuilder.Append("{");
            cssBuilder.Append("border-left:.5pt solid black;");
            cssBuilder.Append("}");

            cssBuilder.Append(".oiltdBorderRight");
            cssBuilder.Append("{");
            cssBuilder.Append("border-right:.5pt solid black;");
            cssBuilder.Append("}");

            cssBuilder.Append(".oiltdBorderBottom");
            cssBuilder.Append("{");
            cssBuilder.Append("border-bottom:.5pt solid black;");
            cssBuilder.Append("}");

            cssBuilder.Append(".oiltdBorder");
            cssBuilder.Append("{");
            cssBuilder.Append("border:.5pt solid black;");
            cssBuilder.Append("}");

            cssBuilder.Append(".oiltdBorderNoLeft");
            cssBuilder.Append("{");
            cssBuilder.Append("border-top:.5pt solid black;");
            cssBuilder.Append("border-right:.5pt solid black;");
            cssBuilder.Append("border-bottom:.5pt solid black;");
            cssBuilder.Append("}");

            cssBuilder.Append(".oiltdBorderNoTop");
            cssBuilder.Append("{");
            cssBuilder.Append("border-left:.5pt solid black;");
            cssBuilder.Append("border-right:.5pt solid black;");
            cssBuilder.Append("border-bottom:.5pt solid black;");
            cssBuilder.Append("}");

            cssBuilder.Append(".oiltdBorderNoLeftTop");
            cssBuilder.Append("{");
            cssBuilder.Append("border-right:.5pt solid black;");
            cssBuilder.Append("border-bottom:.5pt solid black;");
            cssBuilder.Append("}");

            string content = String.Format("<style type='text/css'>{0}</style>{1}", cssBuilder.ToString(), sb.ToString());
            return content;
        }


        /// <param name="content">Excel中内容(Table格式)</param> 
        /// 
        public void DownloadExcelContent(string content, string filename)
        {
            string fileName = filename;//客户端保存的文件名  
            Response.Clear();
            Response.ClearContent();
            Response.ClearHeaders();
            Response.Buffer = false;
            Response.AddHeader("Content-Disposition", "attachment;filename=" + fileName);
            Response.AddHeader("Content-Length",
                System.Text.Encoding.GetEncoding("UTF-8").GetByteCount(content).ToString());
            Response.AddHeader("Content-Transfer-Encoding", "binary");
            Response.ContentType = "application/octet-stream";
            Response.ContentEncoding = System.Text.Encoding.GetEncoding("UTF-8");
            Response.Write(content);
            Response.Flush();
            Response.End();
        }

        /// <summary>
        /// Download excel directly
        /// </summary>
        /// <param name="filename"></param>
        public void DownloadExcel(string filename)
        {
            //if (string.IsNullOrEmpty(filename) == true) 
            //{
            //    this._response = JsonResult(0, "未查找到数据");
            //    return;
            //}

            //res.Clear();
            //res.Buffer = true;
            //res.Charset = "UTF-8";
            //res.AddHeader("Content-Disposition", "attachment; filename=" + "Test.xls");
            //res.ContentEncoding = System.Text.Encoding.GetEncoding("UTF-8");
            //res.ContentType = "application/ms-excel";
            //res.Write(content);
            //res.WriteFile(filename);
            //res.Flush();
            //res.TransmitFile(filename);
            //res.End();

            //WriteFile实现下载  
            string fileName = filename.Substring(filename.LastIndexOf('\\') + 1);//客户端保存的文件名  
            string filePath = filename;//路径  

            FileInfo fileInfo = new FileInfo(filePath);
            Response.Clear();
            Response.ClearContent();
            Response.ClearHeaders();
            Response.Buffer = false;
            Response.AddHeader("Content-Disposition", "attachment;filename=" + fileName);
            Response.AddHeader("Content-Length", fileInfo.Length.ToString());
            Response.AddHeader("Content-Transfer-Encoding", "binary");
            Response.ContentType = "application/octet-stream";
            Response.ContentEncoding = System.Text.Encoding.GetEncoding("UTF-8");
            Response.WriteFile(fileInfo.FullName);
            Response.Flush();
            Response.End();
        }

        /// <summary>
        /// return oil save year record
        /// </summary>
        public void getOilSaveYearRecord()
        {
            DateTime fillMonth = DateTime.Now;
            if (DateTime.TryParse(QueryString("fill_month"), out fillMonth) == false)
            {
                this._response = JsonResult(0, "未查找到数据");
                return;
            }

            HSJ_OilSaveRecordYear saveRecord =
                getOilSaveYearRecord(fillMonth.AddYears(-1));
            if (saveRecord == null)
            {
                this._response = JsonResult(0, "未查找到数据");
                return;
            }
            else
            {
                dtHelp.ResutJsonStr((Object)saveRecord);
            }
        }

        /// <summary>
        /// return oil save year record by given time
        /// </summary>
        /// <param name="stime"></param>
        /// <returns></returns>
        private HSJ_OilSaveRecordYear getOilSaveYearRecord(DateTime stime)
        {
            DateTime btime = new DateTime(stime.Year, 1, 1);
            dohOil.Reset();
            dohOil.SqlCmd =
                "SELECT TOP 1 * " +
                "FROM hsj_oil_save_record_year " +
                "WHERE OilSaveYear = ' " + btime.ToString("yyyy-MM-dd") + "' " +
                "AND mmsi = '" + mmsi + "' ";
            DataTable dt = dohOil.GetDataTable();

            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                HSJ_OilSaveRecordYear saveRecord = new HSJ_OilSaveRecordYear();

                saveRecord.OilYearSaveID = dt.Rows[0]["OilSaveYearID"].ToString();
                saveRecord.OilYearSaveDate = dt.Rows[0]["OilSaveYear"].ToString();
                saveRecord.OilYearSaveMMSI = dt.Rows[0]["mmsi"].ToString();
                saveRecord.OilYearSaveAmount = dt.Rows[0]["OilSaveAmount"].ToString();

                return saveRecord;
            }

            return null;
        }

        /// <summary>
        /// return oil fill month record
        /// </summary>
        public void getOilFillMonthRecord()
        {
            DateTime fillMonth = DateTime.Now;
            if (DateTime.TryParse(QueryString("fill_month"), out fillMonth) == false)
            {
                this._response = JsonResult(0, "未查找到数据");
                return;
            }

            HSJ_OilFillRecordMonth record = getOilFillMonthRecord(fillMonth);
            if (record == null)
            {
                this._response = JsonResult(0, "未查找到数据");
            }
            else
            {
                dtHelp.ResutJsonStr((Object)record);
            }
        }

        /// <summary>
        /// return oil fill year records
        /// </summary>
        public void getOilFillYearRecords()
        {
            DateTime fillMonth = DateTime.Now;
            if (DateTime.TryParse(QueryString("fill_month"), out fillMonth) == false)
            {
                this._response = JsonResult(0, "未查找到数据");
                return;
            }

            List<HSJ_OilFillRecordMonth> records = getOilFillYearRecords(fillMonth);
            if (records == null)
            {
                this._response = JsonResult(0, "未查找到数据");
            }
            else
            {
                WriteLocalLog(dtHelp.ToJson((Object)records));
                dtHelp.ResutJsonStr((Object)records);
            }
        }

        /// <summary>
        /// return oil fill year records by given time
        /// </summary>
        /// <param name="stime"></param>
        /// <param name="includeOilConsume"></param>
        /// <returns></returns>
        public List<HSJ_OilFillRecordMonth> getOilFillYearRecords(DateTime stime, bool includeOilConsume = true)
        {
            List<HSJ_OilFillRecordMonth> records = null;
            for (int i = 1; i <= 12; i++)
            {
                DateTime btime = new DateTime(stime.Year, i, 1);
                HSJ_OilFillRecordMonth record = getOilFillMonthRecord(btime, includeOilConsume);
                if (record != null)
                {
                    if (records == null)
                    {
                        records = new List<HSJ_OilFillRecordMonth>();
                    }
                    records.Add(record);
                }
            }

            return records;
        }

        /// <summary>
        /// return oil fill records by given time
        /// </summary>
        /// <param name="stime"></param>
        /// <param name="includeOilConsume"></param>
        /// <returns></returns>
        private HSJ_OilFillRecordMonth getOilFillMonthRecord(DateTime stime, bool includeOilConsume = true)
        {
            if ((mmsi == null) ||
               (mmsi != null && mmsi.Length <= 0))
            {
                return null;
            }

            DateTime btime = new DateTime(stime.Year, stime.Month, 1);
            DateTime etime = btime.AddMonths(1);

            dohOil.Reset();
            dohOil.SqlCmd =
                "SELECT TOP 1 * " +
                "FROM hsj_oil_fill_record_month " +
                "WHERE " +
                "mmsi = '" + mmsi + "' " +
                "AND " +
                "OilFillMonth = '" + btime.ToString("yyyy-MM-dd") + "' ";

            DataTable dt = dohOil.GetDataTable();
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                HSJ_OilFillRecordMonth fillRecordMonth = new HSJ_OilFillRecordMonth();
                fillRecordMonth.OilMonthFillID = dt.Rows[0]["OilFillMonthID"].ToString();
                fillRecordMonth.OilMonthFillMMSI = mmsi;
                fillRecordMonth.OilMonthFillDate = btime.ToString("yyyy-MM-dd");
                fillRecordMonth.OilMonthFillAmount = dt.Rows[0]["OilFillAmount"].ToString();

                double oil = 0, oilex = 0;
                if (includeOilConsume)
                {
                    OilHis si = getShipAccTotal(btime.ToString("yyyy-MM-dd"), etime.ToString("yyyy-MM-dd"), true, true);
                    double.TryParse(si.oil, out oil);
                    double.TryParse(si.oil_ex, out oilex);
                }

                fillRecordMonth.OilMonthConsumeAmount = (oil + oilex).ToString();

                return fillRecordMonth;
            }

            return null;
        }

        /// <summary>
        /// return oil fill records by fill month
        /// </summary>
        public void getOilFillRecords()
        {
            DateTime fillMonth = DateTime.Now;
            //WriteLocalLog(q("fill_month"));
            if (DateTime.TryParse(QueryString("fill_month"), out fillMonth) == false)
            {
                this._response = JsonResult(0, "未查找到数据");
                return;
            }

            List<Entity.HSJ_OilFillRecord> records = getOilFillRecords(fillMonth);
            if (records == null)
            {
                this._response = JsonResult(0, "未查找到数据");
            }
            else
            {
                //WriteLocalLog("getOilFillRecords for " + q("fill_month") + " : " + "Count: " + records.Count.ToString());
                //for (int i = 0; i < records.Count; i++)
                //{
                //    WriteLocalLog(dtHelp.ToJson((Object)records[i]));
                //}
                dtHelp.ResutJsonStr((Object)records);
            }
        }

        /// <summary>
        /// return oil fill records by given time
        /// </summary>
        /// <param name="stime"></param>
        /// <returns></returns>
        private List<Entity.HSJ_OilFillRecord> getOilFillRecords(DateTime stime)
        {
            if ((mmsi == null) || (mmsi != null && mmsi.Length <= 0))
            {
                return null;
            }

            DateTime btime = new DateTime(stime.Year, stime.Month, 1);
            DateTime etime = btime.AddMonths(1);

            dohOil.Reset();
            dohOil.SqlCmd =
                "SELECT * " +
                "FROM hsj_oil_fill_record " +
                "WHERE " +
                "mmsi = '" + mmsi + "' " +
                "AND " +
                "( " +
                "OilFillDate >= '" + btime.ToString("yyyy-MM-dd") + "' " +
                "AND " +
                "OilFillDate < '" + etime.ToString("yyyy-MM-dd") + "' " +
                ") ";
            //WriteLocalLog(dohOil.SqlCmd);

            DataTable dt = dohOil.GetDataTable();

            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                List<Entity.HSJ_OilFillRecord> records = new List<HSJ_OilFillRecord>();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    Entity.HSJ_OilFillRecord record = new HSJ_OilFillRecord();

                    record.OilFillID = dt.Rows[i]["OilFillID"].ToString();
                    record.OilFillMMSI = mmsi;
                    record.OilFillDate = dt.Rows[i]["OilFillDate"].ToString();
                    record.OilFillAmount = dt.Rows[i]["OilFillAmount"].ToString();

                    records.Add(record);
                }

                return records;
            }

            return null;
        }

        [DataContract]
        class JSOilFillRecord
        {
            [DataMember]
            public string fillID { get; set; }

            [DataMember]
            public string fillDate { get; set; }

            [DataMember]
            public string fillAmount { get; set; }
        }

        /// <summary>
        /// JS oil fill record deserialize
        /// </summary>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        private List<JSOilFillRecord> JSOilFillRecordDeserialize(string jsonString)
        {
            List<JSOilFillRecord> list = null;
            var jser = new JavaScriptSerializer();

            try
            {
                list = jser.Deserialize<List<JSOilFillRecord>>(jsonString);
            }
            catch (Exception e)
            {
                //WriteLocalLog("JsonDeserialize" + e.ToString());
            }

            return list;
        }

        /// <summary>
        /// delete oil fill records list
        /// </summary>
        /// <param name="list"></param>
        private void deleteOilFillRecordsList(List<JSOilFillRecord> list)
        {
            if (list == null) return;

            for (int i = 0; i < list.Count; i++)
            {
                JSOilFillRecord record = list[i];
                if (string.IsNullOrEmpty(record.fillID))
                {
                    continue;
                }
                if (record == null) continue;

                dohOil.Reset();
                dohOil.SqlCmd =
                    "DELETE " +
                    "FROM hsj_oil_fill_record " +
                    "WHERE OilFillID=" + record.fillID;
                dohOil.ExecuteSqlNonQuery();
            }
        }

        /// <summary>
        /// add oil fill records
        /// </summary>
        /// <param name="list"></param>
        private void addOilFillRecordsList(List<JSOilFillRecord> list)
        {
            if (list == null) return;

            for (int i = 0; i < list.Count; i++)
            {
                JSOilFillRecord record = list[i];

                if (record == null) continue;
                if (string.IsNullOrEmpty(record.fillDate) || string.IsNullOrEmpty(record.fillAmount))
                {
                    continue;
                }

                dohOil.Reset();
                dohOil.SqlCmd =
                    "INSERT " +
                    "INTO hsj_oil_fill_record(OilFillDate, mmsi, OilFillAmount) " +
                    "VALUES ( " + "'" + record.fillDate + "', " +
                    "'" + mmsi + "', " +
                    record.fillAmount + ")";
                dohOil.ExecuteSqlNonQuery();
            }
        }

        /// <summary>
        /// update oil fill records
        /// </summary>
        /// <param name="list"></param>
        private void updateOilFillRecordsList(List<JSOilFillRecord> list)
        {
            if (list == null) return;

            List<JSOilFillRecord> listToAdd = null;

            for (int i = 0; i < list.Count; i++)
            {
                JSOilFillRecord record = list[i];

                if (record == null) continue;
                if (string.IsNullOrEmpty(record.fillDate) || string.IsNullOrEmpty(record.fillAmount))
                {
                    continue;
                }

                if (string.IsNullOrEmpty(record.fillID))
                {
                    if (listToAdd == null) listToAdd = new List<JSOilFillRecord>();
                    if (listToAdd != null) listToAdd.Add(record);
                }
                else
                {
                    dohOil.Reset();
                    dohOil.SqlCmd =
                        "UPDATE hsj_oil_fill_record " +
                        "SET " +
                        "OilFillDate='" + record.fillDate + "', " +
                        "OilFillAmount=" + record.fillAmount +
                        "WHERE " +
                        "OilFillID=" + record.fillID;
                    dohOil.ExecuteSqlNonQuery();
                }
            }

            if (listToAdd != null) addOilFillRecordsList(listToAdd);
        }

        /// <summary>
        /// update oil fill month record by given time
        /// </summary>
        /// <param name="stime"></param>
        private void updateOilFillMonthRecord(DateTime stime)
        {
            if ((mmsi == null) || (mmsi != null && mmsi.Length <= 0))
            {
                return;
            }

            DateTime btime = new DateTime(stime.Year, stime.Month, 1);
            DateTime etime = btime.AddMonths(1);

            dohOil.Reset();
            dohOil.SqlCmd =
                "SELECT SUM(OilFillAmount) AS oilFillAmountMonth " +
                "FROM hsj_oil_fill_record " +
                "WHERE (OilFillDate >= '" + btime.ToString("yyyy-MM-dd") +
                "' AND OilFillDate < '" + etime.ToString("yyyy-MM-dd") + "') " +
                "AND mmsi = '" + mmsi + "'";
            DataTable dt = dohOil.GetDataTable();

            if (dt == null || dt.Rows == null || dt.Rows.Count <= 0)
            {
                return;
            }

            double sumOil = 0;
            if (double.TryParse(dt.Rows[0]["oilFillAmountMonth"].ToString(), out sumOil) == false)
            {
                return;
            }

            bool bNewMonthRecord = true;
            dohOil.Reset();
            dohOil.SqlCmd =
                "SELECT TOP 1 * " +
                "FROM hsj_oil_fill_record_month " +
                "WHERE OilFillMonth = '" + btime.ToString("yyyy-MM-dd") + "' " +
                "AND mmsi = '" + mmsi + "'";
            dt = dohOil.GetDataTable();
            if (dt == null || dt.Rows == null || dt.Rows.Count <= 0) bNewMonthRecord = true;
            else bNewMonthRecord = false;

            dohOil.Reset();
            if (bNewMonthRecord)
            {
                dohOil.SqlCmd =
                    "INSERT INTO hsj_oil_fill_record_month(OilFillMonth, mmsi, OilFillAmount) " +
                    "VALUES ('" + btime.ToString("yyyy-MM-dd") + "', '" + mmsi + "', " + sumOil.ToString() + ")";
            }
            else
            {
                dohOil.SqlCmd =
                    "UPDATE hsj_oil_fill_record_month " +
                    "SET " +
                    "OilFillAmount = " + sumOil.ToString() +
                    "WHERE " +
                    "OilFillMonth = '" + btime.ToString("yyyy-MM-dd") + "' " +
                    "AND " +
                    "mmsi = '" + mmsi + "' ";
            }
            dohOil.ExecuteSqlNonQuery();
            this._response = JsonResult(1, "保存成功");
        }

        /// <summary>
        /// update oil fill records
        /// </summary>
        public void updateOilFillRecords()
        {
            string deleted = QueryString("deleted");
            string inserted = QueryString("inserted");
            string updated = QueryString("updated");

            //WriteLocalLog("updateOilFillRecords(): \n" +
            //    "deleted: " + deleted + "\n" + 
            //    "inserted: " + inserted + "\n" + 
            //    "updated: " + updated);

            List<JSOilFillRecord> listDeleted = null;
            List<JSOilFillRecord> listInserted = null;
            List<JSOilFillRecord> listUpdated = null;

            if (string.IsNullOrEmpty(deleted) == false)
            {
                listDeleted = JSOilFillRecordDeserialize(deleted);
            }

            if (string.IsNullOrEmpty(inserted) == false)
            {
                listInserted = JSOilFillRecordDeserialize(inserted);
            }

            if (string.IsNullOrEmpty(updated) == false)
            {
                listUpdated = JSOilFillRecordDeserialize(updated);
            }

            if (listDeleted != null)
            {
                deleteOilFillRecordsList(listDeleted);
            }

            if (listInserted != null)
            {
                addOilFillRecordsList(listInserted);
            }

            if (listUpdated != null)
            {
                updateOilFillRecordsList(listUpdated);
            }

            string fillMonth = QueryString("fill_month");
            DateTime fillDateTime = DateTime.Now;
            if (DateTime.TryParse(fillMonth, out fillDateTime) == true)
            {
                updateOilFillMonthRecord(fillDateTime);
            }
        }

        /// <summary>
        /// return month count from stime to etime
        /// </summary>
        /// <param name="stime"></param>
        /// <param name="etime"></param>
        /// <returns></returns>
        public int GetMonthCount(DateTime stime, DateTime etime)
        {
            return (etime.Year - stime.Year) * 12 +
                etime.Month - stime.Month + 1;
        }

        class OilMonthSaveRecord
        {
            public int saveID { get; set; }
            public DateTime saveDate { get; set; }
            public double saveAmount { get; set; }

            public double lastMonthOilSave { get; set; }
            public double oilFillAmount { get; set; }
            public double oilConsumeAmount { get; set; }
            public List<DateTime> oilFillDates { get; set; }

            public string getOilFillDates()
            {
                string oilFillDatesString = string.Empty;

                for (int i = 0; (oilFillDates != null && i < oilFillDates.Count); i++)
                {
                    oilFillDatesString += oilFillDates[i].ToString("yyyy-MM-dd; ");
                }

                if (string.IsNullOrEmpty(oilFillDatesString) == false)
                {
                    oilFillDatesString = oilFillDatesString.Substring(0, oilFillDatesString.Length - 2);
                }

                return string.IsNullOrEmpty(oilFillDatesString) ? "0" : oilFillDatesString;
            }
        }

        /// <summary>
        /// get oil fill dates
        /// </summary>
        /// <param name="stime"></param>
        /// <param name="etime"></param>
        /// <returns></returns>
        private List<DateTime> GetOilFillDates(DateTime stime, DateTime etime)
        {
            List<DateTime> oilFillDates = null;

            dohOil.Reset();
            dohOil.SqlCmd =
                "SELECT OilFillDate " +
                "FROM hsj_oil_fill_record " +
                "WHERE mmsi='" + mmsi + "' " +
                "AND (OilFillDate >= '" + stime.ToString("yyyy-MM-dd") + "' " +
                "AND OilFillDate < '" + etime.ToString("yyyy-MM-dd") + "') " +
                "ORDER BY OilFillDate ASC ";
            DataTable dt = dohOil.GetDataTable();

            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DateTime fillDate;
                    if (DateTime.TryParse(dt.Rows[i]["OilFillDate"].ToString(), out fillDate) == true)
                    {
                        if (oilFillDates == null)
                        {
                            oilFillDates = new List<DateTime>();
                        }

                        if (oilFillDates != null)
                        {
                            oilFillDates.Add(fillDate);
                        }
                    }
                }

            }

            return oilFillDates;
        }

        /// <summary>
        /// get oil save recoeds month calcuated
        /// </summary>
        /// <param name="stime"></param>
        /// <param name="etime"></param>
        /// <returns></returns>
        private List<OilMonthSaveRecord> GetOilSaveRecordsMonthCalced(DateTime stime, DateTime etime)
        {
            List<OilMonthSaveRecord> records = null;
            int count = GetMonthCount(stime, etime);

            DateTime curDate = new DateTime(stime.Year, stime.Month, 1);
            HSJ_OilSaveRecordYear saveYearRecord = null;
            double[] oilFills = new double[12];
            double[] oilConsumes = new double[12];
            double saveAmount = 0;

            //WriteLocalLog("GetOilSaveRecordsMonthCalced");
            //WriteLocalLog("Count: " + count.ToString());

            int i = 0;
            while (i < count)
            {
                DateTime nextDate = curDate.AddMonths(i);

                if (saveYearRecord == null ||
                    nextDate.Year != curDate.Year)
                {
                    saveAmount = 0;
                    saveYearRecord = getOilSaveYearRecord(nextDate.AddYears(-1));
                    if (saveYearRecord != null) double.TryParse(saveYearRecord.OilYearSaveAmount, out saveAmount);

                    //WriteLocalLog("saveAmount: " + saveAmount.ToString());

                    for (int m = 0; m < 12; m++)
                    {
                        oilFills[m] = 0;
                        oilConsumes[m] = 0;
                    }

                    List<HSJ_OilFillRecordMonth> preRecords = getOilFillYearRecords(nextDate);
                    for (int n = 0; (preRecords != null) && (n < preRecords.Count); n++)
                    {
                        DateTime preDateTime;
                        if (DateTime.TryParse(preRecords[n].OilMonthFillDate, out preDateTime) == false) continue;
                        int month = preDateTime.Month;

                        double oilFill = 0, oilConsume = 0;
                        if (double.TryParse(preRecords[n].OilMonthFillAmount, out oilFill) == true) oilFills[month - 1] = oilFill;
                        if (double.TryParse(preRecords[n].OilMonthConsumeAmount, out oilConsume) == true) oilConsumes[month - 1] = oilConsume;

                        //WriteLocalLog("Month: " + month.ToString() + "Fill: " + oilFill + "Consume: " + oilConsume);
                    }
                }

                if (records == null)
                {
                    records = new List<OilMonthSaveRecord>();
                }

                OilMonthSaveRecord saveRecord = new OilMonthSaveRecord();
                saveRecord.saveID = i;
                saveRecord.saveDate = nextDate;
                saveRecord.oilFillDates = GetOilFillDates(nextDate, nextDate.AddMonths(1));

                saveRecord.lastMonthOilSave = saveAmount;
                {
                    for (int j = 0; j < nextDate.Month - 1; j++)
                    {
                        saveRecord.lastMonthOilSave += oilFills[j];
                        saveRecord.lastMonthOilSave -= oilConsumes[j];
                    }
                }

                saveRecord.saveAmount = saveRecord.lastMonthOilSave;
                {
                    saveRecord.saveAmount += oilFills[nextDate.Month - 1];
                    saveRecord.saveAmount -= oilConsumes[nextDate.Month - 1];
                }

                saveRecord.oilFillAmount = oilFills[nextDate.Month - 1];
                saveRecord.oilConsumeAmount = oilConsumes[nextDate.Month - 1];
                records.Add(saveRecord);

                i++;
            }

            return records;
        }

    }
}