using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace SurveyApp.Controllers
{
    [System.Web.Mvc.Authorize]
    public class ReportController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult AccountRequest()
        {
            return View();
        }
        public ActionResult Completion()
        {
            return View();
        }
    }
}
