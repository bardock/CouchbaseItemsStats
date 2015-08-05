using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Couchbase;
using Newtonsoft.Json;

namespace CouchbaseItemsStats
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Connecting with Couchbase...");
                var client = new CouchbaseClient();

                Console.WriteLine("Obtaining items metadata...");
                var metadata = client.GetView(ConfigurationManager.AppSettings["viewDesignName"], ConfigurationManager.AppSettings["viewName"]);

                Console.WriteLine("Analyzing data...");
                var stats = metadata
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

                Console.WriteLine("Storing...");
                var data = Serialize(stats);
                File.WriteAllText(ConfigurationManager.AppSettings["reportFilePath"], data);

                Console.WriteLine("Done!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                Console.Write("Press enter to exit...");
                Console.ReadLine();
            }
        }

        private static string GetKeyPrefix(string key)
        {
            return Regex.Match(key, ConfigurationManager.AppSettings["keyPrefixRegex"]).Value;
        }

        private static double GetSecondsLeft(IViewRow viewRow)
        {
            var expiration = (long)((IDictionary<string, object>)viewRow.Info["value"])["expiration"];
            return UnixTimestampToSecondsLeft(expiration);
        }

        private static double UnixTimestampToSecondsLeft(long unixData)
        {
            return unixData - DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }

        private static string Serialize(IEnumerable<StatItem> stats)
        {
            var format = ConfigurationManager.AppSettings["reportFormat"];
            if (format == "json")
                return JsonConvert.SerializeObject(stats, Formatting.Indented);

            throw new NotImplementedException(format + " is not an implemented report format");
        }
    }

    internal class StatItem
    {
        public string KeyPrefix { get; set; }
        public StatItemDetail Expired { get; set; }
        public StatItemDetail NotExpired { get; set; }
    }

    internal class StatItemDetail
    {
        public long Count { get; private set; }
        public double? SecondsLeftAvg { get; private set; }
        public double? SecondsLeftStdDev { get; private set; }

        public StatItemDetail(IEnumerable<double> seconds)
        {
            Count = seconds.Count();

            if (Count > 0)
            {
                SecondsLeftAvg = seconds.Average();
                SecondsLeftStdDev = GetStdDev(seconds);
            }
        }

        private double GetStdDev(IEnumerable<double> values)
        {
            double average = values.Average();
            double sumOfSquaresOfDifferences = values.Select(val => (val - average) * (val - average)).Sum();
            return Math.Sqrt(sumOfSquaresOfDifferences / values.Count());
        }
    }
}