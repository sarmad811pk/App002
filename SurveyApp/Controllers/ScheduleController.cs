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
    [Authorize]
    public class ScheduleController : Controller
    {
        //
        // GET: /Schedule/
        public ScheduleController()
        {         
            Database.SetInitializer<ScheduleContext>(null);
            Database.SetInitializer<Child_Survey_ScheduleContext>(null);
            Database.SetInitializer<Child_Study_RespondentContext>(null);
            Database.SetInitializer<ScheduleReminderContext>(null);
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

            if (mSchedule.DaysToRepeat < mSchedule.AvailableUntil)
            {
                ModelState.AddModelError("", "Survey should be repeated after finishing the current iteration i.e. number of days to repeat should be more than available until days.");
                return View(mSchedule);
            }

            if (mSchedule.Id <= 0 && doesScheduleExist(mSchedule.Title))
            {
                ModelState.AddModelError("", "This schedule already exists, please provide different details.");
                return View(mSchedule);
            }
            
            #endregion


            try
            {
                int scheduleId = 0;
                using (var cSchedule = new ScheduleContext())
                {
                    Schedule objSchedule = new Schedule();
                    if (mSchedule.Id > 0)
                    {
                        objSchedule = cSchedule.Schedules.Find(mSchedule.Id);
                        scheduleId = mSchedule.Id;
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
                    objSchedule.StartingYear = mSchedule.StartingYear;
                    objSchedule.Weekday = mSchedule.Weekday.HasValue == true ? mSchedule.Weekday.Value : 0;
                    if(mSchedule.ActiveOn == 1)
                    {
                        objSchedule.Day = null;
                        objSchedule.Month = null;
                        objSchedule.Weekday = null;
                        objSchedule.StartingYear = null;
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
                        cSchedule.SaveChanges();
                        scheduleId = cSchedule.Schedules.Max(item => item.Id);
                    }
                    else
                    {
                        cSchedule.SaveChanges();
                    }
                    
                }

                bool sendEmail = true;
                sendEmail = Convert.ToBoolean(collection["hdnSendEmail"]);
                string path = Server.MapPath("~/Attachments/Survey_Assignment.html");

                setChildSchedulesInformation(path, sendEmail, scheduleId);
                DataHelper.updateFilledQuestionByScheduleId(scheduleId);
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

        public bool doesScheduleExist(string name)
        {
            bool result = false;
            using (var cContext = new ScheduleContext())
            {
                Schedule objSchedule = cContext.Schedules.Where(s => s.Title == name).FirstOrDefault();
                if (objSchedule != null && objSchedule.Id > 0)
                {
                    result = true;
                }
            }

            return result;
        }

        public bool setChildSchedulesInformation(string path, bool sendEmail, int scheduleId)
        {
            bool isSet = false;
            try
            {
                List<Child> lstChildren = new List<Child>();
                using (var cContext = new ChildContext())
                {
                    lstChildren = cContext.Children.ToList();
                }

                DataSet dsStudies = DataHelper.getStudiesByScheduleId(scheduleId);
                if(dsStudies != null && dsStudies.Tables.Count > 0 && dsStudies.Tables[0].Rows.Count > 0)
                {
                    List<Child_Study_Respondent> lstPTStudies = new List<Child_Study_Respondent>();
                    using (var ptsContext = new Child_Study_RespondentContext())
                    {
                        foreach (DataRow drStudy in dsStudies.Tables[0].Rows)
                        {
                            int stuId = (int)drStudy["StudyId"];
                            lstPTStudies = ptsContext.Child_Study_Respondents.Where(pts => pts.StudyId == stuId).ToList();
                            List<int> lstChildrenUpdated = new List<int>();

                            foreach (Child_Study_Respondent objPTStudy in lstPTStudies)
                            {
                                foreach (Child objChild in lstChildren)
                                {
                                    if (objChild.Id == objPTStudy.ChildId)
                                    {
                                        if (lstChildrenUpdated.Contains(objChild.Id) == false)
                                        {
                                            ChildController.setChildSchedules(objChild, path, sendEmail, stuId);
                                            lstChildrenUpdated.Add(objChild.Id);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                
                isSet = true;
            }
            catch (Exception ex)
            {
                isSet = false;
                throw ex;
            }

            return isSet;
        }

    }
}
