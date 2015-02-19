using System;
using System.Runtime.Serialization;

namespace CJHSJ_OilWebView.Entity
{
    /// <summary>
    /// Ship's sailing time during begin time and end time
    /// </summary>
    [DataContract]
    public class ShipSailTime
    {
        [DataMember]
        public string mmsi { get; set; }

        [DataMember]
        public string begin_time { get; set; }

        [DataMember]
        public string end_time { get; set; }

        [DataMember]
        public string sail_time { get; set; }
    }
}