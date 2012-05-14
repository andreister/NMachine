namespace NMachine.Algorithms
{
	public class Settings
	{
		internal double LearningRate { get; set; }
		internal double ConvergenceDelta { get; set; }
		internal int MaxIterations { get; set; }
		internal bool ScaleAndNormalize { get; set; }
		internal InputSplitRatio InputSplitRatio { get; set; }

		internal Settings()
		{
			LearningRate = 0.01;
			ConvergenceDelta = 0.000001;
			MaxIterations = 1500;
			ScaleAndNormalize = true;
			InputSplitRatio = InputSplitRatio.Default;
		}
	}
}