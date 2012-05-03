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
		/// Creates a new Input.
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
