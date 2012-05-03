using System;
using System.Collections.Generic;
using System.IO;
using NMachine.Algorithms.Supervised;
using NUnit.Framework;

namespace NMachine.Tests.Algorithms.Supervised
{
	[TestFixture]
	public class LinearRegressionTestsCar
	{
		private readonly List<Car> _cars = new List<Car>();
		private readonly List<decimal > _carPrices = new List<decimal>();

		#region Setup/Teardown

		[SetUp]
		public void Setup()
		{
			//read the values as per "93cars.txt" description
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

			Assert.That(_cars.Count, Is.GreaterThan(0), "Cars dataset hasn't been read correctly.");
			Assert.That(_carPrices.Count, Is.GreaterThan(0), "Cars dataset hasn't been read correctly.");
		}

		#endregion

		[Test]
		public void PredictCarPrice()
		{
			var algorithm = new LinearRegression(_cars, _carPrices);
			
			var car = new Car {
				Model = "Mazda MPV", 
				EngineSize = 12, 
				MaxHorsePower = 12,
				IsManualShift = false
			};
			var price = algorithm.GetPrediction(car);

			Assert.That(price, Is.InRange(15.9, 16.9), "Incorrect price prediction for [" + car + "]");
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
	}
}
