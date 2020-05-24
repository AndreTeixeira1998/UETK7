using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace UETK7.Storage
{
    [Serializable]
    public class ApplicationVariables
    {
        public const string APPLICATION_VARIABLE_FILE = "variables.xml";

        public string Tekken7PCPath;
        public string Tekken7PS4Path;

        public ApplicationVariables() { }

        public static bool Exists()
        {
            return File.Exists(APPLICATION_VARIABLE_FILE);
        }

        public bool Save()
        {
            var xmlSerializer = new XmlSerializer(typeof(ApplicationVariables));
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
                    File.WriteAllText(APPLICATION_VARIABLE_FILE, stringWriter.ToString());
                }

                TKContext.LogInner("Storage", $"Saved application variables to {APPLICATION_VARIABLE_FILE}");

                return true;
            }
            catch(Exception ex)
            {
                TKContext.LogException(ex.ToString());
                return false;
            }
        }

        public static ApplicationVariables Load()
        {
            try
            {
                XmlSerializer xml = new XmlSerializer(typeof(ApplicationVariables));
                using(StreamReader sr = new StreamReader(APPLICATION_VARIABLE_FILE))
                {
                    TKContext.LogInner("Storage", $"Loaded application variables from {APPLICATION_VARIABLE_FILE}");
                    return (ApplicationVariables)xml.Deserialize(sr);
                }
            }
            catch(Exception ex)
            {
                TKContext.LogException(ex.ToString());
                return null;
            }
        }
    }
}
