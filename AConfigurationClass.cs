namespace BombPriceBot
{
    internal class AConfigurationClass
    {
        public string TokenContract { get; set; }
        public string TokenSymbol { get; set; }
        public string TokenImage { get; set; }
        public string BscScanRPC { get; set; }
        public string BscScanAPIKey { get; set; }
        public string TreasuryABI { get; set; }
        public string TreasuryContract { get; set; }
        public string OracleABI { get; set; }
        public string OracleContract { get; set; }
        public Provider Provider { get; set; }
        public int TimeToUpdatePrice { get; set; }
        public int TimeToUpdatePriceCMC { get; set; }
        public string CMCAPIKey { get; set; }
        public string MoralisAPIKey { get; set; }
        public string CMCTokenID { get; set; }
        public string BOMBBTCLP { get; set; }
        public string BSHAREBNBLP { get; set; }
    }

    internal enum Provider
    {
        PCS = 0,
        Moralis = 1,
        CMC = 2,
    }
}