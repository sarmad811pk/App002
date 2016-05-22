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

            if (roles[0] == "Parent" || roles[0] == "Teacher")
        {
                return RedirectToAction("Index", "UserQuestion");
        }

            }
            catch(Exception ex){
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
                        using (var sssContext = new Study_Survey_ScheduleContext())
                        {
                            sssContext.SSSs.RemoveRange(sssContext.SSSs.Where(sss => sss.StudyId == idToBeDeleted));
                            sssContext.SaveChanges();
                        }
                        using (var ptsContext = new ParentTeacher_StudyContext())
                        {
                            ptsContext.ParentTeacher_Studys.RemoveRange(ptsContext.ParentTeacher_Studys.Where(pts => pts.StudyId == idToBeDeleted));
                            ptsContext.SaveChanges();
                        }
                        using (var csContext = new Child_StudyContext())
                        {
                            csContext.Child_Studies.RemoveRange(csContext.Child_Studies.Where(css => css.StudyId == idToBeDeleted));
                            csContext.SaveChanges();
                        }
                    using (var studyContext = new StudyContext())
                    {
                            Study std = studyContext.Studies.SingleOrDefault(s => s.Id == idToBeDeleted);
                        studyContext.Studies.Remove(std);
                        studyContext.SaveChanges();
                    }
                    isSuccessful = true;
                }
                else if (type == "school")
                {
                        using (var ptSContext = new PParentTeacher_SchoolContext())
                        {
                            ptSContext.ParentTeacher_Schools.RemoveRange(ptSContext.ParentTeacher_Schools.Where(pts => pts.SchoolId == idToBeDeleted));
                            ptSContext.SaveChanges();
                        }

                    using (var schoolContext = new SchoolContext())
                    {
                            School sch = schoolContext.Schools.SingleOrDefault(s => s.SchoolId == idToBeDeleted); //Find(idToBeDeleted);
                        schoolContext.Schools.Remove(sch);
                        schoolContext.SaveChanges();
                    }
                    isSuccessful = true;
                }
                else if (type == "schedule")
                {
                    using (var scheduleContext = new ScheduleContext())
                    {
                            Schedule sch = scheduleContext.Schedules.SingleOrDefault(s => s.Id == idToBeDeleted);
                        scheduleContext.Schedules.Remove(sch);
                        scheduleContext.SaveChanges();
                    }
                    isSuccessful = true;
                }
                else if (type == "child")
                {
                        using (var ctContext = new Child_TeacherContext())
                        {
                            ctContext.Child_Teachers.RemoveRange(ctContext.Child_Teachers.Where(cts => cts.ChildId == idToBeDeleted));
                            ctContext.SaveChanges();
                        }
                        using (var csContext = new Child_StudyContext())
                        {
                            csContext.Child_Studies.RemoveRange(csContext.Child_Studies.Where(css => css.ChildId == idToBeDeleted));
                            csContext.SaveChanges();
                        }

                    using (var childContext = new ChildContext())
                    {
                            Child objChild = childContext.Children.SingleOrDefault(c => c.Id == idToBeDeleted);
                        childContext.Children.Remove(objChild);
                        childContext.SaveChanges();
                    }
                    isSuccessful = true;
                }
                else if (type == "user")
                {
                        isSuccessful = RemoveUser(idToBeDeleted);
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
                            foreach (string role in roles)
                            {
                                if (role == "Teacher")
                                {
                                    using (var ptScContext = new PParentTeacher_SchoolContext())
                                    {
                                        ptScContext.ParentTeacher_Schools.RemoveRange(ptScContext.ParentTeacher_Schools.Where(pts => pts.ParentTeacherId == objUser.UserId));
                                        ptScContext.SaveChanges();
                                    }
                            using (var ctContext = new Child_TeacherContext())
                            {
                                ctContext.Child_Teachers.RemoveRange(ctContext.Child_Teachers.Where(cts => cts.TeacherId == objUser.UserId));
                                ctContext.SaveChanges();
                                }
                        }
                                using (var ptsContext = new ParentTeacher_StudyContext())
                                {
                                    ptsContext.ParentTeacher_Studys.RemoveRange(ptsContext.ParentTeacher_Studys.Where(pts => pts.ParentTeacherId == objUser.UserId));
                                    ptsContext.SaveChanges();
                                }
                            }

                            if (roles.Length > 0)
                            {
                                Roles.RemoveUserFromRoles(objUser.UserName, roles);
                            }
                            
                            uContext.UserProfiles.Remove(objUser);
                            uContext.SaveChanges();

                    return true;
                }
                        else
                        {
                    return false;
            }
            }
        }

        public ActionResult SendReminder(string respos) {
            bool isSuccess = false;
            try
            {
                SurveyApp.Models.Respondent[] objRespos = JsonConvert.DeserializeObject<SurveyApp.Models.Respondent[]>(respos);
                List<string> lstEmails = new List<string>();
                foreach (Respondent objRp in objRespos)
                {
                    //lstEmails.Add(objRp.Email);
                }
                lstEmails.Add("shazeb140@gmail.com");

                bool isSent = SMTPHelper.SendEmail("test email please ignore", "test email please ignore", lstEmails, true);

                if(isSent == true)
                {
                    //string enc = Encryption.Encrypt("ewrwerwerwerwe", System.Web.Configuration.WebConfigurationManager.AppSettings["EncryptionKey"].ToString(), false);
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
            catch (Exception ex){
                isSuccess = false;
            }
            
            
            return Json(new { success = isSuccess });
        }
    }

    
}
