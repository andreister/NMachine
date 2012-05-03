using NMachine.Algorithms;
using NUnit.Framework;

namespace NMachine.Tests
{
	[TestFixture]
	public class InputSplitTests
	{
		[Test]
		[TestCase(90, 60, 15, 15, TestName = "Integer split")]
		[TestCase(93, 62, 16, 15, TestName = "Non-integer split")]
		[TestCase(3, 2, 1, 0, TestName = "Three items split")]
		public void DefaultSplit(int inputSize, int expectedTrainingSetSize, int expectedCrossValidationSetSize, int expectedTestSetSize)
		{
			var split = new InputSplit(inputSize);

			Assert.That(split.TrainingSetSize, Is.EqualTo(expectedTrainingSetSize));
			Assert.That(split.CrossValidationSetSize, Is.EqualTo(expectedCrossValidationSetSize));
			Assert.That(split.TestSetSize, Is.EqualTo(expectedTestSetSize));
		}
	}
}
