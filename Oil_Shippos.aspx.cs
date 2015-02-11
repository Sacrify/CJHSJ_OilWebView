using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CJHSJ_OilWebView
{
    public partial class Oil_Shippos : System.Web.UI.Page
    {
        public string remoteUrl = "http://219.140.192.242:810/AisServ/AisMonitor.aspx";
        public string remoteUrlNoMMSI = "http://219.140.192.242:800/WebEnc/WebYimaEnc.html";

        protected void Page_Load(object sender, EventArgs e)
        {

        }
    }
}