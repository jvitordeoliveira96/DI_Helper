using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using System.Xml.Linq;

namespace DI_Helper {
    [Serializable]
     class TaxesCalculator {
        string bdfilePath;
        SortedDictionary<string, double> DITaxes;
        CalendarWorkingDays calendarWorkingDays;
        DateTime curveDate;
         

        public TaxesCalculator(string bdfilePath, DateTime curveDate) {
            this.bdfilePath = bdfilePath;
            this.DITaxes = new SortedDictionary<string,double>();
            this.calendarWorkingDays = new CalendarWorkingDays();
            this.curveDate = curveDate;
        }
        
        public SortedDictionary<string, double>  GetDITaxes() {
            return DITaxes;
        }

        public void DITaxesCalculator() {
            // Define flag to stop reading the rest of the document after DI1 entries are read
            bool entriesFlagDI1 = false;
            foreach (string line in File.ReadLines(bdfilePath)) {
                if (line.Substring(21,3).Equals("DI1")) {
                    entriesFlagDI1 = true;
                    // Now calculating each tax
                    string taxDate = line.Substring(36, 8);
                    int cotationAj = int.Parse(line.Substring(231, 13));
                    int workingDays = int.Parse(line.Substring(388,5));
                    double DITax = Math.Pow((10000000.0 / (cotationAj)), (252.0 / workingDays)) - 1;
                    DITaxes.Add(taxDate,DITax);
                }
                else {
                    if (entriesFlagDI1)
                        // There are no more DI1 entries to read on the file, interrupt in order to have better performance
                        break;
                }
            }
            // Saving the computed results on a XML file for faster retrieval if the same Date is requested later
            BDDownloader.SaveDates(DITaxes, curveDate);
            
        }


        // If type not specified, do linear interpolation
        public Dictionary<string,Object> DIInterpolation(DateTime calendarDate) {
            string temp = calendarDate.ToShortDateString();
            string interpolatedDate = temp.Substring(6,4) + temp.Substring(3,2) + temp.Substring(0,2);
            string intervalStartDate = "", intervalEndDate = "";
            double taxStartDate = 0, taxEndDate = 0;
            // This variable will have the interpolated tax and the   
            Dictionary<string, Object> Results = new Dictionary<string, Object>();
            // Searching for where the date to be interpolated lies (using the fact that SortedDictionary has it's keys ordered
            foreach (KeyValuePair<string, double> tax in DITaxes) {
                if (tax.Key.CompareTo(interpolatedDate)<0) {
                    intervalStartDate = tax.Key;
                    taxStartDate = tax.Value;
                }
                else {
                    intervalEndDate = tax.Key;
                    taxEndDate = tax.Value;
                    break;
                }
            }
            // Check if the StartDate and EndDate are set, which is the case for a valid InterpolatedDay 
            if (!(String.IsNullOrEmpty(intervalStartDate)) && !(String.IsNullOrEmpty(intervalEndDate))) {
                int workingDaysInterval = calendarWorkingDays.CountWorkingDays(new DateTime(int.Parse(intervalStartDate.Substring(0, 4)), int.Parse(intervalStartDate.Substring(4, 2)), int.Parse(intervalStartDate.Substring(6, 2))), new DateTime(int.Parse(intervalEndDate.Substring(0, 4)), int.Parse(intervalEndDate.Substring(4, 2)), int.Parse(intervalEndDate.Substring(6, 2))));
                int workingDaysInterpolatedDay = calendarWorkingDays.CountWorkingDays(new DateTime(int.Parse(intervalStartDate.Substring(0, 4)), int.Parse(intervalStartDate.Substring(4, 2)), int.Parse(intervalStartDate.Substring(6, 2))), new DateTime(int.Parse(interpolatedDate.Substring(0, 4)), int.Parse(interpolatedDate.Substring(4, 2)), int.Parse(interpolatedDate.Substring(6, 2))));
                // Computing an aproximated tax using linear interpolation 
                double interpolatedTax = taxEndDate * (workingDaysInterpolatedDay / (1.0 * workingDaysInterval)) + taxStartDate * ((workingDaysInterpolatedDay - workingDaysInterval) / (-1.0 * workingDaysInterval));
                // Computing the total of working days between the Calendar 1 (curve) date and the interpolated Day
                int workingDaysTotal = calendarWorkingDays.CountWorkingDays(curveDate, new DateTime(int.Parse(interpolatedDate.Substring(0, 4)), int.Parse(interpolatedDate.Substring(4, 2)), int.Parse(interpolatedDate.Substring(6, 2))));  
                // Adding the variables to the Results dictionary
                Results.Add("interpolatedTax", interpolatedTax);
                Results.Add("workingDaysTotal", workingDaysTotal);
            }
            else {
                Results.Add("workingDaysTotal", -1);
            }
            return Results;
        }

