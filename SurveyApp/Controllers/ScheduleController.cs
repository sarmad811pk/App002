using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SurveyApp.Models;
using Newtonsoft.Json;
using System.Data;

namespace SurveyApp.Controllers
{
    public class ScheduleController : Controller
    {
        //
        // GET: /Schedule/

        public ActionResult Index()
        {
            DataSet dsSchedules = DataHelper.ScheduleGetAll();
            return View(dsSchedules);
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
                        result.StartDate = schedule.StartDate;
                        result.EndDate = schedule.EndDate;
                        result.AssignmentRemider = schedule.AssignmentRemider;
                        result.CompletionRemider = schedule.CompletionRemider;
                        result.OccurenceId = schedule.OccurenceId;
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
