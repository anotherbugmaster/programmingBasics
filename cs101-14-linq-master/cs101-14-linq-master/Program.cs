using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms.DataVisualization.Charting;
using System.Threading;
using System.Diagnostics;

namespace LinqTasks
{
    public enum csvType { calendar, load };
    public enum daysOfWeek { workingDay, weekend, unknown};
	public static class DataAnalysis
	{
		public static IEnumerable<DataPoint> ToHistogramData(this IList<int> data)
        {
            var columnAmount = Math.Round(1 + Math.Log(data.Count, 2));
            var interval = data.Count / columnAmount;
            Console.WriteLine("Column amount: {0}", columnAmount);
            Console.WriteLine("Interval: {0}", interval);
            return data.Select((value, index) => new { groupNum = (int)(index / interval), value })
                              .GroupBy(pair => pair.groupNum)
                              .Select(group => new DataPoint
                              {
                                  XValue = group.First().groupNum,
                                  YValues = new double[] { group.Average(e => e.value) }
                              });
        }
        public static IEnumerable<double> ExponentialSmooth(this IEnumerable<double> x, double a)
        {
            var expSmoothElement = x.First();
            foreach(var e in x)
            {
                yield return expSmoothElement;
                expSmoothElement = a * e + (1 - a) * expSmoothElement;
            }
        }
        public static List<Tuple<DateTime, int>> ReadCSV(string csvPath, char splitter)
        {
            return File.ReadAllLines(csvPath).Where(line => line[0] != '#')
                                             .Select(line => line.Split(splitter))
                                             .Select(array => new Tuple<DateTime, int>(
                                                                                            DateTime.Parse(array[0]),
                                                                                            int.Parse(array[1])
                                                                                        ))
                                             .ToList();
        }
        public static List<Tuple<DateTime, int, int>> Merge(this List<Tuple<DateTime, int>> dataSheet1, List<Tuple<DateTime, int>> dataSheet2)
        {
            return dataSheet1.Select(tuple => new Tuple<DateTime, int, int>(
                                                                                tuple.Item1, 
                                                                                tuple.Item2, 
                                                                                dataSheet2.Find(e => e.Item1 == tuple.Item1).Item2))
                             .ToList();
        }
	}

	static class Program
	{
		private const string calendarPath = "calendar.csv";
		private const string loadPath = "load.csv";

		static void Main(string[] args)
        {
            #region Reading data

            var loadDataSheet = DataAnalysis.ReadCSV(loadPath, ';');
            var calendarDataSheet = DataAnalysis.ReadCSV(calendarPath, '	');

            var dataSheet = loadDataSheet.Merge(calendarDataSheet);
            #endregion

            #region Average values
            var workingDaysLoad = dataSheet.Where(e => e.Item3 == 0 && e.Item2 != -1);
            var weekendsLoad = dataSheet.Where(e => e.Item3 == 1 && e.Item2 != -1);

            Console.WriteLine("loadDataSheet.Count = {0}", loadDataSheet.Count);
            Console.WriteLine("calendarDataSheet.Count = {0}", calendarDataSheet.Count);
            Console.WriteLine("dataSheet.Count = {0}", dataSheet.Count);
            Console.WriteLine("Average load for working days: {0:0.000}", workingDaysLoad.Average(e => e.Item2));
            Console.WriteLine("Average load for weekends: {0:0.000}", weekendsLoad.Average(e => e.Item2));
            #endregion

            #region Exponential Smooth
            var loads = loadDataSheet.Select(e => (double)e.Item2);
            Chart.ShowLines("Exponential smooth", loads,
                                                  loads.ExponentialSmooth(0.5),
                                                  loads.ExponentialSmooth(0.9),
                                                  loads.ExponentialSmooth(0.95));

            //Я не был уверен, что оно работает, поэтому сделал массив с небольшим количеством значений
            var testArray = new double[] { 1, 4, -1, 8, -4, 10, 100, 0, -1 };
            Chart.ShowLines("Testing smoothing", testArray,
                                                 testArray.ExponentialSmooth(0.5),
                                                 testArray.ExponentialSmooth(0.9),
                                                 testArray.ExponentialSmooth(0.95));
            #endregion

            #region Histogram
            Chart.ShowHistogram("Working days load", workingDaysLoad.Select(e => e.Item2).ToList<int>().ToHistogramData());
            Chart.ShowHistogram("Weekends load", weekendsLoad.Select(e => e.Item2).ToList<int>().ToHistogramData());
            #endregion
        }
	}
}
