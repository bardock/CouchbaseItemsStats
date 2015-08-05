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
                        elementSelector: x => (long)((IDictionary<string, object>)x.Info["value"])["expiration"])
                    .Select(x => new StatItem()
                    {
                        keyPrefix = x.Key,
                        count = x.Count(),
                        expirationAvg = x.Average(),
                        expirationStdDev = GetStdDev(x)
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

        private static double GetStdDev(IEnumerable<long> values)
        {
            double average = values.Average();
            double sumOfSquaresOfDifferences = values.Select(val => (val - average) * (val - average)).Sum();
            return Math.Sqrt(sumOfSquaresOfDifferences / values.Count());
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
        public string keyPrefix { get; set; }
        public long count { get; set; }
        public double expirationAvg { get; set; }
        public double expirationStdDev { get; set; }
    }
}