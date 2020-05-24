using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Microsoft.Win32;
namespace UETK7.Style
{
    public static class Styling
    {
        public static readonly StyleTheme Default = new StyleTheme("Light", Color.Gray, Color.White, Color.White, Color.Black);
        public static readonly StyleTheme Dark = new StyleTheme("Dark", Color.FromArgb(19, 19, 19), Color.White, Color.FromArgb(31, 31, 31), Color.White);
        public static readonly StyleTheme SmartDark = new StyleTheme("Smart Dark", Color.FromArgb(26, 29, 36), Color.FromArgb(118, 127, 136), Color.FromArgb(31, 40, 49), Color.FromArgb(118, 127, 136));

        public static StyleTheme Windows()
        {
            Color windowsColor = ManagedMethods.GetWindowColorizationColor(true);
            StyleTheme Windows = new StyleTheme();
            Windows.Name = "Windows";
            Windows.MajorColor = windowsColor;
            Windows.MinorColor = GetForegroundForColor(windowsColor);
            Windows.BackgroundColor = IsWindowsDarkMode() ? Color.FromArgb(37, 37, 37) : Color.White;
            Windows.ForegroundColor = IsWindowsDarkMode() ? Color.White : Color.Black;

            return Windows;
        }


        public static bool DoesSupportWindowsStyling()
        {
            var reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");

            string productName = (string)reg.GetValue("ProductName");
            if (!productName.StartsWith("Windows 10"))
            {
                TKContext.DebugLog("INFO", "Operating System", $"{productName} does not support Windows visual style theme.");
                return false;
            }

            try
            {
                int releaseId = int.Parse(Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ReleaseId", "").ToString());
                if (releaseId >= 1809)
                {
                    TKContext.DebugLog("INFO", "Machine Environment", $"{productName} build {releaseId} supports Windows visual style theme.");
                    return true;
                }
            }
            catch
            {
                TKContext.DebugLog("INFO", "Machine Environment", $"Error getting the operating system's release id.");
                return false;
            }

            return false;
        }

        public static bool IsWindowsDarkMode()
        {
            string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath))
            {
                object registryValueObject = key?.GetValue("AppsUseLightTheme");
                if (registryValueObject == null)
                {
                    return false;
                }

                int registryValue = (int)registryValueObject;

                return registryValue > 0 ? false : true;
            }
        }

        public static Color GetForegroundForColor(Color color)
        {
            if (color.GetBrightness() > 0.7)
            {
                return Color.Black;
            }

            return Color.White;
        }

        public static System.Windows.Media.Color GetMediaColor(Color color)
        {
            var col = new System.Windows.Media.Color();
            col.R = color.R;
            col.G = color.G;
            col.B = color.B;
            col.A = color.A;
            return col;
        }
    }

    [Serializable]
    public class StyleTheme
    {
        public string Name;

        public Color MajorColor;
        public Color MinorColor;

        public Color BackgroundColor;
        public Color ForegroundColor;

        public StyleTheme() { }

        public StyleTheme(string Name, Color majorColor, Color minorColor, Color bgColor, Color fgColor)
        {
            this.Name = Name;
            MajorColor = majorColor;
            MinorColor = minorColor;
            BackgroundColor = bgColor;
            ForegroundColor = fgColor;
        }

        public bool Save(string filename)
        {
            var xmlSerializer = new XmlSerializer(typeof(StyleTheme));
            var stringWriter = new StringWriter();
            try
            {
                var settings = new XmlWriterSettings()
                {
                    Indent = true
                };

                using (var writer = XmlWriter.Create(stringWriter, settings))
                {

                    xmlSerializer.Serialize(writer, this);
                    File.WriteAllText(filename, stringWriter.ToString());
                }

                TKContext.LogInner("Storage", $"Saved style theme to {filename}");

                return true;
            }
            catch (Exception ex)
            {
                TKContext.LogInner("EXCEPTION", ex.ToString(), ConsoleColor.DarkRed);
                return false;
            }
        }

        public static StyleTheme Load(string filename)
        {
            try
            {
                XmlSerializer xml = new XmlSerializer(typeof(StyleTheme));
                using (StreamReader sr = new StreamReader(filename))
                {
                    TKContext.LogInner("Storage", $"Loaded style theme from {filename}");
                    return (StyleTheme)xml.Deserialize(sr);
                }
            }
            catch (Exception ex)
            {
                TKContext.LogException(ex.ToString());
                return null;
            }
        }
    }
}
