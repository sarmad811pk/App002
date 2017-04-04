using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace SurveyApp.Controllers
{
    [Authorize]
    public class StatisticsController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}
