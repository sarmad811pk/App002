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
        public static DataSet QuestionGetbySurveyID(int SurveyID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@SurveyID", SqlDbType.Int);
            cmd.Parameters["@SurveyID"].Value = SurveyID;

            cmd.CommandText = "Question_GetbySurveyID";
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
    }
}