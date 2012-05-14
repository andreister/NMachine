using System;
using System.Collections;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Generic;
using NMachine.Logging;

namespace NMachine.Algorithms.Supervised
{
	/// <summary>
	/// Linear regression algorithm.
	/// 
	/// The objective is to minimize the cost function 
	/// 
	/// 	J(theta) = (1/2m)SUM(h[i]-y[i])^2
	/// 
	///  where the hypothesis 
	/// 
	///		h = theta0 + theta1*x1 + ... + thetaN*xN
	/// 
	/// </summary>
	public class LinearRegression : AbstractAlgorithm
	{
		private static readonly ILogger _logger = LogManager.GetLogger();
		private Matrix<double> _theta;

		public LinearRegression(IEnumerable features, IEnumerable labels, Settings settings = null)
			: base(features, labels, settings)
		{
		}

		/// <summary>
		/// Calculates the "theta" vector via linear regression.
		/// </summary>
		protected override void Analyze(Input input)
		{
			if (!TryNormalEquation(input)) {
				GradientDescent(input);
			}
		}

		/// <summary>
		/// Uses normal equation to calculate the "theta" vector in one go:
		/// 
		///		theta = (X'X)^{-1}*X'y
		/// 
		/// The normal equation algorithm needs to invert a matrix (which is ~O(n^3) operation)
		/// and,setting aside the problem of non-invertability, can become awkwardly slow if used
		/// against a problem with a huge number of features. Andrew Ng recommends n=10K, and we
		/// are using a slightly smaller threshold here.
		/// </summary>
		private bool TryNormalEquation(Input input)
		{
			return false; //don't use Normal Equation for now - for some reason, pseudoinverse is not stable

			int threshold = 100;
			if (input.FeaturesCount > threshold || input.SamplesCount < input.FeaturesCount) {
				_logger.Info("Normal Equation seems not applicable due to the feature-space size, resorting to Gradient Descent.");
				return false;
			}

			try {
				_theta = (input.X.Transpose() * input.X).Inverse() * input.X.Transpose() * input.Y;
				return true;
			}
			catch (Exception ex) {
				_logger.Warn("Normal equation failed to complete.", ex);
				return false;
			}
		}

		/// <summary>
		/// Uses gradient descent to calculate the "theta" vector.
		/// On each iteration gets calculated as per
		/// 
		///		theta = theta - (alpha/m)*(X*theta - Y)'*X
		/// 
		/// </summary>
		private void GradientDescent(Input input)
		{
			var monitor = new CostFunctionMonitor(input);

			_theta = new DenseMatrix(1, input.FeaturesCount);
			
			var multiplier = (Settings.LearningRate / input.SamplesCount);
			for (int i = 0; i < Settings.MaxIterations; i++) {
				_theta -= multiplier * ((input.X * _theta.Transpose() - input.Y).Transpose() * input.X);
				if (monitor.IsConverged(_theta)) {
					break;
				}
			}
		}

		/// <summary>
		/// Predicts the label for the given input.
		/// Prediction is calculated as per
		/// 
		///		y = theta'*X
		/// 
		/// </summary>
		protected override double Predict(Input item)
		{
			var prediction = item.X *_theta.Transpose();
			if (prediction.ColumnCount != 1 || prediction.RowCount != 1) {
				throw new NMachineException("Failed to calculate the cost function. Instead of a raw number, getting a " + prediction.RowCount + "x" + prediction.ColumnCount + " matrix.");
			}

			return prediction.ToColumnWiseArray()[0];
		}
	}
}
