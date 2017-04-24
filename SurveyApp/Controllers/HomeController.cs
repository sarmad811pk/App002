using Newtonsoft.Json;
using SurveyApp.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WebMatrix.WebData;


namespace SurveyApp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public HomeController()
        {
            Database.SetInitializer<RespondentContext>(null);
        }
        public ActionResult Index(int? studyId)
        {
            if (!Request.IsAuthenticated)
            {
                WebSecurity.Logout();
                return RedirectToAction("Login", "Account");
            }
            try
            {
                string[] roles = Roles.GetRolesForUser(User.Identity.Name);

                if (roles[0] != "Administrator")
                {
                    return RedirectToAction("Index", "UserQuestion");
                }

            }
            catch (Exception ex)
            {
                WebSecurity.Logout();
                return RedirectToAction("Login", "Account");
            }

            return View(studyId);
        }


        public ActionResult Clinician()
        {
            return View();
        }

        public ActionResult UserQuestion()
        {
            ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult deleteListItem(string id, string type)
        {
            bool isSuccessful = false;
            if (String.IsNullOrEmpty(id) == false)
            {
                int idToBeDeleted = Convert.ToInt32(id);
                try
                {
                    if (type == "study")
                    {
                        /*
                        using (var sssContext = new Study_Survey_ScheduleContext())
                        {
                            sssContext.SSSs.RemoveRange(sssContext.SSSs.Where(sss => sss.StudyId == idToBeDeleted));
                            sssContext.SaveChanges();
                        }
                        using (var csContext = new Child_Study_RespondentContext())
                        {
                            csContext.Child_Study_Respondents.RemoveRange(csContext.Child_Study_Respondents.Where(css => css.StudyId == idToBeDeleted));
                            csContext.SaveChanges();
                        }
                         */ 
                        using (var studyContext = new StudyContext())
                        {
                            Study std = studyContext.Studies.SingleOrDefault(s => s.Id == idToBeDeleted);
                            std.IsDeleted = true;
                            studyContext.SaveChanges();
                        }
                        isSuccessful = true;
                    }
                    else if (type == "school")
                    {
                        /*
                        using (var ptSContext = new PParentTeacher_SchoolContext())
                        {
                            ptSContext.ParentTeacher_Schools.RemoveRange(ptSContext.ParentTeacher_Schools.Where(pts => pts.SchoolId == idToBeDeleted));
                            ptSContext.SaveChanges();
                        }
                         */ 

                        using (var schoolContext = new SchoolContext())
                        {
                            School sch = schoolContext.Schools.SingleOrDefault(s => s.SchoolId == idToBeDeleted); //Find(idToBeDeleted);
                            sch.IsDeleted = true;
                            schoolContext.SaveChanges();
                        }
                        isSuccessful = true;
                    }
                    else if (type == "schedule")
                    {
                        using (var scheduleContext = new ScheduleContext())
                        {
                            Schedule sch = scheduleContext.Schedules.SingleOrDefault(s => s.Id == idToBeDeleted);
                            sch.IsDeleted = true;
                            scheduleContext.SaveChanges();
                        }
                        isSuccessful = true;
                    }
                    else if (type == "child")
                    {
                        /*
                        using (var csContext = new Child_Study_RespondentContext())
                        {
                            csContext.Child_Study_Respondents.RemoveRange(csContext.Child_Study_Respondents.Where(css => css.ChildId == idToBeDeleted));
                            csContext.SaveChanges();
                        }
                        SurveyApp.DataHelper.removePreviousScheduleDates(idToBeDeleted);
                        */

                        using (var childContext = new ChildContext())
                        {
                            Child objChild = childContext.Children.SingleOrDefault(c => c.Id == idToBeDeleted);
                            objChild.IsDeleted = true;
                            childContext.SaveChanges();
                        }
                        isSuccessful = true;
                    }
                    else if (type == "user")
                    {
                        isSuccessful = RemoveUser(idToBeDeleted);
                    }
                    else if (type == "crintervention")
                    {
                        try
                        {
                            DataHelper.deleteCRIntervention(idToBeDeleted);
                            isSuccessful = true;
                        }
                        catch (Exception ex)
                        {
                            isSuccessful = false;
                        }
                    }
                    else if (type == "adversereaction")
                    {
                        try
                        {
                            DataHelper.deleteAdverseReaction(idToBeDeleted);
                            isSuccessful = true;
                        }
                        catch (Exception ex)
                        {
                            isSuccessful = false;
                        }

                    }
                    else if (type == "lifeevent")
                    {
                        try
                        {
                            DataHelper.deleteLifeEvent(idToBeDeleted);
                            isSuccessful = true;
                        }
                        catch (Exception ex)
                        {
                            isSuccessful = false;
                        }
                    }
                    else if(type == "consent")
                    {
                        try
                        {
                            using (var cContext = new ConsentContext())
                            {
                                Consent[] cons = cContext.Consents.Where(c => c.StudyId == idToBeDeleted).ToArray();
                                foreach (Consent objCon in cons)
                                {
                                    objCon.IsDeleted = true;
                                }                                
                                cContext.SaveChanges();
                                cContext.Dispose();
                            }
                            isSuccessful = true;
                        }
                        catch (Exception ex)
                        {
                            isSuccessful = false;
                        }
                    }
                }

                catch (Exception ex)
                {
                    isSuccessful = false;
                }
            }

            return Json(new { success = isSuccessful });
        }

        public static bool RemoveUser(int id)
        {
            using (var uContext = new UsersContext())
            {
                UserProfile objUser = uContext.UserProfiles.Find(Convert.ToInt32(id)); //new UserProfile { UserId = Convert.ToInt32(id) };
                if (objUser != null)
                {
                    string[] roles = Roles.GetRolesForUser(objUser.UserName);
                    /*
                    foreach (string role in roles)
                    {
                        if (role == "Teacher")
                        {
                            using (var ptScContext = new PParentTeacher_SchoolContext())
                            {
                                ptScContext.ParentTeacher_Schools.RemoveRange(ptScContext.ParentTeacher_Schools.Where(pts => pts.ParentTeacherId == objUser.UserId));
                                ptScContext.SaveChanges();
                            }
                            using (var ctContext = new Child_Study_RespondentContext())
                            {
                                ctContext.Child_Study_Respondents.RemoveRange(ctContext.Child_Study_Respondents.Where(cts => cts.RespondentId == objUser.UserId));
                                ctContext.SaveChanges();
                            }
                        }
                    }
                    

                    if (roles.Length > 0)
                    {
                        Roles.RemoveUserFromRoles(objUser.UserName, roles);
                    }
                    */

                    objUser.IsDeleted = true;
                    uContext.SaveChanges();

                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public ActionResult SendReminder(string respos)
        {
            bool isSuccess = false;
            try
            {
                string body = "";
                using (System.IO.StreamReader reader = new System.IO.StreamReader(System.Web.HttpContext.Current.Server.MapPath("~/Attachments/Reminder.html")))
                {
                    body = reader.ReadToEnd();
                }
                body = body.Replace("_ROOTPATH_", System.Web.Configuration.WebConfigurationManager.AppSettings["_RootPath"].ToString());

                bool isSent = true;

                SurveyApp.Models.Respondent[] objRespos = JsonConvert.DeserializeObject<SurveyApp.Models.Respondent[]>(respos);

                List<UserProfile> lstUsers = new List<UserProfile>();
                using (var upContext = new UsersContext())
                {
                    lstUsers = upContext.UserProfiles.ToList();
                }

                foreach (Respondent objRp in objRespos)
                {
                    string fullname = "";
                    if (lstUsers != null && lstUsers.Count > 0)
                    {
                        foreach (UserProfile objUP in lstUsers)
                        {
                            if (objUP.UserName == objRp.Email)
                            {
                                fullname = objUP.FullName;
                            }
                        }
                    }

                    List<string> lstEmails = new List<string>();
                    string newBody = body.Replace("_USERNAME_", objRp.Email);
                    newBody = newBody.Replace("_PASSWORD_", AccountController.getPwd(objRp.Email));

                    if (fullname != "")
                    {
                        newBody = newBody.Replace("_FULLNAME_", "Dear " + fullname);
                    }
                    else
                    {
                        newBody = newBody.Replace("_FULLNAME_", "Hi");
                    }

                    lstEmails.Add(objRp.Email);
                    isSent = SMTPHelper.SendGridEmail("eBit - Reminder", newBody, lstEmails, true);
                    if (isSent == false)
                    {
                        break;
                    }
                }

                if (isSent == true)
                {
                    using (var cRespondent = new RespondentContext())
                    {
                        foreach (SurveyApp.Models.Respondent objRespo in objRespos)
                        {
                            cRespondent.Respondents.Add(objRespo);
                        }

                        cRespondent.SaveChanges();
                    }
                    isSuccess = true;
                }
            }
            catch (Exception ex)
            {
                isSuccess = false;
            }

            return Json(new { success = isSuccess });
        }

        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public ActionResult _Charts()
        {
            string[] roles = Roles.GetRolesForUser(User.Identity.Name);
            string role = roles[0];
            if(role != "Student" && Request.IsAuthenticated)
            {
                return PartialView("_Charts");
            }
            else
            {
                return null;
            }
        }

        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public ActionResult _AdverseEvent()
        {            
            string[] roles = Roles.GetRolesForUser(User.Identity.Name);
            string role = roles[0];
            if (role != "Student" && Request.IsAuthenticated)
            {
                return PartialView("_AdverseEvent");
            }
            else
            {
                return null;
            }
        }

        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public ActionResult _ChildrenCounts()
        {
            string[] roles = Roles.GetRolesForUser(User.Identity.Name);
            string role = roles[0];
            if (role != "Student" && Request.IsAuthenticated)
            {
                return PartialView("_ChildrenCounts");
            }
            else
            {
                return null;
            }
        }

        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public ActionResult _SurveyCompletion()
        {            
            string[] roles = Roles.GetRolesForUser(User.Identity.Name);
            string role = roles[0];
            if (role != "Student" && Request.IsAuthenticated)
            {
                return PartialView("_SurveyCompletion");
            }
            else
            {
                return null;
            }
        }

        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public ActionResult _DetailComparison(string role)
        {
            if (role == "Administrator" && Request.IsAuthenticated)
            {
                return PartialView("_DetailComparison");
            }
            else
            {
                return null;
            }
        }

        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public ActionResult _AssignmentDetails(string role)
        {
            if (role == "Administrator" && Request.IsAuthenticated)
            {
                return PartialView("_AssignmentDetails");
            }
            else
            {
                return null;
            }
        }
    }


}
