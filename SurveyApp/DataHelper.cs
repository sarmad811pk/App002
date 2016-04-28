using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Configuration;
using System.Web;
using System.Data.Entity;
using System.Linq;

namespace SurveyApp
{
    public class DataHelper
    {
        //comment
        public static DataSet ExecuteCommandAsDataSet(SqlCommand Command) 
        {
            DataSet ds = new DataSet();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
            {
                Command.Connection = conn;
                Command.CommandTimeout = 10000;
                if (Command.CommandType == CommandType.StoredProcedure)
                    Command.CommandType = CommandType.StoredProcedure;
                SqlDataAdapter da = new SqlDataAdapter(Command);
                da.Fill(ds);
                conn.Close();
                return ds;
            }
        }
        public static DataTable ExecuteCommandAsDataTable(SqlCommand Command)
        {
            DataSet ds = new DataSet();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
            {
                Command.Connection = conn;
                Command.CommandTimeout = 10000;
                if (Command.CommandType == CommandType.StoredProcedure)
                    Command.CommandType = CommandType.StoredProcedure;
                SqlDataAdapter da = new SqlDataAdapter(Command);
                da.Fill(ds);
                conn.Close();
                return ds.Tables[0];
            }
        }
        public static int ExecuteCommandAsNonQuery(SqlCommand Command)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
            {
                Command.Connection = conn;
                Command.CommandTimeout = 10000;
                if (Command.CommandType == CommandType.StoredProcedure)
                    Command.CommandType = CommandType.StoredProcedure;
                Command.Connection.Open();
                int r = Command.ExecuteNonQuery();
                conn.Close();
                return r;
            }
        }
        public static DataSet QuestionGetbySurveyID(int SurveyID, int childid)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@SurveyID", SqlDbType.Int);
            cmd.Parameters["@SurveyID"].Value = SurveyID;

            cmd.Parameters.Add("@childID", SqlDbType.Int);
            cmd.Parameters["@childID"].Value = Convert.ToInt32(childid);

