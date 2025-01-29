using System.Collections.Generic;

namespace YMCL.Public.Classes.Netease;

public class SearchSingleSong
{
    public class ArItem
    {
        /// <summary>
        /// </summary>
        public double id { get; set; }

        /// <summary>
        ///     葛雨晴
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// </summary>
        public List<string> tns { get; set; }

        /// <summary>
        /// </summary>
        public List<string> alias { get; set; }
    }

    public class Al
    {
        /// <summary>
        /// </summary>
        public double id { get; set; }

        /// <summary>
        ///     逃向宇宙
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// </summary>
        public string picUrl { get; set; }

        /// <summary>
        /// </summary>
        public List<string> tns { get; set; }

        /// <summary>
        /// </summary>
        public string pic_str { get; set; }

        /// <summary>
        /// </summary>
        public double pic { get; set; }
    }

    public class H
    {
        /// <summary>
        /// </summary>
        public double br { get; set; }

        /// <summary>
        /// </summary>
        public double fid { get; set; }

        /// <summary>
        /// </summary>
        public double size { get; set; }

        /// <summary>
        /// </summary>
        public double vd { get; set; }

        /// <summary>
        /// </summary>
        public double sr { get; set; }
    }

    public class M
    {
        /// <summary>
        /// </summary>
        public double br { get; set; }

        /// <summary>
        /// </summary>
        public double fid { get; set; }

        /// <summary>
        /// </summary>
        public double size { get; set; }

        /// <summary>
        /// </summary>
        public double vd { get; set; }

        /// <summary>
        /// </summary>
        public double sr { get; set; }
    }

    public class L
    {
        /// <summary>
        /// </summary>
        public double br { get; set; }

        /// <summary>
        /// </summary>
        public double fid { get; set; }

        /// <summary>
        /// </summary>
        public double size { get; set; }

        /// <summary>
        /// </summary>
        public double vd { get; set; }

        /// <summary>
        /// </summary>
        public double sr { get; set; }
    }

    public class Sq
    {
        /// <summary>
        /// </summary>
        public double br { get; set; }

        /// <summary>
        /// </summary>
        public double fid { get; set; }

        /// <summary>
        /// </summary>
        public double size { get; set; }

        /// <summary>
        /// </summary>
        public double vd { get; set; }

        /// <summary>
        /// </summary>
        public double sr { get; set; }
    }

    public class FreeTrialPrivilege
    {
        /// <summary>
        /// </summary>
        public string resConsumable { get; set; }

        /// <summary>
        /// </summary>
        public string userConsumable { get; set; }

        /// <summary>
        /// </summary>
        public string listenType { get; set; }

        /// <summary>
        /// </summary>
        public string cannotListenReason { get; set; }
    }

    public class ChargeInfoListItem
    {
        /// <summary>
        /// </summary>
        public double rate { get; set; }

        /// <summary>
        /// </summary>
        public string chargeUrl { get; set; }

        /// <summary>
        /// </summary>
        public string chargeMessage { get; set; }

        /// <summary>
        /// </summary>
        public double chargeType { get; set; }
    }

    public class Privilege
    {
        /// <summary>
        /// </summary>
        public double id { get; set; }

        /// <summary>
        /// </summary>
        public double fee { get; set; }

        /// <summary>
        /// </summary>
        public double payed { get; set; }

        /// <summary>
        /// </summary>
        public double st { get; set; }

        /// <summary>
        /// </summary>
        public double pl { get; set; }

        /// <summary>
        /// </summary>
        public double dl { get; set; }

        /// <summary>
        /// </summary>
        public double sp { get; set; }

        /// <summary>
        /// </summary>
        public double cp { get; set; }

        /// <summary>
        /// </summary>
        public double subp { get; set; }

        /// <summary>
        /// </summary>
        public string cs { get; set; }

        /// <summary>
        /// </summary>
        public double maxbr { get; set; }

        /// <summary>
        /// </summary>
        public double fl { get; set; }

        /// <summary>
        /// </summary>
        public string toast { get; set; }

        /// <summary>
        /// </summary>
        public double flag { get; set; }

        /// <summary>
        /// </summary>
        public string preSell { get; set; }

        /// <summary>
        /// </summary>
        public double playMaxbr { get; set; }

        /// <summary>
        /// </summary>
        public double downloadMaxbr { get; set; }

        /// <summary>
        /// </summary>
        public string maxBrLevel { get; set; }

        /// <summary>
        /// </summary>
        public string playMaxBrLevel { get; set; }

        /// <summary>
        /// </summary>
        public string downloadMaxBrLevel { get; set; }

        /// <summary>
        /// </summary>
        public string plLevel { get; set; }

        /// <summary>
        /// </summary>
        public string dlLevel { get; set; }

        /// <summary>
        /// </summary>
        public string flLevel { get; set; }

        /// <summary>
        /// </summary>
        public string rscl { get; set; }

        /// <summary>
        /// </summary>
        public FreeTrialPrivilege freeTrialPrivilege { get; set; }

        /// <summary>
        /// </summary>
        public double rightSource { get; set; }

        /// <summary>
        /// </summary>
        public List<ChargeInfoListItem> chargeInfoList { get; set; }
    }

    public class SongsItem
    {
        public string name { get; set; }
        public double id { get; set; }
        public List<ArItem> ar { get; set; }
        public Al al { get; set; }
        public double dt { get; set; }
    }

    public class Result
    {
        /// <summary>
        /// 
        /// </summary>
        //public string searchQcReminder { get; set; }
        /// <summary>
        /// </summary>
        public List<SongsItem> songs { get; set; }

        /// <summary>
        /// </summary>
        public double songCount { get; set; }
    }

    public class Root
    {
        /// <summary>
        /// </summary>
        public Result result { get; set; }

        /// <summary>
        /// </summary>
        public double code { get; set; }
    }
}