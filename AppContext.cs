using System;
using System.IO;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Reflection;
using UETK7.Storage;

using static UETK7.NativeMethods;

namespace UETK7
{
    public class TKContext
    {
        public static string APPLICATION_NAME = "UETK7";
        public static string APPLICATION_AUTHOR = "Dennis Stanistan";
        public const string TEMP_PATH = "temp";

        public static bool DebugLogging = false;

        public const int LOG_TYPE_INFO = 1;
        public const int LOG_TYPE_WARNING = 2;
        public const int LOG_TYPE_ERROR = 3;
        public const int LOG_TYPE_DEBUG = 4;

        public static Action<string, string, int> LogAction;

        public static bool IsInitialized { get; private set; }

        /// <summary>
        /// Get the program's current version.
        /// </summary>
        public static Version MainVersion { get { return new Version(FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location).FileVersion); } }

        public static GamePlatform CurrentPlatform { get; private set; }

        private static Version _CachedServerVersion;

        public static ApplicationVariables AppVariables;

        /// <summary>
        /// Initializes the application context.
        /// </summary>
        public static void Initialize(bool showConsole = true)
        {
            // Allocate the console
            if(showConsole)
                AllocConsole();

            LogInner("INFO", "Initializing TKContext...");

            Log("INFO", "Application", $"{APPLICATION_NAME} {MainVersion} by {APPLICATION_AUTHOR}");

            if(showConsole)
                Log("INFO", "Application", "Closing this console window WILL CLOSE the application and all of it's windows.", LOG_TYPE_WARNING, ConsoleColor.DarkRed);

            if(ApplicationVariables.Exists())
            {
                AppVariables = ApplicationVariables.Load();
            }
            else { 
                // TODO: load a window that asks for these variables
                AppVariables = new ApplicationVariables();
                AppVariables.Tekken7PCPath = @"";
                AppVariables.Tekken7PS4Path = @"";

                AppVariables.Save();

                LogInner("Info", $"Created {ApplicationVariables.APPLICATION_VARIABLE_FILE}", ConsoleColor.White);
            }

            IsInitialized = true;
        }

        /// <summary>
        /// Initializes the application's workspace.
        /// </summary>
        public static void InitializeWorkspace()
        {
            Workspace.LoadedPackages = new List<UnrealEngine.Package>();

            Workspace.IsInitialized = true;
        }

        public static bool ArePathsInvalid()
        {
            return string.IsNullOrEmpty(AppVariables.Tekken7PCPath) && string.IsNullOrEmpty(AppVariables.Tekken7PS4Path);
        }

        public static void ClearTempData()
        {
            if (Directory.Exists("temp"))
            {
                try
                {
                    Directory.Delete("temp");
                    LogInner("INFO", "Temp directory cleared.");
                }
                catch(Exception ex)
                {
                    LogException(ex.ToString());
                    LogError("Could not delete the /temp directory.");
                }
            }
        }

        public static bool IsPCPathValid()
        {
            if (string.IsNullOrEmpty(AppVariables.Tekken7PCPath))
                return false;

            if (!File.Exists(Path.Combine(AppVariables.Tekken7PCPath, "TEKKEN 7.exe")))
                return false;

            if (!Directory.Exists(Path.Combine(AppVariables.Tekken7PCPath, "TekkenGame")))
                return false;

            if (!File.Exists(Path.Combine(AppVariables.Tekken7PCPath, @"TekkenGame\Binaries\Win64\TekkenGame-Win64-Shipping.exe")))
                return false;

            if (!Directory.Exists(Path.Combine(AppVariables.Tekken7PCPath, "Engine")))
                return false;

            return true;
        }

        public static bool IsPS4PathValid()
        {
            if (string.IsNullOrEmpty(AppVariables.Tekken7PS4Path))
                return false;

            if (!Directory.Exists(Path.Combine(AppVariables.Tekken7PS4Path, "Image0")))
                return false;

            if (!File.Exists(Path.Combine(AppVariables.Tekken7PS4Path, @"Image0\eboot.bin")))
                return false;

            if (!Directory.Exists(Path.Combine(AppVariables.Tekken7PS4Path, "Sc0")))
                return false;

            return true;
        }

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

        public static class Workspace
        {
            public static List<UnrealEngine.Package> LoadedPackages { get; internal set; }

            public static bool IsInitialized { get; internal set; }

            public static void LoadPackage(string pakFile)
            {
                ThrowCondition(IsInitialized, (() => {
                    // TODO: add ignore magic boolean preference variable
                    var package = new UnrealEngine.Package(pakFile, false);
                    package.Read();
                    LoadedPackages.Add(package);
                }), (() => {
                    LogError("Could not load a package because the workspace hasn't been initialized yet.");
                }));               
            }
        }
    }

    public static class ApplicationReflection
    {
        public static string GetMemberName<T, TValue>(Expression<Func<T, TValue>> memberAccess)
        {
            return ((MemberExpression)memberAccess.Body).Member.Name;
        }
    }
}
