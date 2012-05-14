using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra.Generic;
using NMachine.Logging;
using System.Linq;

namespace NMachine.Algorithms.Supervised
{
	/// <summary>
	/// Keeps the list of cost function values, and monitors
	/// whether the cost truly goes down.
	/// </summary>
	internal class CostFunctionMonitor
	{
		private static readonly ILogger _logger = LogManager.GetLogger();
		private static readonly int _monitorStep = 5;

		private readonly Input _input;
		private readonly double _convergenceDelta;
		private readonly List<double> _costItems = new List<double>();
		private readonly NotificationMode _mode;
		private int _iterationCounter;

		/// <summary>
		/// Creates a new monitor.
		/// </summary>
		/// <param name="input">Input features and labels.</param>
		/// <param name="convergenceDelta">Criteria of convergence. When two subsequent cost function values
		/// don't differ by more than the given delta, we can say that the Gradient Descent has converged.</param>
		/// <param name="mode">Monitor mode - defines how the monitor would notify about cost function problems.</param>
		public CostFunctionMonitor(Input input, double convergenceDelta = 0.000001, NotificationMode mode = NotificationMode.Throw)
		{
			_input = input;
			_convergenceDelta = convergenceDelta;
			_mode = mode;
			_iterationCounter = 0;
		}

		/// <summary>
		/// Calculates the error function for the given theta vector,
		/// and occasionally checks if the error really goes down.
		/// 
		/// The value of the cost function is calculated as per usual
		/// 
		///		J(theta) = (1/2m)*SUM(h[i] - y[i])^2
		/// 
		/// Returns true if the just calculated cost differ from the 
		/// previously calculated one by less than the convergence delta.
		/// </summary>
		/// <param name="theta">Theta vector to use in cost function calculation.</param>
		public bool IsConverged(Matrix<double> theta)
		{
			var cost = CalculateCost(theta);
			_costItems.Add(cost);

			_iterationCounter++;
			var needCheck = (_iterationCounter % _monitorStep == 0);
			if (needCheck) {
				EnsureCostDecrease();
			}

			var total = _costItems.Count;
			if (total < _monitorStep) {
				//not enough evidence
				return false;
			}

			var last = _costItems[total - 1];
			var previous = _costItems[total - 2];
			return Math.Abs(last - previous) <= _convergenceDelta;
		}

		private double CalculateCost(Matrix<double> theta)
		{
			var matrix = _input.X * theta.Transpose() - _input.Y;
			var j = matrix.Transpose() * matrix;
			if (j.ColumnCount != 1 || j.RowCount != 1) {
				_logger.Error("Failed to calculate cost function. Instead of a raw number, getting a " + j.RowCount + "x" + j.ColumnCount + " matrix.");
				return double.MaxValue;
			}
			return ((double)1 / 2 * _input.SamplesCount) * j.ToColumnWiseArray()[0];
		}

		private void EnsureCostDecrease()
		{
			int costBalance = 0;
			for (int i = _costItems.Count - 1; i > 0; i--) {
				if (i == _monitorStep) {
					break;
				}

				bool costDecreases = (_costItems[i-1] > _costItems[i]);
				costBalance += costDecreases ? (1) : (-1);
			}

			if (costBalance < 0) {
				string template = "Over the last {0} iterations the cost mostly goes up: {1}. Looks like Gradient Descent is diverging.";
				string values = string.Join(", ", _costItems.Skip(_costItems.Count - _monitorStep).Take(_monitorStep).Select( x => x.ToString("#.####")));
				var message = string.Format(template, _monitorStep, values);
				Notify(message);
			}
		}

		private void Notify(string message)
		{
			switch (_mode) {
				case NotificationMode.Throw:
					throw new NMachineException(message);
				case NotificationMode.Log:
				default:
					_logger.Warn(message);
					break;
			}
		}

		/// <summary>
		/// Notification mode of the monitor.
		/// </summary>
		internal enum NotificationMode
		{
			Throw = 0,
			Log = 1,
		}
	}
}
