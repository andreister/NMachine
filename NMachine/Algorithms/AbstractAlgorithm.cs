using System.Collections;

namespace NMachine.Algorithms
{
	/// <summary>
	/// Father of all algorithms.
	/// </summary>
	public abstract class AbstractAlgorithm
	{
		protected Input TrainingSet { get; private set; }
		protected Input CrossValidationSet { get; private set; }
		protected Input TestSet { get; private set; }
		protected Settings Settings { get; private set; }
		private bool _analysisDone;

		/// <summary>
		/// Creates a new instance of the algorithm.
		/// </summary>
		/// <param name="features">Matrix of features: each row is a new sample, each column in a new feature.</param>
		/// <param name="labels">Vector of labels. Each element corresponds to a row in the above matrix.</param>
		/// <param name="settings">Settings for the algorithm (input split, learning rate, etc).</param>
		protected AbstractAlgorithm(IEnumerable features, IEnumerable labels, Settings settings = null)
		{
			var samplesOnFeatures = GetSize(features);
			var samplesOnLabels = GetSize(labels);
			if (samplesOnFeatures != samplesOnLabels) {
				throw new NMachineException("The same number of labels and features expected, but received " + samplesOnFeatures + " features and " + samplesOnLabels + " labels.");
			}
			Settings = settings ?? new Settings();

			if (Settings.ScaleAndNormalize) {
				features = ScaleAndNormalize(features);
			}

			InputSplit split;
			switch (Settings.InputSplitType) {
				case InputSplitType.Default:
					split = new InputSplit(samplesOnFeatures);
					TrainingSet = new Input(features, labels, 0, split.TrainingSetSize);
					CrossValidationSet = new Input(features, labels, split.TrainingSetSize, split.CrossValidationSetSize);
					TestSet = new Input(features, labels, split.TrainingSetSize + split.CrossValidationSetSize, split.TestSetSize);
					break;
				case InputSplitType.Custom:
					split = Settings.InputSplit;
					TrainingSet = new Input(features, labels, 0, split.TrainingSetSize);
					CrossValidationSet = new Input(features, labels, split.TrainingSetSize, split.CrossValidationSetSize);
					TestSet = new Input(features, labels, split.TrainingSetSize + split.CrossValidationSetSize, split.TestSetSize);
					break;
				case InputSplitType.NoSplit:
					TrainingSet = new Input(features, labels, 0, samplesOnFeatures);
					CrossValidationSet = new Input(features, labels, 0, 0);
					TestSet = new Input(features, labels, 0, 0);
					break;
				default:
					throw new NMachineException("Unexpected split type: " + Settings.InputSplitType);
			}
		}

		/// <summary>
		/// Analyzes the training, cross-validation and test set data.
		/// </summary>
		protected abstract void Analyze(Input input);

		/// <summary>
		/// Predicts the value for the given item.
		/// </summary>
		protected abstract double Predict(Input item);

		/// <summary>
		/// Performs input data analysis. Can be invoked separately,
		/// and if not then gets invoked as a part of GetPrediction.
		/// </summary>
		public void Analyze()
		{
			if (_analysisDone) {
				return;
			}

			Analyze(TrainingSet);
			_analysisDone = true;
		}

		/// <summary>
		/// Predicts the value for the given item.
		/// </summary>
		public double GetPrediction<TItem>(TItem item)
		{
			Analyze();

			var input = new Input(new [] {item}, new[] {0}, 0, 1);
			return Predict(input);
		}

		/// <summary>
		/// Applies feature scaling to improve analysis.
		/// </summary>
		private IEnumerable ScaleAndNormalize(IEnumerable features)
		{
			return features;
		}

		private static int GetSize(IEnumerable list)
		{
			int result = 0;
			IEnumerator enumerator = list.GetEnumerator();
			while (enumerator.MoveNext()) {
				result++;
			}
			
			return result;
		}
	}
}
