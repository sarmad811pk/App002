using SurveyApp.Filters;
using SurveyApp.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
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
        public StudyController() {
            Database.SetInitializer<StudyContext>(null);
        }

        public ActionResult Index()
        {            
            return View();
        }

        public ActionResult StudyAddEdit(int? id)
        {            
            if(id.HasValue)
            {
                var db = new StudyContext();
                Study study = db.Studies.Find(id);
                return View(study);
            }
            else
            {
                Study study = new Study();
                study.Id = 0;
                return View(study);
            }
        }

        [HttpPost]
        public ActionResult StudyAddEdit(Study studyModel, FormCollection collection)
        {
            if (!ModelState.IsValid)
            {
                return View(studyModel);
            }

            int newStudyId = 0;
            
            try
            {
                using (var db = new StudyContext())
                {
                    if (studyModel.Id > 0)
                    {
                        var result = db.Studies.SingleOrDefault(s => s.Id == studyModel.Id);
                        if (result != null)
                        {
                            var dbSS = new Study_Survey_ScheduleContext();

                            dbSS.SSSs.RemoveRange(dbSS.SSSs.Where(s => s.StudyId == studyModel.Id));
                            dbSS.SaveChanges();

                            result.Name = studyModel.Name;
                            result.Status = studyModel.Status;
                            db.SaveChanges();
                            newStudyId = studyModel.Id;
                        }
                    }
                    else
                    {
                        Study study = new Study { Name = studyModel.Name, Status = studyModel.Status };
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
                ModelState.AddModelError("", ex.Message);
                return View(studyModel);
            }

            return RedirectToAction("Index", "Study");
        }
    }
}
