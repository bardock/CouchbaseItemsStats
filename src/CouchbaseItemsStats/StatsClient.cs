using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using Couchbase;

namespace CouchbaseItemsStats
{
    public class StatsClient
    {
        private CouchbaseClient _client;

        public StatsClient()
            : this(new CouchbaseClient())
        {
        }

        public StatsClient(CouchbaseClient client)
        {
            _client = client;
        }

        public IEnumerable<StatItem> GetStats()
        {
            Console.WriteLine("Connecting with Couchbase...");
            var client = new CouchbaseClient();

            Console.WriteLine("Obtaining items metadata...");
            var metadata = client.GetView(ConfigurationManager.AppSettings["viewDesignName"], ConfigurationManager.AppSettings["viewName"]);

            Console.WriteLine("Analyzing data...");
            return metadata
                .GroupBy(x => GetKeyPrefix(x.ItemId),
                    elementSelector: x => GetSecondsLeft(x))
                .Select(x => new
                {
                    KeyPrefix = x.Key,
                    Expired = x.Where(s => s <= 0),
                    NotExpired = x.Where(s => s > 0)
                })
                .Select(x => new StatItem()
                {
                    KeyPrefix = x.KeyPrefix,
                    Expired = new StatItemDetail(x.Expired),
                    NotExpired = new StatItemDetail(x.NotExpired)
                });
        }

        private string GetKeyPrefix(string key)
        {
            return Regex.Match(key, ConfigurationManager.AppSettings["keyPrefixRegex"]).Value;
        }

        private double GetSecondsLeft(IViewRow viewRow)
        {
            var expiration = (long)((IDictionary<string, object>)viewRow.Info["value"])["expiration"];
            return UnixTimestampToSecondsLeft(expiration);
        }

        private double UnixTimestampToSecondsLeft(long unixData)
        {
            return unixData - DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }
    }
}