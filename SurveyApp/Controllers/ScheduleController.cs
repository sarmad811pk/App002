using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SurveyApp.Models;
using Newtonsoft.Json;
using System.Data;
using System.Data.Entity;

namespace SurveyApp.Controllers
{
    public class ScheduleController : Controller
    {
        //
        // GET: /Schedule/
        public ScheduleController()
        {         
            Database.SetInitializer<ScheduleContext>(null);
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ScheduleAddEdit(int? id)
        {
            Schedule mSchedule = new Schedule();
            if (id.HasValue == true)
            {                
                var cSchedule = new ScheduleContext();
                mSchedule = cSchedule.Schedules.Find(id);
                ViewData["Months"] = mSchedule.Month;
            }

            return View(mSchedule);
        }

        [HttpPost]
        public ActionResult ScheduleAddEdit(Schedule mSchedule, FormCollection collection)
        {
            #region Validation
            if (!ModelState.IsValid)
            {
                return View(mSchedule);
            }

            if (mSchedule.Frequency <= 0)
            {
                ModelState.AddModelError("", "Please select frequncy.");
                return View(mSchedule);
            }

            if (mSchedule.Frequency == 2)
            {
                if (mSchedule.DaysToRepeat == null || mSchedule.DaysToRepeat <= 0)
                {
                    ModelState.AddModelError("", "Please enter number of days for frequency.");
                    return View(mSchedule);
                }
            }

            if (mSchedule.ActiveOn <= 0)
            {
                ModelState.AddModelError("", "Please select activation type.");
                return View(mSchedule);
            }

            if (mSchedule.ActiveOn == 2 && mSchedule.Month <= 0 && mSchedule.Day <= 0 && mSchedule.Weekday <= 0) {
                ModelState.AddModelError("", "Please specify period.");
                return View(mSchedule);
            }
            #endregion


            try
            {
                using (var cSchedule = new ScheduleContext())
                {
                    Schedule objSchedule = new Schedule();
                    if (mSchedule.Id > 0)
                    {
                        objSchedule = cSchedule.Schedules.Find(mSchedule.Id);
                    }
                    objSchedule.ActiveOn = mSchedule.ActiveOn;
                    objSchedule.AvailableUntil = mSchedule.AvailableUntil;
                    objSchedule.Day = mSchedule.Day.HasValue == true ? mSchedule.Day.Value : 0;
                    objSchedule.DaysToRepeat = mSchedule.DaysToRepeat.HasValue == true ? mSchedule.DaysToRepeat.Value : 0;
                    objSchedule.Frequency = mSchedule.Frequency;
                    objSchedule.LastReminder = mSchedule.LastReminder;
                    objSchedule.Month = mSchedule.Month.HasValue == true ? mSchedule.Month.Value : 0;
                    objSchedule.ReminderFrequency = mSchedule.ReminderFrequency;
                    objSchedule.Title = mSchedule.Title;
                    objSchedule.Weekday = mSchedule.Weekday.HasValue == true ? mSchedule.Weekday.Value : 0;
                    if(mSchedule.ActiveOn == 1)
                    {
                        objSchedule.Day = null;
                        objSchedule.Month = null;
                        objSchedule.Weekday = null;
                    }
                    else
                    {
                        objSchedule.Day = objSchedule.Day == 0 ? null : objSchedule.Day;
                        objSchedule.Month = objSchedule.Month == 0 ? null : objSchedule.Month;
                        objSchedule.Weekday = objSchedule.Weekday == 0 ? null : objSchedule.Weekday;
                    }
                    if(mSchedule.Frequency == 1)
                    {
                        objSchedule.DaysToRepeat = null;
                    }

                    if (mSchedule.Id <= 0)
                    {
                        cSchedule.Schedules.Add(objSchedule);
                    }
                    cSchedule.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(mSchedule);
            }

            return RedirectToAction("Index", "Schedule");
        }

        public ActionResult saveSchedule(string objSchedule)
        {
            int newId = 0;
            Schedule schedule = JsonConvert.DeserializeObject<Schedule>(objSchedule);

            using (var db = new ScheduleContext())
            {
                if (schedule != null && schedule.Id > 0)
                {
                    var result = db.Schedules.SingleOrDefault(s => s.Id == schedule.Id);
                    if (result != null)
                    {
                        newId = schedule.Id;
                        result.Title = schedule.Title;
                        //result.StartDate = schedule.StartDate;
                        //result.EndDate = schedule.EndDate;
                        //result.AssignmentRemider = schedule.AssignmentRemider;
                        //result.CompletionRemider = schedule.CompletionRemider;
                        //result.OccurenceId = schedule.OccurenceId;
                    }

                    //school = new School { SchoolId = Convert.ToInt32(ID), Name = "" };
                }
                else
                {
                    db.Schedules.Add(schedule);                    
                }

                //db.Schools            
                db.SaveChanges();
                newId = db.Schedules.Max(item => item.Id);
            }



            return Json(new { success = newId > 0 ? true : false, scheduleid = newId });
        }

    }
}
