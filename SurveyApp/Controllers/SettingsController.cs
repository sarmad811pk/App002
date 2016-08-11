using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SurveyApp.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        //
        // GET: /Settings/

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult SurveyEditor(int? SurveyID) {
            ViewData["SurveyID"] = SurveyID ?? 1;
            return View();
        }
    }
}
