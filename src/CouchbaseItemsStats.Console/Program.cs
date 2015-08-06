using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using Newtonsoft.Json;

namespace CouchbaseItemsStats
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                var client = new StatsClient();

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