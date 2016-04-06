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

                            ctContext.Child_Teachers.Add(objCT);
                        }
                    }

                    ctContext.SaveChanges();
                }
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
