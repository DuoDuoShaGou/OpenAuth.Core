using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Infrastructure;
using System.Threading.Tasks;
using Infrastructure.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using OpenAuth.App.Interface;
using OpenAuth.Mvc.Models;
using System.Text;

namespace OpenAuth.Mvc.Controllers
{
    public class OrderController : BaseController
    {
       // private readonly OrderApp _app;
        private readonly IHostingEnvironment _hostingEnvironment;
        string _targetFilePath = "";
        private IConfiguration _configuration { get; }

        private DbHelper _dbHelper;

        public OrderController(IAuth auth, IHostingEnvironment hostingEnvironment, IConfiguration configuration) : base(auth)
        {
            _hostingEnvironment = hostingEnvironment;
             _targetFilePath = _hostingEnvironment.WebRootPath;
            _dbHelper = new DbHelper(configuration);
        }
        public IActionResult Index()
        {
            return View();
        }

        public string GetOrderList(string keyword, string startPODate, string endPODate, string startShipDate, string endShipDate,string Status)
        {
            StringBuilder str = new StringBuilder();

             string sql = string.Format(@"SELECT [PO_NO], [ITEM], [Customer], [Buyer], [PO_Date], [Name], [Description], [Type], [Project], [Qty], [Price], [Required_Shipping_Date], [Delivery_Point], [Status] FROM XY_POS WHERE 1=1 ");

             str.Append(sql);

            if(!string.IsNullOrEmpty(keyword))
            {
                str.Append(" AND (PO_NO LIKE '%"+ keyword + "%' OR Buyer LIKE '%" + keyword + "%' OR Customer LIKE '%" + keyword + "%' OR Name LIKE '%" + keyword + "%')");
            }

            if(!string.IsNullOrEmpty(startPODate))
            {
                str.Append(" AND PO_Date >='"+startPODate+"'");
            }

            if (!string.IsNullOrEmpty(endPODate))
            {
                str.Append(" AND PO_Date <='" + endPODate + "'");
            }

            if (!string.IsNullOrEmpty(startShipDate))
            {
                str.Append(" AND Required_Shipping_Date >='" + startShipDate + "'");
            }

            if (!string.IsNullOrEmpty(endShipDate))
            {
                str.Append(" AND Required_Shipping_Date <='" + endShipDate + "'");
            }

            if (!string.IsNullOrEmpty(Status))
            {
                str.Append(" AND Status = " + Status);
            }

            var dt = _dbHelper.QueryTable(str.ToString());

            var list = ObjectHelper<OrderView>.ConvertToModel(dt);

            var json = list.ToJson();

            return json;
        }

        public IActionResult AddOrder(OrderView order)
        {
            string sql = $" SELET ITM FROM XY_POS WHERE PO_NO = '{ order.PO_NO }';";
            var item = _dbHelper.ExecuteScalar(sql);

            if (item != null)
            {
                int.TryParse(item.ToString(), out int itm);
                order.ITEM = (itm + 1).ToString();
            }
            else {
                order.ITEM = "1";
            }
            string str = string.Concat(new string[]
                        {
                            "INSERT INTO [DB_PHLP].[dbo].[XY_POS] VALUES('",
                            order.PO_NO,
                            "',",
                            order.ITEM,
                            ",'",
                            order.Customer,
                            "','",
                            order.Buyer,
                            "','",
                            order.PO_Date,
                            "','",
                            order.Name,
                            "','",
                            order.Description,
                            "','",
                            order.Type,
                            "','",
                            order.Project,
                            "',",
                            order.Qty.ToString()??"0",
                            ",",
                            order.Price.ToString()??"0",
                            ",'",
                            order.Required_Shipping_Date,
                            "','",
                            order.Delivery_Point,
                            "',0);"
                        });
            if (_dbHelper.ExecuteNonQuery(str) > 0)
                return Json(new { code = 0, msg = "操作成功！", data = new { src = "" } }.ToJson());
            return Json(new { code = 0, msg = "操作失败！", data = new { src = "" } }.ToJson());
        }

        public IActionResult DeleteOrder(OrderView order)
        {
            if(!string.IsNullOrEmpty(order.PO_NO)&&!string.IsNullOrEmpty(order.ITEM))
            {
                string str = "DELETE  FROM [XY_POS] WHERE PO_NO='" + order.PO_NO + "' AND ITEM = " + order.ITEM;
                if (_dbHelper.ExecuteNonQuery(str) > 0)
                    return Json(new { code = 0, msg = "删除成功！", data = new { src = "" } }.ToJson());
            }
            return Json(new { code = 0, msg = "删除失败！", data = new { src = "" } }.ToJson());
        }

