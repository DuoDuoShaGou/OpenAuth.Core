//using AutoMapper.Configuration;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Helpers
{
    public class DbHelper
	{
		private IConfiguration _configuration { get; }
		private string connstr = "";

		public DbHelper(IConfiguration configuration)
        {
			_configuration = configuration;
		}

		public string GetConnectString()
		{
			return _configuration?.GetSection("ConnectionStrings")?["ERPDBContext"];
			//return connstr = _configuration.GetConnectionString("ERPDBContext");
		}

		public DataTable QueryTable(string sql)
		{
			DataTable dt = new DataTable();

			using (SqlDataAdapter adapter = new SqlDataAdapter(sql, GetConnectString()))
			{
				adapter.Fill(dt);
			}

			return dt;
		}

		public int InserTable(string sql)
		{
			using (SqlConnection conn = new SqlConnection(GetConnectString()))
			{
				conn.Open();
				SqlCommand sqlCommand = new SqlCommand(sql, conn);
				return sqlCommand.ExecuteNonQuery();
			}
		}

        /// <summary>
        /// 执行数组Sql语句
        /// </summary>
        /// <param name="sqlList"></param>
        /// <returns></returns>
        public int ExecuteNonQuery(ArrayList sqlList)
        {
            int result;
            using (SqlConnection sqlConnection = new SqlConnection(GetConnectString()))
            {
                int num = 0;
                sqlConnection.Open();
                SqlCommand sqlCommand = new SqlCommand();
                sqlCommand.Connection = sqlConnection;
                SqlTransaction sqlTransaction = sqlConnection.BeginTransaction();
                sqlCommand.Transaction = sqlTransaction;
                try
                {
                    for (int i = 0; i < sqlList.Count; i++)
                    {
                        string text = sqlList[i].ToString();
                        if (text.Trim().Length > 1)
                        {
                            sqlCommand.CommandText = text;
                            num += sqlCommand.ExecuteNonQuery();
                        }
                    }
                    sqlTransaction.Commit();
                }
                catch (SqlException ex)
                {
                    sqlTransaction.Rollback();
                    throw;
                }
                finally
                {
                    sqlCommand.Dispose();
                    sqlConnection.Close();
                }
                result = num;
            };
            return result;
        }

        public object ExecuteScalar(string sqlText,params SqlParameter[] parameters)
        {
            using (SqlConnection conn = new SqlConnection(GetConnectString()))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    conn.Open();
                    cmd.CommandText = sqlText;
                    cmd.Parameters.AddRange(parameters);
                    return cmd.ExecuteScalar();
                }
            }
        }

        public int ExecuteNonQuery(string text)
        {
            int result;
            using (SqlConnection sqlConnection = new SqlConnection(GetConnectString()))
            {
                int num = 0;
                sqlConnection.Open();
                SqlCommand sqlCommand = new SqlCommand();
                sqlCommand.Connection = sqlConnection;
                try
                {
                    sqlCommand.CommandText = text;
                    num += sqlCommand.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    throw;
                }
                finally
                {
                    sqlCommand.Dispose();
                    sqlConnection.Close();
                }
                result = num;
            };
            return result;
        }
    }
}
