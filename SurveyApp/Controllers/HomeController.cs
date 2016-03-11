using SurveyApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SurveyApp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";

            return View();
        }
        public ActionResult Survey(int ID)
        {
            return View(ID);
        }
        public ActionResult Survey0() {
            ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";

            return View();
        
        }
        public ActionResult Survey1()
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
                    using (var studyContext = new StudyContext())
                    {
                        Study std = new Study { Id = Convert.ToInt32(id) };
                        studyContext.Studies.Attach(std);
                        studyContext.Studies.Remove(std);
                        studyContext.SaveChanges();
                    }
                    isSuccessful = true;
                }
                else if (type == "school")
                {
                    using (var schoolContext = new SchoolContext())
                    {
                        School sch = new School { SchoolId = Convert.ToInt32(id) };
                        schoolContext.Schools.Attach(sch);
                        schoolContext.Schools.Remove(sch);
                        schoolContext.SaveChanges();
                    }
                    isSuccessful = true;
                }
                else if (type == "schedule")
                {
                    using (var scheduleContext = new ScheduleContext())
                    {
                        Schedule sch = new Schedule { Id = Convert.ToInt32(id) };
                        scheduleContext.Schedules.Attach(sch);
                        scheduleContext.Schedules.Remove(sch);
                        scheduleContext.SaveChanges();
                    }
                    isSuccessful = true;
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
