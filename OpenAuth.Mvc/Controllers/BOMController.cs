using Infrastructure;
using Infrastructure.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
using OpenAuth.App;
using OpenAuth.App.Interface;
using OpenAuth.Repository.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace OpenAuth.Mvc.Controllers
{
    public class BOMController : BaseController
    {
        private readonly BOMApp _app;
		private readonly IHostingEnvironment _hostingEnvironment;
		string _targetFilePath = "";
		private IConfiguration _configuration { get; }
		private string connstr = "";

		private DbHelper _dbHelper;

		public BOMController(BOMApp app, IAuth auth, IHostingEnvironment hostingEnvironment,IConfiguration configuration) : base(auth)
        {
            _app = app;
			_hostingEnvironment = hostingEnvironment;
			_targetFilePath = _hostingEnvironment.WebRootPath;
			_dbHelper = new DbHelper(configuration);
		}

        public IActionResult Index()
        {
            return View();
        }


        public string GetList(string keyword)
        {
			string sql = string.Format(@"WITH temBOM AS (
					SELECT
						A.BOM_NO,
						D.PRD_NO ,
						D.NAME ,
						D.SPC ,
						1 AS LEVEL1,
						A.PRD_NO AS ChildPrdNo ,
						A.ID_NO,
						A.NAME AS ChildName,
						B.SNM ,
						B.NAME_ENG,
						B.SPC AS ChildSPC,
						A.QTY
					FROM
						TF_BOM A
					LEFT JOIN PRDT B ON A.PRD_NO = B.PRD_NO
					LEFT JOIN MF_BOM D ON A.BOM_NO = D.BOM_NO
					WHERE
						 D.PRD_NO IN ('{0}')
					UNION ALL
						SELECT
							A.BOM_NO,
							D.PRD_NO,
							D.NAME ,
							D.SPC ,
							LEVEL1 + 1,
							A.PRD_NO AS ChildPrdNo ,
							A.ID_NO,
							A.NAME AS ChildName,
							B.SNM ,
							B.NAME_ENG,
							B.SPC AS ChildSPC,
							A.QTY
						FROM
							TF_BOM A
						INNER JOIN PRDT B ON A.PRD_NO = B.PRD_NO
						INNER JOIN MF_BOM D ON A.BOM_NO = D.BOM_NO
						INNER JOIN temBOM E ON A.BOM_NO = E.ID_NO
						WHERE
							A.BOM_NO = E.ID_NO
				) SELECT
					*
				FROM
					temBOM", keyword);

			var dt = _dbHelper.QueryTable(sql);

			var list = ObjectHelper<TF_BOM>.ConvertToModel(dt);

			var json = list.ToJson();

			return json;
        }

		public string GetBOMList(string keyword)
	    {
			string sql = string.Format(@"SELECT A.BOM_NO,C.NAME_ENG,A.PRD_NO FROM MF_BOM A LEFT JOIN PRDT C ON A.PRD_NO = C.PRD_NO
WHERE C.NAME_ENG LIKE '%{0}%' OR A.BOM_NO LIKE '%{0}%' OR A.PRD_NO LIKE '%{0}%';", keyword.Trim());

			var dt = _dbHelper.QueryTable(sql);

			List<BOMView> list = _app.ConvertToSelectModel(dt);

			var json = list.ToJson();

			return json;
		}

		public string GetFileList(string keyword)
		{
			string sql = string.Format(@"Select * from [DB_PHLP].[dbo].[XY_BOM_FileList] where BOM_NO='{0}';", keyword.Trim());

			var dt = _dbHelper.QueryTable(sql);

			List<BOMView> list = _app.ConvertToSelectModel(dt);

			var json = list.ToJson();

			return json;
		}

		#region 文件上传
		/// <summary>
		/// 流式文件上传
		/// </summary>
		/// <returns></returns>
		[HttpPost]
		[DisableRequestSizeLimit]
		public async Task<IActionResult> Upload()
		 {
            try 

            {
				//获取boundary
				var boundary = HeaderUtilities.RemoveQuotes(MediaTypeHeaderValue.Parse(Request.ContentType).Boundary).Value;

				//得到reader
				var reader = new MultipartReader(boundary, HttpContext.Request.Body);

				var section = await reader.ReadNextSectionAsync();

				StringBuilder sql = new StringBuilder();

				List<string> FileList = new List<string>();

				string BOM_NO="", filePath, tempPath="";

				Stream stream = null;
				//读取section
				while (section != null)
				{
					ContentDispositionHeaderValue header = section.GetContentDispositionHeader();
					if (header.FileName.HasValue || header.FileNameStar.HasValue)
					{
						var fileSection = section.AsFileSection();
						var fileName = fileSection.FileName;
						FileList.Add(fileName);
						stream = section.Body;
						var storage = $"{_targetFilePath }\\uploads\\TEMP";
						tempPath = storage + "\\" + FileList[0];
						await WriteFileAsync(stream, tempPath);
					}
					else {

						var formDataSection = section.AsFormDataSection();
						var name = formDataSection.Name;
						BOM_NO = await formDataSection.GetValueAsync();
					}
					section = await reader.ReadNextSectionAsync();
				}
				if (string.IsNullOrEmpty(BOM_NO))
				{
					return Json(new { code = 0, msg = "未选择BOM代号", data = new { src = "" } }.ToJson());
				}

				var FileStorage = $"{_targetFilePath }\\uploads\\{ BOM_NO?.Replace(">", "$")}";
				filePath = FileStorage + "\\" + FileList[0];
				if (!Directory.Exists(FileStorage))
				{
					Directory.CreateDirectory(FileStorage);
				}

				await WriteFileAsyncByTEMP(tempPath, filePath);

				string query = string.Format($"SELECT MAX(ITM) FROM [DB_PHLP].[dbo].[XY_BOM_FileList] WHERE BOM_NO='{BOM_NO}';");

				var dt = _dbHelper.QueryTable(query);
				int itm;
				int.TryParse(dt.Rows[0][0].ToString(),out itm);

				foreach (var item in FileList)
                {
					sql.AppendFormat($"INSERT INTO [DB_PHLP].[dbo].[XY_BOM_FileList] ([BOM_NO], [ITM], [FileName], [UploadTime]) VALUES ('{ BOM_NO }',{ itm+1 },'{item}', GETDATE());");
					itm ++;
				}

				if(_dbHelper.InserTable(sql.ToString())== FileList.Count)
					return Json(new { code = 0, msg = "上传成功", data = new { src = "" } }.ToJson());
				else
					return Json(new { code = 10, msg = "上传失败", data = new { src = "" } }.ToJson());
			}
            catch (Exception ex)
            {

                throw;
            }

			
		}

		

		/// <summary>
		/// 缓存式文件上传
		/// </summary>
		/// <param name=""></param>
		/// <returns></returns>
		[HttpPost]
		public async Task<IActionResult> UploadingFormFile(IFormFile file)
		{
			using (var stream = file.OpenReadStream())
			{
				var trustedFileNameForFileStorage = Path.GetRandomFileName();
				await WriteFileAsync(stream, Path.Combine(_targetFilePath, trustedFileNameForFileStorage));
			}
			return Created(nameof(BOMController), null);
		}


		/// <summary>
		/// 写文件导到磁盘
		/// </summary>
		/// <param name="stream">流</param>
		/// <param name="path">文件保存路径</param>
		/// <returns></returns>
		public static async Task<int> WriteFileAsync(System.IO.Stream stream, string path)
		{
			const int FILE_WRITE_SIZE = 84975;//写出缓冲区大小
			int writeCount = 0;
			using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write, FILE_WRITE_SIZE, true))
			{
				byte[] byteArr = new byte[FILE_WRITE_SIZE];
				int readCount = 0;
				while ((readCount = await stream.ReadAsync(byteArr, 0, byteArr.Length)) > 0)
				{
					await fileStream.WriteAsync(byteArr, 0, readCount);
					writeCount += readCount;
				}
			}
			return writeCount;
		}

		public static async Task<int> WriteFileAsyncByTEMP(string TEMP,string path)
		{
			//stream.Length = 0;
			const int FILE_WRITE_SIZE = 84975;//写出缓冲区大小
			int writeCount = 0;

			using (FileStream stream = new FileStream(TEMP, FileMode.Open)) {

				using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write, FILE_WRITE_SIZE, true))
				{
					byte[] byteArr = new byte[FILE_WRITE_SIZE];
					int readCount = 0;

					while ((readCount = await stream.ReadAsync(byteArr, 0, byteArr.Length)) > 0)
					{
						await fileStream.WriteAsync(byteArr, 0, readCount);
						writeCount += readCount;
					}
				}
			}
			FileInfo file = new FileInfo(TEMP);
			file.Delete();
			return writeCount;
		}
		#endregion


		#region 下载文件
		/// <summary>
		/// DownloadBigFile用于下载大文件，循环读取大文件的内容到服务器内存，然后发送给客户端浏览器
		/// </summary>
		public IActionResult DownloadBigFile(string BOM_NO, string FileName)
		{
			var FileStorage = $"{_targetFilePath }\\uploads\\{ BOM_NO }";

			var filePath = FileStorage + "\\" + FileName;

			int bufferSize = 849750;//这就是ASP.NET Core循环读取下载文件的缓存大小，这里我们设置为了1024字节，也就是说ASP.NET Core每次会从下载文件中读取1024字节的内容到服务器内存中，然后发送到客户端浏览器，这样避免了一次将整个下载文件都加载到服务器内存中，导致服务器崩溃

			Response.ContentType = "application/octet-stream";

			var contentDisposition = "attachment;" + "filename=" + HttpUtility.UrlEncode(FileName);//在Response的Header中设置下载文件的文件名，这样客户端浏览器才能正确显示下载的文件名，注意这里要用HttpUtility.UrlEncode编码文件名，否则有些浏览器可能会显示乱码文件名
			
			Response.Headers.Add("Content-Disposition", new string[] { contentDisposition });

			//使用FileStream开始循环读取要下载文件的内容
			using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
			{
				using (Response.Body)//调用Response.Body.Dispose()并不会关闭客户端浏览器到ASP.NET Core服务器的连接，之后还可以继续往Response.Body中写入数据
				{
					long contentLength = fs.Length;//获取下载文件的大小
					Response.ContentLength = contentLength;//在Response的Header中设置下载文件的大小，这样客户端浏览器才能正确显示下载的进度

					byte[] buffer;
					long hasRead = 0;//变量hasRead用于记录已经发送了多少字节的数据到客户端浏览器

					//如果hasRead小于contentLength，说明下载文件还没读取完毕，继续循环读取下载文件的内容，并发送到客户端浏览器
					while (hasRead < contentLength)
					{
						//HttpContext.RequestAborted.IsCancellationRequested可用于检测客户端浏览器和ASP.NET Core服务器之间的连接状态，如果HttpContext.RequestAborted.IsCancellationRequested返回true，说明客户端浏览器中断了连接
						if (HttpContext.RequestAborted.IsCancellationRequested)
						{
							//如果客户端浏览器中断了到ASP.NET Core服务器的连接，这里应该立刻break，取消下载文件的读取和发送，避免服务器耗费资源
							break;
						}

						buffer = new byte[bufferSize];

						int currentRead = fs.Read(buffer, 0, bufferSize);//从下载文件中读取bufferSize(1024字节)大小的内容到服务器内存中

						Response.Body.WriteAsync(buffer, 0, currentRead);//发送读取的内容数据到客户端浏览器

						hasRead += currentRead;//更新已经发送到客户端浏览器的字节数
					}
				}
			}

			return new EmptyResult();
		}
		#endregion
	}
}
