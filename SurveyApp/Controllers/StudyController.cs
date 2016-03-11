using SurveyApp.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace SurveyApp.Controllers
{
    public class StudyController : Controller
    {
        //
        // GET: /Study/

        public ActionResult Index()
        {
            DataSet dsStudies = DataHelper.StudyGetAll();
            return View(dsStudies);
        }

        public ActionResult StudyAddEdit(FormCollection collection, int? id)
        {
            return View(id);
        }

        [HttpPost]
        public ActionResult StudyAddEdit(FormCollection collection)
        {
            int newStudyId = 0;
            int StudyId = collection["StudyId"] != null && collection["StudyId"] != "" ? Convert.ToInt32(collection["StudyId"]) : 0;
            try
            {
                using (var db = new StudyContext())
                {
                    if (StudyId > 0)
                    {
                        var result = db.Studies.SingleOrDefault(s => s.Id == StudyId);
                        if (result != null)
                        {
                            var dbSS = new Study_Survey_ScheduleContext();                            
                            
                            dbSS.SSSs.RemoveRange(dbSS.SSSs.Where(s => s.StudyId == StudyId));
                            dbSS.SaveChanges();

                            result.Name = collection["StudyName"];
                            result.Status = Convert.ToInt32(collection["Status"]);
                            db.SaveChanges();
                            newStudyId = StudyId;
                        }
                    }
                    else
                    {
                        Study study = new Study { Name = collection["StudyName"], Status = Convert.ToInt32(collection["Status"]) };
                        db.Studies.Add(study);
                        db.SaveChanges();
                        newStudyId = db.Studies.Max(item => item.Id);
                    }

                    if (newStudyId > 0)
                    {
                        int surveyCount = Convert.ToInt32(collection["SurveyCount"]);
                        for (int i = 0; i < surveyCount; i++)
                        {
                            if (collection["Survey" + i] != null && collection["Survey" + i] != "")
                            {
                                using (var dbSS = new Study_Survey_ScheduleContext())
                                {
                                    Study_Survey_Schedule sss = new Study_Survey_Schedule { StudyId = newStudyId, SurveyId = Convert.ToInt32(collection["Survey" + i]), ScheduleIdParent = Convert.ToInt32(collection["Parent" + i]), ScheduleIdTeacher = Convert.ToInt32(collection["Teacher" + i]) };
                                    dbSS.SSSs.Add(sss);
                                    dbSS.SaveChanges();
                                }
                            }
                        }
                    }                    
                }
                
            }
            catch (Exception ex){ 
                
            }

            return RedirectToAction("Index", "Study");
        }
    }
}
