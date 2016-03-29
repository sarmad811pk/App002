using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace SurveyApp.Models
{
    public class ScheduleContext : DbContext
    {
        public ScheduleContext()
            : base("DefaultConnection")
        {
        }

        public DbSet<Schedule> Schedules { get; set; }
    }
    [Table("Schedule")]
    public class Schedule
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required(ErrorMessage = "Please enter title")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Please select frequency")]
        public int Frequency { get; set; }
        public int? DaysToRepeat { get; set; }
        [Required(ErrorMessage = "Please select activation type")]
        public int ActiveOn { get; set; }
        public int? Day { get; set; }
        public string Month { get; set; }
        public int? Weekday { get; set; }
        [Required(ErrorMessage = "Please enter no. of days for schedule availability")]
        public int AvailableUntil { get; set; }
        [Required(ErrorMessage = "Please enter no. of days for reminder frequency")]
        public int ReminderFrequency { get; set; }
        [Required(ErrorMessage = "Please enter no. of days for last reminder")]
        public int LastReminder { get; set; }
    }

    public class ScheduleDay
    {
        public int Id { get; set; }
        public int Value { get; set; }

        public static List<ScheduleDay> GetScheduleDays()
        {
            List<ScheduleDay> Days = new List<ScheduleDay>();
            for (int i = 1; i <= 31; i++)
            {
                Days.Add(new ScheduleDay { Id = i, Value = i });
            }

            return Days;
        }
    }

    public class ScheduleMonth
    {
        public int Id { get; set; }
        public string Month { get; set; }
        public string MonthShort { get; set; }

        public static List<ScheduleMonth> GetScheduleMonths()
        {
            List<ScheduleMonth> objScheduleMonth = new List<ScheduleMonth>();

            objScheduleMonth.Add(new ScheduleMonth { Id = 1, Month = "January", MonthShort = "Jan" });
            objScheduleMonth.Add(new ScheduleMonth { Id = 2, Month = "February", MonthShort = "Feb" });
            objScheduleMonth.Add(new ScheduleMonth { Id = 3, Month = "March", MonthShort = "Mar" });
            objScheduleMonth.Add(new ScheduleMonth { Id = 4, Month = "April", MonthShort = "Apr" });

            objScheduleMonth.Add(new ScheduleMonth { Id = 5, Month = "May", MonthShort = "May" });
            objScheduleMonth.Add(new ScheduleMonth { Id = 6, Month = "June", MonthShort = "Jun" });
            objScheduleMonth.Add(new ScheduleMonth { Id = 7, Month = "July", MonthShort = "Jul" });
            objScheduleMonth.Add(new ScheduleMonth { Id = 8, Month = "August", MonthShort = "Aug" });

            objScheduleMonth.Add(new ScheduleMonth { Id = 9, Month = "September", MonthShort = "Sep" });
            objScheduleMonth.Add(new ScheduleMonth { Id = 10, Month = "October", MonthShort = "Oct" });
            objScheduleMonth.Add(new ScheduleMonth { Id = 11, Month = "November", MonthShort = "Nov" });
            objScheduleMonth.Add(new ScheduleMonth { Id = 12, Month = "December", MonthShort = "Dec" });

            return objScheduleMonth;
        }

        public static string getMonthName(int month)
        {
            if (month == 1) { return "January"; }
            if (month == 2) { return "February"; }
            if (month == 3) { return "March"; }
            if (month == 4) { return "April"; }

            if (month == 5) { return "May"; }
            if (month == 6) { return "June"; }
            if (month == 7) { return "July"; }
            if (month == 8) { return "August"; }

            if (month == 9) { return "September"; }
            if (month == 10) { return "October"; }
            if (month == 11) { return "November"; }
            if (month == 12) { return "December"; }

            return "";
        }

        public static string getMonthNameShort(int month)
        {
            if (month == 1) { return "Jan"; }
            if (month == 2) { return "Feb"; }
            if (month == 3) { return "Mar"; }
            if (month == 4) { return "Apr"; }

            if (month == 5) { return "May"; }
            if (month == 6) { return "Jun"; }
            if (month == 7) { return "Jul"; }
            if (month == 8) { return "Aug"; }

            if (month == 9) { return "Sep"; }
            if (month == 10) { return "Oct"; }
            if (month == 11) { return "Nov"; }
            if (month == 12) { return "Dec"; }

            return "";
        }
    }

    public class ScheduleWeekday
    {
        public int Id { get; set; }
        public string Weekday { get; set; }
        public string WeekdayShort { get; set; }

        public static List<ScheduleWeekday> GetScheduleWeekdays()
        {
            List<ScheduleWeekday> objScheduleWeekday = new List<ScheduleWeekday>();

            objScheduleWeekday.Add(new ScheduleWeekday { Id = 1, Weekday = "Monday", WeekdayShort = "Mon" });
            objScheduleWeekday.Add(new ScheduleWeekday { Id = 2, Weekday = "Tuesday", WeekdayShort = "Tue" });
            objScheduleWeekday.Add(new ScheduleWeekday { Id = 3, Weekday = "Wednesday", WeekdayShort = "Wed" });
            objScheduleWeekday.Add(new ScheduleWeekday { Id = 4, Weekday = "Thursday", WeekdayShort = "Thu" });

            objScheduleWeekday.Add(new ScheduleWeekday { Id = 5, Weekday = "Friday", WeekdayShort = "Fri" });
            objScheduleWeekday.Add(new ScheduleWeekday { Id = 6, Weekday = "Saturday", WeekdayShort = "Sat" });
            objScheduleWeekday.Add(new ScheduleWeekday { Id = 7, Weekday = "Sunday", WeekdayShort = "Sun" });

            return objScheduleWeekday;
        }

        public static string getWeekdayName(int weekday)
        {
            if (weekday == 1) { return "Monday"; }
            if (weekday == 2) { return "Tuesday"; }
            if (weekday == 3) { return "Wednesday"; }
            if (weekday == 4) { return "Thursday"; }

            if (weekday == 5) { return "Friday"; }
            if (weekday == 6) { return "Saturday"; }
            if (weekday == 7) { return "Sunday"; }            

            return "";
        }
    }

    [Table("Occurence")]
    public class Occurence
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string OccurenceType { get; set; }        
    }
}