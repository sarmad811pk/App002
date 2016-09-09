using Newtonsoft.Json;
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
    [Authorize]
    public class ChildController : Controller
    {
        //
        // GET: /Child/
        public ChildController() {         
            Database.SetInitializer<StudyContext>(null);
            Database.SetInitializer<Child_Survey_ScheduleContext>(null);
            Database.SetInitializer<Child_Study_RespondentContext>(null);
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


                if(childModel.Id <= 0 && doesChildExist(childModel.Name, childModel.dob))
                {
                    ModelState.AddModelError("", "This child already exists in the system, please provide different details.");
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
                    childModel.EnrollmentDate = dtEnrollment;
                }

                #region Child_Study_Respondents
                List<Study> lstStidues = Study.StudyGetAll();
                DataSet dsTeachers = Child_Teacher.Child_TeacherGetAll();
                using (var csrConext = new Child_Study_RespondentContext())
                {
                    csrConext.Child_Study_Respondents.RemoveRange(csrConext.Child_Study_Respondents.Where(csr => csr.ChildId == cId));
                    csrConext.SaveChanges();
                    csrConext.Dispose();
                }

                using(var csrConext = new Child_Study_RespondentContext())
                {
                    foreach (Study objStudy in lstStidues)
                    {
                        if (!String.IsNullOrEmpty(collection["StudyId_" + objStudy.Id]))
                        {
                            foreach (DataRow drTeacher in dsTeachers.Tables[0].Rows)
                            {                                
                                if (!String.IsNullOrEmpty(collection["StudyId_" + objStudy.Id + "_TeacherId_" + drTeacher["UserId"]]))
                                {
                                    Child_Study_Respondent objCSR = new Child_Study_Respondent();
                                    objCSR.ChildId = cId;
                                    objCSR.StudyId = Convert.ToInt32(collection["StudyId_" + objStudy.Id]);
                                    objCSR.RespondentId = Convert.ToInt32(collection["StudyId_" + objStudy.Id + "_TeacherId_" + drTeacher["UserId"]]);

                                    if (!String.IsNullOrEmpty(collection["StudyId_" + objStudy.Id + "_IncludeParent"]))
                                    {
                                        objCSR.IncludeParent = collection["StudyId_" + objStudy.Id + "_IncludeParent"] == "1" ? true : false;
                                    }

                                    csrConext.Child_Study_Respondents.Add(objCSR);
                                }
                            }
                        }

                        #region deviations
                        //save deviations
                        DataSet dsScheduleDeviations = DataHelper.ScheduleDeviationGetSchedules(objStudy.Id);
                        if (dsScheduleDeviations != null && dsScheduleDeviations.Tables[0].Rows.Count > 0)
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

                                        if (activeOn == 2)
                                        {
                                            string day = collection["ddlDays_StudyId_" + objStudy.Id + "_ScheduleId_" + dr["ScheduleID"].ToString()];
                                            objCSS.Day = String.IsNullOrEmpty(day) ? 0 : Convert.ToInt32(day);

                                            string month = collection["ddlMonths_StudyId_" + objStudy.Id + "_ScheduleId_" + dr["ScheduleID"].ToString()];
                                            objCSS.Month = String.IsNullOrEmpty(month) ? 0 : Convert.ToInt32(month);

                                            string weekday = collection["ddlWeekdays_StudyId_" + objStudy.Id + "_ScheduleId_" + dr["ScheduleID"].ToString()];
                                            objCSS.Weekday = String.IsNullOrEmpty(weekday) ? 0 : Convert.ToInt32(weekday);
                                        }

                                        cCSS.Children_Studies_Schedules.Add(objCSS);                                        
                                    }
                                }
                                cCSS.SaveChanges();
                            }
                        }
                        #endregion

                    }
                    csrConext.SaveChanges();
                }

                
                #endregion

                bool sendEmail = true;
                sendEmail = Convert.ToBoolean(collection["hdnSendEmail"]);
                
                string path = Server.MapPath("~/Attachments/Survey_Assignment.html");
                bool isNewChild = childModel.Id <= 0 ? true : false;
                childModel.Id = cId;
                int studId = 0;
                setChildSchedules(childModel, path, sendEmail, studId, isNewChild);
                
                
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

        public bool doesChildExist(string name, DateTime dob) {
            bool result = false;
            using (var cContext = new ChildContext()) {
                Child objChild = cContext.Children.Where(c => c.Name == name && c.dob == dob).FirstOrDefault();
                if(objChild != null && objChild.Id > 0)
                {
                    result = true;
                }
            }

            return result;
        }

        public static string getPEDsqlTitle(DateTime dob) {
            DateTime today = DateTime.Today;
            int age = today.Year - dob.Year;

            string title = "";
            if (age <= 4)
            {
                title = "Peds quality of life for 2yrs-4yrs";
            }
            else if (age <= 7 && age >= 5)
            {
                title = "Peds quality of life for 5yrs-7yrs";
            }
            else if (age <= 12 && age >= 8)
            {
                title = "Peds quality of life for 8yrs-12yrs";
            }
            else if (age >= 13)
            {
                title = "Peds quality of life for 13yrs -18yrs";
            }

            return title;
        }

        public static bool setChildSchedules(Child childModel, string path, bool sendEmail, int studyIdSelected, bool? isNewChild = false)
        {
            bool isSet = false;
            DateTime? dtEnrollment = null;
            try
            {
                dtEnrollment = childModel.EnrollmentDate;
                #region Child_Survey_Schedule
                if (dtEnrollment != null && dtEnrollment.HasValue && dtEnrollment.Value != DateTime.MinValue)
                {
                    using (var cssContext = new Child_Survey_ScheduleContext())
                    {
                        if (childModel.Id > 0)
                        {
                            DataHelper.removePreviousScheduleDates(childModel.Id);                            
                        }
                        
                        Study_Survey_Schedule[] studySchedules = null;                        
                        
                        foreach (int studyId in Child_Study_RespondentContext.Child_GetStudies(childModel.Id))
                        {
                            using (var sssContext = new Study_Survey_ScheduleContext())
                            {
                                studySchedules = sssContext.SSSs.Where(ss => ss.StudyId == studyId).ToArray();

                                #region ParentSchedules
                                foreach (Study_Survey_Schedule objSSS in studySchedules)
                                {
                                    if (objSSS.ScheduleIdParent > 0)
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
                                                objChildSurveySchedule.StudyId = studyId;
                                                objChildSurveySchedule.SurveyId = objSSS.SurveyId;

                                                int activeOn = objSchedule.ActiveOn;
                                                using (var cCSS = new Child_Study_ScheduleContext())
                                                {
                                                    Child_Study_Schedule[] cStudySchedules = cCSS.Children_Studies_Schedules.Where(cs => cs.ChildId == childModel.Id).ToArray();
                                                    foreach (Child_Study_Schedule css in cStudySchedules)
                                                    {
                                                        if (css.StudyId == studyId && css.ScheduleId == objSSS.ScheduleIdParent)
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
                                                        if (css.StudyId == studyId && css.ScheduleId == objSSS.ScheduleIdParent)
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
                                                    dtStartDate = dtEnrollment.Value;//.AddDays(-objSchedule.AvailableUntil);

                                                }
                                                if (activeOn == 2)
                                                {
                                                    dtStartDate = specificDate;//.AddDays(-objSchedule.AvailableUntil);
                                                }
                                                for (int i = 0; i < noi; i++)
                                                {
                                                    Child_Survey_Schedule objChildSurveySchedule = new Child_Survey_Schedule();
                                                    objChildSurveySchedule.ChildId = childModel.Id;
                                                    objChildSurveySchedule.ScheduleIdParent = objSSS.ScheduleIdParent;
                                                    objChildSurveySchedule.ScheduleIdTeacher = null;
                                                    objChildSurveySchedule.StudyId = studyId;
                                                    objChildSurveySchedule.SurveyId = objSSS.SurveyId;
                                                    if(i != 0)
                                                    {
                                                        dtStartDate = dtStartDate.AddDays(objSchedule.DaysToRepeat.HasValue ? objSchedule.DaysToRepeat.Value : 0);
                                                    }                                                        
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
                                    if (objSSS.ScheduleIdTeacher > 0)
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
                                                objChildSurveySchedule.StudyId = studyId;
                                                objChildSurveySchedule.SurveyId = objSSS.SurveyId;

                                                int activeOn = objSchedule.ActiveOn;
                                                using (var cCSS = new Child_Study_ScheduleContext())
                                                {
                                                    Child_Study_Schedule[] cStudySchedules = cCSS.Children_Studies_Schedules.Where(cs => cs.ChildId == childModel.Id).ToArray();
                                                    foreach (Child_Study_Schedule css in cStudySchedules)
                                                    {
                                                        if (css.StudyId == studyId && css.ScheduleId == objSSS.ScheduleIdTeacher)
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
                                                        if (css.StudyId == studyId && css.ScheduleId == objSSS.ScheduleIdTeacher)
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
                                                    dtStartDate = dtEnrollment.Value;//.AddDays(-objSchedule.AvailableUntil);

                                                }
                                                if (activeOn == 2)
                                                {
                                                    dtStartDate = specificDate;//.AddDays(-objSchedule.AvailableUntil);
                                                }
                                                for (int i = 0; i < noi; i++)
                                                {
                                                    Child_Survey_Schedule objChildSurveySchedule = new Child_Survey_Schedule();
                                                    objChildSurveySchedule.ChildId = childModel.Id;
                                                    objChildSurveySchedule.ScheduleIdParent = null;
                                                    objChildSurveySchedule.ScheduleIdTeacher = objSSS.ScheduleIdTeacher;
                                                    objChildSurveySchedule.StudyId = studyId;
                                                    objChildSurveySchedule.SurveyId = objSSS.SurveyId;
                                                    if(i != 0)
                                                    {
                                                        dtStartDate = dtStartDate.AddDays(objSchedule.DaysToRepeat.HasValue ? objSchedule.DaysToRepeat.Value : 0);
                                                    }                                                        
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

                        cssContext.SaveChanges();
                    }
                }
                #endregion

                #region Emails
                if(sendEmail == true)
                {
                    List<int> lstStudies = new List<int>();
                    if(studyIdSelected > 0)
                    {
                        lstStudies.Add(studyIdSelected);
                    }
                    else
                    {
                        lstStudies = Child_Study_RespondentContext.Child_GetStudies(childModel.Id);
                    }
                    foreach(int studyId in lstStudies)
                    {
                        DataSet dsSurveys = DataHelper.getRespondentsAndSurveys(childModel.Id, studyId, isNewChild.Value);
                        List<RespondentEmail> lstRespos = new List<RespondentEmail>();

                        if (dsSurveys != null && dsSurveys.Tables[0].Rows.Count > 0)
                        {
                            foreach (DataRow drRespo in dsSurveys.Tables[0].Rows)
                            {
                                int userType = Convert.ToInt32(drRespo["UserType"]);                                
                                lstRespos.Add(new RespondentEmail { userId = (int)drRespo["UserId"], email = drRespo["UserName"].ToString(), name = (drRespo["FullName"] != DBNull.Value ? drRespo["FullName"] : drRespo["UserName"]).ToString(), userType = (int)drRespo["UserType"] });                                
                            }
                        }

                        #region EmailContent
                        if (lstRespos.Count > 0)
                        {
                            string body = "";
                            using (System.IO.StreamReader reader = new System.IO.StreamReader(path))
                            {
                                body = reader.ReadToEnd();
                            }
                            body = body.Replace("_ROOTPATH_", System.Web.Configuration.WebConfigurationManager.AppSettings["_RootPath"].ToString());
                            foreach (RespondentEmail objRE in lstRespos)
                            {
                                List<string> lstEmails = new List<string>();
                                lstEmails.Add(objRE.email);

                                string newBody = body.Replace("_FULLNAME_", objRE.name);
                                newBody = newBody.Replace("_USERNAME_", objRE.email);
                                newBody = newBody.Replace("_PASSWORD_", AccountController.getPwd(objRE.email));

                                string surveys = "";
                                if (dsSurveys.Tables[1].Rows.Count > 0)
                                {
                                    surveys = "<table style='border:none;'>";
                                    int i = 1;
                                    bool isSelected = false;
                                    foreach (DataRow dr in dsSurveys.Tables[1].Rows)
                                    {
                                        if (objRE.userType == (int)dr["UserType"])
                                        {
                                            string title = dr["Title"].ToString();
                                            if (((int)dr["ID"] == 6 || (int)dr["ID"] == 7 || (int)dr["ID"] == 8 || (int)dr["ID"] == 9 || (int)dr["ID"] == 45) && isSelected == false)
                                            {
                                                title = getPEDsqlTitle(childModel.dob);
                                                isSelected = true;
                                                surveys += "<tr><td>" + i + " : </td><td>" + title + "</td></tr>";
                                                i++;
                                            }
                                            else
                                            {
                                                if ((int)dr["ID"] != 6 && (int)dr["ID"] != 7 && (int)dr["ID"] != 8 && (int)dr["ID"] != 9 && (int)dr["ID"] != 45)
                                                {
                                                    surveys += "<tr><td>" + i + " : </td><td>" + title + "</td></tr>";
                                                    i++;
                                                }
                                            }
                                        }
                                    }
                                    surveys += "</table>";
                                }
                                newBody = newBody.Replace("_SURVEYS_", surveys);
                                newBody = newBody.Replace("_CHILDNAME_", (String.IsNullOrEmpty(childModel.Name) == false ? " for <b>" + childModel.Name + "</b>" : ""));

                                SMTPHelper.SendGridEmail("eBit - Assessment Surveys" + (String.IsNullOrEmpty(childModel.Name) == false ? " - " + childModel.Name : ""), newBody, lstEmails, true, null, null);
                            }
                        }
                        #endregion
                    }
                }
                

                //List<stri>ng lstEmails = new List<string>();
                //List<UserProfile> lstUsers = new List<UserProfile>();
                //using (var cUP = new UsersContext())
                //{
                //    lstUsers = cUP.UserProfiles.ToList();
                //}

                //foreach (int id in lstTeacherIds)
                //{
                //    foreach(UserProfile objUP in lstUsers)
                //    {
                //        if(objUP.UserId == id)
                //        {
                //            lstEmails.Add(objUP.UserName);
                //        }
                //    }   
                //}

                //string body = "";
                //using (System.IO.StreamReader reader = new System.IO.StreamReader(Server.MapPath("~/Attachments/Child_Teacher_Assignment.html")))
                //{
                //    body = reader.ReadToEnd();
                //}
                //body = body.Replace("_ROOTPATH_", System.Web.Configuration.WebConfigurationManager.AppSettings["_RootPath"].ToString());
                //body = body.Replace("_CHIDLREN_", childModel.Name);

                //SMTPHelper.SendGridEmail("USFEBIT SurveyApp - Child_Assignment", body, lstEmails, true, null, null);
                #endregion

                isSet = true;
            }
            catch (Exception ex)
            {
                isSet = false;
                throw ex;
            }
            return isSet;
        }

        #region Respondents
        public ActionResult GetRespondents(string studyId, string childId)
        {
            var msg = "";
            List<int> lstRespos = new List<int>();
            try
            {
                //SurveyApp.DataHelper.getRespondents(Convert.ToInt32(studyId), Convert.ToInt32(childId));
                int sId = Convert.ToInt32(studyId);
                int cId = Convert.ToInt32(childId);
                using (var csrContext = new Child_Study_RespondentContext())
                {
                    List<Child_Study_Respondent> lstCSRs = csrContext.Child_Study_Respondents.Where(csr => csr.StudyId == sId && csr.ChildId == cId).ToList();
                    if(lstCSRs.Count > 0)
                    {
                        foreach (Child_Study_Respondent objCSR in lstCSRs)
                        {
                            lstRespos.Add(objCSR.RespondentId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }

            return Json(new { success = true, Respondents = lstRespos, msg = msg });
        }        
        #endregion
    }
}
