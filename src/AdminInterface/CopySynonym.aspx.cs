using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.Mail;
using System.Web.UI;
using MySql.Data.MySqlClient;

namespace AddUser
{
	partial class CopySynonym : Page
	{
		protected DataSet DS;
		protected DataTable Regions;
		protected DataColumn DataColumn1;
		protected DataColumn DataColumn2;
		MySqlConnection соединение = new MySqlConnection();
		MySqlDataAdapter DA = new MySqlDataAdapter();
		protected DataTable From;
		protected DataColumn DataColumn3;
		protected DataColumn DataColumn4;
		protected DataColumn DataColumn5;
		protected DataColumn DataColumn6;
		protected DataTable ToT;
		string UserName;
		protected DataColumn DataColumn7;

		[DebuggerStepThrough()]
		private void InitializeComponent()
		{
			DS = new DataSet();
			Regions = new DataTable();
			DataColumn1 = new DataColumn();
			DataColumn2 = new DataColumn();
			DataColumn7 = new DataColumn();
			From = new DataTable();
			DataColumn3 = new DataColumn();
			DataColumn4 = new DataColumn();
			ToT = new DataTable();
			DataColumn5 = new DataColumn();
			DataColumn6 = new DataColumn();
			DS.BeginInit();
			Regions.BeginInit();
			From.BeginInit();
			ToT.BeginInit();
			DS.DataSetName = "NewDataSet";
			DS.Locale = new CultureInfo("ru-RU");
			DS.Tables.AddRange(new DataTable[] {Regions, From, ToT});
			Regions.Columns.AddRange(new DataColumn[] {DataColumn1, DataColumn2, DataColumn7});
			Regions.TableName = "Regions";
			DataColumn1.ColumnName = "Region";
			DataColumn2.ColumnName = "RegionCode";
			DataColumn2.DataType = typeof (Int64);
			DataColumn7.ColumnName = "Email";
			From.Columns.AddRange(new DataColumn[] {DataColumn3, DataColumn4});
			From.TableName = "From";
			DataColumn3.ColumnName = "Name";
			DataColumn4.ColumnName = "ClientCode";
			DataColumn4.DataType = typeof (Int32);
			ToT.Columns.AddRange(new DataColumn[] {DataColumn5, DataColumn6});
			ToT.TableName = "ToT";
			DataColumn5.ColumnName = "Name";
			DataColumn6.ColumnName = "ClientCode";
			DataColumn6.DataType = typeof (Int32);
			DS.EndInit();
			Regions.EndInit();
			From.EndInit();
			ToT.EndInit();
		}

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			InitializeComponent();
		}

		protected void FindBT_Click(object sender, EventArgs e)
		{
			FindClient(FromTB.Text, "From");
			FindClient(ToTB.Text, "ToT");
			FromL.Text = DS.Tables["from"].Rows.Count.ToString();
			ToL.Text = DS.Tables["tot"].Rows.Count.ToString();
			if (DS.Tables["from"].Rows.Count > 0)
			{
				if (DS.Tables["from"].Rows.Count > 50)
				{
					LabelErr.Text = "Найдено более 50 записей \"От\". Уточните условие поиска.";
					return;
				}
				FromTB.Visible = false;
				FromDD.Visible = true;
				FromDD.DataBind();
			}
			if (DS.Tables["tot"].Rows.Count > 0)
			{
				if (DS.Tables["tot"].Rows.Count > 50)
				{
					LabelErr.Text = "Найдено более 50 записей \"Для\". Уточните условие поиска.";
					return;
				}
				ToTB.Visible = false;
				ToDD.Visible = true;
				ToDD.DataBind();
			}
			if (DS.Tables["tot"].Rows.Count > 0 & DS.Tables["from"].Rows.Count > 0)
			{
				SetBT.Enabled = true;
				FindBT.Enabled = false;
				SetBT.Visible = true;
			}
			else
			{
				SetBT.Visible = false;
			}
		}

