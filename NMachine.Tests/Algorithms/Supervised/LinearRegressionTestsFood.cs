using System;
using System.Collections.Generic;
using System.IO;
using NMachine.Algorithms;
using NMachine.Algorithms.Supervised;
using NUnit.Framework;

namespace NMachine.Tests.Algorithms.Supervised
{
	[TestFixture]
	public class LinearRegressionTestsFood
	{
		private readonly List<City> _cities = new List<City>();
		private readonly List<decimal> _profits = new List<decimal>();

		#region Setup/Teardown

		[TestFixtureSetUp]
		public void Setup()
		{
			using (var reader = new StreamReader(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "_Data", "FoodChainDataset", "foodchain.dat"))) {
				var line = reader.ReadLine();
				while (line != null) {
					var values = line.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
					_cities.Add(new City { Population = Convert.ToDecimal(values[0]) });
					_profits.Add(Convert.ToDecimal(values[1]));

					line = reader.ReadLine();
				}
			}
			Assert.That(_cities.Count, Is.EqualTo(97), "Cities dataset hasn't been read correctly.");
			Assert.That(_profits.Count, Is.EqualTo(97), "Company profits haven't been read correctly.");
		}

		#endregion

		[Test, TestCaseSource("CityPopulations")]
		public void PredictProfit(decimal population, decimal profitFrom, decimal profitTo)
		{
			var algorithm = new LinearRegression(_cities, _profits, new Settings {ScaleAndNormalize = false, InputSplitRatio = InputSplitRatio.No});

			var city = new City { Population = population };
			var profit = algorithm.GetPrediction(city);

			Assert.That(profit, Is.InRange(profitFrom, profitTo), "Incorrect price prediction for [" + city + "]");
		}

		private class City
		{
			public decimal Population { get; set; }
		}

		public IEnumerable<TestCaseData> CityPopulations
		{
			get
			{
				return new List<TestCaseData> {
					new TestCaseData(3.5m, 0.4519m, 0.452m).SetName("35000 people"),
					new TestCaseData(7m, 4.5341m, 4.5343m).SetName("70000 people"),
					new TestCaseData(0.25m, -3.339m, -3.338m).SetName("2500 people")
				};
			}
		}
	}
}
