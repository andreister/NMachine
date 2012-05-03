using System;
using System.Runtime.Serialization;

namespace NMachine
{
	public class NMachineException : Exception
	{
		public NMachineException(string message) : base(message)
		{
		}

		public NMachineException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected NMachineException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}