using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace LinqTasks
{
	static class Chart
	{
		public static void ShowHistogram(string title, IEnumerable<DataPoint> xy)
		{
			// Подробности о том, как использовать класс Chart, можно найти тут: http://msdn.microsoft.com/ru-ru/library/dd456632.aspx
			// Не бойтесь экспериментировать с кодом самостоятельно!
			var chart = new System.Windows.Forms.DataVisualization.Charting.Chart();
			var series = new Series();
			series.ChartType = SeriesChartType.Column;
			foreach (var dataPoint in xy)
				series.Points.Add(dataPoint);
			chart.Series.Add(series);
			chart.ChartAreas.Add(new ChartArea());
			chart.Dock = DockStyle.Fill;
			if (!string.IsNullOrEmpty(title))
				chart.Titles.Add(title);


			// Form — это привычное нам окно программы. Это одна из главных частей подсистемы под названием Windows Forms http://msdn.microsoft.com/ru-ru/library/ms229601.aspx
			var form = new Form();
			form.Text = title;
			form.Width = 800;
			form.Height = 600;
			form.Controls.Add(chart);
			form.ShowDialog();
		}

		public static void ShowLines(string title, params IEnumerable<double>[] ys)
		{
			var chart = new System.Windows.Forms.DataVisualization.Charting.Chart();
			foreach (var y in ys)
			{
				var series = new Series();
				series.ChartType = SeriesChartType.Line;
				foreach (var dataPoint in y)
					series.Points.AddY(dataPoint);
				chart.Series.Add(series);
			}
			chart.ChartAreas.Add(new ChartArea());
			chart.Dock = DockStyle.Fill;
			if (!string.IsNullOrEmpty(title))
				chart.Titles.Add(title);
			var form = new Form();
			form.Text = title;
			form.Width = 800;
			form.Height = 600;
			form.Controls.Add(chart);
			form.ShowDialog();
		}
	}
}