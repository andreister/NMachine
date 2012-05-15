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
		private static List<House> _houses;
		private static List<decimal> _housePrices;

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			ReadCitiesAndProfits();
			ReadCarsAndPrices();
			ReadHousesAndPrices();
		}

		[Test]
		public void PredictCarPrice()
		{
			//var algorithm = new LinearRegression(_cars, _carPrices, new Settings { InputSplitRatio = InputSplitRatio.No, MaxIterations = 425});
			//the above works - looks like IsConverging isn't working properly
			var algorithm = new LinearRegression(_cars, _carPrices, new Settings { InputSplitRatio = InputSplitRatio.No});

			var car = new Car {
				Model = "Mazda MPV",
				EngineSize = 12,
				MaxHorsePower = 12,
				IsManualShift = false
			};
			var price = algorithm.GetPrediction(car);

			Assert.That(price, Is.InRange(15.9, 16.9), "Incorrect price prediction for [" + car + "]");
		}

		[Test, TestCaseSource("Cities")]
		public void Univariate(decimal population, decimal profitFrom, decimal profitTo)
		{
			var algorithm = new LinearRegression(_cities, _profits, new Settings { ScaleAndNormalize = false, InputSplitRatio = InputSplitRatio.No });

			var city = new City { Population = population };
			var profit = algorithm.GetPrediction(city);

			Assert.That(profit, Is.InRange(profitFrom, profitTo), "Incorrect price prediction for [" + city + "]");
		}

		[Test, TestCaseSource("Houses")]
		public void MultivariateScaled(decimal size, int bedroomCount, decimal priceFrom, decimal priceTo)
		{
			//var algorithm = new LinearRegression(_houses, _housePrices, new Settings { InputSplitRatio = InputSplitRatio.No, MaxIterations = 400});
			//the above works - looks like IsConverging isn't working properly
			var algorithm = new LinearRegression(_houses, _housePrices, new Settings { InputSplitRatio = InputSplitRatio.No});

			var house = new House { Size = size, BedroomCount = bedroomCount };
			var price = algorithm.GetPrediction(house);

			Assert.That(price, Is.InRange(priceFrom, priceTo), "Incorrect price prediction for [" + house + "]");
		}

		#region Helper code

		public IEnumerable<TestCaseData> Cities
		{
			get
			{
				return new List<TestCaseData> {
					new TestCaseData(3.5m, 0.4519m, 0.452m).SetName("city 35000 people -> profit 0.45"),
					new TestCaseData(7m, 4.5341m, 4.5343m).SetName("city 70000 people -> profit 4.5"),
					new TestCaseData(0.25m, -3.339m, -3.338m).SetName("city 2500 people -> profit -3.3")
				};
			}
		}

		public IEnumerable<TestCaseData> Houses
		{
			get
			{
				return new List<TestCaseData> {
					new TestCaseData(1000m, 1, 197000m, 198000m).SetName("house 1K feet, 1 bedroom -> price 197K"),
					new TestCaseData(852m, 2, 180000m, 185000m).SetName("house 0.8K feet, 2 bedrooms -> price 185K")
				};
			}
		}

		private class City
		{
			public decimal Population { get; set; }
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

		private class House
		{
			public decimal Size { get; set; }
			public int BedroomCount { get; set; }
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

		private void ReadHousesAndPrices()
		{
			_houses = new List<House>();
			_housePrices = new List<decimal>();
			using (var reader = new StreamReader(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "_Data", "HousePricesDataset", "houseprices.txt"))) {
				var line = reader.ReadLine();
				while (line != null) {
					var values = line.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
					var house = new House {
						Size = Convert.ToDecimal(values[0]),
						BedroomCount = Convert.ToInt32(values[1])
					};
					_houses.Add(house);
					_housePrices.Add(Convert.ToDecimal(values[2]));
					
					line = reader.ReadLine();
				}
			}

			Assert.That(_houses.Count, Is.EqualTo(47), "Houses dataset hasn't been read correctly.");
			Assert.That(_housePrices.Count, Is.EqualTo(47), "Houses dataset hasn't been read correctly.");
		}

		#endregion

	}
}
