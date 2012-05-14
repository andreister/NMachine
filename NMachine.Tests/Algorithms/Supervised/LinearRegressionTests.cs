using System;
using System.Collections.Generic;
using System.IO;
using NMachine.Algorithms;
using NMachine.Algorithms.Supervised;
using NUnit.Framework;

namespace NMachine.Tests.Algorithms.Supervised
{
	[TestFixture]
	public class LinearRegressionTests
	{
		private static List<Car> _cars;
		private static List<decimal> _carPrices;
		private static List<City> _cities;
		private static List<decimal> _profits;

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			ReadCitiesAndProfits();
			ReadCarsAndPrices();
		}

		[Test]
		public void PredictCarPrice()
		{
			var algorithm = new LinearRegression(_cars, _carPrices, new Settings { InputSplitRatio = InputSplitRatio.No });

			var car = new Car {
				Model = "Mazda MPV",
				EngineSize = 12,
				MaxHorsePower = 12,
				IsManualShift = false
			};
			var price = algorithm.GetPrediction(car);

			Assert.That(price, Is.InRange(15.9, 16.9), "Incorrect price prediction for [" + car + "]");
		}

		[Test, TestCaseSource("CityPopulations")]
		public void PredictProfit(decimal population, decimal profitFrom, decimal profitTo)
		{
			var algorithm = new LinearRegression(_cities, _profits, new Settings { ScaleAndNormalize = false, InputSplitRatio = InputSplitRatio.No });

			var city = new City { Population = population };
			var profit = algorithm.GetPrediction(city);

			Assert.That(profit, Is.InRange(profitFrom, profitTo), "Incorrect price prediction for [" + city + "]");
		}

		[Test]
		public void FeatureScaledInput()
		{
			var songs = new[] {
				new Song {Volume = 1, Pleasure = 1},
				new Song {Volume = 4, Pleasure = 4},
				new Song {Volume= 2, Pleasure = 2},
			};
			var prices = new double[] {
				2,
				8,
				4
			};

			var algorithm = new LinearRegression(songs, prices, new Settings { InputSplitRatio = InputSplitRatio.No });

			var song = new Song { Volume = 9, Pleasure = 9 };
			var price = algorithm.GetPrediction(song);

			Assert.That(price, Is.InRange(17.5, 18.5), "Incorrect prediction for " + song);
		}

		#region Helper code

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

		private class Car
		{
			public string Model { get; set; }
			public decimal EngineSize { get; set; }
			public decimal MaxHorsePower { get; set; }
			public bool IsManualShift { get; set; }

			public override string ToString()
			{
				return string.Format("{0} {1}l, {2} rpm, {3}", Model, EngineSize, MaxHorsePower, IsManualShift ? "manual" : "automatic");
			}
		}

		private class Song
		{
			public decimal Volume { get; set; }
			public decimal Pleasure { get; set; }
		}

		private void ReadCitiesAndProfits()
		{
			_cities = new List<City>();
			_profits = new List<decimal>();
			using (var reader = new StreamReader(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "_Data", "FoodChainDataset", "foodchain.dat"))) {
				var line = reader.ReadLine();
				while (line != null) {
					var values = line.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
					_cities.Add(new City { Population = Convert.ToDecimal(values[0]) });
					_profits.Add(Convert.ToDecimal(values[1]));

					line = reader.ReadLine();
				}
			}
			Assert.That(_cities.Count, Is.EqualTo(97), "Cities dataset hasn't been read correctly.");
			Assert.That(_profits.Count, Is.EqualTo(97), "Company profits haven't been read correctly.");
		}

		private void ReadCarsAndPrices()
		{
			_cars = new List<Car>();
			_carPrices = new List<decimal>();
			using (var reader = new StreamReader(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "_Data", "CarsDataset", "93cars.dat"))) {
				var line1 = reader.ReadLine();
				var line2 = reader.ReadLine();
				while (line1 != null && line2 != null) {
					var car = new Car {
						Model = line1.Substring(0, 14).Trim() + " " + line1.Substring(14, 14).Trim(),
						EngineSize = Convert.ToDecimal(line1.Substring(64, 3)),
						MaxHorsePower = Convert.ToDecimal(line1.Substring(68, 3)),
						IsManualShift = Convert.ToBoolean(Convert.ToInt32(line2.Substring(5, 1)))
					};

					_cars.Add(car);
					_carPrices.Add(Convert.ToDecimal(line1.Substring(42, 4).Trim()));

					line1 = reader.ReadLine();
					line2 = reader.ReadLine();
				}
			}

			Assert.That(_cars.Count, Is.EqualTo(93), "Cars dataset hasn't been read correctly.");
			Assert.That(_carPrices.Count, Is.EqualTo(93), "Cars dataset hasn't been read correctly.");
		}

		#endregion

	}
}
