using System;
using System.Data;
using System.Data.OleDb;

namespace Infrastructure.Helpers
{
   public class ExcelHelp
    {
        //public static OleDbConnection GetOlbConn(string fileName)
        //{
        //    OleDbConnection conn;

        //    var strConn = "Provider=Microsoft.ACE.OLEDB.12.0;" + "Data Source=" + fileName + ";" + "Extended Properties='Excel 12.0;HDR=Yes;IMEX=1;'";

        //    conn = new OleDbConnection(strConn);

        //    return conn;
        //}


        //public static DataSet ConnExcel(string path)
        //{
        //    FileInfo file = new FileInfo(path);
        //    string connectionString = string.Empty;
        //    DataSet result;
        //    try
        //    {
        //        OleDbConnection oleDbConnection = new OleDbConnection();
        //        oleDbConnection = GetOlbConn(file.FullName);
        //        oleDbConnection.Open();
        //        //返回Excel的架构，包括各个sheet表的名称,类型，创建时间和修改时间等  
        //        DataTable dtSheetName = oleDbConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "Table" });
        //        string tableName = "Sheet1$";//dtSheetName.Rows[0]["TABLE_NAME"].ToString();
        //        OleDbDataAdapter oleDbDataAdapter = new OleDbDataAdapter("SELECT * FROM [" + tableName + "]", oleDbConnection);
        //        DataSet dataSet = new DataSet();
        //        oleDbDataAdapter.Fill(dataSet);

        //        //关闭连接，释放资源
        //        oleDbConnection.Close();
        //        oleDbConnection.Dispose();
        //        result = dataSet;
        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        HttpContext.Current.Response.Write("<script>alert('没有安装Excel或者导入Excel文档不是默认的Sheet1');</script>");
        //    }
        //    result = null;
        //    return result;
        //}
    }
}
