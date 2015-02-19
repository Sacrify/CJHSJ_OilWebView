using System;
using System.Runtime.Serialization;

namespace CJHSJ_OilWebView.Entity
{
    [DataContract]
    public class HSJ_OilPrice
    {
        [DataMember]
        public string PriceID { get; set; }

        [DataMember]
        public string PriceBTime { get; set; }

        [DataMember]
        public string OilType { get; set; }

        [DataMember]
        public string OilPrice { get; set; }
    }
}