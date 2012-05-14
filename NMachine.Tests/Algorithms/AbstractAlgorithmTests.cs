using System;
using System.Collections;
using System.Collections.Generic;
using NMachine.Algorithms;
using NUnit.Framework;
using System.Linq;

namespace NMachine.Tests.Algorithms
{
	[TestFixture]
	public class AbstractAlgorithmTests
	{
		[Test, TestCaseSource("Input")]
		[ExpectedException(ExpectedException = typeof(NMachineException), ExpectedMessage = "The same number of labels and features expected, but received", MatchType = MessageMatch.Contains)]
		public void InputSizeIncorrect(List<object> samples, List<double> labels)
		{
			new FakeAlgorithm(samples, labels.Take(2));
		}

		[Test, TestCaseSource("Input")]
		public void DefaultInputSplit(List<object> samples, List<double> labels)
		{
			var people = samples.ConvertAll(x => (Person)x);
			var trainingSetSize = (int)Math.Ceiling(((double)2 / 3) * people.Count);
			var crossValidationSetSize = (int)Math.Ceiling(((double)2 / 3) * people.Count);

			var algorithm = new FakeAlgorithm(people, labels, new Settings {ScaleAndNormalize = false});

			AssertInput(algorithm.MyTrainingSetX, algorithm.MyTrainingSetY, people, labels, 0, trainingSetSize);
			AssertInput(algorithm.MyCrossValidationSetX, algorithm.MyCrossValidationSetY, people, labels, trainingSetSize, crossValidationSetSize);
			Assert.That(algorithm.MyTestSetX, Is.Null);
			Assert.That(algorithm.MyTestSetY, Is.Null);
		}

		[Test, TestCaseSource("Input")]
		public void ScaledInput(List<object> samples, List<double> labels)
		{
			var people = samples.ConvertAll(x => (Person)x);
			var age = new { Avg = people.Select(x => x.Age).Average(), Max = people.Select(x => x.Age).Max(), Min = people.Select(x => x.Age).Min() };
			var height = new { Avg = people.Select(x => x.Height).Average(), Max = people.Select(x => x.Height).Max(), Min = people.Select(x => x.Height).Min() };

			var algorithm = new FakeAlgorithm(people, labels, new Settings {InputSplitType = InputSplitType.NoSplit});

			Assert.That(algorithm.MyTrainingSetX[0, 0], Is.EqualTo(1), "A column of ones should be added to the input matrix.");
			Assert.That(algorithm.MyTrainingSetX[0, 1], Is.EqualTo(  (people[0].Age - age.Avg) / (age.Max - age.Min) ));
			Assert.That(algorithm.MyTrainingSetX[0, 2], Is.EqualTo(  (people[0].Height - height.Avg) / (height.Max - height.Min) ));
			Assert.That(algorithm.MyTrainingSetY[0], Is.EqualTo(labels[0]));
		}

		private void AssertInput(double[,] xMatrix, double[] yMatrix, List<Person> xList, List<double> yList, int skip, int take)
		{
			for (int rows = skip; rows < take; rows++) {
				Assert.That(xMatrix[rows, 0], Is.EqualTo(1), "A column of ones should be added to the input matrix."); 
				Assert.That(xMatrix[rows, 1], Is.EqualTo(xList[rows].Age));
				Assert.That(xMatrix[rows, 2], Is.EqualTo(xList[rows].Height));
				Assert.That(yMatrix[rows], Is.EqualTo(yList[rows]));
			}
		}

		public IEnumerable<TestCaseData> Input
		{
			get
			{
				return new List<TestCaseData> {
					new TestCaseData(
						new List<Person> {
							new Person { Age = 10, Height = 12 },
							new Person { Age = 42, Height = 34 },
							new Person { Age = 21, Height = 19 }
						}.ConvertAll(x => (object)x),
						new List<double> {
							23,
							323,
							32
						}
					).SetName("3 samples input"),
					new TestCaseData(
						new List<Person> {
							new Person { Age = 2, Height = 5 },
							new Person { Age = 7, Height = 11 },
							new Person { Age = 15, Height = 19 },
							new Person { Age = 40, Height = 25 },
							new Person { Age = 70, Height = 20 }
						}.ConvertAll(x => (object)x),
						new List<double> {
							23,
							323,
							43,
							34,
							42
						}
					).SetName("5 samples input"),
				};
			}
		}

		private class Person
		{
			public int Age { get; set; }
			public double Height { get; set; }
		}

		private class FakeAlgorithm : AbstractAlgorithm
		{
			public FakeAlgorithm(IEnumerable features, IEnumerable labels, Settings settings = null)
				: base(features, labels, settings ?? new Settings { ScaleAndNormalize = false })
			{
			}

			protected override void Analyze(Input input)
			{
			}

			protected override double Predict(Input item)
			{
				return 0;
			}

			internal double[,] MyTrainingSetX { get { return TrainingSet.X.ToArray(); } }
			internal double[] MyTrainingSetY { get { return TrainingSet.Y.ToColumnWiseArray(); } }

			internal double[,] MyCrossValidationSetX { get { return CrossValidationSet.X.ToArray(); } }
			internal double[] MyCrossValidationSetY { get { return CrossValidationSet.Y.ToColumnWiseArray(); } }

			internal double[,] MyTestSetX { get { return (TestSet.X == null) ? null : TestSet.X.ToArray(); } }
			internal double[] MyTestSetY { get { return (TestSet.Y == null) ? null : TestSet.Y.ToColumnWiseArray(); } }
		}
	}
}
