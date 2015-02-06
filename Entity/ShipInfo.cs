using System;
using System.Runtime.Serialization;

namespace CJHSJ_OilWebView.Entity
{
    /// <summary>
    /// 会员-------表映射实体
    /// </summary>
    [DataContract]
    public class ShipInfo
    {
        public ShipInfo()
        { }

        private string _mmsi;		//唯一标识
        private string _shipname;	//船舶名称
        private string _engname;	//船舶名称
        private string _speed;		//额定航速
        private string _lweight; //额定载重
        private string _draft;		//吃水
        private string _llun_sp;	//左轮机转速
        private string _rlun_sp;	//右轮机转速
        private string _length;	//长
        private string _width;	//宽
        private string _remark;		//备注
        private string _comid;		//备注

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
        public string shipname
        {
            set { _shipname = value; }
            get { return _shipname; }
        }
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string engname
        {
            set { _engname = value; }
            get { return _engname; }
        }
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string speed
        {
            set { _speed = value; }
            get { return _speed; }
        }
        /// <summary>
        /// 昵称
        /// </summary>
        [DataMember]
        public string load_weight
        {
            set { _lweight = value; }
            get { return _lweight; }
        }
        /// <summary>
        /// 个性签名
        /// </summary>
        [DataMember]
        public string draft
        {
            set { _draft = value; }
            get { return _draft; }
        }
        /// <summary>
        /// 真实姓名
        /// </summary>
        [DataMember]
        public string llunsp
        {
            set { _llun_sp = value; }
            get { return _llun_sp; }
        }
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string rlunsp
        {
            set { _rlun_sp = value; }
            get { return _rlun_sp; }
        }
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string length
        {
            set { _length = value; }
            get { return _length; }
        }
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string width
        {
            set { _width = value; }
            get { return _width; }
        }
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string remark
        {
            set { _remark = value; }
            get { return _remark; }
        }
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string comid
        {
            set { _comid = value; }
            get { return _comid; }
        }
    }
}