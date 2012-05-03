using System;

namespace NMachine.Logging
{
	/// <summary>
	/// Logger facade to abstract off any logging framework we'd use.
	/// </summary>
	public interface ILogger
	{
		string Name { get; }
		bool IsEnabled(LogLevel level);

		void Debug(string message, Exception exception = null);
		void Info(string message, Exception exception = null);
		void Warn(string message, Exception exception = null);
		void Error(string message, Exception exception = null);
	}
}