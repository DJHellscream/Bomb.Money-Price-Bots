using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BombPriceBot
{
    public class Status
    {
        [JsonProperty("timestamp")]
        public DateTime Timestamp;

        [JsonProperty("error_code")]
        public int ErrorCode;

        [JsonProperty("error_message")]
        public object ErrorMessage;

        [JsonProperty("elapsed")]
        public int Elapsed;

        [JsonProperty("credit_count")]
        public int CreditCount;

        [JsonProperty("notice")]
        public object Notice;
    }

    public class Platform
    {
        [JsonProperty("id")]
        public int Id;

        [JsonProperty("name")]
        public string Name;

        [JsonProperty("symbol")]
        public string Symbol;

        [JsonProperty("slug")]
        public string Slug;

        [JsonProperty("token_address")]
        public string TokenAddress;
    }

    public class USD
    {
        [JsonProperty("price")]
        public double Price;

        [JsonProperty("volume_24h")]
        public double Volume24h;

        [JsonProperty("volume_change_24h")]
        public double VolumeChange24h;

        [JsonProperty("percent_change_1h")]
        public double PercentChange1h;

        [JsonProperty("percent_change_24h")]
        public double PercentChange24h;

        [JsonProperty("percent_change_7d")]
        public double PercentChange7d;

        [JsonProperty("percent_change_30d")]
        public double PercentChange30d;

        [JsonProperty("percent_change_60d")]
        public double PercentChange60d;

        [JsonProperty("percent_change_90d")]
        public double PercentChange90d;

        [JsonProperty("market_cap")]
        public int MarketCap;

        [JsonProperty("market_cap_dominance")]
        public int MarketCapDominance;

        [JsonProperty("fully_diluted_market_cap")]
        public double FullyDilutedMarketCap;

        [JsonProperty("last_updated")]
        public DateTime LastUpdated;
    }

    public class Quote
    {
        [JsonProperty("USD")]
        public USD USD;
    }

    public class _15876
    {
        [JsonProperty("id")]
        public int Id;

        [JsonProperty("name")]
        public string Name;

        [JsonProperty("symbol")]
        public string Symbol;

        [JsonProperty("slug")]
        public string Slug;

        [JsonProperty("num_market_pairs")]
        public int NumMarketPairs;

        [JsonProperty("date_added")]
        public DateTime DateAdded;

        [JsonProperty("tags")]
        public List<object> Tags;

        [JsonProperty("max_supply")]
        public int MaxSupply;

        [JsonProperty("circulating_supply")]
        public int CirculatingSupply;

        [JsonProperty("total_supply")]
        public int TotalSupply;

        [JsonProperty("platform")]
        public Platform Platform;

        [JsonProperty("is_active")]
        public int IsActive;

        [JsonProperty("cmc_rank")]
        public int CmcRank;

        [JsonProperty("is_fiat")]
        public int IsFiat;

        [JsonProperty("last_updated")]
        public DateTime LastUpdated;

        [JsonProperty("quote")]
        public Quote Quote;
    }

    public class Data
    {
        [JsonProperty("15876")]
        public _15876 _15876;
    }

    public class CMCQuotesLatest
    {
        [JsonProperty("status")]
        public Status Status;

        [JsonProperty("data")]
        public Data Data;
    }

}
