using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
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
                var couchbaseClient = new CouchbaseClient();

                var client = new StatsClient(couchbaseClient);

                Console.WriteLine("Obtaining statistics...");
                var stats = client.GetStats();

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

        private static string Serialize(IEnumerable<StatItem> stats)
        {
            var format = ConfigurationManager.AppSettings["reportFormat"];
            if (format == "json")
                return JsonConvert.SerializeObject(stats, Formatting.Indented);

            throw new NotImplementedException(format + " is not an implemented report format");
        }
    }
}