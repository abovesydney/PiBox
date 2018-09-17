using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using PiBox;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Collections.ObjectModel;

namespace PiBox
{

    public class Feed
    {
        public int id { get; set; }
        public string name { get; set; }
        public bool polarPlot { get; set; }
    }

    public class AcList
    {
        public int Id { get; set; }
        public int Rcvr { get; set; }
        public bool HasSig { get; set; }
        public string Icao { get; set; }
        public bool Bad { get; set; }
        public string Reg { get; set; }
        public DateTime FSeen { get; set; }
        public int TSecs { get; set; }
        public int CMsgs { get; set; }
        public int Alt { get; set; }
        public int GAlt { get; set; }
        public int AltT { get; set; }
        public bool Tisb { get; set; }
        public float Spd { get; set; }
        public double Trak { get; set; }
        public bool TrkH { get; set; }
        public string Type { get; set; }
        public string Mdl { get; set; }
        public string Man { get; set; }
        public string CNum { get; set; }
        public bool IsFerryFlight { get; set; }
        public bool IsCharterFlight { get; set; }
        public string Op { get; set; }
        public string OpIcao { get; set; }
        public string Sqk { get; set; }
        public bool Ident { get; set; }
        public bool Help { get; set; }
        public int VsiT { get; set; }
        public int WTC { get; set; }
        public int Species { get; set; }
        public string Engines { get; set; }
        public int EngType { get; set; }
        public int EngMount { get; set; }
        public bool Mil { get; set; }
        public string Cou { get; set; }
        public bool HasPic { get; set; }
        public bool Interested { get; set; }
        public int FlightsCount { get; set; }
        public bool Gnd { get; set; }
        public int SpdTyp { get; set; }
        public bool CallSus { get; set; }
        public int Trt { get; set; }
        public string Year { get; set; }
        public int? Vsi { get; set; }
        public double? InHg { get; set; }
        public string Call { get; set; }
        public double Lat { get; set; }
        public double Long { get; set; }
        public double PosTime { get; set; }
        public bool? Mlat { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public List<string> Stops { get; set; }
        public bool? PosStale { get; set; }
        public string FullUri
        {
            get
            {
                return "/Assets/images/logos/smalltails/" + OpIcao + ".png";
            }
        }
        public List<double> Cos { get; set; }
        public string FromTo
        {
            get
            {
                string _from = "";
                string _to = "";

                if (From == null)
                {
                    _from = "   ";
                }
                else
                {
                    _from = From;
                }

                if (To == null)
                {
                    _to = "   ";
                }
                else
                {
                    _to = To;
                }

                string _fromShort = _from.Substring(0, 3);
                string _toShort = _to.Substring(0, 3);
                return _fromShort + "-" + _toShort;
            }
        }
        public string Bearing
        {
            get
            {
                //GET BRNG
                return "";
            }
        }

        public DateTime LastPosUpdate
        {
            get
            {
                DateTime dtDateTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                dtDateTime = dtDateTime.AddMilliseconds(PosTime);
                return dtDateTime;
            }

        }
    }

    public class RootObject
    {
        public int src { get; set; }
        public List<Feed> feeds { get; set; }
        public int srcFeed { get; set; }
        public bool showSil { get; set; }
        public bool showFlg { get; set; }
        public bool showPic { get; set; }
        public int flgH { get; set; }
        public int flgW { get; set; }
        public List<AcList> acList { get; set; }
        public int totalAc { get; set; }
        public string lastDv { get; set; }
        public int shtTrlSec { get; set; }
        public long stm { get; set; }
        public bool configChanged { get; set; }
        public int status { get; set; }
        public int? count { get; set; }
        public List<Datum> data { get; set; }
        public string error { get; set; }

        public class Datum
        {
            public string image { get; set; }
            public string link { get; set; }
            public string photographer { get; set; }
        }
    }
}
