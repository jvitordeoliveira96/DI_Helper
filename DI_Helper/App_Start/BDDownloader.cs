using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Linq;
using System.Net;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;


namespace DI_Helper  {
    public static class BDDownloader  {
        public static string GetBDfromDate(DateTime calendarDate) {
            string chosenDate = calendarDate.ToShortDateString();
            chosenDate = chosenDate[8] + "" + chosenDate[9] + "" + chosenDate[3] + "" + chosenDate[4] + "" + chosenDate[0] + "" + chosenDate[1];
            WebClient webClient = new WebClient();
            // May fail to download file, exception handling is necessary
            try { 
                webClient.DownloadFile("http://www.bmf.com.br/ftp/ContratosPregaoFinal/BF" + chosenDate + ".ex_", HttpContext.Current.Server.MapPath("/Data") + "/"  + chosenDate + ".exe");
            }
            catch (WebException) {
                return "WebException ERROR";
            }
            // If downloaded successfully, run file to get the .txt file
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = HttpContext.Current.Server.MapPath("/Data") + "/" + chosenDate + ".exe";
            psi.WorkingDirectory = System.IO.Path.GetDirectoryName(psi.FileName);
            System.Diagnostics.Process unzipperApp = System.Diagnostics.Process.Start(psi);
            
            // May happen exception if .exe is already closed (weirdly, it hangs sometimes...)
            // Sleep is used to guarantee the unzip ocurred succefully
            System.Threading.Thread.Sleep(1000);
            try {
                unzipperApp.Kill();
                unzipperApp.Close();
            }
            catch (InvalidOperationException) { }

            System.Threading.Thread.Sleep(2000);
            // Renaming the .txt file to be identified by it's own date, also deleting the .exes
            File.Delete(HttpContext.Current.Server.MapPath("/Data/Temp") +  @"/BD_" + chosenDate + ".txt");
            File.Delete(HttpContext.Current.Server.MapPath("/Data") + "/" + chosenDate + ".exe");
            File.Move(HttpContext.Current.Server.MapPath("/Data") + @"/BD_Final.txt", HttpContext.Current.Server.MapPath("/Data/Temp") + @"/BD_" + chosenDate + ".txt"); // Rename the oldFileName into newFileName
            return HttpContext.Current.Server.MapPath("/Data/Temp") + @"/BD_" + chosenDate + ".txt";
        }

        public static void SaveDates(SortedDictionary<string, double> DITaxes, DateTime date) {
            string temp = date.ToShortDateString();
            string chosenDate = temp.Substring(6, 4) + temp.Substring(3, 2) + temp.Substring(0, 2);
           
            XElement xmlTree = new XElement("DITaxes");
            
            foreach (KeyValuePair<string, double> tax in DITaxes) {
                XElement xmlNode = new XElement("Date");
                xmlNode.Add(new XElement("id", tax.Key));
                xmlNode.Add(new XElement("tax", tax.Value.ToString(System.Globalization.CultureInfo.GetCultureInfo("en-Us"))));
                xmlTree.Add(xmlNode);
            }
            File.Delete(HttpContext.Current.Server.MapPath("/Data/Temp") + @"/BD_" + chosenDate.Substring(2,6) + ".txt");
            xmlTree.Save(HttpContext.Current.Server.MapPath("/Data/XML") + @"/BD_" + chosenDate + ".xml");
        }

        // The following method will verify if there is a XML associated to this date on the server already
        public static string findDateOnServer(DateTime date) {
            string temp = date.ToShortDateString();
            string chosenDate = temp.Substring(6, 4) + temp.Substring(3, 2) + temp.Substring(0, 2);

            if (File.Exists(HttpContext.Current.Server.MapPath("/Data/XML") + @"/BD_" + chosenDate + ".xml")) {
                return HttpContext.Current.Server.MapPath("/Data/XML") + @"/BD_" + chosenDate + ".xml";
            }
            else {
                return "XML NOT FOUND";
            }
        }
    }
}