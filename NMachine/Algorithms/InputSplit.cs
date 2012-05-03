using System;

namespace NMachine.Algorithms
{
	/// <summary>
	/// Helper class to ease the split between the training set,
	/// cross-validation set, and the test set.
	/// </summary>
	public class InputSplit
	{
		public int TrainingSetSize { get; private set; }
		public int CrossValidationSetSize { get; private set; }
		public int TestSetSize { get; private set; }

		/// <summary>
		/// Creates a custom split.
		/// </summary>
		internal InputSplit(int trainingSetSize, int crossValidationSetSize, int testSetSize)
		{
			TrainingSetSize = trainingSetSize;
			CrossValidationSetSize = crossValidationSetSize;
			TestSetSize = testSetSize;
		}

		/// <summary>
		/// Creates a default split: about two-thirds of the total input dataset
		/// go to the training set, about one-third goes to the cross-validation
		/// set and the rest goes to the test set.
		/// </summary>
		/// <param name="totalInputSize">The overall size of the dataset.</param>
		internal InputSplit(int totalInputSize)
		{
			TrainingSetSize = (int)Math.Ceiling(((double)2/3)*totalInputSize);
			CrossValidationSetSize = (int)Math.Ceiling(((double)1/3)*totalInputSize/2);
			TestSetSize = totalInputSize - (TrainingSetSize + CrossValidationSetSize);
		}
	}
}
