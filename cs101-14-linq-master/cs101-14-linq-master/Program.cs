using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms.DataVisualization.Charting;

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
            return x.Select((value, index) => a * x.ElementAt(index) + (1 - a) * ((index > 0)?x.ElementAt(index - 1):x.First()));
        }
        public static List<Tuple<DateTime, int>> ReadCSV(string csvPath, char splitter)
        {
            var dataSheet = new List<Tuple<DateTime, int>>();
            var lines = File.ReadAllLines(csvPath);
            foreach (var line in lines)
            {
                if (line[0] == '#')
                    continue;

                var data = line.Split(splitter);
                var dataDate = DateTime.Parse(data[0]);
                var dataValue = int.Parse(data[1]);

                dataSheet.Add(new Tuple<DateTime, int>(dataDate, dataValue));
            }
            return dataSheet;
        }
        public static void Fill(this List<Tuple<DateTime, int, int>> dataSheet, List<Tuple<DateTime, int>> csvData, csvType type)
        {
            foreach (var csvTuple in csvData)
            {
                var csvTupleDate = csvTuple.Item1;

                var eIndex = dataSheet.FindIndex(x => x.Item1 == csvTupleDate);

                switch(type)
                {
                    case csvType.calendar:
                        var dataDayOfWeek = csvTuple.Item2;
                        if (eIndex != -1)
                        {
                            var matchingTuple = dataSheet[eIndex];
                            var tupleDate = matchingTuple.Item1;
                            var tupleLoad = matchingTuple.Item2;
                            dataSheet[eIndex] = new Tuple<DateTime, int, int>(tupleDate, tupleLoad, dataDayOfWeek);
                        }
                        else
                            dataSheet.Add(new Tuple<DateTime, int, int>(csvTupleDate, -1, dataDayOfWeek));
                        break;
                    case csvType.load:
                        var dataLoad = csvTuple.Item2;
                        if (eIndex != -1)
                        {
                            var matchingTuple = dataSheet[eIndex];
                            var tupleDate = matchingTuple.Item1;
                            var tupleDayOfWeek = matchingTuple.Item3;
                            dataSheet[eIndex] = new Tuple<DateTime, int, int>(tupleDate, dataLoad, tupleDayOfWeek);
                        }
                        else
                            dataSheet.Add(new Tuple<DateTime, int, int>(csvTupleDate, dataLoad, -1));
                        break;
                }
            }
        }
	}

	static class Program
	{
		private const string calendarPath = "calendar.csv";
		private const string loadPath = "load.csv";

		static void Main(string[] args)
        {
            #region Reading data
            var dataSheet = new List<Tuple<DateTime, int, int>>();

            var loadDataSheet = DataAnalysis.ReadCSV(loadPath, ';');
            var calendarDataSheet = DataAnalysis.ReadCSV(calendarPath, '	');

            dataSheet.Fill(loadDataSheet, csvType.load);
            dataSheet.Fill(calendarDataSheet, csvType.calendar);
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
