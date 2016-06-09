using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SurveyApp.Models
{
    public class LifeEvent
    {
        public int ID { get; set; }
        public string EventCategory { get; set; }
        public string EventName { get; set; }
        public DateTime EventDate { get; set; }
        public DateTime DateSubmitted { get; set; }
        public int ChildID { get; set; }
        public int UserID { get; set; }
    }

    public class LifeEventChart
    {
        public int IndexOnChart { get; set; }
        public string EventsName { get; set; }
    }
}