            cmd.CommandText = "Question_GetbySurveyID";
            return DataHelper.ExecuteCommandAsDataSet(cmd);
        }
        public static DataSet QuestionGetFilledAnswers(int userId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@UserId", SqlDbType.Int);
            cmd.Parameters["@UserId"].Value = userId;

            cmd.CommandText = "Question_GetFilledAnswers";
            return DataHelper.ExecuteCommandAsDataSet(cmd);
        }        
        public static DataSet SurveyGetByID(int SurveyID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@SurveyID", SqlDbType.Int);
            cmd.Parameters["@SurveyID"].Value = SurveyID;

            cmd.CommandText = "Survey_GetByID";
            return DataHelper.ExecuteCommandAsDataSet(cmd);
        }

        public static DataSet SurveyGetAll()
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            

            cmd.CommandText = "Survey_GetAll";
            return DataHelper.ExecuteCommandAsDataSet(cmd);
        }

        public static DataSet SchoolGetAll()
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.CommandText = "School_GetAll";
            return DataHelper.ExecuteCommandAsDataSet(cmd);
        }
        public static DataSet ScheduleGetAll()
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.CommandText = "Schedule_GetAll";
            return DataHelper.ExecuteCommandAsDataSet(cmd);
        }
        public static DataSet ScheduleGetAllOccurence()
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.CommandText = "Schedule_GetAllOccurence";
            return DataHelper.ExecuteCommandAsDataSet(cmd);
        }

        public static DataSet StudyGetByID(int Id)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@ID", SqlDbType.Int);
            cmd.Parameters["@ID"].Value = Id;

            cmd.CommandText = "Study_GetByID";
            return DataHelper.ExecuteCommandAsDataSet(cmd);
        }

        public static DataSet StudyGetAll()
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.CommandText = "Study_GetAll";
            return DataHelper.ExecuteCommandAsDataSet(cmd);
        }

        public static DataSet Child_TeacherGetAll(int? childId = null)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@ChildID", SqlDbType.Int);
            cmd.Parameters["@ChildID"].Value = childId.HasValue ? childId : null;

            cmd.CommandText = "Child_TeacherGetAll";
            return DataHelper.ExecuteCommandAsDataSet(cmd);
        }

        public static DataSet Child_Study_TeacherGetAll(int childId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@ChildID", SqlDbType.Int);
            cmd.Parameters["@ChildID"].Value = childId;

            cmd.CommandText = "Child_Study_TeacherGetAll";
            return DataHelper.ExecuteCommandAsDataSet(cmd);
        }

        public static DataSet UserProfileGetUserByID(int UserID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@UserID", SqlDbType.Int);
            cmd.Parameters["@UserID"].Value = UserID;

            cmd.CommandText = "UserProfile_GetUserByID";
            return DataHelper.ExecuteCommandAsDataSet(cmd);
        }

        public static List<SurveyApp.Models.UserProfile> Parent_GetAll()
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.CommandText = "Parent_GetAll";
            DataTable dt = DataHelper.ExecuteCommandAsDataTable(cmd);
            
            var convertedList = (from rw in dt.AsEnumerable()
                                 select new SurveyApp.Models.UserProfile()
                                 {
                                     UserId = Convert.ToInt32(rw["UserId"]),
                                     UserName = rw["UserName"].ToString(),
                                     FullName = Convert.ToString(rw["FullName"])
                                 }).ToList();

            return convertedList;

        }

        public static DataSet UserManagementGetUsers(int? UserID = null)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@UserID", SqlDbType.Int);
            if (UserID.HasValue)
            {
                cmd.Parameters["@UserID"].Value = UserID.Value;
            }
            else
            {
                cmd.Parameters["@UserID"].Value = DBNull.Value;
            }
            cmd.CommandText = "UserManagement_GetUsers";
            return DataHelper.ExecuteCommandAsDataSet(cmd);
        }

        public static DataSet ChildGetAll()
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.CommandText = "Child_GetAll";
            return DataHelper.ExecuteCommandAsDataSet(cmd);
        }
        public static int SaveuserQuestions(int UserID, string  questionid, string answerid, string score, string childid, string SurveyID, string status, string percentage, DateTime dtSchedule)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@UserID", SqlDbType.Int);
            if (UserID != null) { cmd.Parameters["@UserID"].Value = UserID; } else { cmd.Parameters["@UserID"].Value = DBNull.Value; }
           
            
            
            cmd.Parameters.Add("@QuestionID", SqlDbType.Int);
            if (questionid != "") { cmd.Parameters["@QuestionID"].Value = questionid; } else { cmd.Parameters["@QuestionID"].Value = DBNull.Value; }

           
        
            cmd.Parameters.Add("@PAnswer", SqlDbType.NVarChar);
            if (answerid != "") { cmd.Parameters["@PAnswer"].Value = answerid; } else { cmd.Parameters["@PAnswer"].Value=DBNull.Value; }

            cmd.Parameters.Add("@Score", SqlDbType.Int);
            if (score != "" && score!="NaN") { cmd.Parameters["@Score"].Value = Convert.ToInt32(score); } else { cmd.Parameters["@Score"].Value = DBNull.Value; }
           

            cmd.Parameters.Add("@ChildId", SqlDbType.Int);
            if (childid != "") { cmd.Parameters["@ChildId"].Value = Convert.ToInt32(childid); } else { cmd.Parameters["@ChildId"].Value = DBNull.Value; }
            


            cmd.Parameters.Add("@SurveyID", SqlDbType.Int);
            if(SurveyID!=""){  cmd.Parameters["@SurveyID"].Value = Convert.ToInt32(SurveyID);} else {cmd.Parameters["@SurveyID"].Value = DBNull.Value;}

       
            cmd.Parameters.Add("@Pstatus", SqlDbType.NVarChar);
            if (status != "") { cmd.Parameters["@Pstatus"].Value = status; } else { cmd.Parameters["@Pstatus"].Value=DBNull.Value; }

            cmd.Parameters.Add("@Percentage", SqlDbType.NVarChar);
            if (percentage != "") { cmd.Parameters["@Percentage"].Value = percentage; } else { cmd.Parameters["@Percentage"].Value = DBNull.Value; }

            cmd.Parameters.Add("@ScheduleDate", SqlDbType.NVarChar);
            if (dtSchedule != DateTime.MinValue) { cmd.Parameters["@ScheduleDate"].Value = dtSchedule; } else { cmd.Parameters["@ScheduleDate"].Value = DBNull.Value; }
            

            cmd.CommandText = "SaveUserQuestions";
            return DataHelper.ExecuteCommandAsNonQuery(cmd);



        }


        public static DataSet GetChildbyUserID(int Id)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@ID", SqlDbType.Int);
            cmd.Parameters["@ID"].Value = Id;

            cmd.CommandText = "Getchild_ByUserID";
            return DataHelper.ExecuteCommandAsDataSet(cmd);
        }

        public static DataSet RolesGetAll()
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.CommandText = "Roles_GetAll";
            return DataHelper.ExecuteCommandAsDataSet(cmd);
        }

        public static DataSet ScheduleDeviationGetSchedules(int? studyId = null)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@StudyID", SqlDbType.Int);
            cmd.Parameters["@StudyID"].Value = studyId;

            cmd.CommandText = "ScheduleDeviation_GetSchedules";
            return DataHelper.ExecuteCommandAsDataSet(cmd);
        }

        public static DataSet getChildrenSchedulesByUserId(int userId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@UserID", SqlDbType.Int);
            cmd.Parameters["@UserID"].Value = userId;

            cmd.CommandText = "User_GetChildSchedules";
            return DataHelper.ExecuteCommandAsDataSet(cmd);
        }

        #region Dashborad
        public static DataSet DashboardGetByStudyId(int? studyId = 0)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@StudyId", SqlDbType.Int);
            cmd.Parameters["@StudyId"].Value = studyId;

            cmd.CommandText = "Dashboard_GetByStudyId";
            return DataHelper.ExecuteCommandAsDataSet(cmd);            
        }

        public static DataSet DashboardGetSchedulesByStudyId(int? studyId = 0)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@StudyId", SqlDbType.Int);
            cmd.Parameters["@StudyId"].Value = studyId;

            cmd.CommandText = "Dashboard_GetSchedulesByStudyId";
            return DataHelper.ExecuteCommandAsDataSet(cmd);
        }

        public static DataSet DashboardGetSurveyQAInfo(int surveyId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@SurveyId", SqlDbType.Int);
            cmd.Parameters["@SurveyId"].Value = surveyId;

            cmd.CommandText = "Dashboard_GetSurveyQAInfo";
            return DataHelper.ExecuteCommandAsDataSet(cmd);
        }
        public static DataSet DashboardGetSchedulesForUserRoles(int? studyId = 0)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@StudyId", SqlDbType.Int);
            cmd.Parameters["@StudyId"].Value = studyId;

            cmd.CommandText = "Dashboard_GetSchedulesForUserRoles";
            return DataHelper.ExecuteCommandAsDataSet(cmd);
        }
        #endregion

    }
}