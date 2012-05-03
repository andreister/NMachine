using System.Collections.Generic;
using NMachine.Algorithms.Extensions;
using NUnit.Framework;

namespace NMachine.Tests.Algorithms.Extensions
{
	[TestFixture]
	public class EnumerableConverterTests
	{
		[Test]
		public void ToMatrix()
		{
			var items = new List<MyType> {
				new MyType { Foo = 12.2m, Boo = 14.3m },
				new MyType { Foo = -3.1m, Boo = 29.8m },
				new MyType { Foo = 43.9m, Boo = -2.4m },
				new MyType { Foo = 11.3m, Boo = 12.5m },
				new MyType { Foo = -9.2m, Boo = -8.2m },
			};
			var matrix = items.ToMatrix(skip:1, take:3);

			Assert.That(matrix.Length, Is.EqualTo(6), "Total number of elements is incorrect.");
			Assert.That(matrix[0, 0], Is.EqualTo(-3.1m));   Assert.That(matrix[0, 1], Is.EqualTo(29.8m));
			Assert.That(matrix[1, 0], Is.EqualTo(43.9m));   Assert.That(matrix[1, 1], Is.EqualTo(-2.4m));
			Assert.That(matrix[2, 0], Is.EqualTo(11.3m));   Assert.That(matrix[2, 1], Is.EqualTo(12.5m));
		}

		[Test]
		public void ToVector()
		{
			var items = new List<double> { 12.3, 13.5, -9.3, 23.9, -8.3 };
			var vector = items.ToVector(skip:1, take:3);

			Assert.That(vector.Length, Is.EqualTo(3), "Total number of elements is incorrect.");
			Assert.That(vector[0], Is.EqualTo(13.5m));
			Assert.That(vector[1], Is.EqualTo(-9.3m));
			Assert.That(vector[2], Is.EqualTo(23.9m));
		}

		[Test]
		public void PrependWithOnes()
		{
			var matrix = new double[,] {
				{ 21, 22 }, 
				{ 31, 32 },
				{ 41, 42 }
			};

			var result = matrix.PrependWithOnesColumn();
			for (int row = 0; row < 3; row++) {
				Assert.That(result[row, 0], Is.EqualTo(1));
				Assert.That(result[row, 1], Is.EqualTo(matrix[row, 0]));
				Assert.That(result[row, 2], Is.EqualTo(matrix[row, 1]));
			}
		}

		class MyType
		{
			public decimal Foo { get; set; }
			public decimal Boo { get; set; }
		}
	}
}
