using Couchbase;
using Couchbase.Configuration.Client;
using Couchbase.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CouchbaseItemsStats
{
    public class StatsClient
    {
        private BucketConfiguration _bucketConfig;

        public StatsClient(BucketConfiguration bucketConfig)
        {
            _bucketConfig = bucketConfig;
        }

        public IEnumerable<StatItem> GetStats(string keyPrefixRegex = null)
        {
            var bucket = ClusterHelper.GetBucket(_bucketConfig.BucketName, _bucketConfig.Password);
            var query = bucket.CreateQuery(ConfigSection.Default.ViewDesignName, ConfigSection.Default.ViewName, ConfigSection.Default.Dev);

            var result = bucket.Query<Dictionary<string, object>>(query);

            var metadata = result.Rows;

            return metadata
                .GroupBy(x => GetKeyPrefix(x.Id, keyPrefixRegex),
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

        private double GetSecondsLeft(ViewRow<Dictionary<string, object>> viewRow)
        {
            var expiration = (long)viewRow.Value["expiration"];
            return UnixTimestampToSecondsLeft(expiration);
        }

        private double UnixTimestampToSecondsLeft(long unixData)
        {
            return unixData - DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }
    }
}