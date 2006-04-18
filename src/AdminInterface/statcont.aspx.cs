using System;
using System.Drawing;
using System.Web.UI;
using System.Web.UI.WebControls;
using MySql.Data.MySqlClient;
using Image = System.Web.UI.WebControls.Image;

namespace AddUser
{
	public partial class statcont : Page
	{
		public int ob;
		public string Picstr;
		public HyperLink HL = new HyperLink();


		private void ShowStatS()
		{
			Image Pic;
			MySqlCommand Комманда = new MySqlCommand();
			MySqlDataReader Reader;
			TableRow row;
			TableCell cell;
			string Order = string.Empty;
			MySqlConnection соединение = new MySqlConnection(Literals.GetConnectionString());
			HL = new HyperLink();
			Pic = new Image();
			Pic.ImageUrl = Picstr;
			if (ob == 0)
			{
				Table5.Rows[0].Cells[0].BackColor = ColorTranslator.FromHtml("#D8F1FF");
				Table5.Rows[0].Cells[0].Controls.Add(Pic);
				Order = Order + "writetime";
			}
			if (ob == 1)
			{
				Table5.Rows[0].Cells[1].BackColor = ColorTranslator.FromHtml("#D8F1FF");
				Table5.Rows[0].Cells[1].Controls.Add(Pic);
				Order = Order + "clientsinfo.UserName";
			}
			if (ob == 2)
			{
				Table5.Rows[0].Cells[2].BackColor = ColorTranslator.FromHtml("#D8F1FF");
				Table5.Rows[0].Cells[2].Controls.Add(Pic);
				Order = Order + "ShortName";
			}
			if (ob == 3)
			{
				Table5.Rows[0].Cells[3].BackColor = ColorTranslator.FromHtml("#D8F1FF");
				Table5.Rows[0].Cells[3].Controls.Add(Pic);
				Order = Order + "Region";
			}
			if (Picstr == "arrow-down-blue-reversed.gif")
				Order = Order + " desc";
			соединение.Open();
			
			Комманда.CommandText =
				" SELECT WriteTime, clientsinfo.UserName, ShortName, Region, Message, FirmCode, osuseraccessright.rowid, clientsinfo.rowid" +
				" FROM logs.clientsinfo, usersettings.clientsdata, accessright.showright, usersettings.osuseraccessright, farm.regions" +
				" where clientsinfo.clientcode=firmcode  and osuseraccessright.clientcode=clientsinfo.clientcode" +
				" and showright.RegionMask & clientsdata.RegionCode > 0 and writetime between ?FromDate and ?ToDate" +
				" and regions.regioncode=clientsdata.RegionCode and showright.username=?userName order by ?order";
			Комманда.Parameters.Add("?userName", Session["UserName"]);
			Комманда.Parameters.Add("?order", Order);
			
			Комманда.Connection = соединение;
			Комманда.Parameters.Add(new MySqlParameter("FromDate", MySqlDbType.Datetime));
			Комманда.Parameters["FromDate"].Value = CalendarFrom.SelectedDate;
			Комманда.Parameters.Add(new MySqlParameter("ToDate", MySqlDbType.Datetime));
			Комманда.Parameters["ToDate"].Value = CalendarTo.SelectedDate.AddDays(1);
			Reader = Комманда.ExecuteReader();
			if (Request.Cookies["Inforoom.Admins.ShowStatsC"] == null)
			{
				Response.Cookies["Inforoom.Admins.ShowStatsC"].Value = "0";
				Response.Cookies["Inforoom.Admins.ShowStatsC"].Expires = DateTime.Now.AddYears(2);
			}
			int OMaxID = Convert.ToInt32(Request.Cookies["Inforoom.Admins.ShowStatsC"].Value);
			int MaxID = 0;
			while (Reader.Read())
			{
				if (Convert.ToInt32(Reader[7]) > MaxID)
				{
					MaxID = Convert.ToInt32(Reader[7]);
				}
				row = new TableRow();
				if (OMaxID < Convert.ToInt32(Reader[7]))
				{
					row.BackColor = Color.White;
				}
				for (int i = 0; i <= Table5.Rows[0].Cells.Count - 1; i++)
				{
					cell = new TableCell();
					if (ob == i)
					{
						cell.BackColor = Color.AliceBlue;
					}
					if (i == 2)
					{
						HL = new HyperLink();
						HL.Text = Reader[i].ToString();
						HL.NavigateUrl = "info.aspx?cc=" + Reader[5].ToString() + "&ouar=" + Reader[6].ToString();
						cell.Controls.Add(HL);
						HL.Dispose();
					}
					else if (i == 0)
					{
						DateTime PriceDate = Convert.ToDateTime(Reader[i]);
						cell.Text = PriceDate.ToString("dd.MM.yy HH:mm");
						cell.Font.Size = FontUnit.Point(8);
					}
					else
					{
						cell.Text = Reader[i].ToString();
					}
					row.Cells.Add(cell);
				}
				Table5.Rows.Add(row);
			}
			Reader.Close();
			Session["MaxID"] = MaxID;
			соединение.Close();
		}


