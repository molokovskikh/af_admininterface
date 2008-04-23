using System;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using MySql.Data.MySqlClient;
using AdminInterface.Helpers;

namespace AddUser
{
	partial class WebForm1 : Page
	{
		private readonly MySqlCommand _command = new MySqlCommand();
		private readonly MySqlConnection _connection = new MySqlConnection();
		protected DataTable admin;
		protected DataTable Clientsdata;
		protected DataColumn DataColumn1;
		protected DataColumn DataColumn10;
		protected DataColumn DataColumn11;
		protected DataColumn DataColumn12;
		protected DataColumn DataColumn13;
		protected DataColumn DataColumn14;
		protected DataColumn DataColumn15;
		protected DataColumn DataColumn16;
		protected DataColumn DataColumn17;
		protected DataColumn DataColumn18;
		protected DataColumn DataColumn19;
		protected DataColumn DataColumn2;
		protected DataColumn DataColumn20;
		protected DataColumn DataColumn21;
		protected DataColumn DataColumn22;
		protected DataColumn DataColumn23;
		protected DataColumn DataColumn24;
		protected DataColumn DataColumn25;
		protected DataColumn DataColumn26;
		protected DataColumn DataColumn3;
		protected DataColumn DataColumn4;
		protected DataColumn DataColumn5;
		protected DataColumn DataColumn6;
		protected DataColumn DataColumn7;
		protected DataColumn DataColumn8;
		protected DataColumn DataColumn9;
		protected DataTable DataTable1;


		protected DataSet DS1;
		protected DataTable FreeCodes;
		protected DataTable Incudes;
		private MySqlTransaction mytrans;
		protected TextBox OldCodeTB;
		protected DataTable Regions;
		protected RequiredFieldValidator RequiredFieldValidator7;
		protected DataTable WorkReg;

		[DebuggerStepThrough]
		private void InitializeComponent()
		{
			DS1 = new DataSet();
			Regions = new DataTable();
			DataColumn1 = new DataColumn();
			DataColumn2 = new DataColumn();
			FreeCodes = new DataTable();
			DataColumn3 = new DataColumn();
			DataColumn4 = new DataColumn();
			WorkReg = new DataTable();
			DataColumn5 = new DataColumn();
			DataColumn6 = new DataColumn();
			DataColumn7 = new DataColumn();
			DataColumn8 = new DataColumn();
			Clientsdata = new DataTable();
			DataColumn9 = new DataColumn();
			DataColumn12 = new DataColumn();
			DataColumn13 = new DataColumn();
			DataColumn14 = new DataColumn();
			DataColumn15 = new DataColumn();
			DataColumn16 = new DataColumn();
			DataColumn17 = new DataColumn();
			DataColumn18 = new DataColumn();
			DataColumn19 = new DataColumn();
			DataColumn20 = new DataColumn();
			DataColumn21 = new DataColumn();
			admin = new DataTable();
			DataTable1 = new DataTable();
			DataColumn22 = new DataColumn();
			DataColumn23 = new DataColumn();
			Incudes = new DataTable();
			DataColumn24 = new DataColumn();
			DataColumn25 = new DataColumn();
			DataColumn26 = new DataColumn();
			DS1.BeginInit();
			Regions.BeginInit();
			FreeCodes.BeginInit();
			WorkReg.BeginInit();
			Clientsdata.BeginInit();
			admin.BeginInit();
			DataTable1.BeginInit();
			Incudes.BeginInit();
			DS1.DataSetName = "NewDataSet";
			DS1.Locale = new CultureInfo("ru-RU");
			DS1.Tables.AddRange(new[] {Regions, FreeCodes, WorkReg, Clientsdata, admin, DataTable1, Incudes});
			Regions.Columns.AddRange(new[] {DataColumn1, DataColumn2});
			Regions.TableName = "Regions";
			DataColumn1.ColumnName = "Region";
			DataColumn2.ColumnName = "RegionCode";
			DataColumn2.DataType = typeof (Int64);
			FreeCodes.Columns.AddRange(new[] {DataColumn3, DataColumn4});
			FreeCodes.TableName = "FreeCodes";
			DataColumn3.ColumnName = "FirmCode";
			DataColumn3.DataType = typeof (Int32);
			DataColumn4.ColumnName = "ShortName";
			WorkReg.Columns.AddRange(new[] {DataColumn5, DataColumn6, DataColumn7, DataColumn8});
			WorkReg.TableName = "WorkReg";
			DataColumn5.ColumnName = "RegionCode";
			DataColumn5.DataType = typeof (Int32);
			DataColumn6.ColumnName = "Region";
			DataColumn7.ColumnName = "ShowMask";
			DataColumn7.DataType = typeof (Boolean);
			DataColumn8.ColumnName = "RegMask";
			DataColumn8.DataType = typeof (Boolean);
			Clientsdata.Columns.AddRange(
				new[]
					{
						DataColumn9, DataColumn10, DataColumn11, DataColumn12, DataColumn13, DataColumn14, DataColumn15, DataColumn16,
						DataColumn17, DataColumn18, DataColumn19, DataColumn20, DataColumn21
					});
			Clientsdata.TableName = "Clientsdata";
			DataColumn9.ColumnName = "adress";
			DataColumn12.ColumnName = "fax";
			DataColumn13.ColumnName = "firmsegment";
			DataColumn13.DataType = typeof (Int16);
			DataColumn14.ColumnName = "firmtype";
			DataColumn14.DataType = typeof (Int16);
			DataColumn15.ColumnName = "oldcode";
			DataColumn15.DataType = typeof (Int16);
			DataColumn16.ColumnName = "phone";
			DataColumn17.ColumnName = "regioncode";
			DataColumn17.DataType = typeof (Int64);
			DataColumn18.ColumnName = "shortname";
			DataColumn19.ColumnName = "url";
			DataColumn20.ColumnName = "fullname";
			DataColumn21.ColumnName = "mail";
			admin.TableName = "admin";
			DataTable1.Columns.AddRange(new[] {DataColumn22, DataColumn23});
			DataTable1.TableName = "Payers";
			DataColumn22.ColumnName = "PayerID";
			DataColumn22.DataType = typeof (Int32);
			DataColumn23.ColumnName = "PayerName";
			Incudes.Columns.AddRange(new[] {DataColumn24, DataColumn25, DataColumn26});
			Incudes.TableName = "Includes";
			DataColumn24.ColumnName = "FirmCode";
			DataColumn24.DataType = typeof (uint);
			DataColumn25.ColumnName = "ShortName";
			DataColumn26.ColumnName = "RegionCode";
			DataColumn26.DataType = typeof (ulong);
			DS1.EndInit();
			Regions.EndInit();
			FreeCodes.EndInit();
			WorkReg.EndInit();
			Clientsdata.EndInit();
			admin.EndInit();
			DataTable1.EndInit();
			Incudes.EndInit();
		}

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			InitializeComponent();
		}

