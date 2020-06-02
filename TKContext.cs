using System;
using System.IO;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Reflection;

using static UETK7.NativeMethods;

namespace UETK7
{
    /// <summary>
    /// A class for application data, planned to be removed so ignore this completely.
    /// </summary>
    public class TKContext
    {
        public static string APPLICATION_NAME = "UETK7";
        public static string APPLICATION_AUTHOR = "Dennis Stanistan";

        /// <summary>
        /// Log things.
        /// </summary>
        public static bool DebugLogging = false;

        public const int LOG_TYPE_INFO = 1;
        public const int LOG_TYPE_WARNING = 2;
        public const int LOG_TYPE_ERROR = 3;
        public const int LOG_TYPE_DEBUG = 4;

        public static Action<string, string, int> LogAction;

        /// <summary>
        /// Get the program's current version.
        /// </summary>
        public static Version MainVersion { get { return new Version(FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location).FileVersion); } }

        public static GamePlatform CurrentPlatform { get; private set; }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string GetCurrentMethod()
        {
            var method = new StackTrace().GetFrame(2).GetMethod();
            return string.Format("{0}::{1}", method.ReflectedType.Name, method.Name);
        }

        // I really like the debug log from UASSET-Toolkit so I am using it for the context class.
        /// <summary>
        /// Outputs a debug log to the console.
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="msg"></param>
        /// <param name="color"></param>
        public static void Log(string prefix, string topic, string msg, int logType = LOG_TYPE_INFO, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            Console.WriteLine($"[{prefix} -> {topic}] " + msg);
            Console.ForegroundColor = ConsoleColor.White;

            LogAction?.Invoke($"{prefix} -> {topic}", msg, logType);
        }

        public static void DebugLog(string prefix, string topic, string msg, ConsoleColor color = ConsoleColor.White)
        {
            if (!DebugLogging)
                return;

            Log(prefix, topic, msg, LOG_TYPE_DEBUG, color);

            LogAction?.Invoke($"{prefix} -> {topic}", msg, LOG_TYPE_DEBUG);
        }

        public static void LogWarning(string msg)
        {
            Log("WARNING", GetCurrentMethod(), msg, LOG_TYPE_WARNING, ConsoleColor.Red);
        }

        public static void LogException(string msg)
        {
            Log("EXCEPTION", GetCurrentMethod(), msg, LOG_TYPE_ERROR, ConsoleColor.Red);
        }

        public static void LogError(string msg)
        {
            Log("ERROR", GetCurrentMethod(), msg, LOG_TYPE_ERROR, ConsoleColor.DarkRed);
        }

        public static void LogInner(string prefix, string msg, ConsoleColor color = ConsoleColor.White)
        {
            Log(prefix, GetCurrentMethod(), msg, LOG_TYPE_INFO, color);
        }

        public static void Throw()
        {
            #if DEBUG
            Debugger.Break();
            #endif
        }

        public static void ThrowCondition(bool isConditionMet, Action valid, Action invalid)
        {
            if (!isConditionMet)
            { 
                invalid();
                Throw();
                return;
            }

            valid();
        }
    }
}
