using System.Diagnostics;

namespace NMachine.Logging
{
	/// <summary>
	/// Log manager to abstract off any logging framework we'd use.
	/// </summary>
	public class LogManager
	{
		/// <summary>
		/// Returns a logger for the calling type. That's based on reflection, so you're better off storing the result in a static field. 
		/// </summary>
		/// <returns></returns>
		public static ILogger GetLogger()
		{
			var type = new StackTrace().GetFrame(1).GetMethod().DeclaringType.FullName;
			return new Logger(NLog.LogManager.GetLogger(type));
		}
	}
}