		//******** InsertOldData *************
		//заполнить форму из базы, базируясь на FirmCode

		//******** SelectTODS *************
		//выполнить запрос, данные запроса будут внесены в DataSource DS1, в таблицу с имененм Table

		//******** SetWorkRegions *************
		//при выборе региона клиента обновляет 'Регионы работы' и 'Показываемые регионы'
		//выделяя те регионы, которые установлены как регионы по умолчанию в таблице regions

		private void SetWorkRegions(string RegCode, bool AllRegions)
		{
			string commandText;
			if (AllRegions)
			{
				commandText =
					@"
SELECT  a.RegionCode, 
        a.Region, 
        (b.defaultshowregionmask & ?RegionCode) > 0           as ShowMask, 
        a.regioncode                            = ?RegionCode as RegMask 
FROM    farm.regions                                          as a, 
        farm.regions                                          as b, 
        accessright.regionaladmins 
WHERE   a.regioncode & b.defaultshowregionmask       > 0 
        AND regionaladmins.username                  = ?UserName 
        AND a.regioncode & regionaladmins.RegionMask > 0 
GROUP BY regioncode 
ORDER BY region;
";
			}
			else
			{
				commandText =
					@"
SELECT  a.RegionCode, 
        a.Region, 
        (b.defaultshowregionmask & ?RegionCode) > 0           as ShowMask, 
        a.regioncode                            = ?RegionCode as RegMask 
FROM    farm.regions                                          as a, 
        farm.regions                                          as b, 
        accessright.regionaladmins 
WHERE   b.regioncode                                 = ?RegionCode 
        AND a.regioncode & b.defaultshowregionmask   > 0 
        AND regionaladmins.username                  = ?UserName 
        AND a.regioncode & regionaladmins.RegionMask > 0 
GROUP BY regioncode 
ORDER BY region;
";
			}

			var adapter = new MySqlDataAdapter(commandText, _connection);
			try
			{
				_connection.Open();
				adapter.SelectCommand.Transaction = _connection.BeginTransaction(IsolationLevel.ReadCommitted);
				adapter.SelectCommand.Parameters.AddWithValue("?RegionCode", RegCode);
				adapter.SelectCommand.Parameters.AddWithValue("?UserName", Session["UserName"]);
				adapter.Fill(DS1, "WorkReg");
				adapter.SelectCommand.Transaction.Commit();
			}
			finally
			{
				_connection.Close();
			}

			WRList.DataBind();
			WRList2.DataBind();
			OrderList.DataBind();
			for (int i = 0; i <= WRList.Items.Count - 1; i++)
			{
				if (WRList.Items[i].Value == RegCode)
				{
					WRList.Items[i].Selected = true;
				}
				if (WRList2.Items[i].Value == RegCode)
				{
					WRList2.Items[i].Selected = true;
				}
				OrderList.Items[i].Selected = WRList.Items[i].Selected;
			}
		}

		protected void RegionDD_SelectedIndexChanged(object sender, EventArgs e)
		{
			SetWorkRegions(RegionDD.SelectedItem.Value, CheckBox1.Checked);
		}

