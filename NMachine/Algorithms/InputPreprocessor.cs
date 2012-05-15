using System;
using System.Collections;
using System.Reflection;

namespace NMachine.Algorithms
{
	internal class InputPreprocessor
	{
		private readonly bool _scaleAndNormalize;
		private double[] _mean;
		private double[] _deviation;
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

			var samplesMatrix = GetSamplesMatrix(samples, samplesCount, out _mean, out _deviation);
			var labelsVector = GetLabelsVector(labels, samplesCount);

			if (_scaleAndNormalize) {
				int sample = 0;
				while (sample < samplesCount) {
					for (int feature = 0; feature < _features.Length; feature++) {
						samplesMatrix[sample, feature] = (samplesMatrix[sample, feature] - _mean[feature]) / _deviation[feature];
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

			var samplesMatrix = GetSamplesMatrix(samples, 1);
			var labelsVector = GetLabelsVector(labels, 1);

			if (_scaleAndNormalize) {
				for (int feature = 0; feature < _features.Length; feature++) {
					samplesMatrix[0, feature] = (samplesMatrix[0, feature] - _mean[feature]) / _deviation[feature];
				}
			}

			return new Input(samplesMatrix, labelsVector, 0, 1);
		}

		private double[,] GetSamplesMatrix(IEnumerable samples, int samplesCount)
		{
			double[] mean; 
			double[] deviation;

			return GetSamplesMatrix(samples, samplesCount, out mean, out deviation);
		}

		private double[,] GetSamplesMatrix(IEnumerable samples, int samplesCount, out double[] mean, out double[] deviation)
		{
			var samplesMatrix = new double[samplesCount, _features.Length];

			mean = new double[_features.Length];
			deviation = new double[_features.Length];

			var enumerator = samples.GetEnumerator();
			int sample = 0;
			while (enumerator.MoveNext()) {
				for (int feature = 0; feature < _features.Length; feature++) {
					var value = GetValue(enumerator.Current, feature);

					samplesMatrix[sample, feature] = value;
					mean[feature] += value;
				}
				sample++;
			}
			for (int property = 0; property < _features.Length; property++) {
				mean[property] /= samplesCount;
			}

			//Now calculate standard deviation. Note: would be interesting to implement one-pass algorithm as per http://zach.in.tu-clausthal.de/teaching/info_literatur/Welford.pdf
			for (sample = 0; sample < samplesCount; sample++) {
				for (int feature = 0; feature < _features.Length; feature++) {
					deviation[feature] += Math.Pow((samplesMatrix[sample, feature] - mean[feature]), 2);
				}
			}
			for (int feature = 0; feature < _features.Length; feature++) {
				deviation[feature] = Math.Sqrt(deviation[feature] / (samplesCount - 1));
			}

			return samplesMatrix;
		}

		private double GetValue(object sample, int feature)
		{
			var valueObj = _features[feature].GetValue(sample, null);
			if (valueObj == null) {
				throw new NMachineException("Currenctly the system doesn't support NULL values - all features must have a value.");
			}

			double value;
			var convertible = valueObj as IConvertible;
			if (convertible != null) {
				value = (convertible is string) ? convertible.GetHashCode() : convertible.ToDouble(null);
			}
			else {
				throw new NMachineException("Failed to convert " + valueObj + " to double.");
			}
			return value;
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
