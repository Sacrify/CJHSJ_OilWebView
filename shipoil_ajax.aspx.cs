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

            //using (System.IO.StreamWriter sw = System.IO.File.AppendText("C:\\Temp\\log.txt"))
            //{
            //    sw.WriteLine(sqlCmd);
            //    sw.Flush();
            //}

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
            WriteLocalLog("getShipAccTotal " + btime + " to " + etime);
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
                //WriteLocalLog("getOilHistoryStatistics" + Cxw.Utils.dtHelp.ToJson((Object)si));
            }
            else
            {
                this._response = JsonResult(0, "未查找到数据");
            }
        }
    }
}