using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using System.Data;
using System;

namespace SurveyReminder
{
    public class Functions
    {
        // This function will be triggered based on the schedule you have set for this WebJob
        // This function will enqueue a message on an Azure Queue called queue
        [NoAutomaticTrigger]
        public static void ManualTrigger(TextWriter log, int value, [Queue("queue")] out string message)
        {
            log.WriteLine("Function is invoked with value={0}", value);
            message = value.ToString();
            log.WriteLine("Following message will be written on the Queue={0}", message);
            sendEmailsToRespondents(log);
        }

        public static void sendReminderEmails(TextWriter log)
        {
            try
            {
                DataSet dsReminders = DataHelper.getScheduleRemindersFixed();

                List<string> lstEmails = new List<string>();
                if (dsReminders != null && dsReminders.Tables[0] != null && dsReminders.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dr in dsReminders.Tables[0].Rows)
                    {
                        if (dr["UserName"] != DBNull.Value && lstEmails.Contains(dr["UserName"].ToString()) == false)
                        {
                            lstEmails.Add(dr["UserName"].ToString());
                        }
                    }
                }

                if(lstEmails.Count > 0)
                {
                    string body = "";
                    string path = System.IO.Directory.GetCurrentDirectory() + @"\Reminder.html";
                    using (System.IO.StreamReader reader = new System.IO.StreamReader(path))
                    {
                        body = reader.ReadToEnd();
                    }
                    body = body.Replace("_ROOTPATH_", System.Configuration.ConfigurationManager.AppSettings["_RootPath"].ToString());

                    List<string> lstOurEmail = new List<string>();
                    lstOurEmail.Add("no-reply@ucsfebit.azurewebsites.net");

                    SMTPHelper.SendGridEmail("eBit - Reminder", body, lstOurEmail, true, null, lstEmails);
                }
                foreach (string usr in lstEmails)
                {
                    log.WriteLine("Emails successfully sent to " + usr);
                }
            }
            catch(Exception ex)
            {
                log.WriteLine("Exception:" + ex.StackTrace);
            }
            
        }

        public static void sendEmailsToRespondents(TextWriter log) {
            try
            {
                DataSet dsRespondents = DataHelper.getRespondents(DateTime.Now);
                
                if (dsRespondents != null && dsRespondents.Tables[0] != null && dsRespondents.Tables[0].Rows.Count > 0)
                {
                    string body = "";
                    string path = System.IO.Directory.GetCurrentDirectory() + @"\Reminder_Respondent.html";
                    using (System.IO.StreamReader reader = new System.IO.StreamReader(path))
                    {
                        body = reader.ReadToEnd();
                    }

                    List<RespondentEmail> lstResps = new List<RespondentEmail>();
                    foreach (DataRow dr in dsRespondents.Tables[0].Rows)
                    {

                        if (dr["UserName"] != DBNull.Value)
                        {
                            
                            if(lstResps.Find(ls => ls.email == dr["UserName"].ToString()) == null)
                            {
                                RespondentEmail objRE = new RespondentEmail();
                                objRE.email = dr["UserName"].ToString();
                                objRE.userId = Convert.ToInt32(dr["RespondentId"]);
                                lstResps.Add(objRE);
                            }
                        }
                    }

                    foreach (RespondentEmail obj in lstResps)
                    {
                        if (String.IsNullOrEmpty(obj.email) == false)
                        {
                            DataRow[] drSurveys = dsRespondents.Tables[0].Select("RespondentId = " + obj.userId);
                            string html = body;
                            string pwd = "";
                            string tbl = "<table border='1' style='text-align:left;'><thead><tr><th>Child</th><th>Study</th><th>Survey</th><th>Completetion %</th><th>End Date</th></tr></thead><tbody>";
                            foreach (DataRow dr in drSurveys)
                            {
                                tbl += "<tr><td>" +
                                    (dr["Child"] != DBNull.Value ? dr["Child"].ToString() : " - ") + "</td><td>" +
                                    (dr["Study"] != DBNull.Value ? dr["Study"].ToString() : " - ") + "</td><td>" +
                                    (dr["Survey"] != DBNull.Value ? dr["Survey"].ToString() : " - ") + "</td><td>" +
                                    (dr["Percentage"] != DBNull.Value ? dr["Percentage"].ToString() + "%" : " - ") + "</td><td>" +
                                    (dr["ScheuleEndDate"] != DBNull.Value ? Convert.ToDateTime(dr["ScheuleEndDate"]).ToString("MM/dd/yyyy") : " - ") + "</td></tr>";
                                pwd = dr["pwd"] != DBNull.Value ? dr["pwd"].ToString() : "";
                            }
                            tbl += "</tbody></table>";

                            html = html.Replace("_ROOTPATH_", System.Configuration.ConfigurationManager.AppSettings["_RootPath"].ToString());
                            html = html.Replace("_USERNAME_", obj.email);
                            html = html.Replace("_PASSWORD_", pwd);
                            html = html.Replace("_SURVEYLIST_", tbl);


                            List<string> lstOurEmail = new List<string>();
                            lstOurEmail.Add("no-reply@ucsfebit.azurewebsites.net");

                            List<string> ems = new List<string>();
                            ems.Add(obj.email);

                            SMTPHelper.SendGridEmail("eBit - Reminder", html, ems, true, null, null);
                        }

                        log.WriteLine("Emails successfully sent to " + obj.email);
                    }
                    

                }
            }
            catch(Exception ex)
            {

            }
        }
    }

    public class RespondentEmail
    {
        public int userId { get; set; }
        public string email { get; set; }
        public string name { get; set; }
        public int userType { get; set; }
    }
}
