using System;
using System.Runtime.Serialization;

namespace CJHSJ_OilWebView.Entity
{
    [DataContract]
    public class HSJ_OilFillRecord
    {
        [DataMember]
        public string OilFillID { get; set; }

        [DataMember]
        public string OilFillDate { get; set; }

        [DataMember]
        public string OilFillAmount { get; set; }

        [DataMember]
        public string OilFillMMSI { get; set; }
    }
}