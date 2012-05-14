using System.Collections;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Generic;
using NMachine.Algorithms.Extensions;

namespace NMachine.Algorithms
{
	/// <summary>
	/// Represents traning, cross-validation, or test set.
	/// </summary>
	public class Input
	{
		/// <summary>
		/// Matrix of features. Each column is a feature, each row is a new sample
		/// </summary>
		internal Matrix<double> X { get; set; }

		/// <summary>
		/// Vector of the outputs for the above matrix of features. Each element
		/// in the vector corresponds to a row (ie, sample) in the matrix.
		/// </summary>
		internal Matrix<double> Y { get; set; }

		/// <summary>
		/// Creates a new Input from double values.
		/// Note that we store each example as a row in the X matrix. While calculating Theta vector, we need to insert the top column of all ones into the X matrix - this will allow us to treat theta0 as just another feature.
		/// </summary>
		internal Input(double[,] x, double[] y, int skip, int take)
		{
			if (take == 0) {
				X = null;
				Y = null;
				return;
			}

			var samples = x.GetLength(0);
			var features = x.GetLength(1);

			//make sure we add first column of ones
			var x1 = new double[take, features + 1];
			var y1 = new double[take];

			for (int sample = 0; sample < samples; sample++) {
				if (sample < skip) {
					continue;
				}
				for (int feature = 0; feature < features + 1; feature++) {
					x1[sample - skip, feature] = (feature == 0) ? 1 : x[sample, feature - 1];
				}
				y1[sample - skip] = y[sample];

				take--;
				if (take == 0) {
					break;
				}
			}

			X = new DenseMatrix(x1);
			Y = new DenseVector(y1).ToColumnMatrix();
		}

		/// <summary>
		/// Creates a new Input from objects.
		/// Note that we store each example as a row in the X matrix. While calculating Theta vector, we need to insert the top column of all ones into the X matrix - this will allow us to treat theta0 as just another feature.
		/// </summary>
		internal Input(IEnumerable x, IEnumerable y, int skip, int take)
		{
			if (take == 0) {
				X = null;
				Y = null;
			}
			else {
				X = new DenseMatrix(x.ToMatrix(skip, take).PrependWithOnesColumn());
				Y = new DenseVector(y.ToVector(skip, take)).ToColumnMatrix();
			}
		}

		internal int FeaturesCount
		{
			get { return X.ColumnCount; }
		}

		internal int SamplesCount
		{
			get { return X.RowCount; }
		}
	}
}
