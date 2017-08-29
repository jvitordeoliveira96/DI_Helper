using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using System.Text;

namespace DI_Helper {
    public partial class _Default : Page {

        // This object is responsible for calculating and interpolating taxes, based on a certain choosenDate 
        TaxesCalculator TaxesCalc;
        // This boolean variable checks if Date (Calendar 1, Taxes) was loaded or some error ocurred
        bool DateisLoded;
        

        protected void Page_Load(object sender, EventArgs e) {
            if (!IsPostBack) {
                // Populating the DropDownList for both Calendars 
                Populate_MonthList();
                Populate_YearList();
                Populate_InterpolationTypeList();
                /* Setting the default date to today's date for both Calendars. Also, the arrow for the 
                 next month is hidden (the dropdown list already does the work better) */
                Calendar1.TodaysDate = DateTime.Today;
                Calendar1.SelectedDate = Calendar1.TodaysDate;
                Calendar1.ShowNextPrevMonth = false;
                Calendar2.TodaysDate = DateTime.Today;
                Calendar2.SelectedDate = Calendar1.TodaysDate;
                Calendar2.ShowNextPrevMonth = false;
                DateisLoded = false;
            }

            else {
                // The page has already been loaded at least one time
                if (ViewState["DateisLoaded"] != null) {
                    DateisLoded = (bool)ViewState["DateisLoaded"];
                }

                if (ViewState["TaxesCalc"] != null) {
                    TaxesCalc = (TaxesCalculator)ViewState["TaxesCalc"];
                }
            }
        }


        // Credits to the Populate_MothList and Populate_YearList: https://stackoverflow.com/questions/14379898/add-next-previous-year-button-to-asp-calendar-control
        protected void Populate_MonthList() {
            //Add each month to the list
            var dtf = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat;
            for (int i = 1; i <= 12; i++) { 
                MonthDropDownList1.Items.Add(new ListItem(dtf.GetMonthName(i), i.ToString()));
                MonthDropDownList2.Items.Add(new ListItem(dtf.GetMonthName(i), i.ToString()));
            }
            //Make the current month selected item in the list
            MonthDropDownList1.Items.FindByValue(DateTime.Now.Month.ToString()).Selected = true;
            MonthDropDownList2.Items.FindByValue(DateTime.Now.Month.ToString()).Selected = true;
        }
        
        protected void Populate_YearList() {
            /*Year list can be changed by changing the lower and upper 
             limits of the For statement */    
            for (int intYear = DateTime.Now.Year - 25; intYear <= DateTime.Now.Year; intYear++) {
                YearDropDownList1.Items.Add(intYear.ToString());
                YearDropDownList2.Items.Add(intYear.ToString());
            }
            for (int intYear = DateTime.Now.Year + 1; intYear <= DateTime.Now.Year + 30; intYear++) {
                YearDropDownList2.Items.Add(intYear.ToString());
            }
            //Make the current year selected item in the list
            YearDropDownList1.Items.FindByValue(DateTime.Now.Year.ToString()).Selected = true;
            YearDropDownList2.Items.FindByValue(DateTime.Now.Year.ToString()).Selected = true;
        }

        protected void Populate_InterpolationTypeList() {
            InterpolationTypeList.Items.Add("Linear");
            InterpolationTypeList.Items.Add("Exponencial");

            // Make linear interpolation as default
            InterpolationTypeList.Items.FindByValue("Linear").Selected = true;
        }
        
        
        protected void LoadDate_Click(object sender, EventArgs e) {
            // First check if the date is already on the server as a XML file. If thats the case, wont need to parse the whole BD txt file from
            string XMLFile = BDDownloader.findDateOnServer(Calendar1.SelectedDate);
            if (!(XMLFile.Equals("XML NOT FOUND"))) {
                // XML is on the server, only need to retrieve the taxes from the file  
                // Updating flag
                DateisLoded = true;
                // DI1 taxes calculator init
                TaxesCalc = new TaxesCalculator(XMLFile, Calendar1.SelectedDate);
                TaxesCalc.DITaxesRetrieve();
            }
            // If XML file is not found, download de BD from the internet
            else { 
                string downloadedFile = BDDownloader.GetBDfromDate(Calendar1.SelectedDate);
                // If there are results from previous iteration, clean it
                Clean_Results();

                if (downloadedFile.Equals("WebException ERROR")) {
                    // Write something to text in the middle 
                    Clean_Results();
                    TaxesTitle.Text = "<h2>" + "Não foi possível obter as taxas do dia carregado (" + Calendar1.SelectedDate.ToShortDateString() + ")  </h2>";
                    // Early return, did not succeed in getting taxes of the SelectedDate, also saving variable states
                    ViewState["DateisLoaded"] = false;
                    ViewState["TaxesCalc"] = TaxesCalc;
                    return;
                }
                else {
                    // Updating flag
                    DateisLoded = true;
                    // DI1 taxes calculator init
                    TaxesCalc = new TaxesCalculator(downloadedFile, Calendar1.SelectedDate);
                    TaxesCalc.DITaxesCalculator();
                }
            }
            // Writing the Taxes table
            TaxesTitle.Text = "<h2>" + "Taxas DI de " + Calendar1.SelectedDate.ToShortDateString() + "</h2>";
            StringBuilder TaxesListTemp = new StringBuilder();
            TaxesListTemp.Append("<div id = \"DITaxesTable\" style=\"overflow: auto; max-height: 19em;\"> <table class=\"table table-bordered\"><thead> <tr> <th>Data</th><th>Taxa DI</th></tr></thead><tbody>");
            foreach (KeyValuePair<string, double> tax in TaxesCalc.GetDITaxes()) {
                TaxesListTemp.Append("<tr><td>" + (tax.Key.Substring(6, 2) + @"/" + tax.Key.Substring(4, 2) + @"/" + tax.Key.Substring(0, 4)) + "</td> <td>" + (tax.Value).ToString("P") + " </td></tr>");
            }
            TaxesListTemp.Append("</tbody></table></div>");
            TaxesList.Text = TaxesListTemp.ToString();
            
            // Saving the variables values between Sessions
            ViewState["DateisLoaded"] = DateisLoded;
            ViewState["TaxesCalc"] = TaxesCalc;
         }

