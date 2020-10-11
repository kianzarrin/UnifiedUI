namespace KianCommons {
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using UnityEngine;
    using System.Linq;
    using ColossalFramework.UI;

    /// <summary>
    /// A simple logging class.
    ///
    /// When mod activates, it creates a log file in same location as `output_log.txt`.
    /// Mac users: It will be in the Cities app contents.
    /// </summary>
    public class Log {
        /// <summary>
        /// Set to <c>true</c> to include log level in log entries.
        /// </summary>
        private static readonly bool ShowLevel = true;

        /// <summary>
        /// Set to <c>true</c> to include timestamp in log entries.
        /// </summary>
        private static readonly bool ShowTimestamp = true;

        private static string assemblyName_ = Assembly.GetExecutingAssembly().GetName().Name;

        /// <summary>
        /// File name for log file.
        /// </summary>
        private static readonly string LogFileName
            = Path.Combine(Application.dataPath, assemblyName_ + ".log");

        /// <summary>
        /// Full path and file name of log file.
        /// </summary>
        private static readonly string LogFilePath = Path.Combine(Application.dataPath, LogFileName);

        /// <summary>
        /// Stopwatch used if <see cref="ShowTimestamp"/> is <c>true</c>.
        /// </summary>
        private static readonly Stopwatch Timer;

        /// <summary>
        /// Initializes static members of the <see cref="Log"/> class.
        /// Resets log file on startup.
        /// </summary>
        static Log() {
            try {
                if (File.Exists(LogFilePath)) {
                    File.Delete(LogFilePath);
                }

                if (ShowTimestamp) {
                    Timer = Stopwatch.StartNew();
                }

                AssemblyName details = typeof(Log).Assembly.GetName();
                Info($"{details.Name} v{details.Version.ToString()}", true);
            }
            catch {
                // ignore
            }
        }

        /// <summary>
        /// Log levels. Also output in log file.
        /// </summary>
        private enum LogLevel {
            Debug,
            Info,
            Error,
            Exception,
        }


        public const int MAX_WAIT_ID = 1000;
        static DateTime[] times_ = new DateTime[MAX_WAIT_ID];

        [Conditional("DEBUG")]
        public static void DebugWait(string message, int id, float seconds = 0.5f, bool copyToGameLog = true) {
            float diff = seconds + 1;
            if (id < 0) id = -id;
            id = System.Math.Abs(id % MAX_WAIT_ID);
            if (times_[id] != null) {
                var diff0 = DateTime.Now - times_[id];
                diff = diff0.Seconds;
            }
            if (diff >= seconds) {
                Log.Debug(message, copyToGameLog);
                times_[id] = DateTime.Now;
            }
        }

        [Conditional("DEBUG")]
        public static void DebugWait(string message, object id = null, float seconds = 0.5f, bool copyToGameLog = true) {
            if (id == null)
                id = Environment.StackTrace + message;
            DebugWait(message, id.GetHashCode(), seconds, copyToGameLog);

        }

        /// <summary>
        /// Logs debug trace, only in <c>DEBUG</c> builds.
        /// </summary>
        /// <param name="message">Log entry text.</param>
        /// <param name="copyToGameLog">If <c>true</c> will copy to the main game log file.</param>
        [Conditional("DEBUG")]
        public static void Debug(string message, bool copyToGameLog = true) {
            LogImpl(message, LogLevel.Debug, copyToGameLog);
        }

        /// <summary>
        /// Logs info message.
        /// </summary>
        /// 
        /// <param name="message">Log entry text.</param>
        /// <param name="copyToGameLog">If <c>true</c> will copy to the main game log file.</param>
        public static void Info(string message, bool copyToGameLog = false) {
            LogImpl(message, LogLevel.Info, copyToGameLog);
        }

        /// <summary>
        /// Logs error message and also outputs a stack trace.
        /// </summary>
        /// 
        /// <param name="message">Log entry text.</param>
        /// <param name="copyToGameLog">If <c>true</c> will copy to the main game log file.</param>
        public static void Error(string message, bool copyToGameLog = true) {
            LogImpl(message, LogLevel.Error, copyToGameLog);

        }

        internal static void Exception(Exception e, string m = "", bool showInPanel=true) {
            string message = e.ToString() + $"\n\t-- {assemblyName_}:end of inner stack trace --";
            if (!m.IsNullorEmpty())
                message = m + " -> \n" + message;
            LogImpl(message, LogLevel.Exception, true);
            if(showInPanel)
                UIView.ForwardException(e);
        }

        static string nl = Environment.NewLine;

        /// <summary>
        /// Write a message to log file.
        /// </summary>
        /// 
        /// <param name="message">Log entry text.</param>
        /// <param name="level">Logging level. If set to <see cref="LogLevel.Error"/> a stack trace will be appended.</param>
        private static void LogImpl(string message, LogLevel level, bool copyToGameLog) {
            try {
                var ticks = Timer.ElapsedTicks;
                string m = "";
                if (ShowLevel) {
                    int maxLen = Enum.GetNames(typeof(LogLevel)).Select(str => str.Length).Max();
                    m += string.Format($"{{0, -{maxLen}}}", $"[{level}] ");
                }

                if (ShowTimestamp) {
                    long secs = ticks / Stopwatch.Frequency;
                    long fraction = ticks % Stopwatch.Frequency;
                    m += string.Format($"{secs.ToString("n0")}.{fraction.ToString("D7")} | ");
                }

                m += message + nl;

                if (level == LogLevel.Error || level == LogLevel.Exception) {
                    m += new StackTrace(true).ToString() + nl + nl;
                }

                using (StreamWriter w = File.AppendText(LogFilePath)) {
                    w.Write(m);
                }

                if (copyToGameLog) {
                    m = assemblyName_ + " | " + m;
                    switch (level) {
                        case LogLevel.Error:
                        case LogLevel.Exception:
                            UnityEngine.Debug.LogError(m);
                            break;
                        default:
                            UnityEngine.Debug.Log(m);
                            break;
                    }
                }
            }
            catch {
                // ignore
            }
        }

        internal static void LogToFileSimple(string file, string message) {
            using (StreamWriter w = File.AppendText(file)) {
                w.WriteLine(message);
                w.WriteLine(new StackTrace().ToString());
                w.WriteLine();
            }
        }
    }

    internal static class LogExtensions {
        /// <summary>
        /// useful for easily debuggin inline functions
        /// to be used like this example:
        /// TYPE inlinefunctionname(...) => expression
        /// TYPE inlinefunctionname(...) => expression.LogRet("messege");
        /// </summary>
        internal static T LogRet<T>(this T a, string m) {
            Log.Debug(m + a);
            return a;
        }
    }
}