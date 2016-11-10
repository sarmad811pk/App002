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
    [Authorize]
    public class StudyController : Controller
    {
        //
        // GET: /Study/
        public StudyController() {
            Database.SetInitializer<StudyContext>(null);
            Database.SetInitializer<Child_Survey_ScheduleContext>(null);
            Database.SetInitializer<Child_Study_RespondentContext>(null);
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

            if (studyModel.Id <= 0 && doesStudyExist(studyModel.Name))
            {
                ModelState.AddModelError("", "This study already exists, please provide different details.");
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
                                    int[] parentSchedules = collection["Parent" + i] != null ? Array.ConvertAll(collection["Parent" + i].ToString().Split(','), int.Parse) : null;
                                    int[] teacherSchedules = collection["Teacher" + i] != null ? Array.ConvertAll(collection["Teacher" + i].ToString().Split(','), int.Parse) : null;
                                    int[] childSchedules = collection["Teacher" + i] != null ? Array.ConvertAll(collection["Child" + i].ToString().Split(','), int.Parse) : null;

                                    int parentSchCount = parentSchedules != null ? parentSchedules.Length : 0;
                                    int teacherSchCount = teacherSchedules != null ? teacherSchedules.Length : 0;
                                    int childSchCount = childSchedules != null ? childSchedules.Length : 0;

                                    int maxSchCount = 0;
                                    if(parentSchCount > teacherSchCount)
                                    {
                                        maxSchCount = parentSchCount;
                                    }
                                    else
                                    {
                                        maxSchCount = teacherSchCount;
                                    }

                                    if(maxSchCount < childSchCount)
                                    {
                                        maxSchCount = childSchCount;
                                    }

                                    for(int index = 0; index < maxSchCount; index++)
                                    {
                                        int pId = parentSchedules != null && parentSchedules.Length > index ? parentSchedules[index] : 0;
                                        int tId = teacherSchedules != null && teacherSchedules.Length > index ? teacherSchedules[index] : 0;
                                        int cId = teacherSchedules != null && childSchedules.Length > index ? childSchedules[index] : 0;

                                        int surveyId = Convert.ToInt32(collection["Survey" + i]);
                                        if(surveyId == 6)
                                        {
                                            for(int pedsIndex = 6; pedsIndex <= 9; pedsIndex++)
                                            {
                                                Study_Survey_Schedule sss = new Study_Survey_Schedule { StudyId = newStudyId, SurveyId = pedsIndex, ScheduleIdParent = (pId), ScheduleIdTeacher = tId, ScheduleIdChild = cId };
                                                dbSS.SSSs.Add(sss);
                                            }

                                            Study_Survey_Schedule s45 = new Study_Survey_Schedule { StudyId = newStudyId, SurveyId = 45, ScheduleIdParent = (pId), ScheduleIdTeacher = tId, ScheduleIdChild = cId };
                                            dbSS.SSSs.Add(s45);
                                        }
                                        else
                                        {
                                            Study_Survey_Schedule sss = new Study_Survey_Schedule { StudyId = newStudyId, SurveyId = surveyId, ScheduleIdParent = (pId), ScheduleIdTeacher = tId, ScheduleIdChild = cId };
                                            dbSS.SSSs.Add(sss);
                                        }                                        
                                    }
                                    
                                    dbSS.SaveChanges();
                                }
                            }
                        }
                    }                    
                }

                bool sendEmail = true;
                sendEmail = Convert.ToBoolean(collection["hdnSendEmail"]);
                
                try
                {
                    string path = Server.MapPath("~/Attachments/Survey_Assignment.html");
                    List<Child> lstChildren = new List<Child>();
                    using (var cContext = new ChildContext())
                    {
                        lstChildren = cContext.Children.ToList();
                    }

                    List<Child_Study_Respondent> lstPTStudies = new List<Child_Study_Respondent>();
                    using (var ptsContext = new Child_Study_RespondentContext())
                    {
                        lstPTStudies = ptsContext.Child_Study_Respondents.Where(pts => pts.StudyId == newStudyId).ToList();
                    }

                    List<int> lstChildrenUpdated = new List<int>();

                    foreach (Child_Study_Respondent objPTStudy in lstPTStudies)
                    {
                        foreach (Child objChild in lstChildren)
                        {
                            if (objChild.Id == objPTStudy.ChildId)
                            {
                                if (lstChildrenUpdated.Contains(objChild.Id) == false)
                                {
                                    ChildController.setChildSchedules(objChild, path, sendEmail, newStudyId);
                                    lstChildrenUpdated.Add(objChild.Id);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                    return View(studyModel);
                }
                
            }
            catch (Exception ex){
                ModelState.AddModelError("", ex.Message);
                return View(studyModel);
            }

            return RedirectToAction("Index", "Study");
        }

        public bool doesStudyExist(string name)
        {
            bool result = false;
            using (var cContext = new StudyContext())
            {
                Study objStudy = cContext.Studies.Where(s => s.Name == name).FirstOrDefault();
                if (objStudy != null && objStudy.Id > 0)
                {
                    result = true;
                }
            }

            return result;
        }
    }
}