		protected void Register_Click(object sender, EventArgs e)
		{
			if (!IsValid)
				return;

			_connection.Open();
			mytrans = _connection.BeginTransaction(IsolationLevel.RepeatableRead);
			Int64 MaskRegion = 0;
			Int64 ShowRegionMask = 0;
			Int64 WorkMask = 0;
			Int64 OrderMask = 0;
			for (int i = 0; i <= WRList.Items.Count - 1; i++)
			{
				if (WRList.Items[i].Selected)
				{
					MaskRegion += Convert.ToInt64(WRList.Items[i].Value);
				}
				if (WRList2.Items[i].Selected)
				{
					WorkMask += Convert.ToInt64(WRList2.Items[i].Value);
				}
				if (OrderList.Items[i].Selected)
				{
					OrderMask += Convert.ToInt64(OrderList.Items[i].Value);
				}
			}
			_command.Connection = _connection;
			_command.Transaction = mytrans;
			_command.CommandText =
				@"
set @inHost = ?Host;
set @inUser = ?UserName;
";
			_command.Parameters.AddWithValue("?Host", HttpContext.Current.Request.UserHostAddress);
			_command.Parameters.AddWithValue("?UserName", Session["UserName"]);
			_command.ExecuteNonQuery();

			_command.Parameters.Add(new MySqlParameter("?MaskRegion", MySqlDbType.Int64));
			_command.Parameters["?MaskRegion"].Value = MaskRegion;
			_command.Parameters.Add(new MySqlParameter("?OrderMask", MySqlDbType.Int64));
			_command.Parameters["?OrderMask"].Value = OrderMask;
			_command.Parameters.Add(new MySqlParameter("?ShowRegionMask", MySqlDbType.Int64));
			_command.Parameters["?ShowRegionMask"].Value = ShowRegionMask;
			_command.Parameters.Add(new MySqlParameter("?WorkMask", MySqlDbType.Int64));
			_command.Parameters["?WorkMask"].Value = WorkMask;
			_command.Parameters.Add(new MySqlParameter("?fullname", MySqlDbType.VarString));
			_command.Parameters["?fullname"].Value = FullNameTB.Text;
			_command.Parameters.Add(new MySqlParameter("?shortname", MySqlDbType.VarString));
			_command.Parameters["?shortname"].Value = ShortNameTB.Text;
			_command.Parameters.Add(new MySqlParameter("?BeforeNamePrefix", MySqlDbType.VarString));
			_command.Parameters["?BeforeNamePrefix"].Value = "";
			if (TypeDD.SelectedItem.Text == "Аптека")
			{
				_command.Parameters["?BeforeNamePrefix"].Value = "Аптека";
			}
			_command.Parameters.Add(new MySqlParameter("?firmsegment", MySqlDbType.Int24));
			_command.Parameters["?firmsegment"].Value = SegmentDD.SelectedItem.Value;
			_command.Parameters.Add(new MySqlParameter("?RegionCode", MySqlDbType.Int24));
			_command.Parameters["?RegionCode"].Value = RegionDD.SelectedItem.Value;
			_command.Parameters.Add(new MySqlParameter("?adress", MySqlDbType.VarString));
			_command.Parameters["?adress"].Value = AddressTB.Text;
			_command.Parameters.Add(new MySqlParameter("?firmtype", MySqlDbType.Int24));
			_command.Parameters["?firmtype"].Value = TypeDD.SelectedItem.Value;
			_command.Parameters.Add(new MySqlParameter("?registrant", MySqlDbType.VarString));
			_command.Parameters["?registrant"].Value = Session["UserName"];
			_command.Parameters.Add(new MySqlParameter("?ClientCode", MySqlDbType.Int24));
			_command.Parameters.Add(new MySqlParameter("?AllowGetData", MySqlDbType.Int24));
			_command.Parameters["?AllowGetData"].Value = TypeDD.SelectedItem.Value;
			_command.Parameters.Add(new MySqlParameter("?OSUserName", MySqlDbType.VarString));
			_command.Parameters["?OSUserName"].Value = LoginTB.Text;
			_command.Parameters.Add(new MySqlParameter("?OSUserPass", MySqlDbType.VarString));
			_command.Parameters["?OSUserPass"].Value = PassTB.Text;
			_command.Parameters.AddWithValue("?ServiceClient", ServiceClient.Checked);

			if (IncludeCB.Checked && IncludeType.SelectedItem.Value != "0")
				_command.Parameters.AddWithValue("?PrimaryClientCode", IncludeSDD.SelectedValue);

			_command.Parameters.AddWithValue("?IncludeType", IncludeType.SelectedValue);
			_command.Parameters.AddWithValue("?invisibleonfirm", CustomerType.SelectedItem.Value);

			try
			{
				if (IncludeCB.Checked)
					Session["DogN"] = Convert.ToInt32(new MySqlCommand("select billingcode from clientsdata where firmcode=" + IncludeSDD.SelectedValue, _connection).ExecuteScalar());
				else if (!PayerPresentCB.Checked || (PayerPresentCB.Checked && PayerDDL.SelectedItem == null))
					Session["DogN"] = CreateClientOnBilling();
				else
					Session["DogN"] = PayerDDL.SelectedItem.Value;

				_command.Parameters["?ClientCode"].Value = CreateClientOnClientsData();
				Session["Code"] = _command.Parameters["?ClientCode"].Value;

				CreateClientOnOSUserAccessRight();
				if (IncludeCB.Checked)
					CreateClientOnShowInclude(Convert.ToInt32(IncludeSDD.SelectedValue));

				if (TypeDD.SelectedItem.Text == "Аптека")
					CreateClientOnRCS_and_I(CustomerType.SelectedItem.Text != "Стандартный");
				else
					CreatePriceRecords();

				if (!IncludeCB.Checked || (IncludeCB.Checked && IncludeType.SelectedItem.Text != "Стандартный"))
				{
					ADHelper.CreateUserInAD(_command.Parameters["?OSUserName"].Value.ToString(),
					                        _command.Parameters["?OSUserPass"].Value.ToString(),
					                        _command.Parameters["?ClientCode"].Value.ToString());
#if !DEBUG
					CreateFtpDirectory(String.Format(@"\\acdcserv\ftp\optbox\{0}\",
					                                 _command.Parameters["?ClientCode"].Value),
					                   String.Format(@"ANALIT\{0}",
					                                 _command.Parameters["?OSUserName"].Value));
#endif
				}
				mytrans.Commit();
				Session["strStatus"] = "Yes";
				try
				{
					if (TypeDD.SelectedItem.Text == "Аптека"
						&& !IncludeCB.Checked
						&& !ServiceClient.Checked
					    && CustomerType.SelectedItem.Text == "Стандартный"
					    || (TypeDD.SelectedItem.Text == "Аптека"
					        && IncludeCB.Checked
							&& !ServiceClient.Checked
					        && IncludeType.SelectedItem.Text != "Скрытый"))
					{
						var dataAdapter =
							new MySqlDataAdapter(
								@"
select c.contactText
from usersettings.clientsdata cd
  join contacts.contact_groups cg on cd.ContactGroupOwnerId = cg.ContactGroupOwnerId
    join contacts.contacts c on cg.Id = c.ContactOwnerId
where length(c.contactText) > 0
      and firmcode in (select pd.FirmCode
                        from pricesdata as pd, pricesregionaldata as prd
                        where pd.enabled = 1
                              and prd.enabled = 1
                              and firmstatus = 1
                              and firmtype = 0
                              and firmsegment = 0
                              and MaskRegion & ?Region >0)
      and cg.Type = ?ContactGroupType
      and c.Type = ?ContactType

union

select c.contactText
from usersettings.clientsdata cd
  join contacts.contact_groups cg on cd.ContactGroupOwnerId = cg.ContactGroupOwnerId
    join contacts.persons p on cg.id = p.ContactGroupId
      join contacts.contacts c on p.Id = c.ContactOwnerId
where length(c.contactText) > 0
      and firmcode in (select pd.FirmCode
                        from pricesdata as pd, pricesregionaldata as prd
                        where pd.enabled = 1
                              and prd.enabled = 1
                              and firmstatus = 1
                              and firmtype = 0
                              and firmsegment = 0
                              and MaskRegion & ?Region > 0)
      and cg.Type = ?ContactGroupType
      and c.Type = ?ContactType;",
								_connection);
						dataAdapter.SelectCommand.Parameters.AddWithValue("?Region", RegionDD.SelectedItem.Value);
						dataAdapter.SelectCommand.Parameters.AddWithValue("?ContactGroupType", ContactGroupType.ClientManagers);
						dataAdapter.SelectCommand.Parameters.AddWithValue("?ContactType", ContactType.Email);
						dataAdapter.Fill(DS1, "FirmEmail");
						if (DS1.Tables["FirmEmail"].Rows.Count > 0)
						{
							foreach (DataRow Row in DS1.Tables["FirmEmail"].Rows)
							{
								Func.Mail("pharm@analit.net", "Аналитическая Компания Инфорум",
								          "Новый клиент в системе \"АналитФАРМАЦИЯ\"",
								          false,
								          "Добрый день. \n\nВ информационной системе \"АналитФАРМАЦИЯ\", участником которой является Ваша организация, зарегистрирован новый клиент: "
								          + String.Format("{0} ( {1} ) в регионе(городе)", FullNameTB.Text, ShortNameTB.Text)
								          + RegionDD.SelectedItem.Text + "."
								          +
								          "\nПожалуйста произведите настройки для данного клиента (Раздел \"Для зарегистрированных пользователей\" на сайте www.analit.net )."
								          +
								          String.Format("\nАдрес доставки накладных: {0}@waybills.analit.net",
								                        _command.Parameters["?ClientCode"].Value)
								          + "\r\nС уважением, Аналитическая компания \"Инфорум\", г. Воронеж"
								          + @"
Москва  +7 495 6628727
С.-Петербург +7 812 3090521
Воронеж +7 4732 606000
Челябинск +7 351 729 8143"
								          + "\n", Row["ContactText"].ToString(), "", null, Encoding.UTF8);
							}
						}
						else
						{
							Func.Mail("register@analit.net", String.Empty,
									  "\"" + String.Format("{0} ( {1} )", FullNameTB.Text, ShortNameTB.Text) + "\" - ошибка уведомления поставщиков",
							          false, "Оператор: " + Session["UserName"] + "\nРегион: "
							                 + RegionDD.SelectedItem.Text + "\nLogin: " + LoginTB.Text
							                 + "\nКод: " + Session["Code"] + "\n\nСегмент: "
							                 + SegmentDD.SelectedItem.Text + "\nТип: " + TypeDD.SelectedItem.Text
							                 + "Ошибка: Ничего не получилось выбрать из базы",
							          "RegisterList@subscribe.analit.net", String.Empty,
							          DS1.Tables["admin"].Rows[0]["email"].ToString(), Encoding.UTF8);
						}
					}
				}
				catch (Exception err)
				{
					Func.Mail("register@analit.net", String.Empty,
					          "\"" + FullNameTB.Text + "\" - ошибка уведомления поставщиков",
					          false, "Оператор: " + Session["UserName"] + "\nРегион: "
					                 + RegionDD.SelectedItem.Text + "\nLogin: " + LoginTB.Text + "\nКод: "
					                 + Session["Code"] + "\n\nСегмент: " + SegmentDD.SelectedItem.Text
					                 + "\nТип: " + TypeDD.SelectedItem.Text + "Ошибка: " + err.Source + ": "
					                 + err.Message, "RegisterList@subscribe.analit.net", String.Empty,
					          DS1.Tables["admin"].Rows[0]["email"].ToString(), Encoding.UTF8);
				}

				Func.Mail("register@analit.net", String.Empty, "\"" + FullNameTB.Text + "\" - успешная регистрация",
				          false, "Оператор: " + Session["UserName"] + "\nРегион: "
				                 + RegionDD.SelectedItem.Text + "\nLogin: " + LoginTB.Text
				                 + "\nКод: " + Session["Code"] + "\n\nСегмент: " + SegmentDD.SelectedItem.Text
				                 + "\nТип: " + TypeDD.SelectedItem.Text, "RegisterList@subscribe.analit.net", String.Empty,
				          DS1.Tables["admin"].Rows[0]["email"].ToString(), Encoding.UTF8);

				Func.Mail("register@analit.net",
				          "",
				          "Регистрация нового клиента",
				          false,
				          String.Format(
				          	@"Зарегистрирован новый клиент
Название: {0}
Код: {1}
Биллинг код: {2}
Кем зарегистрирован: {3}",
				          	ShortNameTB.Text, Session["Code"], Session["DogN"], Session["UserName"]),
				          "billing@analit.net",
				          "",
				          "",
				          Encoding.UTF8);

				Session["Name"] = FullNameTB.Text;
				Session["ShortName"] = ShortNameTB.Text;
				Session["Login"] = LoginTB.Text;
				Session["Password"] = PassTB.Text;
				Session["Tariff"] = TypeDD.SelectedItem.Text;
				Session["Register"] = true;
				if (!IncludeCB.Checked 
					|| (IncludeCB.Checked && IncludeType.SelectedItem.Text != "Базовый"))
				{
					if (!IncludeCB.Checked && EnterBillingInfo.Checked)
						Response.Redirect(String.Format("Billing/Register.rails?id={0}", Session["DogN"]));
					else
						Response.Redirect("report.aspx");
				}
				else
				{
					Page.Controls.Clear();
					var LB = new Label();
					LB.Text = "Регистрация завершена успешно.";
					LB.Font.Name = "Verdana";
					Page.Controls.Add(LB);
				}
			}
			catch (Exception excL)
			{
				if (!(excL is ThreadAbortException))
					mytrans.Rollback();
				throw;
			}
			finally
			{
				if (_connection.State == ConnectionState.Open)
					_connection.Close();

				_command.Dispose();
				_connection.Dispose();
			}
		}