        public IActionResult UpdateOrder(OrderView order)
        {
            string str = string.Concat(new string[]
                 {
                    "UPDATE [XY_POS] SET Customer = '",
                    order.Customer,
                    "', Buyer = '",
                    order.Buyer,
                    "', PO_Date = '",
                    order.PO_Date,
                    "', Name = '",
                    order.Name,
                    "', Description = '",
                    order.Description,
                    "', Type = '",
                    order.Type,
                    "', Project = '",
                    order.Project,
                    "', Qty = ",
                    order.Qty.ToString()??"0",
                    ", Price = ",
                    order.Price.ToString()??"0",
                    ", Required_Shipping_Date ='",
                    order.Required_Shipping_Date,
                    "', Delivery_Point = '",
                    order.Delivery_Point,
                    "';"
                 });
            if (_dbHelper.ExecuteNonQuery(str) > 0)
                return Json(new { code = 0, msg = "操作成功！", data = new { src = "" } }.ToJson());
            return Json(new { code = 0, msg = "操作失败！", data = new { src = "" } }.ToJson());
        }


        public async Task<IActionResult> ImportOrderList(IFormFile file)
        {
            string ReturnValue = string.Empty;
            //定义一个bool类型的变量用来做验证
             bool flag = true;
            try
            {
                string fileExt = Path.GetExtension(file.FileName).ToLower();
                //定义一个集合一会儿将数据存储进来,全部一次丢到数据库中保存
                var Data = new List<OrderView>();
                MemoryStream ms = new MemoryStream();
                file.CopyTo(ms);
                ms.Seek(0, SeekOrigin.Begin);
                IWorkbook book;
                if (fileExt == ".xlsx")
                {
                    book = new XSSFWorkbook(ms);
                }
                else if (fileExt == ".xls")
                {
                    book = new HSSFWorkbook(ms);
                }
                else
                {
                    book = null;
                }
                ISheet sheet = book.GetSheetAt(0);

                int CountRow = sheet.LastRowNum + 1;//获取总行数

                if (CountRow - 1 == 0) ReturnValue = "Excel列表数据项为空!";


               ArrayList list = new ArrayList();

                #region 循环验证
                for (int i = 1; i < CountRow; i++)
                {
                    //获取第i行的数据
                    var row = sheet.GetRow(i);
                    if (row != null)
                    {
                        //循环的验证单元格中的数据
                        for (int j = 0; j < 5; j++)
                        {
                            if (row.GetCell(j) == null || row.GetCell(j).ToString().Trim().Length == 0)
                            {
                                flag = false;
                                ReturnValue += $"第{i + 1}行,第{j + 1}列数据不能为空。";
                            }
                        }
                    }
                }
                #endregion
                if (flag)
                {
                    for (int i = 1; i < CountRow; i++)
                    {
                        //实例化实体对象
                        OrderView order = new OrderView();
                        var row = sheet.GetRow(i);

                        if (row.GetCell(0) != null && row.GetCell(0).ToString().Trim().Length > 0)
                        {
                            order.PO_NO = row.GetCell(0).ToString();
                        }
                        if (row.GetCell(1) != null && row.GetCell(1).ToString().Trim().Length > 0)
                        {
                            order.Customer = row.GetCell(1).ToString();
                        }
                        if (row.GetCell(2) != null && row.GetCell(2).ToString().Trim().Length > 0)
                        {
                            order.Buyer = row.GetCell(2).ToString();
                        }
                        if (row.GetCell(3) != null && row.GetCell(3).ToString().Trim().Length > 0)
                        {
                            order.PO_Date = row.GetCell(3).DateCellValue.ToString();
                        }
                        if (row.GetCell(4) != null && row.GetCell(4).ToString().Trim().Length > 0)
                        {
                            order.Name = row.GetCell(4).ToString();
                        }
                        if (row.GetCell(5) != null && row.GetCell(5).ToString().Trim().Length > 0)
                        {
                            order.Description = row.GetCell(5).ToString();
                        }
                        if (row.GetCell(6) != null && row.GetCell(6).ToString().Trim().Length > 0)
                        {
                            order.Type = row.GetCell(6).ToString();
                        }
                        if (row.GetCell(7) != null && row.GetCell(7).ToString().Trim().Length > 0)
                        {
                            order.Project = row.GetCell(7).ToString();
                        }
                        if (row.GetCell(8) != null && row.GetCell(8).ToString().Trim().Length > 0)
                        {
                            order.Qty = row.GetCell(8).ToString();
                        }
                        if (row.GetCell(9) != null && row.GetCell(9).ToString().Trim().Length > 0)
                        {
                            order.Price = row.GetCell(9).ToString();
                        }
                        if (row.GetCell(10) != null && row.GetCell(10).ToString().Trim().Length > 0)
                        {
                            order.Required_Shipping_Date = row.GetCell(10).DateCellValue.ToString();
                        }
                        if (row.GetCell(11) != null && row.GetCell(11).ToString().Trim().Length > 0)
                        {
                            order.Delivery_Point = row.GetCell(11).ToString();
                        }

                        string str = string.Concat(new string[]
                        {
                            "INSERT INTO [DB_PHLP].[dbo].[XY_POS] VALUES('",
                            order.PO_NO,
                            "',",
                            i.ToString(),
                            ",'",
                            order.Customer,
                            "','",
                            order.Buyer,
                            "','",
                            order.PO_Date,
                            "','",
                            order.Name,
                            "','",
                            order.Description,
                            "','",
                            order.Type,
                            "','",
                            order.Project,
                            "',",
                            order.Qty.ToString()??"0",
                            ",",
                            order.Price.ToString()??"0",
                            ",'",
                            order.Required_Shipping_Date,
                            "','",
                            order.Delivery_Point,
                            "',0);"
                        });

                        list.Add(str);
                    }
                    var num = _dbHelper.ExecuteNonQuery(list);
                    if (num == CountRow - 1)
                        return Json(new { code = 0, msg = $"数据导入成功,共导入{CountRow - 1}条数据。", data = new { src = "" } }.ToJson());
                }
                else ReturnValue = "数据存在问题！" + ReturnValue;
            }
            catch (Exception ex)
             {
               ReturnValue = "服务器异常";
            }
            return Json(new { code = 10, msg =  ReturnValue, data = new { src = "" } }.ToJson());
        }


