﻿using System.Collections.Generic;
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
            sendReminderEmails(log);
        }

        public static void sendReminderEmails(TextWriter log)
        {
            try
            {
                DataSet dsReminders = DataHelper.getScheduleReminders();

                List<string> lstEmails = new List<string>();
                if (dsReminders != null && dsReminders.Tables[0] != null && dsReminders.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dr in dsReminders.Tables[0].Rows)
                    {
                        if (dr["UserName"] != DBNull.Value)
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
    }
}