		protected void PayerPresentCB_CheckedChanged(object sender, EventArgs e)
		{
			if (PayerPresentCB.Checked)
			{
				PayerPresentCB.Text = "Плательщик существует:";
				PayerFTB.Visible = true;
				FindPayerB.Visible = true;
			}
			else
			{
				PayerPresentCB.Text = "Плательщик существует";
				PayerDDL.Visible = false;
				PayerFTB.Visible = false;
				FindPayerB.Visible = false;
				PayerCountLB.Visible = false;
			}
		}

		protected void FindPayerB_Click(object sender, EventArgs e)
		{
			var adapter =
				new MySqlDataAdapter(
					@"
SELECT  DISTINCT PayerID, 
        convert(concat(PayerID, '. ', p.ShortName) using cp1251) PayerName  
FROM    clientsdata as cd, 
        accessright.regionaladmins, 
        billing.payers p 
WHERE   p.payerid                                = cd.billingcode 
        AND cd.regioncode & regionaladmins.regionmask > 0 
        AND regionaladmins.UserName                   = ?UserName  
        AND FirmType                             = if(ShowRetail+ShowVendor = 2, FirmType, if(ShowRetail = 1, 1, 0)) 
        AND if(UseRegistrant                     = 1, Registrant = ?UserName, 1 = 1) 
        AND firmstatus                           = 1 
        AND billingstatus                        = 1 
        AND p.ShortName like ?SearchText  
ORDER BY p.shortname;
",
					_connection);
			try
			{
				_connection.Open();
				adapter.SelectCommand.Transaction = _connection.BeginTransaction(IsolationLevel.ReadCommitted);
				adapter.SelectCommand.Parameters.AddWithValue("?UserName", Session["UserName"]);
				adapter.SelectCommand.Parameters.AddWithValue("?SearchText", String.Format("%{0}%", PayerFTB.Text));
				adapter.Fill(DS1, "Payers");

				adapter.SelectCommand.Transaction.Commit();
			}
			finally
			{
				_connection.Close();
			}
			PayerDDL.DataBind();
			PayerCountLB.Text = "[" + PayerDDL.Items.Count + "]";
			PayerCountLB.Visible = true;
			if (PayerDDL.Items.Count > 0)
			{
				PayerDDL.Visible = true;
				PayerFTB.Visible = false;
				FindPayerB.Visible = false;
			}
		}

		protected void IncludeCB_CheckedChanged(object sender, EventArgs e)
		{
			EnterBillingInfo.Enabled = !IncludeCB.Checked;
			EnterBillingInfo.Checked = !IncludeCB.Checked;
			PayerPresentCB.Enabled = !IncludeCB.Checked;
			PayerPresentCB.Checked = false;
			CustomerType.SelectedIndex = 0;
			if (IncludeCB.Checked)
			{
				IncludeCB.Text = "Подчинен клиенту:";
				PayerFTB.Visible = false;
				FindPayerB.Visible = false;
				RegionDD.Enabled = false;
				TypeDD.Enabled = false;
				TypeDD.SelectedIndex = 0;
				SegmentDD.Enabled = false;
				CustomerType.Enabled = false;
				WRList.Enabled = false;
				WRList2.Enabled = false;
				PayerDDL.Visible = false;
				PayerCountLB.Visible = false;
				IncludeSTB.Visible = true;
				IncludeSB.Visible = true;
			}
			else
			{
				RegionDD.Enabled = true;
				TypeDD.Enabled = true;
				SegmentDD.Enabled = true;
				CustomerType.Enabled = true;
				WRList.Enabled = true;
				WRList2.Enabled = true;
				IncludeCB.Text = "Подчиненный клиент";
				IncludeSTB.Visible = false;
				IncludeSB.Visible = false;
				IncludeSDD.Visible = false;
				IncludeType.Visible = false;
				IncludeCountLB.Visible = false;
			}
		}

