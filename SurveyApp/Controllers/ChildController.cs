﻿using Newtonsoft.Json;
using SurveyApp.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebMatrix.WebData;

namespace SurveyApp.Controllers
{
    public class ChildController : Controller
    {
        //
        // GET: /Child/
        public ChildController() {         
            Database.SetInitializer<StudyContext>(null);
            Database.SetInitializer<Child_Survey_ScheduleContext>(null);
        }

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult ChildAddEdit(int? id)
        {
            ViewData["dsParents"] = DataHelper.Parent_GetAll();
            Child objChild = new Child();
            if (id.HasValue)
            {
                var cContext = new ChildContext();
                objChild = cContext.Children.Find(id);
            }

            return View(objChild);
        }

        [HttpPost]
        public ActionResult ChildAddEdit(Child childModel, FormCollection collection)
        {
            if (!ModelState.IsValid)
            {
                return View(childModel);
            }
            try
            {
                if (childModel.Gender == 0)
                {
                    ModelState.AddModelError("", "Please select gender.");
                    return View(childModel);
                }

                if (childModel.StatusId == 0)
                {
                    ModelState.AddModelError("", "Please select enrollment status.");
                    return View(childModel);
                }

                int studyCount = 0;
                foreach (var key in collection.Keys)
                {
                    if (key.ToString().Contains("StudyId_"))
                    {
                        studyCount++;
                    }
                }
                if (studyCount == 0)
                {
                    ModelState.AddModelError("", "Please select at least one study.");
                    return View(childModel);
                }

                int teacherCount = 0;
                foreach (var key in collection.Keys)
                {
                    if (key.ToString().Contains("TeacherId_"))
                    {
                        teacherCount++;
                    }
                }
                if (teacherCount == 0)
                {
                    ModelState.AddModelError("", "Please select at least one teacher.");
                    return View(childModel);
                }

                int cId = 0;
                DateTime? dtEnrollment = null;
                using (var cContext = new ChildContext())
                {
                    Child objChild = new Child();
                    if (childModel.Id > 0)
                    {
                        objChild = cContext.Children.Find(childModel.Id);
                    }

                    objChild.Name = childModel.Name;
                    objChild.dob = childModel.dob;
                    objChild.Gender = childModel.Gender;
                    objChild.ParentId = childModel.ParentId;
                    objChild.SchoolId = childModel.SchoolId;
                    objChild.StatusId = childModel.StatusId;
                    if(childModel.StatusId == 1 && objChild.EnrollmentDate == null)
                    {
                        objChild.EnrollmentDate = DateTime.Now;                        
                    }
                    if(childModel.Id <= 0)
                    { 
                        if (childModel.StatusId != 1)
                        { 
                            objChild.EnrollmentDate = null;
                        }
                    }
                    dtEnrollment = objChild.EnrollmentDate;

                    if (childModel.Id <= 0)
                    {
                        cContext.Children.Add(objChild);
                    }

                    cContext.SaveChanges();
                    cId = childModel.Id > 0 ? childModel.Id : cContext.Children.Max(item => item.Id);
                    
                }

                //deviations
                using (var cCSS = new Child_Study_ScheduleContext())
                {
                    cCSS.Children_Studies_Schedules.RemoveRange(cCSS.Children_Studies_Schedules.Where(c => c.ChildId == cId/* && c.StudyId == objStudy.Id && c.ScheduleId == (int)dr["ScheduleID"]*/));
                    cCSS.SaveChanges();
                }

                //save child study relationship info                
                using (var cContext = new Child_StudyContext())
                {
                    if (childModel.Id > 0)
                    {
                        cContext.Child_Studies.RemoveRange(cContext.Child_Studies.Where(c => c.ChildId == childModel.Id));
                        cContext.SaveChanges();
                    }

                    foreach (Study objStudy in Study.StudyGetAll())
                    {
                        if (!String.IsNullOrEmpty(collection["StudyId_" + objStudy.Id]))
                        {
                            Child_Study objCS = new Child_Study();
                            objCS.ChildId = cId;
                            objCS.StudyId = Convert.ToInt32(collection["StudyId_" + objStudy.Id]);

                            cContext.Child_Studies.Add(objCS);
                        }

                        cContext.SaveChanges();
                        
                        //save deviations
                        DataSet dsScheduleDeviations = DataHelper.ScheduleDeviationGetSchedules(objStudy.Id);
                        if(dsScheduleDeviations != null && dsScheduleDeviations.Tables[0].Rows.Count > 0)
                        {
                            using (var cCSS = new Child_Study_ScheduleContext())
                            {
                                foreach (DataRow dr in dsScheduleDeviations.Tables[0].Rows)
                                {
                                    int activeOn = String.IsNullOrEmpty(collection["ActiveOn_StudyId_" + objStudy.Id + "_ScheduleId_" + dr["ScheduleID"].ToString()]) == false ? Convert.ToInt32(collection["ActiveOn_StudyId_" + objStudy.Id + "_ScheduleId_" + dr["ScheduleID"].ToString()]) : 0;
                                    if (activeOn > 0)
                                    {
                                        Child_Study_Schedule objCSS = new Child_Study_Schedule();

                                        objCSS.ChildId = cId;
                                        objCSS.StudyId = objStudy.Id;
                                        objCSS.ScheduleId = (int)dr["ScheduleID"];
                                        objCSS.ActiveOn = activeOn;

                                        if(activeOn == 2)
                                        {
                                            string day = collection["ddlDays_StudyId_" + objStudy.Id + "_ScheduleId_" + dr["ScheduleID"].ToString()];
                                            objCSS.Day = String.IsNullOrEmpty(day) ? 0 : Convert.ToInt32(day);

                                            string month = collection["ddlMonths_StudyId_" + objStudy.Id + "_ScheduleId_" + dr["ScheduleID"].ToString()];
                                            objCSS.Month = String.IsNullOrEmpty(month) ? 0 : Convert.ToInt32(month);

                                            string weekday = collection["ddlWeekdays_StudyId_" + objStudy.Id + "_ScheduleId_" + dr["ScheduleID"].ToString()];
                                            objCSS.Weekday = String.IsNullOrEmpty(weekday) ? 0 : Convert.ToInt32(weekday);
                                        }

                                        cCSS.Children_Studies_Schedules.Add(objCSS);
                                        cCSS.SaveChanges();
                                    }
                                }
                            }
                        }
                    }
                }

                //save child teacher relationship info                
                List<int> lstTeacherIds = new List<int>();
                using (var ctContext = new Child_TeacherContext())
                {
                    if (childModel.Id > 0)
                    {
                        ctContext.Child_Teachers.RemoveRange(ctContext.Child_Teachers.Where(c => c.ChildId == childModel.Id));
                        ctContext.SaveChanges();
                    }

                    foreach (DataRow drT in DataHelper.Child_TeacherGetAll().Tables[0].Rows)
                    {
                        if (!String.IsNullOrEmpty(collection["TeacherId_" + drT["UserId"].ToString()]))
                        {
                            Child_Teacher objCT = new Child_Teacher();
                            objCT.ChildId = cId;
                            objCT.TeacherId = Convert.ToInt32(collection["TeacherId_" + drT["UserId"].ToString()]);

                            lstTeacherIds.Add(objCT.TeacherId);
                            ctContext.Child_Teachers.Add(objCT);
                        }
                    }

                    

                    ctContext.SaveChanges();
                }

                #region Child_Survey_Schedule
                if(dtEnrollment != null && dtEnrollment.HasValue && dtEnrollment.Value != DateTime.MinValue)
                {
                    using (var cssContext = new Child_Survey_ScheduleContext())
                    {
                        if (childModel.Id > 0)
                        {
                            cssContext.Child_Survey_Schedules.RemoveRange(cssContext.Child_Survey_Schedules.Where(ct => ct.ChildId == childModel.Id));
                            cssContext.SaveChanges();
                        }


                        Child_Study[] studies = null;
                        Study_Survey_Schedule[] studySchedules = null;
                        using (var csContext = new Child_StudyContext())
                        {
                            studies = csContext.Child_Studies.Where(cs => cs.ChildId == childModel.Id).ToArray();
                            foreach (Child_Study objCS in studies)
                            {
                                using (var sssContext = new Study_Survey_ScheduleContext())
                                {
                                    studySchedules = sssContext.SSSs.Where(ss => ss.StudyId == objCS.StudyId).ToArray();
                                    
                                    #region ParentSchedules
                                    foreach (Study_Survey_Schedule objSSS in studySchedules)
                                    {
                                        if(objSSS.ScheduleIdParent > 0)
                                        {
                                            using (var scContext = new ScheduleContext())
                                            {
                                                Schedule objSchedule = scContext.Schedules.Where(sc => sc.Id == objSSS.ScheduleIdParent).ToArray().FirstOrDefault();
                                                DateTime specificWeekday = DateTime.MinValue;
                                                if (objSchedule.Weekday > 0)
                                                {
                                                    specificWeekday = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                                                    while (specificWeekday.DayOfWeek != (DayOfWeek)(objSchedule.Weekday - 1))
                                                    {
                                                        specificWeekday = specificWeekday.AddDays(1);
                                                    }
                                                }
                                                DateTime specificDate = objSchedule.Day != null && objSchedule.Month != null ? new DateTime(DateTime.Now.Year, (int)objSchedule.Month, (int)objSchedule.Day) : (objSchedule.Weekday > 0 ? specificWeekday : DateTime.MinValue);

                                                DateTime endDate = DateTime.MinValue;

                                                if (objSchedule.Frequency == 1)
                                                {
                                                    Child_Survey_Schedule objChildSurveySchedule = new Child_Survey_Schedule();
                                                    objChildSurveySchedule.ChildId = childModel.Id;
                                                    objChildSurveySchedule.ScheduleIdParent = objSSS.ScheduleIdParent;
                                                    objChildSurveySchedule.ScheduleIdTeacher = null;
                                                    objChildSurveySchedule.StudyId = objCS.StudyId;
                                                    objChildSurveySchedule.SurveyId = objSSS.SurveyId;

                                                    int activeOn = objSchedule.ActiveOn;
                                                    using (var cCSS = new Child_Study_ScheduleContext())
                                                    {
                                                        Child_Study_Schedule[] cStudySchedules = cCSS.Children_Studies_Schedules.Where(cs => cs.ChildId == childModel.Id).ToArray();
                                                        foreach(Child_Study_Schedule css in cStudySchedules)
                                                        {
                                                            if(css.StudyId == objCS.StudyId && css.ScheduleId == objSSS.ScheduleIdParent)
                                                            {
                                                                DateTime weekday = DateTime.MinValue;
                                                                if (css.Weekday > 0)
                                                                {
                                                                    weekday = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                                                                    while (weekday.DayOfWeek != (DayOfWeek)(css.Weekday - 1))
                                                                    {
                                                                        weekday = weekday.AddDays(1);
                                                                    }
                                                                }

                                                                activeOn = css.ActiveOn;
                                                                specificDate = css.Day > 0 && css.Month > 0 ? new DateTime(DateTime.Now.Year, (int)css.Month, (int)css.Day) : (css.Weekday > 0 ? weekday : DateTime.MinValue);
                                                            }
                                                        }
                                                    }

                                                    if (activeOn == 1)
                                                    {
                                                        endDate = dtEnrollment.Value.AddDays(objSchedule.AvailableUntil);
                                                        objChildSurveySchedule.ScheuleStartDate = dtEnrollment.Value;
                                                    }
                                                    if (activeOn == 2)
                                                    {
                                                        endDate = specificDate.AddDays(objSchedule.AvailableUntil);
                                                        objChildSurveySchedule.ScheuleStartDate = specificDate;
                                                    }

                                                    objChildSurveySchedule.ScheuleEndDate = endDate;
                                                    cssContext.Child_Survey_Schedules.Add(objChildSurveySchedule);
                                                }

                                                if (objSchedule.Frequency == 2)
                                                {
                                                    string nods = System.Web.Configuration.WebConfigurationManager.AppSettings["NoOfDaysToSaveScheduleUpto"] == null ? "" : System.Web.Configuration.WebConfigurationManager.AppSettings["NoOfDaysToSaveScheduleUpto"];
                                                    int noi = Convert.ToInt32(nods == "" ? "10" : nods);
                                                    DateTime dtStartDate = DateTime.MinValue;

                                                    int activeOn = objSchedule.ActiveOn;
                                                    using (var cCSS = new Child_Study_ScheduleContext())
                                                    {
                                                        Child_Study_Schedule[] cStudySchedules = cCSS.Children_Studies_Schedules.Where(cs => cs.ChildId == childModel.Id).ToArray();
                                                        foreach (Child_Study_Schedule css in cStudySchedules)
                                                        {
                                                            if (css.StudyId == objCS.StudyId && css.ScheduleId == objSSS.ScheduleIdParent)
                                                            {
                                                                DateTime weekday = DateTime.MinValue;
                                                                if (css.Weekday > 0)
                                                                {
                                                                    weekday = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                                                                    while (weekday.DayOfWeek != (DayOfWeek)(css.Weekday - 1))
                                                                    {
                                                                        weekday = weekday.AddDays(1);
                                                                    }
                                                                }

                                                                activeOn = css.ActiveOn;
                                                                specificDate = css.Day > 0 && css.Month > 0 ? new DateTime(DateTime.Now.Year, (int)css.Month, (int)css.Day) : (css.Weekday > 0 ? weekday : DateTime.MinValue);
                                                            }
                                                        }
                                                    }

                                                    if (activeOn == 1)
                                                    {
                                                        dtStartDate = dtEnrollment.Value.AddDays(-objSchedule.AvailableUntil);

                                                    }
                                                    if (activeOn == 2)
                                                    {
                                                        dtStartDate = specificDate.AddDays(-objSchedule.AvailableUntil);
                                                    }
                                                    for (int i = 0; i < noi; i++)
                                                    {
                                                        Child_Survey_Schedule objChildSurveySchedule = new Child_Survey_Schedule();
                                                        objChildSurveySchedule.ChildId = childModel.Id;
                                                        objChildSurveySchedule.ScheduleIdParent = objSSS.ScheduleIdParent;
                                                        objChildSurveySchedule.ScheduleIdTeacher = null;
                                                        objChildSurveySchedule.StudyId = objCS.StudyId;
                                                        objChildSurveySchedule.SurveyId = objSSS.SurveyId;
                                                        dtStartDate = dtStartDate.AddDays(objSchedule.DaysToRepeat.HasValue ? objSchedule.DaysToRepeat.Value : 0);
                                                        objChildSurveySchedule.ScheuleStartDate = dtStartDate;
                                                        objChildSurveySchedule.ScheuleEndDate = dtStartDate.AddDays(objSchedule.AvailableUntil);
                                                        cssContext.Child_Survey_Schedules.Add(objChildSurveySchedule);
                                                    }
                                                }
                                            }
                                        }                                        
                                    }
                                    #endregion
                                    #region TeacherSchedules
                                    foreach (Study_Survey_Schedule objSSS in studySchedules)
                                    {
                                        if(objSSS.ScheduleIdTeacher > 0)
                                        {
                                            using (var scContext = new ScheduleContext())
                                            {
                                                Schedule objSchedule = scContext.Schedules.Where(sc => sc.Id == objSSS.ScheduleIdTeacher).ToArray().FirstOrDefault();
                                                DateTime specificWeekday = DateTime.MinValue;
                                                if (objSchedule.Weekday > 0)
                                                {
                                                    specificWeekday = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                                                    while (specificWeekday.DayOfWeek != (DayOfWeek)(objSchedule.Weekday - 1))
                                                    {
                                                        specificWeekday = specificWeekday.AddDays(1);
                                                    }
                                                }

                                                DateTime specificDate = objSchedule.Month != null ? new DateTime(DateTime.Now.Year, (int)objSchedule.Month, (int)objSchedule.Day) : (objSchedule.Weekday > 0 ? specificWeekday : DateTime.MinValue);
                                                DateTime endDate = DateTime.MinValue;

                                                if (objSchedule.Frequency == 1)
                                                {
                                                    Child_Survey_Schedule objChildSurveySchedule = new Child_Survey_Schedule();
                                                    objChildSurveySchedule.ChildId = childModel.Id;
                                                    objChildSurveySchedule.ScheduleIdParent = null;
                                                    objChildSurveySchedule.ScheduleIdTeacher = objSSS.ScheduleIdTeacher;
                                                    objChildSurveySchedule.StudyId = objCS.StudyId;
                                                    objChildSurveySchedule.SurveyId = objSSS.SurveyId;

                                                    int activeOn = objSchedule.ActiveOn;
                                                    using (var cCSS = new Child_Study_ScheduleContext())
                                                    {
                                                        Child_Study_Schedule[] cStudySchedules = cCSS.Children_Studies_Schedules.Where(cs => cs.ChildId == childModel.Id).ToArray();
                                                        foreach (Child_Study_Schedule css in cStudySchedules)
                                                        {
                                                            if (css.StudyId == objCS.StudyId && css.ScheduleId == objSSS.ScheduleIdParent)
                                                            {
                                                                DateTime weekday = DateTime.MinValue;
                                                                if (css.Weekday > 0)
                                                                {
                                                                    weekday = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                                                                    while (weekday.DayOfWeek != (DayOfWeek)(css.Weekday - 1))
                                                                    {
                                                                        weekday = weekday.AddDays(1);
                                                                    }
                                                                }

                                                                activeOn = css.ActiveOn;
                                                                specificDate = css.Day > 0 && css.Month > 0 ? new DateTime(DateTime.Now.Year, (int)css.Month, (int)css.Day) : (css.Weekday > 0 ? weekday : DateTime.MinValue);
                                                            }
                                                        }
                                                    }

                                                    if (activeOn == 1)
                                                    {
                                                        endDate = dtEnrollment.Value.AddDays(objSchedule.AvailableUntil);
                                                        objChildSurveySchedule.ScheuleStartDate = dtEnrollment.Value;
                                                    }
                                                    if (activeOn == 2)
                                                    {
                                                        endDate = specificDate.AddDays(objSchedule.AvailableUntil);
                                                        objChildSurveySchedule.ScheuleStartDate = specificDate;
                                                    }

                                                    objChildSurveySchedule.ScheuleEndDate = endDate;
                                                    cssContext.Child_Survey_Schedules.Add(objChildSurveySchedule);
                                                }

                                                if (objSchedule.Frequency == 2)
                                                {
                                                    string nods = System.Web.Configuration.WebConfigurationManager.AppSettings["NoOfDaysToSaveScheduleUpto"] == null ? "" : System.Web.Configuration.WebConfigurationManager.AppSettings["NoOfDaysToSaveScheduleUpto"];
                                                    int noi = Convert.ToInt32(nods == "" ? "10" : nods);
                                                    DateTime dtStartDate = DateTime.MinValue;

                                                    int activeOn = objSchedule.ActiveOn;
                                                    using (var cCSS = new Child_Study_ScheduleContext())
                                                    {
                                                        Child_Study_Schedule[] cStudySchedules = cCSS.Children_Studies_Schedules.Where(cs => cs.ChildId == childModel.Id).ToArray();
                                                        foreach (Child_Study_Schedule css in cStudySchedules)
                                                        {
                                                            if (css.StudyId == objCS.StudyId && css.ScheduleId == objSSS.ScheduleIdParent)
                                                            {
                                                                DateTime weekday = DateTime.MinValue;
                                                                if (css.Weekday > 0)
                                                                {
                                                                    weekday = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                                                                    while (weekday.DayOfWeek != (DayOfWeek)(css.Weekday - 1))
                                                                    {
                                                                        weekday = weekday.AddDays(1);
                                                                    }
                                                                }

                                                                activeOn = css.ActiveOn;
                                                                specificDate = css.Day > 0 && css.Month > 0 ? new DateTime(DateTime.Now.Year, (int)css.Month, (int)css.Day) : (css.Weekday > 0 ? weekday : DateTime.MinValue);
                                                            }
                                                        }
                                                    }

                                                    if (activeOn == 1)
                                                    {
                                                        dtStartDate = dtEnrollment.Value.AddDays(-objSchedule.AvailableUntil);

                                                    }
                                                    if (activeOn == 2)
                                                    {
                                                        dtStartDate = specificDate.AddDays(-objSchedule.AvailableUntil);
                                                    }
                                                    for (int i = 0; i < noi; i++)
                                                    {
                                                        Child_Survey_Schedule objChildSurveySchedule = new Child_Survey_Schedule();
                                                        objChildSurveySchedule.ChildId = childModel.Id;
                                                        objChildSurveySchedule.ScheduleIdParent = null;
                                                        objChildSurveySchedule.ScheduleIdTeacher = objSSS.ScheduleIdTeacher;
                                                        objChildSurveySchedule.StudyId = objCS.StudyId;
                                                        objChildSurveySchedule.SurveyId = objSSS.SurveyId;
                                                        dtStartDate = dtStartDate.AddDays(objSchedule.DaysToRepeat.HasValue ? objSchedule.DaysToRepeat.Value : 0);
                                                        objChildSurveySchedule.ScheuleStartDate = dtStartDate;
                                                        objChildSurveySchedule.ScheuleEndDate = dtStartDate.AddDays(objSchedule.AvailableUntil);
                                                        cssContext.Child_Survey_Schedules.Add(objChildSurveySchedule);
                                                    }
                                                }
                                            }
                                        }                                        
                                    }
                                    #endregion
                                }
                            }
                        }

                        cssContext.SaveChanges();
                    }
                }
                #endregion

                #region Emails
                List<string> lstEmails = new List<string>();
                List<UserProfile> lstUsers = new List<UserProfile>();
                using (var cUP = new UsersContext())
                {
                    lstUsers = cUP.UserProfiles.ToList();
                }

                foreach (int id in lstTeacherIds)
                {
                    foreach(UserProfile objUP in lstUsers)
                    {
                        if(objUP.UserId == id)
                        {
                            lstEmails.Add(objUP.UserName);
                        }
                    }   
                }

                string body = "";
                using (System.IO.StreamReader reader = new System.IO.StreamReader(Server.MapPath("~/Attachments/Child_Teacher_Assignment.html")))
                {
                    body = reader.ReadToEnd();
                }
                body = body.Replace("_ROOTPATH_", System.Web.Configuration.WebConfigurationManager.AppSettings["_RootPath"].ToString());
                body = body.Replace("_CHIDLREN_", childModel.Name);

                SMTPHelper.SendGridEmail("USFEBIT SurveyApp - Child_Assignment", body, lstEmails, true, null, null);
                #endregion

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(childModel);
            }
            
            return RedirectToAction("Index", "Child");
        }

        public ActionResult AddParentTeacher(string objUser, string role)
        {
            int newId = 0;
            string msg = "";
            try
            {
                RegisterModel registerModel = JsonConvert.DeserializeObject<RegisterModel>(objUser);
                AccountController.CreateAccount(registerModel, role);
                newId = WebSecurity.GetUserId(registerModel.UserName);
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }

            return Json(new { success = newId > 0 ? true : false, UserId = newId, msg = msg });
        }
    }
}
