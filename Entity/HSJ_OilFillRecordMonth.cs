using System;
using System.Runtime.Serialization;

namespace CJHSJ_OilWebView.Entity
{
    [DataContract]
    public class HSJ_OilFillRecordMonth
    {
        [DataMember]
        public string OilMonthFillID { get; set; }

        [DataMember]
        public string OilMonthFillDate { get; set; }

        [DataMember]
        public string OilMonthFillAmount { get; set; }

        [DataMember]
        public string OilMonthConsumeAmount { get; set; }

        [DataMember]
        public string OilMonthFillMMSI { get; set; }
    }
}