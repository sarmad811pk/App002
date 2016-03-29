using SurveyApp.Models;
using System;
using System.Collections.Generic;
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
        public ActionResult Index()
        {            
            try
            {
                string[] roles = Roles.GetRolesForUser(User.Identity.Name);
                if (roles[0] == "Parent")
                {
                    return RedirectToAction("Index", "UserQuestion");
                }

            }
            catch(Exception ex){
                WebSecurity.Logout(); 
                return RedirectToAction("Login", "Account");
            }

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
            
            try
            {
                if (type == "study")
                {
                    using (var sssContext = new Study_Survey_ScheduleContext())
                    {
                        sssContext.SSSs.RemoveRange(sssContext.SSSs.Where(sss => sss.StudyId == Convert.ToInt32(id)));
                        sssContext.SaveChanges();
                    }

                    using (var studyContext = new StudyContext())
                    {
                        Study std = studyContext.Studies.Find(Convert.ToInt32(id));                        
                        studyContext.Studies.Remove(std);
                        studyContext.SaveChanges();
                    }
                    isSuccessful = true;
                }
                else if (type == "school")
                {
                    using (var ptSContext = new PParentTeacher_SchoolContext())
                    {
                        ptSContext.ParentTeacher_Schools.RemoveRange(ptSContext.ParentTeacher_Schools.Where(pts => pts.SchoolId == Convert.ToInt32(id)));
                        ptSContext.SaveChanges();
                    }

                    using (var schoolContext = new SchoolContext())
                    {
                        School sch = schoolContext.Schools.Find(Convert.ToInt32(id));                        
                        schoolContext.Schools.Remove(sch);
                        schoolContext.SaveChanges();
                    }
                    isSuccessful = true;
                }
                else if (type == "schedule")
                {
                    using (var scheduleContext = new ScheduleContext())
                    {
                        Schedule sch = scheduleContext.Schedules.Find(Convert.ToInt32(id));                        
                        scheduleContext.Schedules.Remove(sch);
                        scheduleContext.SaveChanges();
                    }
                    isSuccessful = true;
                }
                else if (type == "child")
                {
                    using (var ctContext = new Child_TeacherContext())
                    {
                        ctContext.Child_Teachers.RemoveRange(ctContext.Child_Teachers.Where(cts => cts.ChildId == Convert.ToInt32(id)));
                        ctContext.SaveChanges();
                    }
                    using (var csContext = new Child_StudyContext())
                    {
                        csContext.Child_Studies.RemoveRange(csContext.Child_Studies.Where(css => css.ChildId == Convert.ToInt32(id)));
                        csContext.SaveChanges();
                    }
                    using (var childContext = new ChildContext())
                    {
                        Child objChild = childContext.Children.Find(Convert.ToInt32(id));                        
                        childContext.Children.Remove(objChild);
                        childContext.SaveChanges();
                    }
                    isSuccessful = true;
                }
                else if (type == "user")
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

                    isSuccessful = true;
                }
                        else
                        {
                            isSuccessful = false;
                        }                        
                    }                    
                }
            }
            catch (Exception ex)
            {
                isSuccessful = false;
            }
            
            return Json(new { success = isSuccessful });
        }
    }
}
