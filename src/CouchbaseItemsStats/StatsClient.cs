using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Couchbase;

namespace CouchbaseItemsStats
{
    public class StatsClient
    {
        private CouchbaseClient _client;

        public StatsClient(CouchbaseClient client)
        {
            _client = client;
        }

        public IEnumerable<StatItem> GetStats(string keyPrefixRegex = null)
        {
            var metadata = _client.GetView(ConfigSection.Default.ViewDesignName, ConfigSection.Default.ViewName);

            return metadata
                .GroupBy(x => GetKeyPrefix(x.ItemId, keyPrefixRegex),
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

        private string GetKeyPrefix(string key, string keyPrefixRegex = null)
        {
            return Regex.Match(key, keyPrefixRegex ?? ConfigSection.Default.KeyPrefixRegex).Value;
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