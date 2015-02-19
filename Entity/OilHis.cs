using System;
using System.Runtime.Serialization;

namespace CJHSJ_OilWebView.Entity
{
    [DataContract]
    public class OilHis
    {
        [DataMember]
        public string mmsi { get; set; }

        [DataMember]
        public string ibtime { get; set; }

        [DataMember]
        public string ietime { get; set; }

        [DataMember]
        public string stime { get; set; }

        [DataMember]
        public string obtime { get; set; }

        [DataMember]
        public string oetime { get; set; }

        [DataMember]
        public string oil { get; set; }

        [DataMember]
        public string oilcost { get; set; }

        [DataMember]
        public string mil { get; set; }

        [DataMember]
        public string sail_time { get; set; }

        [DataMember]
        public string running_time { get; set; }

        [DataMember]
        public string oil_ex { get; set; }

        [DataMember]
        public string oilcost_ex { get; set; }
    }
}