		protected void IncludeSB_Click(object sender, EventArgs e)
		{
			var adapter = new MySqlDataAdapter(
				@"
SELECT  DISTINCT cd.FirmCode, 
        convert(concat(cd.FirmCode, '. ', cd.ShortName) using cp1251) ShortName, 
        cd.RegionCode  
FROM    (accessright.regionaladmins, clientsdata as cd)  
LEFT JOIN includeregulation ir 
        ON ir.includeclientcode              = cd.firmcode  
WHERE   cd.regioncode & regionaladmins.regionmask > 0  
        AND regionaladmins.UserName               = ?UserName  
        AND FirmType                         = if(ShowRetail+ShowVendor = 2, FirmType, if(ShowRetail = 1, 1, 0)) 
        AND if(UseRegistrant                 = 1, Registrant = ?UserName, 1 = 1)  
        AND cd.ShortName like ?SearchText 
        AND FirmStatus    = 1  
        AND billingstatus = 1  
        AND FirmType      = 1  
        AND ir.primaryclientcode is null  
ORDER BY cd.shortname;
",
				_connection);
			try
			{
				_connection.Open();
				adapter.SelectCommand.Transaction = _connection.BeginTransaction(IsolationLevel.ReadCommitted);
				adapter.SelectCommand.Parameters.AddWithValue("?UserName", Session["UserName"]);
				adapter.SelectCommand.Parameters.AddWithValue("?SearchText", String.Format("%{0}%", IncludeSTB.Text));
				adapter.Fill(DS1, "Includes");
				adapter.SelectCommand.Transaction.Commit();
			}
			finally
			{
				_connection.Close();
			}

			IncludeSDD.DataBind();
			IncludeCountLB.Text = "[" + IncludeSDD.Items.Count + "]";
			IncludeCountLB.Visible = true;
			if (DS1.Tables["Includes"].Rows.Count > 0)
			{
				RegionDD.SelectedValue = DS1.Tables["Includes"].Rows[0]["RegionCode"].ToString();
				SetWorkRegions(RegionDD.SelectedItem.Value, CheckBox1.Checked);
			}
			if (IncludeSDD.Items.Count > 0)
			{
				IncludeType.Visible = true;
				IncludeSDD.Visible = true;
				IncludeSTB.Visible = false;
				IncludeSB.Visible = false;
			}
		}

		private int CreateClientOnBilling()
		{
			_command.CommandText =
				"insert into billing.payers(OldTariff, OldPayDate, Comment, PayerID, ShortName, BeforeNamePrefix, ContactGroupOwnerId) values(0, now(), 'Дата регистрации: " +
				DateTime.Now + "', null, ?ShortName, ?BeforeNamePrefix, ?BillingContactGroupOwnerId); ";
			_command.CommandText += "SELECT LAST_INSERT_ID()";
			_command.Parameters.AddWithValue("?BillingContactGroupOwnerId", CreateContactsForBilling(_command.Connection));
			return Convert.ToInt32(_command.ExecuteScalar());
		}

