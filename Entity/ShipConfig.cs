using System;
using System.Runtime.Serialization;

namespace CJHSJ_OilWebView.Entity
{
    /// <summary>
    /// 船的配置信息，比如说油耗报警值等
    /// </summary>
    [DataContract]
    public class ShipConfig
    {
        [DataMember]
        public string mmsi { get; set; }

        [DataMember]
        public string warning_llun { get; set; }

        [DataMember]
        public string warning_loil { get; set; }

        [DataMember]
        public string warning_speed { get; set; }

        [DataMember]
        public string warning_moil { get; set; } // month oil

        [DataMember]
        public string warning_rlun { get; set; }

        [DataMember]
        public string warning_roil { get; set; }

        [DataMember]
        public string oil_type { get; set; } // Based on one assumption that one ship use one type of oil

        [DataMember]
        public string oil_density_summer { get; set; }

        [DataMember]
        public string oil_density_winter { get; set; }
    }
}