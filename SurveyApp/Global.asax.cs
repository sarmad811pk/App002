using SurveyApp.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using WebMatrix.WebData;

namespace SurveyApp
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            //hhSystem.Data.Entity.Database.SetInitializer(new System.Data.Entity.DropCreateDatabaseIfModelChanges<SurveyApp.Models.SchoolContext>());

            if (!WebSecurity.Initialized)
            {
                WebSecurity.InitializeDatabaseConnection("DefaultConnection", "UserProfile", "UserId", "UserName", autoCreateTables: true);
            }

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AuthConfig.RegisterAuth();
        }        

        protected void Application_Error(object sender, EventArgs e)
        {
            object enableErrorLog = System.Web.Configuration.WebConfigurationManager.AppSettings["EnableErrorLog"];
            bool isErrorLogEnabled = enableErrorLog == null ? false : Convert.ToBoolean(enableErrorLog);
            if (isErrorLogEnabled == true)
            {
                Exception exc = Server.GetLastError();

                DateTime errorTime = DateTime.Now;
                if (HttpContext.Current != null && HttpContext.Current.Request != null)
                {
                    string url = HttpContext.Current.Request.Url.ToString();
                    string urlReferrer = HttpContext.Current.Request.UrlReferrer == null ? "" : HttpContext.Current.Request.UrlReferrer.ToString();
                    string errorMessage = exc.Message;
                    string stackTrace = exc.StackTrace;
                    string userAgent = HttpContext.Current.Request.UserAgent;
                    string userHostAddress = HttpContext.Current.Request.UserHostAddress;
                    string sessionId = HttpContext.Current.Session.SessionID;
                    string userName = HttpContext.Current.User.Identity.Name;

                    SurveyApp.DataHelper.ErrorLog_Add(errorTime, url, urlReferrer, errorMessage, stackTrace, userAgent, userHostAddress, sessionId, userName);
                }
            }
        }
    }
}