		public void FindClient(string NameStr, string Where)
		{
			DA.SelectCommand =
				new MySqlCommand(
					"select convert(concat(FirmCode, '. ', ShortName) using cp1251) name from clientsdata, accessright.regionaladmins" +
					" where regioncode =" + RegionDD.SelectedItem.Value + " and firmtype=1 and firmstatus=1" +
					" and shortname like ?NameStr" + " and UserName='" + Session["UserName"] + "'" +
					" and FirmSegment=if(regionaladmins.AlowChangeSegment=1, FirmSegment, DefaultSegment)" +
					"\n and if(UseRegistrant=1, Registrant='" + Session["UserName"] + "', 1=1)" + " and username='" + UserName + "'",
					соединение);
			DA.SelectCommand.Parameters.Add(new MySqlParameter("NameStr", MySqlDbType.VarString));
			DA.SelectCommand.Parameters["NameStr"].Value = "%" + NameStr + "%";
			DA.Fill(DS, Where);
		}

		protected void SetBT_Click(object sender, EventArgs e)
		{
			Int32 ClientCode;
			Int32 ParentClientCode;
			string Query = String.Empty;
			ClientCode = Convert.ToInt32(ToDD.SelectedItem.Value);
			ParentClientCode = Convert.ToInt32(FromDD.SelectedItem.Value);
			MySqlCommand MyCommand = new MySqlCommand();
			MySqlTransaction MyTrans = null;
			try
			{
				Query += 
@"
set @inHost = ?Host;
set @inUser = ?UserName;
";

				Query += " update intersection set MaxSynonymCode=0, MaxSynonymFirmCrCode=0," +
				         " lastsent='2003-01-01' where clientcode=" + ClientCode + ";";
				Query += " update retclientsset as a, retclientsset as b" +
				         " set b.updatetime=a.updatetime, b.AlowCumulativeUpdate=0, b.Active=0 where a.clientcode=" +
				         ParentClientCode + " and b.clientcode=" + ClientCode + ";";
				Query += " update intersection as a, intersection as b" + " set a.MaxSynonymFirmCrCode=b.MaxSynonymFirmCrCode," +
				         " a.MaxSynonymCode=b.MaxSynonymCode, a.lastsent=b.lastsent" + " where a.clientcode=" + ClientCode +
				         " and b.clientcode=" + ParentClientCode + " and a.pricecode=b.pricecode;";
				Query += " insert into logs.clone (LogTime, UserName, FromClientCode, ToClientCode) values (now(), '" +
				         Session["UserName"] + "', " + ParentClientCode + ", " + ClientCode + ")";
				MyCommand.Parameters.Add("Host", HttpContext.Current.Request.UserHostAddress);
				MyCommand.Parameters.Add("UserName", Session["UserName"]);

				соединение.Open();
				MyTrans = соединение.BeginTransaction();
				MyCommand.CommandText = Query;
				MyCommand.Transaction = MyTrans;
				MyCommand.Connection = соединение;
				MyCommand.ExecuteNonQuery();
				MyTrans.Commit();
				Func.Mail("register@analit.net", "Успешное присвоение кодов(" + ParentClientCode + " > " + ClientCode + ")",
				          MailFormat.Text,
				          "От: " + FromDD.SelectedItem.Text + "\nДля: " + ToDD.SelectedItem.Text + "\nОператор: " + UserName,
				          DS.Tables["Regions"].Rows[0]["email"].ToString(), "RegisterList@subscribe.analit.net", Encoding.UTF8);
				LabelErr.ForeColor = Color.Green;
				LabelErr.Text = "Присвоение успешно завершено.Время операции: " + DateTime.Now;
				FromDD.Visible = false;
				ToDD.Visible = false;
				FromTB.Visible = true;
				ToTB.Visible = true;
				FindBT.Visible = true;
				FindBT.Enabled = true;
				SetBT.Visible = false;
			}
			catch (Exception err)
			{
				LabelErr.Text = err.Message;
				if (MyTrans != null)
					MyTrans.Rollback();
			}
			finally
			{
				соединение.Close();
			}
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			if (Convert.ToInt32(Session["AccessGrant"]) != 1)
			{
				Response.Redirect("default.aspx");
			}
			соединение.ConnectionString = Literals.GetConnectionString();
			UserName = Session["UserName"].ToString();
			DA.SelectCommand =
				new MySqlCommand(
					"select regions.region, regions.regioncode, Email from accessright.regionaladmins, farm.regions where accessright.regionaladmins.regionmask & farm.regions.regioncode >0 and username='" +
					UserName + "' order by region", соединение);
			DA.Fill(DS, "Regions");
			if (DS.Tables["Regions"].Rows.Count < 1)
			{
			}
			if (!(IsPostBack))
			{
				RegionDD.DataBind();
			}
		}
	}
}