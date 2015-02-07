using System;
using System.Runtime.Serialization;

namespace CJHSJ_OilWebView.Entity
{
    [DataContract]
    public class RealtimeInfo
    {
        public RealtimeInfo()
        { }
        private string _mmsi; //唯一标识
        private string _name; // 
        private string _stime;	//
        private string _shorttime;
        private string _pos_x;	//
        private string _pos_y;		//
        private string _speed; //
        private string _llun_rps;		//
        private string _rlun_rps;	//
        private string _lmain_oil_gps;	//
        private string _lmain_oil_accgps;	//
        private string _rmain_oil_gps;	//
        private string _rmain_oil_accgps;	//
        private string _lasist_oil_gps;	//
        private string _lasist_oil_accgps;	//
        private string _rasist_oil_gps;	//
        private string _rasist_oil_accgps;	//
        private string _interval_minute; // Offline check
        private string _drift_interval;	//
        private string _drift_accinterval;	//
        private string _lmpg_status;
        private string _rmpg_status;
        private string _lrun_time;
        private string _rrun_time;
        private string _sail_time;

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string mmsi
        {
            set { _mmsi = value; }
            get { return _mmsi; }
        }
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string name
        {
            set { _name = value; }
            get { return _name; }
        }
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string stime
        {
            set { _stime = value; }
            get { return _stime; }
        }
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string shorttime
        {
            set { _shorttime = value; }
            get { return _shorttime; }
        }
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string pos_x
        {
            set { _pos_x = value; }
            get { return _pos_x; }
        }
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string pos_y
        {
            set { _pos_y = value; }
            get { return _pos_y; }
        }
        /// <summary>
        /// 个性签名
        /// </summary>
        [DataMember]
        public string speed
        {
            set { _speed = value; }
            get { return _speed; }
        }
        /// <summary>
        /// 真实姓名
        /// </summary>
        [DataMember]
        public string llunrps
        {
            set { _llun_rps = value; }
            get { return _llun_rps; }
        }
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string rlunrps
        {
            set { _rlun_rps = value; }
            get { return _rlun_rps; }
        }
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string lmain_gps
        {
            set { _lmain_oil_gps = value; }
            get { return _lmain_oil_gps; }
        }
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string lmain_accgps
        {
            set { _lmain_oil_accgps = value; }
            get { return _lmain_oil_accgps; }
        }
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string rmain_gps
        {
            set { _rmain_oil_gps = value; }
            get { return _rmain_oil_gps; }
        }
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string rmain_accgps
        {
            set { _rmain_oil_accgps = value; }
            get { return _rmain_oil_accgps; }
        }
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string lasist_gps
        {
            set { _lasist_oil_gps = value; }
            get { return _lasist_oil_gps; }
        }
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string lasist_accgps
        {
            set { _lasist_oil_accgps = value; }
            get { return _lasist_oil_accgps; }
        }
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string rasist_gps
        {
            set { _rasist_oil_gps = value; }
            get { return _rasist_oil_gps; }
        }
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string rasist_accgps
        {
            set { _rasist_oil_accgps = value; }
            get { return _rasist_oil_accgps; }
        }
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string interval_minute
        {
            get { return _interval_minute; }
            set { _interval_minute = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string drift_interval
        {
            set { _drift_interval = value; }
            get { return _drift_interval; }
        }
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string drift_accval
        {
            set { _drift_accinterval = value; }
            get { return _drift_accinterval; }
        }

        [DataMember]
        public string lmpg_status
        {
            set { _lmpg_status = value; }
            get { return _lmpg_status; }
        }

        [DataMember]
        public string rmpg_status
        {
            set { _rmpg_status = value; }
            get { return _rmpg_status; }
        }

        [DataMember]
        public string lrun_time
        {
            set { _lrun_time = value; }
            get { return _lrun_time; }
        }

        [DataMember]
        public string rrun_time
        {
            set { _rrun_time = value; }
            get { return _rrun_time; }
        }

        [DataMember]
        public string sail_time
        {
            set { _sail_time = value; }
            get { return _sail_time; }
        }
    }
}