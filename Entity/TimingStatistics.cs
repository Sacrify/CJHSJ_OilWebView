using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CJHSJ_OilWebView.Entity
{
    /// <summary>
    /// 计时统计值
    /// </summary>
    [DataContract]
    public class TimingStatistics
    {
        public TimingStatistics() { }

        [DataMember]
        public string mmsi { get; set; }

        [DataMember]
        public string datetiming { get; set; }
        [DataMember]
        public string datetiming_dyna { get; set; }

        [DataMember]
        public string oil_accu { get; set; }
        [DataMember]
        public string oilcost_accu { get; set; }
        [DataMember]
        public string mil_accu { get; set; }


        [DataMember]
        public string oil_dyna { get; set; }
        [DataMember]
        public string oilcost_dyna { get; set; }
        [DataMember]
        public string mil_dyna { get; set; }


        [DataMember]
        public string oil_ex { get; set; }
        [DataMember]
        public string oilcost_ex { get; set; }

        [DataMember]
        public string sail_time { get; set; }
        [DataMember]
        public string running_time { get; set; }
    }
}