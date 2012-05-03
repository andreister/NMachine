using System;
using System.Collections;
using System.Collections.Generic;
using NMachine.Algorithms;
using NUnit.Framework;

namespace NMachine.Tests.Algorithms
{
	[TestFixture]
	public class AbstractAlgorithmTests
	{
		[Test, ExpectedException(ExpectedException = typeof(NMachineException), ExpectedMessage = "The same number of labels and features expected, but received 2 features and 3 labels.")]
		public void DifferentNumberOfInputsAndOutputs()
		{
			var people = new List<Person> {
				new Person {Age = 10, Height = 12},
				new Person {Age = 42, Height = 34}
			};
			var peopleWeights = new List<double> {
				23, 
				323, 
				32
			};
			new FakeAlgorithm(people, peopleWeights);
		}

		[Test]
		public void InputSplit()
		{
			var people = new List<Person> {
				new Person { Age = 10, Height = 12 }, 
				new Person { Age = 12, Height = 14 }, 
				new Person { Age = 21, Height = 19 }
			};
			var peopleWeights = new List<double> {
				23, 
				323, 
				32
			};

			var algorithm = new FakeAlgorithm(people, peopleWeights);

			var trainingSetSize = (int)Math.Ceiling(((double)2 / 3) * people.Count);
			var crossValidationSetSize = (int)Math.Ceiling(((double)2 / 3) * people.Count);

			AssertInput(algorithm.MyTrainingSetX, algorithm.MyTrainingSetY, people, peopleWeights, 0, trainingSetSize);
			AssertInput(algorithm.MyCrossValidationSetX, algorithm.MyCrossValidationSetY, people, peopleWeights, trainingSetSize, crossValidationSetSize);

			Assert.That(algorithm.MyTestSetX, Is.Null);
			Assert.That(algorithm.MyTestSetY, Is.Null);
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

		private class Person
		{
			public int Age { get; set; }
			public double Height { get; set; }
		}

		private class FakeAlgorithm : AbstractAlgorithm
		{
			public FakeAlgorithm(IEnumerable features, IEnumerable labels) 
				: base(features, labels)
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