        public IActionResult ExportOrderList(string keyword, string startPODate, string endPODate, string startShipDate, string endShipDate)
        {
            StringBuilder str = new StringBuilder();

            string sql = string.Format(@"SELECT [PO_NO], [ITEM], [Customer], [Buyer], [PO_Date], [Name], [Description], [Type], [Project], [Qty], [Price], [Required_Shipping_Date], [Delivery_Point] FROM XY_POS WHERE 1=1 ");

            str.Append(sql);

            if (!string.IsNullOrEmpty(keyword))
            {
                str.Append(" AND (PO_NO LIKE '%" + keyword + "%' OR Buyer LIKE '%" + keyword + "%' OR Customer LIKE '%" + keyword + "%' OR Name LIKE '%" + keyword + "%'");
            }

            if (!string.IsNullOrEmpty(startPODate))
            {
                str.Append(" AND PO_Date >='" + startPODate + "'");
            }

            if (!string.IsNullOrEmpty(endPODate))
            {
                str.Append(" AND PO_Date <='" + endPODate + "'");
            }

            if (!string.IsNullOrEmpty(startShipDate))
            {
                str.Append(" AND Required_Shipping_Date >='" + startShipDate + "'");
            }

            if (!string.IsNullOrEmpty(endShipDate))
            {
                str.Append(" AND Required_Shipping_Date <='" + endShipDate + "'");
            }

            var dt = _dbHelper.QueryTable(sql);


            IWorkbook workbook = new XSSFWorkbook();

            var sheet = workbook.CreateSheet("Sheet");
            //表头
            var row = sheet.CreateRow(0);

            for (var i = 0; i < dt.Columns.Count; i++)
            {
                var cell = row.CreateCell(i);
                //列名称,数据库中字段
                var columnName = dt.Columns[i].ColumnName;
                cell.SetCellValue(columnName);
            }

            //数据
            for (var i = 0; i < dt.Rows.Count; i++)
            {
                var row1 = sheet.CreateRow(i + 1);
                for (var j = 0; j < dt.Columns.Count; j++)
                {
                    var cell = row1.CreateCell(j);
                    cell.SetCellValue(dt.Rows[i][j].ToString());
                }
            }

             MemoryStream stream = new MemoryStream();

            workbook.Write(stream);

            var buf = stream.ToArray();

            Response.ContentType = "application/octet-stream";

            Response.ContentLength = buf.Length;//在Response的Header中设置下载文件的大小，这样客户端浏览器才能正确显示下载的进度

            var contentDisposition = "attachment;" + "filename="+ DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")+".xls";//在Response的Header中设置下载文件的文件名，这样客户端浏览器才能正确显示下载的文件名，注意这里要用HttpUtility.UrlEncode编码文件名，否则有些浏览器可能会显示乱码文件名

            Response.Headers.Add("Content-Disposition", new string[] { contentDisposition });

            Response.Body.WriteAsync(buf, 0, buf.Length);

            return new EmptyResult();
        }

    }
}
