using System;
using System.Collections;
using System.Reflection;
using NMachine.Logging;

namespace NMachine.Algorithms.Extensions
{
	internal static class EnumerableConverter
	{
		private static readonly ILogger _logger = LogManager.GetLogger();

		/// <summary>
		/// Converts the given list to a two-dimensional array of doubles.
		/// </summary>
		/// <param name="list">List of elements to convert. All items must be convertible
		/// to a double, and strings are automatically converted via GetHashCode.</param>
		/// <param name="skip">Number of list elements to skip before converting.</param>
		/// <param name="take">Number of list elements to convert.</param>
		internal static double[,] ToMatrix(this IEnumerable list, int skip, int take)
		{
			var properties = GetProperties(list);
			_logger.Debug("Converting to a matrix. Type=" + list.GetType() + ", properties={" + string.Join(",", (object[])properties) + "}.");

			return ToMatrixInternal(list, properties, take, skip);
		}

		/// <summary>
		/// Converts the given list to a one-dimensional array.
		/// </summary>
		/// <param name="list">List of elements to convert. All items must be convertible
		/// to a double, and strings are automatically converted via GetHashCode.</param>
		/// <param name="skip">Number of list elements to skip before converting.</param>
		/// <param name="take">Number of list elements to convert.</param>
		internal static double[] ToVector(this IEnumerable list, int skip, int take)
		{
			var properties = GetProperties(list);
			if (properties.Length > 0) {
				throw new NMachineException("Expected a list of primitive objects - like int, double, etc.");
			}
			_logger.Debug("Converting to a vector. Type=" + list.GetType() + ".");

			return ToVectorInternal(list, take, skip);
		}

		/// <summary>
		/// Prepends the matrix with a column of 1s.
		///			| 21 22 |					| 1 21 22 |
		///	So for	| 31 32 |  it would return	| 1 31 32 |
		///			| 41 42 |					| 1 41 42 |
		/// </summary>
		internal static double[,] PrependWithOnesColumn(this double[,] matrix)
		{
			int rowsCount = matrix.GetLength(0);
			int columnsCount = matrix.GetLength(1);
			var result = new double[rowsCount, columnsCount + 1];

			for (int row = 0; row < rowsCount; row++) {
				for (int column = 0; column < columnsCount + 1; column++) {
					result[row, column] = (column == 0) ? 1 : matrix[row, column - 1];
				}
			}
			return result;
		}

		private static double[,] ToMatrixInternal(IEnumerable list, PropertyInfo[] properties, int take, int skip)
		{
			var result = new double[take, properties.Length];

			var enumerator = list.GetEnumerator();
			int row = 0;
			int skipped = 0;
			while (enumerator.MoveNext()) {
				if (skipped < skip) {
					skipped++;
					continue;
				}
				if (row == take) {
					break;
				}

				for (int column = 0; column < properties.Length; column++) {
					var value = properties[column].GetValue(enumerator.Current, null);
					var convertible = value as IConvertible;
					if (convertible != null) {
						if (convertible is string) {
							result[row, column] = convertible.GetHashCode();
						}
						else {
							result[row, column] = convertible.ToDouble(null);
						}
					}
					else {
						_logger.Warn("Failed to convert " + value + " to decimal, going to assign 0 and continue.");
						result[row, column] = 0;
					}
				}
				row++;
			}
			return result;
		}

		private static double[] ToVectorInternal(IEnumerable list, int take, int skip)
		{
			var result = new double[take];

			var enumerator = list.GetEnumerator();
			int row = 0;
			int skipped = 0;
			while (enumerator.MoveNext()) {
				if (skipped < skip) {
					skipped++;
					continue;
				}
				if (row == take) {
					break;
				}

				var value = enumerator.Current;
				var convertible = value as IConvertible;
				if (convertible != null) {
					if (convertible is string) {
						result[row] = convertible.GetHashCode();
					}
					else {
						result[row] = convertible.ToDouble(null);
					}
				}
				else {
					_logger.Warn("Failed to convert " + value + " to decimal, going to assign 0 and continue.");
					result[row] = 0;
				}
				row++;
			}
			return result;
		}

		internal static PropertyInfo[] GetProperties(IEnumerable list)
		{
			var result = new PropertyInfo[0];

			var enumerator = list.GetEnumerator();
			if (enumerator.MoveNext()) {
				result = enumerator.Current.GetType().GetProperties();
			}

			return result;
		}
	}
}