		protected void CalendarTo_SelectionChanged(object sender, EventArgs e)
		{
			Session["SelectedToDate"] = CalendarTo.SelectedDate;
			ShowStatS();
		}


		protected void CalendarFrom_SelectionChanged(object sender, EventArgs e)
		{
			Session["SelectedFromDate"] = CalendarFrom.SelectedDate;
			ShowStatS();
		}


		protected void Page_Load(object sender, EventArgs e)
		{
			if (Convert.ToInt32(Session["AccessGrant"]) != 1)
			{
				Response.Redirect("default.aspx");
			}
			if (!(IsPostBack))
			{
				if (Session["SelectedFromDate"] != null)
				{
					CalendarFrom.SelectedDate = Convert.ToDateTime(Session["SelectedFromDate"]);
				}
				else
				{
					CalendarFrom.SelectedDate = DateTime.Now.AddDays(-14);
				}
				if (Session["SelectedToDate"] != null)
				{
					CalendarTo.SelectedDate = Convert.ToDateTime(Session["SelectedToDate"]);
				}
				else
				{
					CalendarTo.SelectedDate = DateTime.Now;
				}
				if (String.IsNullOrEmpty(Request["ob"]))
				{
					if (!(Request.Cookies["Inforoom.Stat.OrderBy"] == null))
					{
						ob = Convert.ToInt32(Request.Cookies["Inforoom.Stat.OrderBy"].Value);
					}
					else
					{
						ob = 0;
					}
				}
				else
				{
					if (Convert.ToInt32(Session["ob"]) == Convert.ToInt32(Request["ob"]))
					{
						if (Session["Picstr"] == "arrow-down-blue.gif")
						{
							Picstr = "arrow-down-blue-reversed.gif";
						}
					}
					Session["Picstr"] = Picstr;
					ob = Convert.ToInt32(Request["ob"]);
					Response.Cookies["Inforoom.Stat.OrderBy"].Value = ob.ToString();
					Response.Cookies["Inforoom.Stat.OrderBy"].Expires = DateTime.Now.AddYears(2);
				}
				
				if (String.IsNullOrEmpty((string)Session["Picstr"]))
				{
					Picstr = "arrow-down-blue.gif";
					Session["Picstr"] = Picstr;
				}
				Session["ob"] = ob;
				ShowStatS();
			}
			HL.NavigateUrl = "statcont.aspx?Ob=0";
			HL.Text = "Дата";
			Table5.Rows[0].Cells[0].Controls.Add(HL);
			HL = new HyperLink();
			HL.Text = "Оператор";
			HL.NavigateUrl = "statcont.aspx?Ob=1";
			Table5.Rows[0].Cells[1].Controls.Add(HL);
			HL = new HyperLink();
			HL.Text = "Клиент";
			HL.NavigateUrl = "statcont.aspx?Ob=2";
			Table5.Rows[0].Cells[2].Controls.Add(HL);
			HL = new HyperLink();
			HL.Text = "Регион";
			HL.NavigateUrl = "statcont.aspx?Ob=3";
			Table5.Rows[0].Cells[3].Controls.Add(HL);
			Picstr = Session["Picstr"].ToString();
			ob = Convert.ToInt32(Session["ob"]);
		}
	}
}