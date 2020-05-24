using System;
using System.Text;
using System.Net;

namespace UETK7.Management
{
    public class Web
    {
        public static bool TryGetDataFromURL(string url, out string output)
        {
            try
            {
                WebClient wc = new System.Net.WebClient();
                byte[] raw = wc.DownloadData(url);

                output = Encoding.UTF8.GetString(raw);
                return true;
            }
            catch (Exception ex)
            {
                output = "";

                TKContext.LogException(ex.ToString());
                return false;
            }
        }
    }
}