        public Dictionary<string, Object> DIInterpolation(DateTime calendarDate , string interpolationType) {
            if (interpolationType.Equals("Linear")) {
                // Using the initial/default method definition
                return DIInterpolation(calendarDate);
            }

            else {

                string temp = calendarDate.ToShortDateString();
                string interpolatedDate = temp.Substring(6, 4) + temp.Substring(3, 2) + temp.Substring(0, 2);
                string intervalStartDate = "", intervalEndDate = "";
                double taxStartDate = 0, taxEndDate = 0;
                // This variable will have the interpolated tax and the   
                Dictionary<string, Object> Results = new Dictionary<string, Object>();
                // Searching for where the date to be interpolated lies (using the fact that SortedDictionary has it's keys ordered
                foreach (KeyValuePair<string, double> tax in DITaxes) {
                    if (tax.Key.CompareTo(interpolatedDate) < 0) {
                        intervalStartDate = tax.Key;
                        taxStartDate = Math.Log(1 + tax.Value * (30.0/360.0)) ;
                    }
                    else {
                        intervalEndDate = tax.Key;
                        taxEndDate = Math.Log(1 + tax.Value * (30.0 / 360.0));
                        break;
                    }
                }
                // Check if the StartDate and EndDate are set, which is the case for a valid InterpolatedDay 
                if (!(String.IsNullOrEmpty(intervalStartDate)) && !(String.IsNullOrEmpty(intervalEndDate))) {
                    int workingDaysInterval = calendarWorkingDays.CountWorkingDays(new DateTime(int.Parse(intervalStartDate.Substring(0, 4)), int.Parse(intervalStartDate.Substring(4, 2)), int.Parse(intervalStartDate.Substring(6, 2))), new DateTime(int.Parse(intervalEndDate.Substring(0, 4)), int.Parse(intervalEndDate.Substring(4, 2)), int.Parse(intervalEndDate.Substring(6, 2))));
                    int workingDaysInterpolatedDay = calendarWorkingDays.CountWorkingDays(new DateTime(int.Parse(intervalStartDate.Substring(0, 4)), int.Parse(intervalStartDate.Substring(4, 2)), int.Parse(intervalStartDate.Substring(6, 2))), new DateTime(int.Parse(interpolatedDate.Substring(0, 4)), int.Parse(interpolatedDate.Substring(4, 2)), int.Parse(interpolatedDate.Substring(6, 2))));
                    
                    // Computing an aproximated Discount using log-linear interpolation 
                    double interpolatedDiscount = taxEndDate * (workingDaysInterpolatedDay / (1.0 * workingDaysInterval)) + taxStartDate * ((workingDaysInterpolatedDay - workingDaysInterval) / (-1.0 * workingDaysInterval));
                    // To get a tax once again, we have to exponentiate de previous results
                    double interpolatedTax = (Math.Exp(interpolatedDiscount) - 1) * (360.0 / 30.0);
                    // Computing the total of working days between the Calendar 1 (curve) date and the interpolated Day
                    int workingDaysTotal = calendarWorkingDays.CountWorkingDays(curveDate, new DateTime(int.Parse(interpolatedDate.Substring(0, 4)), int.Parse(interpolatedDate.Substring(4, 2)), int.Parse(interpolatedDate.Substring(6, 2))));
                    // Adding the variables to the Results dictionary
                    Results.Add("interpolatedTax", interpolatedTax);
                    Results.Add("workingDaysTotal", workingDaysTotal);
                }
                else {
                    Results.Add("workingDaysTotal", -1);
                }
                return Results;
            }   
        }
        
        public void DITaxesRetrieve() {
            foreach (XElement level1Element in XElement.Load(bdfilePath).Elements("Date")) {
                DITaxes.Add(level1Element.Element("id").Value, double.Parse(level1Element.Element("tax").Value, System.Globalization.CultureInfo.GetCultureInfo("en-Us")));
            }
        }
     }
}