        protected void CalculateTax_Click(object sender, EventArgs e) {   
            // If the Taxes associated with some choosen date are still not loaded, write error message.
            if (!(DateisLoded)) {
                TaxesTitle.Text = "<h2> É necessário carregar as taxas de alguma data válida. </h2>";
            }

            else {
                Dictionary<string, Object> Results;
                if(InterpolationTypeList.SelectedValue.Equals("Linear")) {
                    Results = TaxesCalc.DIInterpolation(Calendar2.SelectedDate);
                }
                else {
                    Results = TaxesCalc.DIInterpolation(Calendar2.SelectedDate, "Exponencial");
                }

                // Add default value
                if (String.IsNullOrEmpty(InterestRateShock.Text)) {
                    InterestRateShock.Text = "0,0";
                }
                
                if ((((int)Results["workingDaysTotal"])) > 0) { 
                     FinalResults.Text = "<p> Taxa interpolada: " + ((double)Results["interpolatedTax"] + (0.01*(double.Parse(InterestRateShock.Text)))).ToString("P") + "</p> <p> Dias úteis entre data da curva e dia selecionado:" + ((int)Results["workingDaysTotal"]) + "</p>";
                     GenerateGraph(InterpolationTypeList.SelectedValue);
                }
                    
                else {
                     FinalResults.Text = "<p> A data selecionada a ser interpolada não é válida. Tente novamente com alguma outra. </p>";
                     TaxPlotly.Text = "";
                }
            }
        }
       
        // This and the following method are used to hide the dates from other months (this is default on ASP.Net Calendars, weirdly)
        protected void Calendar1_DayRender(object sender, DayRenderEventArgs e) {
            if (e.Day.IsOtherMonth) {
                e.Cell.Controls.Clear();
                e.Cell.Text = string.Empty;
            }
        }

        protected void Calendar2_DayRender(object sender, DayRenderEventArgs e) {
            if (e.Day.IsOtherMonth) {
                e.Cell.Controls.Clear();
                e.Cell.Text = string.Empty;
            }
        }

        protected void Update_Calendar1(object sender, EventArgs e) {
            // This method updates the Calendar with the selected dropdown Month/Year (Calendar 1)
            int year = int.Parse(YearDropDownList1.SelectedValue);
            int month = int.Parse(MonthDropDownList1.SelectedValue);
            Calendar1.TodaysDate = new DateTime(year, month, 1);
            Calendar1.SelectedDate = Calendar1.TodaysDate;
        }

        protected void Update_Calendar2(object sender, EventArgs e) {
            // This method updates the Calendar with the selected dropdown Month/Year (Calendar 2)
            int year = int.Parse(YearDropDownList2.SelectedValue);
            int month = int.Parse(MonthDropDownList2.SelectedValue);
            Calendar2.TodaysDate = new DateTime(year, month, 1);
            Calendar2.SelectedDate = Calendar2.TodaysDate;
        }

