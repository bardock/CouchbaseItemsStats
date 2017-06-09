using Couchbase;
using Couchbase.Configuration.Client;
using Couchbase.Configuration.Client.Providers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

namespace CouchbaseItemsStats
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Connecting with Couchbase...");

                var clientConfig = new ClientConfiguration((CouchbaseClientSection)ConfigurationManager.GetSection("couchbaseClients/couchbase"));
                ClusterHelper.Initialize(clientConfig);

                var bucketConfig = clientConfig.BucketConfigs.Values.FirstOrDefault();

                var client = new StatsClient(bucketConfig);

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