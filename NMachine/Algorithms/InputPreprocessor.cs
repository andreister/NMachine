using System;
using System.Collections;
using System.Reflection;

namespace NMachine.Algorithms
{
	internal class InputPreprocessor
	{
		private readonly bool _scaleAndNormalize;
		private double[] _min;
		private double[] _avg;
		private double[] _max;
		private PropertyInfo[] _features;
		
		internal Input TrainingSet { get; private set; }
		internal Input CrossValidationSet { get; private set; }
		internal Input TestSet { get; private set; }
		
		/// <summary>
		/// Creates a new processor.
		/// </summary>
		public InputPreprocessor(bool scaleAndNormalize)
		{
			_scaleAndNormalize = scaleAndNormalize;
		}

		/// <summary>
		/// Converts samples and labels to "TrainingSet" instances.
		/// If required, applies feature scaling and mean normalization.
		/// </summary>
		internal void Run(IEnumerable samples, IEnumerable labels, InputSplitRatio splitRatio)
		{
			int samplesCount;
			_features = GetFeatures(samples, labels, out samplesCount);

			var samplesMatrix = GetSamplesMatrix(samples, samplesCount, out _min, out _max, out _avg);
			var labelsVector = GetLabelsVector(labels, samplesCount);

			if (_scaleAndNormalize) {
				int sample = 0;
				while (sample < samplesCount) {
					for (int feature = 0; feature < _features.Length; feature++) {
						var scale = (_max[feature] == _min[feature]) ? 1 : _max[feature] - _min[feature];
						samplesMatrix[sample, feature] = (samplesMatrix[sample, feature] - _avg[feature]) / scale;
					}
					sample++;
				}
			}

			switch (splitRatio) {
				case InputSplitRatio.No:
					TrainingSet = new Input(samplesMatrix, labelsVector, 0, samplesCount);
					break;
				case InputSplitRatio.Default:
					var trainingSetSize = (int) Math.Ceiling(((double) 2/3)*samplesCount);
					var crossValidationTestSize = (int)Math.Ceiling(((double)1 / 3) * samplesCount / 2);
					var testSetSize = samplesCount - (trainingSetSize + crossValidationTestSize);
					
					TrainingSet = new Input(samplesMatrix, labelsVector, 0, trainingSetSize);
					CrossValidationSet = new Input(samplesMatrix, labelsVector, trainingSetSize, crossValidationTestSize);
					TestSet = new Input(samplesMatrix, labelsVector, (trainingSetSize + crossValidationTestSize), testSetSize);
					break;
				default:
					throw new NMachineException("Unexpected split type: " + splitRatio);
			}
		}

		/// <summary>
		/// Converts given sample and label to a "TrainingSet" instance. 
		/// If required, applies feature scaling and normalization.
		/// </summary>
		internal Input CreateInput(object sample, double label)
		{
			var samples = new[] {sample};
			var labels = new[] {label};

			double[] min;
			double[] avg;
			double[] max;
			var samplesMatrix = GetSamplesMatrix(samples, 1, out min, out max, out avg);
			var labelsVector = GetLabelsVector(labels, 1);

			if (_scaleAndNormalize) {
				for (int feature = 0; feature < _features.Length; feature++) {
					var scale = (_max[feature] == _min[feature]) ? 1 : _max[feature] - _min[feature];
					samplesMatrix[0, feature] = (samplesMatrix[0, feature] - _avg[feature]) / scale;
				}
			}

			return new Input(samplesMatrix, labelsVector, 0, 1);
		}

		private double[,] GetSamplesMatrix(IEnumerable samples, int samplesCount, out double[] min, out double[] max, out double[] avg)
		{
			var samplesMatrix = new double[samplesCount, _features.Length];

			avg = new double[_features.Length];
			max = new double[_features.Length];
			min = new double[_features.Length];
			for (int property = 0; property < _features.Length; property++) {
				max[property] = double.MinValue;
				min[property] = double.MaxValue;
			}

			var enumerator = samples.GetEnumerator();
			int sample = 0;
			while (enumerator.MoveNext()) {
				for (int feature = 0; feature < _features.Length; feature++) {
					var valueObj = _features[feature].GetValue(enumerator.Current, null);
					double value;
					var convertible = valueObj as IConvertible;
					if (convertible != null) {
						value = (convertible is string) ? convertible.GetHashCode() : convertible.ToDouble(null);
					}
					else {
						throw new NMachineException("Failed to convert " + valueObj + " to double.");
					}

					samplesMatrix[sample, feature] = value;
					max[feature] = Math.Max(max[feature], value);
					min[feature] = Math.Min(min[feature], value);
					avg[feature] += value;
				}
				sample++;
			}
			for (int property = 0; property < _features.Length; property++) {
				avg[property] /= samplesCount;
			}

			return samplesMatrix;
		}

		private double[] GetLabelsVector(IEnumerable labels, int samplesCount)
		{
			var labelsVector = new double[samplesCount];

			var enumerator = labels.GetEnumerator();
			int sample = 0;
			while (enumerator.MoveNext()) {
				var valueString = enumerator.Current.ToString();
				double value;
				if (!double.TryParse(valueString, out value)) {
					value = valueString.GetHashCode();
				}
				labelsVector[sample] = value;
				sample++;
			}

			return labelsVector;
		}

		private PropertyInfo[] GetFeatures(IEnumerable samples, IEnumerable labels, out int samplesCount)
		{
			var result = (PropertyInfo[])null;

			samplesCount = 0;
			var enumerator = samples.GetEnumerator();
			while (enumerator.MoveNext()) {
				if (result == null) {
					result = enumerator.Current.GetType().GetProperties();
				}
				samplesCount++;
			}
			if (result == null) {
				throw new NMachineException("Empty list of features received.");
			}

			int labelsCount = 0;
			enumerator = labels.GetEnumerator();
			while (enumerator.MoveNext()) {
				labelsCount++;
			}

			if (samplesCount != labelsCount) {
				throw new NMachineException("The same number of labels and features expected, but received " + samplesCount + " samples and " + labelsCount + " labels.");
			}
			return result;
		}
	}
}
