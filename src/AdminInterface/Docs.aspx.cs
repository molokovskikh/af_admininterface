using System;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using ICSharpCode.SharpZipLib.Zip;

namespace AddUser
{
	partial class Documents : Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			if (Convert.ToInt32(Session["AccessGrant"]) != 1)
			{
				Response.Redirect("default.aspx");
			}
			const string ResPath = @"\\offdc\Data\Общего пользования\";
			string[] FileList;
			TableRow Row;
			TableCell Cel;
			FileInfo FileInfo;
			HyperLink HL;
			if (String.IsNullOrEmpty(Request["doc"]) || Request["doc"].Length < 5)
			{
				FileList = Directory.GetFiles(ResPath);
				foreach (string FileName in FileList)
				{
					Row = new TableRow();
					Cel = new TableCell();
					FileInfo = new FileInfo(FileName);
					HL = new HyperLink();
					HL.Text = FileInfo.Name;
					HL.NavigateUrl = string.Format("Docs.aspx?Doc={0}", FileInfo.Name);
					Cel.Controls.Add(HL);
					Row.Cells.Add(Cel);
					Cel = new TableCell();
					Cel.Text = FileInfo.LastWriteTime.ToShortDateString();
					Row.Cells.Add(Cel);
					Cel = new TableCell();
					Cel.Text = string.Format("{0} k", Math.Round((double) (FileInfo.Length/1024), 1));
					Row.Cells.Add(Cel);
					FileListTab.Rows.Add(Row);
				}
			}
			else
			{
				FileInfo = new FileInfo(ResPath + Request["doc"]);
				if (FileInfo.Exists)
				{
					string FileName = Request["doc"];
					MemoryStream ZipOutputStream = new MemoryStream();
					ZipOutputStream ZipInputStream = new ZipOutputStream(ZipOutputStream);
					ZipEntry ZipObject = new ZipEntry("ReqFile" + FileName.Substring(FileName.Length - 4));
					FileStream InputFileStream =
						new FileStream(ResPath + Request["doc"], FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 10240);
					byte[] InputFileByteArray = new byte[InputFileStream.Length];
					InputFileStream.Read(InputFileByteArray, 0, Convert.ToInt32(InputFileStream.Length));
					ZipInputStream.SetLevel(9);
					ZipObject.DateTime = DateTime.Now;
					ZipInputStream.PutNextEntry(ZipObject);
					ZipInputStream.Write(InputFileByteArray, 0, Convert.ToInt32(InputFileStream.Length));
					ZipInputStream.Finish();
					InputFileStream.Close();
					ZipInputStream.Close();
					Response.Clear();
					Response.ContentType = "application/octet-stream";
					Response.AddHeader("Content-Disposition", "attachment; filename=\"ReqFile.zip\"");
					Response.Flush();
					Response.BinaryWrite(ZipOutputStream.ToArray());
					ZipOutputStream.Close();
				}
			}
		}
	}
}