using System;
using System.Collections.Generic;
using System.Linq;

namespace CouchbaseItemsStats
{
    public class StatItem
    {
        public string KeyPrefix { get; set; }
        public StatItemDetail Expired { get; set; }
        public StatItemDetail NotExpired { get; set; }
    }

    public class StatItemDetail
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