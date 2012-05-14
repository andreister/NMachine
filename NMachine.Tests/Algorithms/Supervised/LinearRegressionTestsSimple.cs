using NMachine.Algorithms;
using NMachine.Algorithms.Supervised;
using NUnit.Framework;

namespace NMachine.Tests.Algorithms.Supervised
{
	[TestFixture]
	public class LinearRegressionTestsSimple
	{
		[Test]
		public void FeatureScaledInput()
		{
			var cars = new[] {
				new Car {Doors = 1, Seats = 1},
				new Car {Doors = 4, Seats = 4},
				new Car {Doors = 2, Seats = 2},
			};
			var prices = new double[] {
				2,
				8,
				4
			};

			var algorithm = new LinearRegression(cars, prices, new Settings {InputSplitRatio = InputSplitRatio.No});
			
			var car = new Car {Doors = 9, Seats = 9};
			var price = algorithm.GetPrediction(car);

			Assert.That(price, Is.InRange(17.5, 18.5), "Incorrect prediction for " + car);
		}

		class Car
		{
			public int Doors { get; set; }
			public int Seats { get; set; }
		}
	}
}
