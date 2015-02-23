using System;
using System.Runtime.Serialization;

namespace CJHSJ_OilWebView.Entity
{
    [DataContract]
    public class HSJ_OilSaveRecordYear
    {
        [DataMember]
        public string OilYearSaveID { get; set; }

        [DataMember]
        public string OilYearSaveDate { get; set; }

        [DataMember]
        public string OilYearSaveAmount { get; set; }

        [DataMember]
        public string OilYearSaveMMSI { get; set; }
    }
}