        protected void GenerateGraph(string interpolationType) {
            // This method writes the plot of the curve on the Web Page, using the plotly library
            StringBuilder scriptPlotlyJS = new StringBuilder();
            scriptPlotlyJS.AppendLine("<script>");
            scriptPlotlyJS.AppendLine("var taxCurve = [");
            scriptPlotlyJS.AppendLine("{");
            scriptPlotlyJS.Append("x : [");
            foreach (KeyValuePair<string, double> tax in TaxesCalc.GetDITaxes()) {
                if (interpolationType.Equals("Linear")) {
                    string plotlyDate = "'" + tax.Key.Substring(0, 4) + "-" + tax.Key.Substring(4, 2) + "-" + tax.Key.Substring(6, 2) + " 10:00:00', ";
                    scriptPlotlyJS.Append(plotlyDate);
                }
                else {
                    // Adding more dates to the plot (the ones between will be interpolated), in order to have a more smooth plot
                    // If it is the last one, there is no need to interpolate (it will be an error)
                    if (TaxesCalc.GetDITaxes().Keys.Last() != tax.Key) {
                        DateTime iterDate = new DateTime(int.Parse(tax.Key.Substring(0, 4)), int.Parse(tax.Key.Substring(4, 2)), int.Parse(tax.Key.Substring(6, 2)));
                        DateTime lastAuxInterpolatedDate = iterDate.AddDays(24);
                        for (iterDate = iterDate.AddDays(1); iterDate <= lastAuxInterpolatedDate; iterDate = iterDate.AddDays(1)) {
                            string temp = iterDate.ToShortDateString();
                            string plotlyDate = "'" + temp.Substring(6, 4) + "-" + temp.Substring(3, 2) + "-" + temp.Substring(0, 2) + " 10:00:00', ";
                            scriptPlotlyJS.Append(plotlyDate);
                        }
                    }
                    else {
                        DateTime lastDate = new DateTime(int.Parse(tax.Key.Substring(0, 4)), int.Parse(tax.Key.Substring(4, 2)), int.Parse(tax.Key.Substring(6, 2)));
                        string temp = lastDate.ToShortDateString();
                        string plotlyDate = "'" + temp.Substring(6, 4) + "-" + temp.Substring(3, 2) + "-" + temp.Substring(0, 2) + " 10:00:00', ";
                        scriptPlotlyJS.Append(plotlyDate);
                    }
                    
                }
               
            }
            scriptPlotlyJS.Remove(scriptPlotlyJS.Length - 2, 2);
            scriptPlotlyJS.AppendLine("],");
            scriptPlotlyJS.Append("y: [");
            foreach (KeyValuePair<string, double> tax in TaxesCalc.GetDITaxes()) {
                if(interpolationType.Equals("Linear")) {
                    string plotlyDate = ((tax.Value) * 100 + double.Parse(InterestRateShock.Text)).ToString(System.Globalization.CultureInfo.GetCultureInfo("en-Us")) + ", ";
                    scriptPlotlyJS.Append(plotlyDate);
                }
                else {
                    // The taxes must be recalculated. Will calculate in the new dates inside the interval, to make the exponential smooth
                    // If it is the last one, there is no need to interpolate (it will be an error)
                    if ( ((double)TaxesCalc.GetDITaxes().Values.Last()) != ((double) tax.Value)) { 
                        DateTime iterDate = new DateTime(int.Parse(tax.Key.Substring(0, 4)),int.Parse(tax.Key.Substring(4, 2)),int.Parse(tax.Key.Substring(6, 2)));
                        DateTime lastAuxInterpolatedDate = iterDate.AddDays(24);
                        for (iterDate = iterDate.AddDays(1); iterDate <= lastAuxInterpolatedDate; iterDate = iterDate.AddDays(1)) {
                            Dictionary<string, Object> resultTax = TaxesCalc.DIInterpolation(iterDate, interpolationType);
                            string plotlyDate = (((double)resultTax["interpolatedTax"]) * 100 + double.Parse(InterestRateShock.Text)).ToString(System.Globalization.CultureInfo.GetCultureInfo("en-Us")) + ", ";
                            scriptPlotlyJS.Append(plotlyDate);
                        }
                    }
                    else {
                        DateTime lastDate = new DateTime(int.Parse(tax.Key.Substring(0, 4)), int.Parse(tax.Key.Substring(4, 2)), int.Parse(tax.Key.Substring(6, 2)));
                        Dictionary<string, Object> resultTax = TaxesCalc.DIInterpolation(lastDate, interpolationType);
                        string plotlyDate = (((double)resultTax["interpolatedTax"]) * 100 + double.Parse(InterestRateShock.Text)).ToString(System.Globalization.CultureInfo.GetCultureInfo("en-Us")) + ", ";
                        scriptPlotlyJS.Append(plotlyDate);
                    }
                }   
            }
            scriptPlotlyJS.Remove(scriptPlotlyJS.Length - 2, 2);
            scriptPlotlyJS.AppendLine("],");
            scriptPlotlyJS.AppendLine("type: 'scatter'");
            scriptPlotlyJS.AppendLine("}");
            scriptPlotlyJS.AppendLine("];");
            scriptPlotlyJS.AppendLine("var layout = {");
            scriptPlotlyJS.AppendLine("title: 'Curva de juros (Taxas DI) de "+ Calendar1.SelectedDate.ToShortDateString() + "',");
            scriptPlotlyJS.AppendLine("xaxis: {title: 'Data'}, yaxis: { title: 'Taxa (%)'}");
            scriptPlotlyJS.AppendLine("}");
            scriptPlotlyJS.AppendLine("Plotly.newPlot('plotDiv', taxCurve, layout);");
            scriptPlotlyJS.AppendLine("</script>");
            TaxPlotly.Text = scriptPlotlyJS.ToString();
        }

        protected void InterpolationType_Change(object sender, EventArgs e) {
            FinalResults.Text = "";
            TaxPlotly.Text = "";
        }

        // Clean All fields
        protected void Clean_Results() {
            FinalResults.Text = "";
            TaxPlotly.Text = "";
            TaxesTitle.Text = "";
            InterestRateShock.Text = "";
        }
    }
}