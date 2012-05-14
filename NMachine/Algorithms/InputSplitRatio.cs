namespace NMachine.Algorithms
{
	public enum InputSplitRatio
	{
		/// <summary>
		/// Split 2/3 for the training set, 1/3 for the validation and 1/3 for the test set.
		/// </summary>
		Default = 0,

		/// <summary>
		/// Custom split - the client should provide the split ratio.
		/// </summary>
		Custom = 1,

		/// <summary>
		/// Don't split the input and use all data as a training set. (Not recommended).
		/// </summary>
		No = 2
	}
}