		private int CreateClientOnClientsData()
		{
			_command.CommandText =
				@"INSERT INTO usersettings.clientsdata (
MaskRegion, ShowRegionMask, FullName, ShortName, FirmSegment, RegionCode, Adress, 
FirmType, FirmStatus, registrant, BillingCode, BillingStatus, ContactGroupOwnerId, RegistrationDate) ";
			_command.Parameters.AddWithValue("?ClientContactGroupOwnerId",
			                                 CreateContactsForClientsData(_command.Connection,
			                                                              TypeDD.SelectedItem.Text == "Аптека"
			                                                              	? ClientType.Drugstore
			                                                              	: ClientType.Supplier));
			if (!IncludeCB.Checked)
			{
				_command.CommandText += @" 
Values(?maskregion, ?ShowRegionMask, ?FullName, ?ShortName, ?FirmSegment, ?RegionCode, ?Adress, 
?FirmType, 1, ?registrant, " + Session["DogN"] + ", 1, ?ClientContactGroupOwnerId, now()); ";
			}
			else
			{
				_command.CommandText += @"
select maskregion, ShowRegionMask, ?FullName, ?ShortName, FirmSegment, RegionCode, ?Adress, 
FirmType, 1, ?registrant, BillingCode, BillingStatus, ?ClientContactGroupOwnerId, now()
from usersettings.clientsdata where firmcode=" +
					IncludeSDD.SelectedValue + "; ";
			}
			_command.CommandText += "SELECT LAST_INSERT_ID()";
			return Convert.ToInt32(_command.ExecuteScalar());
		}

		private void CreateClientOnOSUserAccessRight()
		{
			_command.CommandText =
				"INSERT INTO usersettings.osuseraccessright (ClientCode, AllowGetData, OSUserName) Values(?ClientCode, ?AllowGetData, ?OSUserName)";
			_command.ExecuteNonQuery();
		}

		public void CreatePriceRecords()
		{
			_command.CommandText = @"
INSERT INTO OrderSendRules.order_send_rules(Firmcode, FormaterId, SenderId)
VALUES(?ClientCode,
		(SELECT id FROM OrderSendRules.order_handlers o WHERE ClassName = 'DefaultFormater'),
		(SELECT id FROM OrderSendRules.order_handlers o WHERE ClassName = 'EmailSender'));

INSERT INTO pricesdata(Firmcode, PriceCode) VALUES(?ClientCode, null);   
SET @NewPriceCode:=Last_Insert_ID(); 

INSERT INTO farm.formrules() VALUES();
SET @NewFormRulesId = Last_Insert_ID();
INSERT INTO farm.sources() VALUES(); 
SET @NewSourceId = Last_Insert_ID();

INSERT INTO usersettings.PriceItems(FormRuleId, SourceId) VALUES(@NewFormRulesId, @NewSourceId);
SET @NewPriceItemId = Last_Insert_ID();

INSERT INTO PricesCosts (PriceCode, BaseCost, PriceItemId) SELECT @NewPriceCode, 1, @NewPriceItemId;
SET @NewPriceCostId:=Last_Insert_ID(); 

INSERT INTO farm.costformrules (CostCode) SELECT @NewPriceCostId; 

INSERT 
INTO    regionaldata
        (
                regioncode, 
                firmcode
        )  
SELECT  DISTINCT regions.regioncode, 
        clientsdata.firmcode  
FROM    (clientsdata, farm.regions, pricesdata)  
LEFT JOIN regionaldata 
        ON regionaldata.firmcode                         =clientsdata.firmcode 
        AND regionaldata.regioncode                      = regions.regioncode  
WHERE   pricesdata.firmcode                              =clientsdata.firmcode  
        AND clientsdata.firmcode                         =?ClientCode  
        AND (clientsdata.maskregion & regions.regioncode)>0  
        AND regionaldata.firmcode is null; 

INSERT 
INTO    pricesregionaldata
        (
                regioncode, 
                pricecode
        )  
SELECT  DISTINCT regions.regioncode, 
        pricesdata.pricecode  
FROM    (clientsdata, farm.regions, pricesdata, clientsdata as a)  
LEFT JOIN pricesregionaldata 
        ON pricesregionaldata.pricecode                  =pricesdata.pricecode 
        AND pricesregionaldata.regioncode                = regions.regioncode  
WHERE   pricesdata.firmcode                              =clientsdata.firmcode  
        AND clientsdata.firmcode                         =?ClientCode  
        AND (clientsdata.maskregion & regions.regioncode)>0  
        AND pricesregionaldata.pricecode is null; 


INSERT 
INTO    intersection
        (
                ClientCode, 
                regioncode, 
                pricecode, 
                invisibleonclient, 
                InvisibleonFirm, 
                costcode
        )
SELECT  DISTINCT clientsdata2.firmcode,
        regions.regioncode, 
        pricesdata.pricecode,  
		if(pricesdata.PriceType = 0, 0, 1) as invisibleonclient,
        a.invisibleonfirm,
        (
          SELECT costcode
          FROM    pricescosts pcc
          WHERE   basecost
                  AND pcc.PriceCode = pricesdata.PriceCode
        ) as CostCode
FROM pricesdata 
	JOIN clientsdata ON pricesdata.firmcode = clientsdata.firmcode
		JOIN clientsdata as clientsdata2 ON clientsdata.firmsegment = clientsdata2.firmsegment
			JOIN retclientsset as a ON a.clientcode = clientsdata2.firmcode
	JOIN farm.regions ON (clientsdata.maskregion & regions.regioncode) > 0 and (clientsdata2.maskregion & regions.regioncode) > 0
		JOIN pricesregionaldata ON pricesregionaldata.pricecode = pricesdata.pricecode AND pricesregionaldata.regioncode = regions.regioncode
	LEFT JOIN intersection ON intersection.pricecode = pricesdata.pricecode AND intersection.regioncode = regions.regioncode AND intersection.clientcode = clientsdata2.firmcode
WHERE   intersection.pricecode IS NULL
        AND clientsdata.firmstatus = 1
        AND clientsdata.firmtype = 0
		AND pricesdata.PriceCode = @NewPriceCode
		AND clientsdata2.firmtype = 1;
";
			_command.ExecuteNonQuery();
		}

		private void CreateClientOnRCS_and_I(bool Invisible)
		{
			_command.CommandText = @"
INSERT INTO usersettings.retclientsset (ClientCode, InvisibleOnFirm, WorkRegionMask, OrderRegionMask, BasecostPassword, ServiceClient) 
Values(?ClientCode, ?InvisibleOnFirm, ?WorkMask, ?OrderMask, GeneratePassword(), ?ServiceClient);

INSERT INTO usersettings.ret_update_info (ClientCode, CurrentExeVersion) 
Values(?ClientCode, (SELECT max(currentExeVersion)
FROM usersettings.ret_update_info r
  join usersettings.clientsdata cd on cd.firmcode = r.clientcode
where billingcode <> 921));

INSERT 
INTO    intersection
        (
                ClientCode, 
                regioncode, 
                pricecode, 
                invisibleonclient, 
                InvisibleonFirm, 
                costcode
        )
SELECT  DISTINCT clientsdata2.firmcode,
        regions.regioncode, 
        pricesdata.pricecode,  
        if(pricesdata.PriceType = 0, 0, 1) as invisibleonclient,
        a.invisibleonfirm,
        (
          SELECT costcode
          FROM    pricescosts pcc
          WHERE   basecost
                  AND pcc.PriceCode = pricesdata.PriceCode
        ) as CostCode
FROM clientsdata as clientsdata2
	JOIN retclientsset as a ON a.clientcode = clientsdata2.firmcode
	JOIN clientsdata ON clientsdata.firmsegment = clientsdata2.firmsegment
		JOIN pricesdata ON pricesdata.firmcode = clientsdata.firmcode
	JOIN farm.regions ON (clientsdata.maskregion & regions.regioncode) > 0 and (clientsdata2.maskregion & regions.regioncode) > 0
		JOIN pricesregionaldata ON pricesregionaldata.pricecode = pricesdata.pricecode AND pricesregionaldata.regioncode = regions.regioncode
	LEFT JOIN intersection ON intersection.pricecode = pricesdata.pricecode AND intersection.regioncode = regions.regioncode AND intersection.clientcode = clientsdata2.firmcode
WHERE   intersection.pricecode IS NULL
        AND clientsdata.firmstatus = 1
        AND clientsdata.firmtype = 0
		AND clientsdata2.FirmCode = ?clientCode
		AND clientsdata2.firmtype = 1;
";
			if (IncludeCB.Checked && IncludeType.SelectedItem.Value != "0")
			{
				if (IncludeType.SelectedItem.Value == "1")
				{
					_command.CommandText += @"
UPDATE includeregulation i, 
        intersection as src, 
        intersection as dst 
        SET dst.costcode      = src.costcode, 
        dst.firmcostcorr      = src.firmcostcorr, 
        dst.publiccostcorr    = src.publiccostcorr, 
        dst.minreq            = src.minreq, 
        dst.controlminreq     = src.controlminreq, 
        dst.invisibleonclient = src.invisibleonclient 
WHERE   dst.clientcode        = ?ClientCode    
        AND src.clientcode    = ?PrimaryClientCode    
		AND	dst.regioncode    = src.regioncode 
        AND dst.pricecode     = src.pricecode 
        AND src.clientcode    = i.primaryclientcode 
        AND dst.clientcode    = i.includeclientcode;
";
				}
				else
				{
					_command.CommandText += @"
UPDATE includeregulation i, 
        intersection as src, 
        intersection as dst 
        SET dst.costcode      = src.costcode, 
        dst.firmcostcorr      = src.firmcostcorr, 
        dst.publiccostcorr    = src.publiccostcorr, 
        dst.minreq            = src.minreq, 
        dst.controlminreq     = src.controlminreq, 
        dst.invisibleonclient = src.invisibleonclient, 
        dst.FirmClientCode    = src.FirmClientCode, 
        dst.FirmClientCode2   = src.FirmClientCode2, 
        dst.FirmClientCode3   = src.FirmClientCode3
WHERE	dst.clientcode        = ?ClientCode    
        AND src.clientcode    = ?PrimaryClientCode
		AND dst.regioncode    = src.regioncode 
        AND dst.pricecode     = src.pricecode 
        AND src.clientcode    = i.primaryclientcode 
        AND dst.clientcode    = i.includeclientcode;  
";
				}
			}
			if (!Invisible)
				_command.CommandText += " insert into inscribe(ClientCode) values(?ClientCode); ";
			_command.ExecuteNonQuery();
		}

		private void CreateClientOnShowInclude(int PrimaryClientCode)
		{
			_command.CommandText = "INSERT INTO showregulation" + "(PrimaryClientCode, ShowClientCode, Addition)" + " VALUES (" +
			                       PrimaryClientCode + ", ?ClientCode, ?ShortName);" + "INSERT INTO includeregulation" +
			                       "(ID, PrimaryClientCode, IncludeClientCode, Addition, IncludeType)" + "VALUES(NULL," +
			                       PrimaryClientCode +
			                       ", ?ClientCode, ?ShortName, ?IncludeType)";
			_command.ExecuteNonQuery();
		}

		protected void IncludeSDD_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				_connection.Open();
				var command = new MySqlCommand("SELECT RegionCode FROM clientsdata WHERE firmcode = ?firmCode;");
				command.Parameters.AddWithValue("?firmCode", IncludeSDD.SelectedValue);
				command.Connection = _connection;
				command.Transaction = _connection.BeginTransaction(IsolationLevel.ReadCommitted);
				using (var reader = command.ExecuteReader())
					if (reader.Read())
						RegionDD.SelectedValue = reader[0].ToString();

				command.Transaction.Commit();
			}
			finally
			{
				_connection.Close();
			}
			SetWorkRegions(RegionDD.SelectedItem.Value, CheckBox1.Checked);
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			if (Convert.ToInt32(Session["AccessGrant"]) != 1)
				Response.Redirect("default.aspx");

			_connection.ConnectionString = Literals.GetConnectionString();
			try
			{
				_connection.Open();
				var adapter = new MySqlDataAdapter(@"
SELECT  regionaladmins.username, 
        regions.regioncode, 
        regions.region, 
        regionaladmins.alowcreateretail, 
        regionaladmins.alowcreatevendor, 
        regionaladmins.alowchangesegment, 
        regionaladmins.defaultsegment, 
        AlowCreateInvisible, 
        regionaladmins.email 
FROM    accessright.regionaladmins, 
        farm.regions 
WHERE   accessright.regionaladmins.regionmask & farm.regions.regioncode > 0 
        AND username                                                    = ?UserName 
ORDER BY region;
",
					_connection);
				adapter.SelectCommand.Transaction = _connection.BeginTransaction(IsolationLevel.ReadCommitted);
				adapter.SelectCommand.Parameters.AddWithValue("?UserName", Session["UserName"]);
				adapter.Fill(DS1, "admin");
				adapter.SelectCommand.Transaction.Commit();
			}
			finally
			{
				_connection.Close();
			}
			if (DS1.Tables["admin"].Rows.Count < 1)
			{
				Session["strError"] = "Пользователь " + Session["UserName"] + " не найден!";
				Response.Redirect("error.aspx");
			}
			if (!IsPostBack)
			{
				if (Convert.ToInt32(DS1.Tables["admin"].Rows[0]["AlowCreateInvisible"]) == 1)
					CustomerType.Visible = true;

				RegionDD.DataBind();
				for (int i = 0; i <= RegionDD.Items.Count - 1; i++)
				{
					if (RegionDD.Items[i].Text == DS1.Tables["admin"].Rows[0][2].ToString())
					{
						RegionDD.SelectedIndex = i;
						break;
					}
				}
				string iInt = DS1.Tables["admin"].Rows[0][1].ToString();
				SetWorkRegions(iInt, CheckBox1.Checked);
				if (DS1.Tables["admin"].Rows[0][3].ToString() == "1")
				{
					TypeDD.Items.Add("Аптека");
					TypeDD.Items[0].Value = "1";
				}
				if (DS1.Tables["admin"].Rows[0][4].ToString() == "1")
				{
					TypeDD.Items.Add("Поставщик");
					TypeDD.Items[TypeDD.Items.Count - 1].Value = "0";
				}
				if (TypeDD.Items.Count == 1)
				{
					TypeDD.Enabled = false;
				}
				if (DS1.Tables["admin"].Rows[0][5].ToString() == "1")
				{
					SegmentDD.Items.Add("Опт");
					SegmentDD.Items[0].Value = "0";
					SegmentDD.Items.Add("Розница");
					SegmentDD.Items[1].Value = "1";
					SegmentDD.SelectedIndex = Convert.ToInt32(DS1.Tables["admin"].Rows[0][6]);
				}
				else
				{
					if (DS1.Tables["admin"].Rows[0][6].ToString() == "0")
					{
						SegmentDD.Items.Add("Опт");
						SegmentDD.Items[0].Value = "0";
					}
					else
					{
						SegmentDD.Items.Add("Розница");
						SegmentDD.Items[0].Value = "1";
					}
					SegmentDD.Enabled = false;
				}
			}
			Session["strStatus"] = "Yes";
		}

		protected void CheckBox1_CheckedChanged(object sender, EventArgs e)
		{
			var checkBox = sender as CheckBox;
			SetWorkRegions(RegionDD.SelectedItem.Value, checkBox.Checked);
		}

		private void CreateFtpDirectory(string directory, string userName)
		{
			var supplierDirectory = Directory.CreateDirectory(directory);
			var supplierDirectorySecurity = supplierDirectory.GetAccessControl();
		    supplierDirectorySecurity.AddAccessRule(new FileSystemAccessRule(userName,
		                                                                     FileSystemRights.Read,
		                                                                     InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
		                                                                     PropagationFlags.None,
		                                                                     AccessControlType.Allow));
		    supplierDirectorySecurity.AddAccessRule(new FileSystemAccessRule(userName,
		                                                                     FileSystemRights.Write,
		                                                                     InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
		                                                                     PropagationFlags.None,
		                                                                     AccessControlType.Allow));
		    supplierDirectorySecurity.AddAccessRule(new FileSystemAccessRule(userName,
		                                                                     FileSystemRights.ListDirectory,
		                                                                     InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
		                                                                     PropagationFlags.None, 
                                                                             AccessControlType.Allow));
			supplierDirectory.SetAccessControl(supplierDirectorySecurity);

			var ordersDirectory = Directory.CreateDirectory(directory + "Orders\\");
			var ordersDirectorySecurity = supplierDirectory.GetAccessControl();
			ordersDirectorySecurity.AddAccessRule(new FileSystemAccessRule(userName,
			                                                               FileSystemRights.DeleteSubdirectoriesAndFiles,
			                                                               InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, 
                                                                           PropagationFlags.None,
			                                                               AccessControlType.Allow));
			ordersDirectory.SetAccessControl(ordersDirectorySecurity);

		    Directory.CreateDirectory(directory + "Docs\\");
		}

		protected void LoginValidator_ServerValidate(object source, ServerValidateEventArgs args)
		{
			if (!IncludeCB.Checked || (IncludeCB.Checked && TypeDD.SelectedValue != "0"))
			{
				args.IsValid = args.Value.Length > 0;

				if (args.IsValid)
				{
					bool existsInDataBase;
					bool existsInActiveDirectory;
					_connection.Open();
					existsInDataBase = Convert.ToUInt32(new MySqlCommand("select Max(osusername='" + args.Value + "') as Present from (osuseraccessright)", _connection).ExecuteScalar()) == 1;
					_connection.Close();
					existsInActiveDirectory = ADHelper.IsLoginExists(args.Value);
					args.IsValid = !(existsInActiveDirectory || existsInDataBase);
					if (existsInActiveDirectory || existsInDataBase)
					{
						args.IsValid = false;
						LoginValidator.ErrorMessage = String.Format("Учетное имя '{0}' существует в системе.", args.Value);
					}
					else
						PassTB.Text = Func.GeneratePassword();
				}
				else
				{
					LoginValidator.ErrorMessage = "Поле «Login» должно быть заполнено";
				}
			}
			else
				args.IsValid = true;

			if (args.IsValid)
				LoginValidator.Visible = false;
			else
				LoginValidator.Visible = true;
		}

		protected void TypeValidator_ServerValidate(object source, ServerValidateEventArgs args)
		{
			args.IsValid = args.Value == "Поставщик" && !IncludeCB.Checked;
		}

		private object CreateContactsForClientsData(MySqlConnection connection, ClientType clientType)
		{
			var contactGroupOwnerId = GetNewContactsGroupOwnerId(connection);

			CreateContactGroup(ContactGroupType.General,
			                   PhoneTB.Text,
			                   EmailTB.Text,
			                   null,
			                   connection,
			                   contactGroupOwnerId);
			CreateContactGroup(ContactGroupType.OrderManagers,
			                   TBOrderManagerPhone.Text,
			                   TBOrderManagerMail.Text,
			                   TBOrderManagerName.Text,
			                   connection,
			                   contactGroupOwnerId);

			if (clientType == ClientType.Supplier)
				CreateContactGroup(ContactGroupType.ClientManagers,
				                   TBClientManagerPhone.Text,
				                   TBClientManagerMail.Text,
				                   TBClientManagerName.Text,
				                   connection,
				                   contactGroupOwnerId);

			return contactGroupOwnerId;
		}


		private static object CreateContactsForBilling(MySqlConnection connection)
		{
			object contactGroupOwnerId = GetNewContactsGroupOwnerId(connection);
			CreateContactGroup(ContactGroupType.Billing,
			                   null,
			                   null,
			                   null,
			                   connection,
			                   contactGroupOwnerId);
			return contactGroupOwnerId;
		}

		private static void CreateContactGroup(ContactGroupType contactGroupType,
		                                       string phone,
		                                       string email,
		                                       string person,
		                                       MySqlConnection connection,
		                                       object contactGroupOwnerId)
		{
			MySqlCommand innerCommand = connection.CreateCommand();
			object contactGroupID = GetNewContactsOwnerId(connection);
			innerCommand.CommandText =
				@"
insert into contacts.contact_groups(Id, Name, Type, ContactGroupOwnerId) 
values(?ID, ?Name, ?Type, ?ContactGroupOwnerId);";

			innerCommand.Parameters.AddWithValue("?Type", contactGroupType);
			innerCommand.Parameters.AddWithValue("?Name", BindingHelper.GetDescription(contactGroupType));
			innerCommand.Parameters.AddWithValue("?ContactGroupOwnerId", contactGroupOwnerId);
			innerCommand.Parameters.AddWithValue("?ID", contactGroupID);
			innerCommand.ExecuteNonQuery();

			innerCommand = connection.CreateCommand();
			innerCommand.CommandText =
				@"
insert into contacts.contacts(Type, ContactText, ContactOwnerId) 
values(?Type, ?Contact, ?ContactOwnerId);";

			innerCommand.Parameters.AddWithValue("?ContactOwnerId", contactGroupID);
			innerCommand.Parameters.AddWithValue("?Contact", phone);
			innerCommand.Parameters.AddWithValue("?Type", 1);
			if (!String.IsNullOrEmpty(phone))
				innerCommand.ExecuteNonQuery();

			if (!String.IsNullOrEmpty(email))
			{
				innerCommand.Parameters["?Contact"].Value = email;
				innerCommand.Parameters["?Type"].Value = 0;
				innerCommand.ExecuteNonQuery();
			}

			if (!String.IsNullOrEmpty(person))
			{
				innerCommand = connection.CreateCommand();
				innerCommand.CommandText =
					@"
insert into contacts.persons(Id, Name, ContactGroupId) 
values(?Id, ?Name, ?ContactGroupId);";

				innerCommand.Parameters.AddWithValue("?ContactGroupId", contactGroupID);
				innerCommand.Parameters.AddWithValue("?Id", GetNewContactsOwnerId(connection));
				innerCommand.Parameters.AddWithValue("?Name", person);
				innerCommand.ExecuteNonQuery();
			}
		}

		private static object GetNewContactsOwnerId(MySqlConnection connection)
		{
			MySqlCommand command = connection.CreateCommand();
			command.CommandText = @"
insert into contacts.contact_owners values();
select Last_Insert_ID();";
			return command.ExecuteScalar();
		}

		private static object GetNewContactsGroupOwnerId(MySqlConnection connection)
		{
			MySqlCommand innerCommand = connection.CreateCommand();
			innerCommand.CommandText = @"
insert into contacts.contact_group_owners values();
select Last_Insert_ID();";

			return innerCommand.ExecuteScalar();
		}

		protected void ClientTypeChanged(object sender, EventArgs e)
		{
			var isCusomer = TypeDD.SelectedItem.Text == "Аптека";
			CustomerType.Enabled = isCusomer;
			ServiceClient.Enabled = isCusomer;
			IncludeCB.Enabled = isCusomer;
			if (!isCusomer)
				IncludeCB.Checked = false;

			WorkRegionLable.Visible = !isCusomer;
			WorkRegion.Visible = !isCusomer;
			OrderManagerGroupLabel.InnerText = isCusomer
			                              	? "Ответственный за работу с программой:"
			                              	: "Ответственный за отправку прайс-листа:";
			ClientManagerGropBlock.Visible = !isCusomer;
		}
	}
}