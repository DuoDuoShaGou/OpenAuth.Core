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
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using OpenAuth.App.Interface;
using OpenAuth.Mvc.Models;
using System.Text;
using Newtonsoft.Json;
using NPOI.SS.Util;

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

        #region 订单录入
        public IActionResult Index()
        {
            return View();
        }

        public string GetOrderList(string keyword, string startPODate, string endPODate, string startShipDate, string endShipDate,string Status)
        {
            StringBuilder str = new StringBuilder();

             string sql = string.Format(@"SELECT [PO_NO], [ITEM], [Customer], [Buyer], CONVERT(varchar(100), [PO_Date], 111) PO_Date, [Name], [Description], [Type], [Project], [Qty], [Price], CONVERT(varchar(100), [Required_Shipping_Date], 111) Required_Shipping_Date, [Delivery_Point], [Status] FROM XY_POS WHERE 1=1 ");

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
            string sql = $"SELECT ITEM FROM XY_POS WHERE PO_NO = '{ order.PO_NO }';";
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
                            "',0,0,0);"
                        });
            if (_dbHelper.ExecuteNonQuery(str) > 0)
                return Json(new { code = 0, msg = "操作成功！", data = new { src = "" } }.ToJson());
            return Json(new { code = 0, msg = "操作失败！", data = new { src = "" } }.ToJson());
        }

        [HttpPost]
        public IActionResult DeleteOrder(string AllData)
        {
            int result = 0;
            List<OrderView> list = JsonConvert.DeserializeObject<List<OrderView>>(AllData);
            foreach (var order in list)
            {
                if (!string.IsNullOrEmpty(order.PO_NO) && !string.IsNullOrEmpty(order.ITEM))
                {
                    string str = "DELETE  FROM [XY_POS] WHERE PO_NO='" + order.PO_NO + "' AND ITEM = " + order.ITEM;
                    result+=_dbHelper.ExecuteNonQuery(str);
                }
            }
            if (result > 0)
                return Json(new { code = 0, msg = $"共删除{result}条数据！", data = new { src = "" } }.ToJson());
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
                    "' WHERE PO_NO='",
                     order.PO_NO,
                     "' AND ITEM =",
                     order.ITEM,
                     ";"
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
                             "',0,0,0);"
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

            string sql = string.Format(@"SELECT [PO_NO], [ITEM], [Customer], [Buyer], CONVERT(varchar(100), [PO_Date], 111) PO_Date, [Name], [Description], [Type], [Project], [Qty], [Price], CONVERT(varchar(100), [Required_Shipping_Date], 111) Required_Shipping_Date, [Delivery_Point], [Status] FROM XY_POS WHERE 1=1  ");

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
        #endregion

        #region 生产制造
        public IActionResult ProductIndex()
        {
            return View();
        }

        public string GetProuctOrderList(string keyword,string Status)
        {
            StringBuilder str = new StringBuilder();

            string sql = string.Format(@"SELECT [PO_NO], [ITEM], [Customer], [Buyer], CONVERT(varchar(100), [PO_Date], 111) PO_Date, [Name], [Description], [Type], [Project], [Qty], [Price], CONVERT(varchar(100), [Required_Shipping_Date], 111) Required_Shipping_Date, [Delivery_Point], [Status] ,[Product_Qty] FROM XY_POS WHERE 1=1 ");

            str.Append(sql);

            if (!string.IsNullOrEmpty(keyword))
            {
                str.Append(" AND (PO_NO LIKE '%" + keyword + "%' OR Buyer LIKE '%" + keyword + "%' OR Customer LIKE '%" + keyword + "%' OR Name LIKE '%" + keyword + "%')");
            }

            if (!string.IsNullOrEmpty(Status))
            {
                str.Append(" AND Status = " + Status);
            }
            else {
                str.Append(" AND Status = 0");
            }

            var dt = _dbHelper.QueryTable(str.ToString());

            var list = ObjectHelper<OrderView>.ConvertToModel(dt);

            var json = list.ToJson();

            return json;
        }

        public IActionResult ConfirmOrder(string AllData)
        {
            int result = 0;
            List<OrderView> list = JsonConvert.DeserializeObject<List<OrderView>>(AllData);
            foreach (var order in list)
            {
                if (!string.IsNullOrEmpty(order.PO_NO) && !string.IsNullOrEmpty(order.ITEM))
                {
                    string str = string.Concat(new string[]
                    {
                        "UPDATE [XY_POS] SET Status = 1",
                        " WHERE PO_NO='",
                        order.PO_NO,
                        "' AND ITEM =",
                        order.ITEM,
                        ";"
                    });
                    result += _dbHelper.ExecuteNonQuery(str);
                }
            }

            if (result > 0)
                return Json(new { code = 0, msg = $"共确认{result}条订单！", data = new { src = "" } }.ToJson());
            return Json(new { code = 0, msg = "确认失败！", data = new { src = "" } }.ToJson());
        }

        public IActionResult UpdateProduct(OrderView order)
        {
            if (order.Status != "1")
            {

                string str = string.Concat(new string[]
                {
               "UPDATE [XY_POS] SET Product_Qty = Product_Qty+",
               order.Product_Qty,
               " WHERE PO_NO='",
                order.PO_NO ,
                "' AND ITEM =",
                order.ITEM,
                ";"
                });
                if (_dbHelper.ExecuteNonQuery(str) > 0)
                {
                    string sql = string.Format($"SELECT Product_Qty FROM [XY_POS] WHERE PO_NO = '{order.PO_NO}' AND ITEM ={ order.ITEM };");
                    var Qty = _dbHelper.ExecuteScalar(sql);
                    if (Qty != null)
                    {
                        double Product_Qty = double.Parse(Qty.ToString());
                        if (Product_Qty == double.Parse(order.Qty))
                        {
                            string str1 = string.Concat(new string[]
                            {
                            "UPDATE [XY_POS] SET Status = 2",
                            " WHERE PO_NO='",
                            order.PO_NO,
                            "' AND ITEM =",
                            order.ITEM,
                            ";"
                            });
                            if (_dbHelper.ExecuteNonQuery(str1) == 0)
                                return Json(new { code = 0, msg = "操作失败！", data = new { src = "" } }.ToJson());
                        }
                    }
                    return Json(new { code = 0, msg = "操作成功！", data = new { src = "" } }.ToJson());
                }
                return Json(new { code = 0, msg = "操作失败！", data = new { src = "" } }.ToJson());
            }
            else {
                return Json(new { code = 0, msg = "当前状态不能设置生产数量！", data = new { src = "" } }.ToJson());
            }
        }

        public IActionResult ExportProductList(string keyword, string Status)
        {
            StringBuilder str = new StringBuilder();

            string sql = string.Format(@"SELECT [PO_NO], [ITEM], [Customer], [Buyer], CONVERT(varchar(100), [PO_Date], 111) PO_Date, [Name], [Description], [Type], [Project], [Qty], [Price], CONVERT(varchar(100), [Required_Shipping_Date], 111) Required_Shipping_Date, [Delivery_Point], [Status] ,[Product_Qty] FROM XY_POS WHERE 1=1 ");

            str.Append(sql);

            if (!string.IsNullOrEmpty(keyword))
            {
                str.Append(" AND (PO_NO LIKE '%" + keyword + "%' OR Buyer LIKE '%" + keyword + "%' OR Customer LIKE '%" + keyword + "%' OR Name LIKE '%" + keyword + "%')");
            }

            if (!string.IsNullOrEmpty(Status))
            {
                str.Append(" AND Status = " + Status);
            }
            else
            {
                str.Append(" AND Status = 0");
            }

            var dt = _dbHelper.QueryTable(sql);

            IWorkbook workbook = new XSSFWorkbook();

            var sheet = workbook.CreateSheet("Sheet");
            //表头
            var row = sheet.CreateRow(0);

            for (var i = 0; i < dt.Columns.Count; i++)
            {
                var cell = row.CreateCell(i);
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

            var contentDisposition = "attachment;" + "filename=" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".xls";//在Response的Header中设置下载文件的文件名，这样客户端浏览器才能正确显示下载的文件名，注意这里要用HttpUtility.UrlEncode编码文件名，否则有些浏览器可能会显示乱码文件名

            Response.Headers.Add("Content-Disposition", new string[] { contentDisposition });

            Response.Body.WriteAsync(buf, 0, buf.Length);

            return new EmptyResult();
        }

        #endregion

        #region 出货登记
        public IActionResult ShipmentIndex()
        {
            return View();
        }

        public string GetShipmentOrderList(string keyword, string startShipDate,string endShipDate, string Status)
        {
            StringBuilder str = new StringBuilder();

            string sql = string.Format(@"SELECT [PO_NO], [ITEM], [Customer], [Buyer], CONVERT(varchar(100), [PO_Date], 111) PO_Date, [Name], [Description], [Type], [Project], [Qty], [Price], CONVERT(varchar(100), [Required_Shipping_Date], 111) Required_Shipping_Date, [Delivery_Point], [Status] ,[Product_Qty],[Shipment_Qty] FROM XY_POS WHERE 1=1 ");

            str.Append(sql);

            if (!string.IsNullOrEmpty(keyword))
            {
                str.Append(" AND (PO_NO LIKE '%" + keyword + "%' OR Buyer LIKE '%" + keyword + "%' OR Customer LIKE '%" + keyword + "%' OR Name LIKE '%" + keyword + "%')");
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
            else
            {
                str.Append(" AND Status = 0");
            }

            var dt = _dbHelper.QueryTable(str.ToString());

            var list = ObjectHelper<OrderView>.ConvertToModel(dt);

            var json = list.ToJson();

            return json;
        }

        public string GetShipmentList(string PO_NO,string ITEM)
        {
            string sql = string.Format($"SELECT [SIDR_NO],[PO_NO], [PO_ITEM], [Qty],CONVERT(varchar(100), [BILL_DATE], 111) [BILL_DATE], [REMARK] FROM [XY_SIDR_BILL] WHERE PO_NO='{PO_NO}' AND PO_ITEM={ITEM}");

            var dt = _dbHelper.QueryTable(sql);

            var list = ObjectHelper<ShipmentView>.ConvertToModel(dt);

            var json = list.ToJson();

            return json;
        }

        public string GetUnSubmitShipmentList()
        {
            string sql = string.Format(@"SELECT A.ID, A.[SIDR_NO], A.[PO_NO], B.[Customer],
	        B.[Name], A.[PO_ITEM], A.[Qty], CONVERT ( VARCHAR (100), A.[BILL_DATE], 111 ) [BILL_DATE],
	        A.[REMARK] FROM [XY_SIDR_BILL] A LEFT JOIN [XY_POS] B ON A.PO_NO=B.PO_NO AND A.PO_ITEM=B.ITEM
            WHERE A.SIDR_NO IS NULL;");

            var dt = _dbHelper.QueryTable(sql);

            var list = ObjectHelper<ShipmentView>.ConvertToModel(dt);

            var json = list.ToJson();

            return json;
        }

        public IActionResult EditShipment(OrderView order)
        {
            ArrayList list = new ArrayList();

            string str = string.Concat(new string[]
            {
               "INSERT INTO [XY_SIDR_BILL] ([PO_NO], [PO_ITEM], [Qty], [BILL_DATE], [REMARK]) VALUES ('",
               order.PO_NO ,
               "',",
                order.ITEM,
                ",",
                order.UNShipment_Qty,
                ",'",
                DateTime.Now.ToString(),
                 "','",
                 order.Remark,
                "');"
            });

            string str1 = string.Concat(new string[]
            {
                "UPDATE XY_POS SET Shipment_Qty=Shipment_Qty+",
                order.UNShipment_Qty,
                " WHERE PO_NO='",
                order.PO_NO ,
                "' AND ITEM =",
                order.ITEM,
                ";"
            });

            list.Add(str);
            list.Add(str1);

            if (_dbHelper.ExecuteNonQuery(list) > 0)
                return Json(new { code = 0, msg = "操作成功！", data = new { src = "" } }.ToJson());
            return Json(new { code = 0, msg = "操作失败！", data = new { src = "" } }.ToJson());
        }
                                                   
        [HttpPost]
        public IActionResult SubmitShipment(string AllData)
        {
            List<ShipmentView> list = JsonConvert.DeserializeObject<List<ShipmentView>>(AllData);

            string customer = list[0].Customer;

            ArrayList array = new ArrayList();

            foreach (var shipment in list)
            {
                if (customer == shipment.Customer)
                {
                    var SIDR_NO = GetShipmentNo(shipment.Customer, shipment.PO_NO, shipment.PO_ITEM);
                    string str = string.Concat(new string[]
                    {
                        "UPDATE XY_SIDR_BILL SET SIDR_NO='",
                        SIDR_NO,
                        "' WHERE ID=",
                        shipment.ID,
                        ";"
                    });
                    array.Add(str);
                }
                else {
                    return Json(new { code = 0, msg = "所选货品非同一客户！", data = new { src = "" } }.ToJson());
                }
            }
            if (_dbHelper.ExecuteNonQuery(array) > 0)
                return Json(new { code = 0, msg = "操作成功！", data = new { src = "" } }.ToJson());
            return Json(new { code = 0, msg = "操作失败！", data = new { src = "" } }.ToJson());

        }

        private string GetShipmentNo(string Customer, string PO_NO, string ITEM)
        {
            string start = Customer.Substring(0, 2) + "TP";
            string year = DateTime.Now.Year.ToString().Substring(2);
            string sql = string.Format(@"SELECT RIGHT(SIDR_NO,4) FROM XY_SIDR_BILL WHERE PO_NO='" + PO_NO + "'" + "AND PO_ITEM=" + ITEM + " ORDER BY RIGHT(SIDR_NO,4) DESC;");
            var item = _dbHelper.ExecuteScalar(sql);
            string tempVal = string.Empty;
            int dbNum, tempNum;
            if (item != null&& !item.Equals(DBNull.Value))
            {
                dbNum = int.Parse(item.ToString());
                tempNum = dbNum + 1;
                tempVal = GetTempVal(dbNum, tempNum);
            }
            else
            {
                string str = string.Format($"SELECT RIGHT(SIDR_NO,4) FROM XY_SIDR_BILL WHERE LEFT(SIDR_NO,6) = '{ start + year }' ORDER BY RIGHT(SIDR_NO,4) DESC;");
                var num = _dbHelper.ExecuteScalar(str);
                if (num != null && !num.Equals(DBNull.Value))
                {
                    dbNum = int.Parse(num.ToString());
                    tempNum = dbNum + 1;
                    tempVal = GetTempVal(dbNum, tempNum);
                }
                else {
                    tempVal = "0001";
                }
            }
            return start + year + tempVal;
        }

        private string GetTempVal(int dbNum,int tempNum)
        {
            if (dbNum < 9)
                return "000" + tempNum.ToString();
            else if (dbNum == 9) return "0010";
            else if (dbNum > 9 && dbNum < 99)
                return "00" + tempNum.ToString();
            else if (dbNum == 99) return "0100";
            else
                return "0" + tempNum.ToString();
        }

        public IActionResult ExportShipmentList(string keyword, string startShipDate, string endShipDate, string Status)
        {
            StringBuilder str = new StringBuilder();

            string sql = string.Format(@"SELECT [PO_NO], [ITEM], [Customer], [Buyer], CONVERT(varchar(100), [PO_Date], 111) PO_Date, [Name], [Description], [Type], [Project], [Qty], [Price], CONVERT(varchar(100), [Required_Shipping_Date], 111) Required_Shipping_Date, [Delivery_Point], [Status] ,[Product_Qty],[Shipment_Qty] FROM XY_POS WHERE 1=1 ");

            str.Append(sql);

            if (!string.IsNullOrEmpty(keyword))
            {
                str.Append(" AND (PO_NO LIKE '%" + keyword + "%' OR Buyer LIKE '%" + keyword + "%' OR Customer LIKE '%" + keyword + "%' OR Name LIKE '%" + keyword + "%')");
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
            else
            {
                str.Append(" AND Status = 0");
            }

            var dt = _dbHelper.QueryTable(str.ToString());

            IWorkbook workbook = new XSSFWorkbook();

            var sheet = workbook.CreateSheet("Sheet");
            //表头
            var row = sheet.CreateRow(0);

            for (var i = 0; i < dt.Columns.Count; i++)
            {
                var cell = row.CreateCell(i);
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

            var contentDisposition = "attachment;" + "filename=" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".xls";//在Response的Header中设置下载文件的文件名，这样客户端浏览器才能正确显示下载的文件名，注意这里要用HttpUtility.UrlEncode编码文件名，否则有些浏览器可能会显示乱码文件名

            Response.Headers.Add("Content-Disposition", new string[] { contentDisposition });

            Response.Body.WriteAsync(buf, 0, buf.Length);

            return new EmptyResult();
        }

        #endregion

        #region 发票管理
        public IActionResult InvoiceIndex()
        {
            return View();
        }

        public string GetInvoiceList(string keyword)
        {
            StringBuilder str = new StringBuilder();

            string sql = string.Format(@"SELECT A.ID, A.[SIDR_NO], A.[PO_NO], B.[Customer],B.[Name], A.[PO_ITEM], A.[Qty], CONVERT ( VARCHAR (100), A.[BILL_DATE], 111 ) [BILL_DATE],
            A.[REMARK],A.[CONTAINER_NO], A.[Seal_NO], A.[BL_NO], A.[VESSEL], A.[ETD], A.[ETA], A.[TERMS_OF_SALE], A.[COUNTRY_OF_ORIGIN], 
            A.[SHIPMENT_FROM], A.[TAX], A.[FREIGHT] FROM [XY_SIDR_BILL] A LEFT JOIN [XY_POS] B ON A.PO_NO=B.PO_NO AND A.PO_ITEM=B.ITEM
            WHERE A.SIDR_NO IS NOT NULL ");

            str.Append(sql);

            if (!string.IsNullOrEmpty(keyword))
            {
                str.Append(" AND SIDR_NO LIKE '%" + keyword + "%'");
            }

            var dt = _dbHelper.QueryTable(str.ToString());

            var list = ObjectHelper<ShipmentView>.ConvertToModel(dt);

            var json = list.ToJson();

            return json;
        }

        [HttpPost]
        public IActionResult EditInvoice(ShipmentView shipment)
        {
            string str = string.Concat(new string[]
            {
                "UPDATE [XY_SIDR_BILL] SET [CONTAINER_NO] = '",
                shipment.CONTAINER_NO,
                "',[Seal_NO] = '",
                shipment.Seal_NO,
                "',[BL_NO] = '",
                shipment.BL_NO,
                "',[VESSEL] = '",
                shipment.VESSEL,
                "',[ETD] = '",
                shipment.ETD,
                "',[ETA] = '",
                shipment.ETA,
                "',[TERMS_OF_SALE] = '",
                shipment.TERMS_OF_SALE,
                "',[COUNTRY_OF_ORIGIN] = '",
                shipment.COUNTRY_OF_ORIGIN,
                "',[SHIPMENT_FROM] = '",
                shipment.SHIPMENT_FROM,
                "',[TAX] = '",
                shipment.TAX,
                "',[FREIGHT] = '",
                shipment.FREIGHT,
                "' WHERE ID=",
                shipment.ID,
                ";"
            });
            if (_dbHelper.ExecuteNonQuery(str) > 0)
                return Json(new { code = 0, msg = "操作成功！", data = new { src = "" } }.ToJson());
            return Json(new { code = 0, msg = "操作失败！", data = new { src = "" } }.ToJson());

        }

        [HttpPost]
        public IActionResult ExportInvoice(ShipmentView shipment)
        {
            IWorkbook workbook = new XSSFWorkbook();

            var sheet = workbook.CreateSheet("Sheet");

            sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 7));
            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 0, 7));
            sheet.AddMergedRegion(new CellRangeAddress(2, 2, 0, 7));
            sheet.AddMergedRegion(new CellRangeAddress(3, 3, 0, 7));

            #region 表头样式
            ICellStyle titleStyle = workbook.CreateCellStyle();
            IFont font = workbook.CreateFont();
            font.FontName = "Arial";
            font.FontHeightInPoints = 22;
            titleStyle.SetFont(font);
            titleStyle.VerticalAlignment = VerticalAlignment.Center;
            titleStyle.Alignment = HorizontalAlignment.Center;

            ICellStyle rowTitleStyle = workbook.CreateCellStyle();
            IFont rowFont = workbook.CreateFont();
            rowFont.FontName = "Arial";
            rowFont.FontHeightInPoints = 11;
            rowTitleStyle.SetFont(rowFont);
            rowTitleStyle.VerticalAlignment = VerticalAlignment.Center;
            rowTitleStyle.Alignment = HorizontalAlignment.Center;

            ICellStyle row3Style = workbook.CreateCellStyle();
            IFont row3Font = workbook.CreateFont();
            row3Font.FontName = "Arial";
            row3Font.FontHeightInPoints = 18;
            row3Font.IsBold = true;
            row3Style.SetFont(row3Font);
            row3Style.VerticalAlignment = VerticalAlignment.Center;
            row3Style.Alignment = HorizontalAlignment.Center;


            ICellStyle headLeftFieldStyle = workbook.CreateCellStyle();
            IFont headLeftFieldFont = workbook.CreateFont();
            headLeftFieldFont.FontName = "Arial";
            headLeftFieldFont.FontHeightInPoints = 9;
            headLeftFieldFont.IsBold = true;
            headLeftFieldStyle.SetFont(headLeftFieldFont);
            headLeftFieldStyle.VerticalAlignment = VerticalAlignment.Center;
            headLeftFieldStyle.Alignment = HorizontalAlignment.Left;

            ICellStyle headRightFieldStyle = workbook.CreateCellStyle();
            IFont headRightFieldFont = workbook.CreateFont();
            headRightFieldFont.FontName = "Arial";
            headRightFieldFont.FontHeightInPoints = 9;
            headRightFieldStyle.SetFont(headRightFieldFont);
            headRightFieldStyle.VerticalAlignment = VerticalAlignment.Center;
            headRightFieldStyle.Alignment = HorizontalAlignment.Right;
            #endregion

            #region 表头
            var row = sheet.CreateRow(0);
            row.Height = 600;
            var row0 = row.CreateCell(0);
            row0.SetCellValue("TARGET LIGHTING MFTG. CORP.");
            row0.CellStyle = titleStyle;
            var row1 = sheet.CreateRow(1);
            var cell1 = row1.CreateCell(0);
            cell1.SetCellValue("Building 02, 03,04, Lot 1, Berthaphil VIII Compound, M. Rosas Highway,");
            cell1.CellStyle = rowTitleStyle;
            var row2 = sheet.CreateRow(2);
            var cell2 = row2.CreateCell(0);
            cell2.SetCellValue("Clark Freeport Zone, Philippines");
            cell2.CellStyle = rowTitleStyle;
            var row3 = sheet.CreateRow(3);
            var cell3 = row3.CreateCell(0);
            cell3.SetCellValue("INVOICE");
            cell3.CellStyle = row3Style;

            var row4 = sheet.CreateRow(4);
            var cell40= row3.CreateCell(0);
            cell40.SetCellValue("BILL TO:");
            cell40.CellStyle = headLeftFieldStyle;
            var cell44 = row3.CreateCell(5);
            cell44.SetCellValue("INVOICE NUMBER.");
            cell44.CellStyle = headRightFieldStyle;
            #endregion

            MemoryStream stream = new MemoryStream();

            workbook.Write(stream);

            var buf = stream.ToArray();

            Response.ContentType = "application/octet-stream";

            Response.ContentLength = buf.Length;

            var contentDisposition = "attachment;" + "filename=" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".xls";//在Response的Header中设置下载文件的文件名，这样客户端浏览器才能正确显示下载的文件名，注意这里要用HttpUtility.UrlEncode编码文件名，否则有些浏览器可能会显示乱码文件名

            Response.Headers.Add("Content-Disposition", new string[] { contentDisposition });

            Response.Body.WriteAsync(buf, 0, buf.Length);

            return new EmptyResult();
        }
        #endregion
    }
}
