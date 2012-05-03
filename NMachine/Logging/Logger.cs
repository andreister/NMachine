using System;

namespace NMachine.Logging
{
	/// <summary>
	/// Implementation of our logger facade using log4net.
	/// </summary>
	internal class Logger : ILogger
	{
		private readonly NLog.Logger _logger;

		internal Logger(NLog.Logger logger)
		{
			_logger = logger;
		}

		public string Name
		{
			get { return _logger.Name; }
		}

		public bool IsEnabled(LogLevel level)
		{
			switch (level)
			{
				case LogLevel.Debug:
					return _logger.IsDebugEnabled;
				case LogLevel.Info:
					return _logger.IsInfoEnabled;
				case LogLevel.Warn:
					return _logger.IsWarnEnabled;
				case LogLevel.Error:
					return _logger.IsErrorEnabled;
				default:
					return false;
			}
		}

		public void Debug(string message, Exception exception = null)
		{
			if (!IsEnabled(LogLevel.Debug)) return;

			_logger.Debug(message);
			if (exception != null) {
				_logger.Debug(exception);
			}
		}

		public void Info(string message, Exception exception = null)
		{
			if (!IsEnabled(LogLevel.Info)) return;

			_logger.Info(message);
			if (exception != null) {
				_logger.Info(exception);
			}
		}

		public void Warn(string message, Exception exception = null)
		{
			if (!IsEnabled(LogLevel.Warn)) return;

			_logger.Warn(message);
			if (exception != null) {
				_logger.Warn(exception);
			}
		}

		public void Error(string message, Exception exception = null)
		{
			if (!IsEnabled(LogLevel.Error)) return;

			_logger.Error(message);
			if (exception != null) {
				_logger.Error(exception);
			}
		}
	}
}