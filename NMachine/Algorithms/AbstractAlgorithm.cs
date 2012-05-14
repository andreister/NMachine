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
		private readonly InputPreprocessor _preprocessor;
		
		/// <summary>
		/// Creates a new instance of the algorithm.
		/// </summary>
		/// <param name="samples">Matrix of features: each row is a new sample, each column in a new feature.</param>
		/// <param name="labels">Vector of labels. Each element corresponds to a row in the above matrix.</param>
		/// <param name="settings">Settings for the algorithm (input split, learning rate, etc).</param>
		protected AbstractAlgorithm(IEnumerable samples, IEnumerable labels, Settings settings = null)
		{
			Settings = settings ?? new Settings();
			_preprocessor = new InputPreprocessor(Settings.ScaleAndNormalize);

			_preprocessor.Run(samples, labels, Settings.InputSplitRatio);

			TrainingSet = _preprocessor.TrainingSet;
			CrossValidationSet = _preprocessor.CrossValidationSet;
			TestSet = _preprocessor.TestSet;
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

			var input = _preprocessor.CreateInput(item, 0);
			return Predict(input);
		}
	}
}
