using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Configuration;
using System.Web;

namespace SurveyApp
{
    public class DataHelper
    {
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
        
    }
}