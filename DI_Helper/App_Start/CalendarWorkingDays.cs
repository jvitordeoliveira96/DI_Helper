using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace DI_Helper {
    [Serializable]
    public class CalendarWorkingDays {
        HashSet<DateTime> Holidays;

        public CalendarWorkingDays() {
            this.Holidays = GetHolidaysSet();
        }
        
        // This static method generates a set from the hollidays.XML file
        static HashSet<DateTime> GetHolidaysSet() {
            HashSet<DateTime> HolidaySet = new HashSet<DateTime>();
            foreach (XElement level1Element in XElement.Load((HttpContext.Current.Server.MapPath("Data/hollidays.XML"))).Elements("holiday")) { 
                string holidayDate = level1Element.Attribute("date").Value;
                HolidaySet.Add(new DateTime(int.Parse(holidayDate.Substring(0, 4)), int.Parse(holidayDate.Substring(4, 2)), int.Parse(holidayDate.Substring(6, 2))));
            }
            return HolidaySet;
        }
 
        public bool isWeekend(DateTime day) {
            if((day.DayOfWeek==DayOfWeek.Saturday) || (day.DayOfWeek==DayOfWeek.Sunday)) {
                return true;
            } 
            else {
                return false;
            }   
        }
        
        // This method will count working days (no weekends or holidays) between two days.
        public int CountWorkingDays(DateTime startDay, DateTime endDay) {
            int workingdays = 0;
            for (DateTime date = startDay.AddDays(1); date <=endDay; date = date.AddDays(1)) {
                if (!(isWeekend(date)) && !(Holidays.Contains(date))) {
                    workingdays++;
                }
            }
            return workingdays;
        }
    }


}