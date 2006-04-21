using System;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mail;
using System.Web.UI;
using System.Web.UI.WebControls;
using ActiveDs;
using MySql.Data.MySqlClient;

namespace AddUser
{
	partial class WebForm1 : Page
	{
		protected DataTable Regions;
		protected DataColumn DataColumn1;
		protected DataColumn DataColumn2;
		protected DataSet DS1;
		protected DataTable FreeCodes;
		protected DataColumn DataColumn3;
		protected DataColumn DataColumn4;
		protected DataTable WorkReg;
		protected DataColumn DataColumn5;
		protected DataColumn DataColumn6;
		protected DataColumn DataColumn7;
		protected DataColumn DataColumn8;
		protected DataTable Clientsdata;
		protected DataColumn DataColumn9;
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
		protected DataColumn DataColumn20;
		protected DataColumn DataColumn21;
		protected TextBox OldCodeTB;
		protected RequiredFieldValidator RequiredFieldValidator7;
		protected DataTable admin;
		protected DataTable DataTable1;
		protected DataColumn DataColumn22;
		protected DataColumn DataColumn23;
		protected DataTable Incudes;
		protected DataColumn DataColumn24;
		protected DataColumn DataColumn25;
		protected DataColumn DataColumn26;

		[DebuggerStepThrough()]
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
			DataColumn10 = new DataColumn();
			DataColumn11 = new DataColumn();
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
			DS1.Tables.AddRange(new DataTable[] {Regions, FreeCodes, WorkReg, Clientsdata, admin, DataTable1, Incudes});
			Regions.Columns.AddRange(new DataColumn[] {DataColumn1, DataColumn2});
			Regions.TableName = "Regions";
			DataColumn1.ColumnName = "Region";
			DataColumn2.ColumnName = "RegionCode";
			DataColumn2.DataType = typeof (Int64);
			FreeCodes.Columns.AddRange(new DataColumn[] {DataColumn3, DataColumn4});
			FreeCodes.TableName = "FreeCodes";
			DataColumn3.ColumnName = "FirmCode";
			DataColumn3.DataType = typeof (Int32);
			DataColumn4.ColumnName = "ShortName";
			WorkReg.Columns.AddRange(new DataColumn[] {DataColumn5, DataColumn6, DataColumn7, DataColumn8});
			WorkReg.TableName = "WorkReg";
			DataColumn5.ColumnName = "RegionCode";
			DataColumn5.DataType = typeof (Int32);
			DataColumn6.ColumnName = "Region";
			DataColumn7.ColumnName = "ShowMask";
			DataColumn7.DataType = typeof (Boolean);
			DataColumn8.ColumnName = "RegMask";
			DataColumn8.DataType = typeof (Boolean);
			Clientsdata.Columns.AddRange(
				new DataColumn[]
					{
						DataColumn9, DataColumn10, DataColumn11, DataColumn12, DataColumn13, DataColumn14, DataColumn15, DataColumn16,
						DataColumn17, DataColumn18, DataColumn19, DataColumn20, DataColumn21
					});
			Clientsdata.TableName = "Clientsdata";
			DataColumn9.ColumnName = "adress";
			DataColumn10.ColumnName = "bussinfo";
			DataColumn11.ColumnName = "bussstop";
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
			DataTable1.Columns.AddRange(new DataColumn[] {DataColumn22, DataColumn23});
			DataTable1.TableName = "Payers";
			DataColumn22.ColumnName = "PayerID";
			DataColumn22.DataType = typeof (Int32);
			DataColumn23.ColumnName = "PayerName";
			Incudes.Columns.AddRange(new DataColumn[] {DataColumn24, DataColumn25, DataColumn26});
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
			
		private MySqlConnection myMySqlConnection = new MySqlConnection(Literals.GetConnectionString());
		private MySqlDataReader myMySqlDataReader;
		private IADsUser ADUser;
		private IADs Domain;
		private MySqlCommand ms_Command = new MySqlCommand();
		private MySqlTransaction mytrans;

		//******** InsertOldData *************
		//заполнить форму из базы, базируясь на FirmCode

		//******** SelectTODS *************
		//выполнить запрос, данные запроса будут внесены в DataSource DS1, в таблицу с имененм Table

		//******** SetWorkRegions *************
		//при выборе региона клиента обновляет 'Регионы работы' и 'Показываемые регионы'
		//выделяя те регионы, которые установлены как регионы по умолчанию в таблице regions

		public void SetWorkRegions(string RegCode)
		{
			Func.SelectTODS(
				"select a.RegionCode, a.Region, (b.defaultshowregionmask & " + RegCode.ToString() + ")>0 as ShowMask," +
				"a.regioncode=" + RegCode.ToString() + " as RegMask from (farm.regions as a, farm.regions as b) where b.regioncode=" +
				RegCode.ToString() + " and a.regioncode & b.defaultshowregionmask>0 order by region", "WorkReg", DS1);
			WRList.DataBind();
			WRList2.DataBind();
			OrderList.DataBind();
			ShowList.DataBind();
			for (int i = 0; i <= WRList.Items.Count - 1; i++)
			{
				WRList.Items[i].Selected = Convert.ToBoolean(DS1.Tables["Workreg"].Rows[i]["regmask"]);
				OrderList.Items[i].Selected = Convert.ToBoolean(DS1.Tables["Workreg"].Rows[i]["regmask"]);
				WRList2.Items[i].Selected = Convert.ToBoolean(DS1.Tables["Workreg"].Rows[i]["regmask"]);
				ShowList.Items[i].Selected = Convert.ToBoolean(DS1.Tables["Workreg"].Rows[i]["showmask"]);
			}
		}

		protected void RegionDD_SelectedIndexChanged(object sender, EventArgs e)
		{
			SetWorkRegions(RegionDD.SelectedItem.Value);
		}

		private object CheckLogin()
		{
			float rc;
			float rc1;
			Label2.Text = "";
			myMySqlConnection.Open();
			myMySqlDataReader =
				new MySqlCommand("select Max(osusername='" + LoginTB.Text + "') as Present from (osuseraccessright)",
				                 myMySqlConnection).ExecuteReader();
			myMySqlDataReader.Read();
			if (myMySqlDataReader.Read())
			{
				rc = Convert.ToInt32(myMySqlDataReader[0]);
			}
			else
			{
				rc = 0;
			}
			myMySqlDataReader.Close();
			myMySqlConnection.Close();
			try
			{
				ADUser = Marshal.BindToMoniker("WinNT://adc.analit.net/" + LoginTB.Text) as IADsUser;
				rc1 = 1;
			}
			catch
			{
				rc1 = 0;
			}
			ADUser = null;
			if (rc > 0 | rc1 > 0)
			{
				Label2.Text = "Учетное имя '" + LoginTB.Text + "' существует в системе.";
				LoginTB.Text = "";
				return -1;
			}
			PassTB.Text = Func.GeneratePassword();
			return 0;
		}

		protected void Register_Click(object sender, EventArgs e)
		{
			if (Convert.ToInt32(CheckLogin()) == -1 & !(IncludeCB.Checked))
			{
				Label3.Text = "Ошибка в Учетном имени!";
				return;
			}
			myMySqlConnection.Open();
			mytrans = myMySqlConnection.BeginTransaction();
			Int64 MaskRegion = 0;
			Int64 ShowRegionMask = 0;
			Int64 WorkMask = 0;
			Int64 OrderMask = 0;
			for (int i = 0; i <= ShowList.Items.Count - 1; i++)
			{
				if (ShowList.Items[i].Selected)
				{
					ShowRegionMask += Convert.ToInt64(ShowList.Items[i].Value);
				}
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
			ms_Command.Transaction = mytrans;
			ms_Command.Parameters.Add(new MySqlParameter("MaskRegion", MySqlDbType.Int64));
			ms_Command.Parameters["MaskRegion"].Value = MaskRegion;
			ms_Command.Parameters.Add(new MySqlParameter("OrderMask", MySqlDbType.Int64));
			ms_Command.Parameters["OrderMask"].Value = OrderMask;
			ms_Command.Parameters.Add(new MySqlParameter("ShowRegionMask", MySqlDbType.Int64));
			ms_Command.Parameters["ShowRegionMask"].Value = ShowRegionMask;
			ms_Command.Parameters.Add(new MySqlParameter("WorkMask", MySqlDbType.Int64));
			ms_Command.Parameters["WorkMask"].Value = WorkMask;
			ms_Command.Parameters.Add(new MySqlParameter("fullname", MySqlDbType.String));
			ms_Command.Parameters["fullname"].Value = FullNameTB.Text;
			ms_Command.Parameters.Add(new MySqlParameter("shortname", MySqlDbType.String));
			ms_Command.Parameters["shortname"].Value = ShortNameTB.Text;
			ms_Command.Parameters.Add(new MySqlParameter("BeforeNamePrefix", MySqlDbType.String));
			ms_Command.Parameters["BeforeNamePrefix"].Value = "";
			if (TypeDD.SelectedItem.Value == "1")
			{
				ms_Command.Parameters["BeforeNamePrefix"].Value = "Аптека";
			}
			ms_Command.Parameters.Add(new MySqlParameter("phone", MySqlDbType.String));
			ms_Command.Parameters["phone"].Value = PhoneTB.Text;
			ms_Command.Parameters.Add(new MySqlParameter("fax", MySqlDbType.String));
			ms_Command.Parameters["fax"].Value = FaxTB.Text;
			ms_Command.Parameters.Add(new MySqlParameter("url", MySqlDbType.String));
			ms_Command.Parameters["url"].Value = URLTB.Text;
			ms_Command.Parameters.Add(new MySqlParameter("firmsegment", MySqlDbType.Int24));
			ms_Command.Parameters["firmsegment"].Value = SegmentDD.SelectedItem.Value;
			ms_Command.Parameters.Add(new MySqlParameter("RegionCode", MySqlDbType.Int24));
			ms_Command.Parameters["RegionCode"].Value = RegionDD.SelectedItem.Value;
			ms_Command.Parameters.Add(new MySqlParameter("adress", MySqlDbType.String));
			ms_Command.Parameters["adress"].Value = AddressTB.Text;
			ms_Command.Parameters.Add(new MySqlParameter("bussinfo", MySqlDbType.String));
			ms_Command.Parameters["bussinfo"].Value = BusInfoTB.Text;
			ms_Command.Parameters.Add(new MySqlParameter("bussstop", MySqlDbType.String));
			ms_Command.Parameters["bussstop"].Value = BussStopTB.Text;
			ms_Command.Parameters.Add(new MySqlParameter("firmtype", MySqlDbType.Int24));
			ms_Command.Parameters["firmtype"].Value = TypeDD.SelectedItem.Value;
			ms_Command.Parameters.Add(new MySqlParameter("registrant", MySqlDbType.String));
			ms_Command.Parameters["registrant"].Value = Session["UserName"];
			ms_Command.Parameters.Add(new MySqlParameter("mail", MySqlDbType.String));
			ms_Command.Parameters["mail"].Value = EmailTB.Text;
			ms_Command.Parameters.Add(new MySqlParameter("InvisibleOnFirm", MySqlDbType.Byte));
			ms_Command.Parameters["InvisibleOnFirm"].Value = 0;
			ms_Command.Parameters.Add(new MySqlParameter("OrderManagerName", MySqlDbType.String));
			ms_Command.Parameters["OrderManagerName"].Value = TBOrderManagerName.Text;
			ms_Command.Parameters.Add(new MySqlParameter("OrderManagerPhone", MySqlDbType.String));
			ms_Command.Parameters["OrderManagerPhone"].Value = TBOrderManagerPhone.Text;
			ms_Command.Parameters.Add(new MySqlParameter("OrderManagerMail", MySqlDbType.String));
			ms_Command.Parameters["OrderManagerMail"].Value = TBOrderManagerMail.Text;
			ms_Command.Parameters.Add(new MySqlParameter("ClientManagerName", MySqlDbType.String));
			ms_Command.Parameters["ClientManagerName"].Value = TBClientManagerName.Text;
			ms_Command.Parameters.Add(new MySqlParameter("ClientManagerPhone", MySqlDbType.String));
			ms_Command.Parameters["ClientManagerPhone"].Value = TBClientManagerPhone.Text;
			ms_Command.Parameters.Add(new MySqlParameter("ClientManagerMail", MySqlDbType.String));
			ms_Command.Parameters["ClientManagerMail"].Value = TBClientManagerMail.Text;
			ms_Command.Parameters.Add(new MySqlParameter("AccountantName", MySqlDbType.String));
			ms_Command.Parameters["AccountantName"].Value = TBAccountantName.Text;
			ms_Command.Parameters.Add(new MySqlParameter("AccountantPhone", MySqlDbType.String));
			ms_Command.Parameters["AccountantPhone"].Value = TBAccountantPhone.Text;
			ms_Command.Parameters.Add(new MySqlParameter("AccountantMail", MySqlDbType.String));
			ms_Command.Parameters["AccountantMail"].Value = TBAccountantMail.Text;
			ms_Command.Parameters.Add(new MySqlParameter("ClientCode", MySqlDbType.Int24));
			ms_Command.Parameters.Add(new MySqlParameter("AllowGetData", MySqlDbType.Int24));
			ms_Command.Parameters["AllowGetData"].Value = TypeDD.SelectedItem.Value;
			ms_Command.Parameters.Add(new MySqlParameter("OSUserName", MySqlDbType.String));
			ms_Command.Parameters["OSUserName"].Value = LoginTB.Text;
			ms_Command.Parameters.Add(new MySqlParameter("OSUserPass", MySqlDbType.String));
			ms_Command.Parameters["OSUserPass"].Value = PassTB.Text;
			ms_Command.Connection = myMySqlConnection;
			if (InvCB.Checked)
			{
				ms_Command.Parameters["invisibleonfirm"].Value = 1;
			}
			try
			{
				Label3.Text = "";
				if (IncludeCB.Checked)
				{
					myMySqlDataReader =
						new MySqlCommand("select billingcode from clientsdata where firmcode=" + IncludeSDD.SelectedValue,
						                 myMySqlConnection).ExecuteReader();
					if (myMySqlDataReader.Read())
					{
						Session["DogN"] = Convert.ToInt32(myMySqlDataReader[0].ToString());
					}
					if (!(myMySqlDataReader.IsClosed))
					{
						myMySqlDataReader.Close();
					}
				}
				else
				{
					if (!(PayerPresentCB.Checked))
					{
						Session["DogN"] = CreateClientOnBilling();
					}
					else
					{
						Session["DogN"] = PayerDDL.SelectedItem.Value;
					}
				}
				ms_Command.Parameters["ClientCode"].Value = CreateClientOnClientsData();
				Session["Code"] = ms_Command.Parameters["ClientCode"].Value;
				if (IncludeCB.Checked)
				{
					CreateClientOnShowInclude(Convert.ToInt32(IncludeSDD.SelectedValue));
				}
				else
				{
					CreateClientOnOSUserAccessRight();
				}
				LogRegister();
				if (TypeDD.SelectedItem.Value == "1")
				{
					CreateClientOnRCS_and_I(InvCB.Checked);
				}
				else
				{
					CreatePriceRecords();
				}
				if (!(IncludeCB.Checked))
				{
					Domain = Marshal.BindToMoniker("LDAP://OU=Пользователи,OU=Клиенты,DC=adc,DC=analit,DC=net") as IADs;
					ADUser = (Domain as IADsContainer).Create("user", "cn=" + ms_Command.Parameters["OSUserName"].Value) as IADsUser;
					ADUser.Put("samAccountName", ms_Command.Parameters["OSUserName"].Value);
					ADUser.SetInfo();
					ADUser = null;
					ADUser = Marshal.BindToMoniker("WinNT://adc.analit.net/" + ms_Command.Parameters["OSUserName"].Value) as IADsUser;
					ADUser.SetPassword(ms_Command.Parameters["OSUserPass"].Value.ToString());
					ADUser.SetInfo();
					Int32 fl = 66049;
					ADUser.Put("userFlags", fl);
					ADUser.Description = ms_Command.Parameters["ClientCode"].Value.ToString();
					ADUser.AccountDisabled = false;
					ADUser.LoginWorkstations = "ISRV";
					IADsGroup grp;
					grp = Marshal.BindToMoniker("WinNT://adc.analit.net/Базовая группа клиентов - получателей данных") as IADsGroup;
					grp.Add("WinNT://adc.analit.net/" + ms_Command.Parameters["OSUserName"].Value);
					ADUser.SetInfo();
					ADUser = null;
				}
				mytrans.Commit();
				Session["strStatus"] = "Yes";
				try
				{
					if (!((InvCB.Checked) || (TypeDD.SelectedItem.Value == "0")))
					{
						if (
							Func.SelectTODS(
								"SELECT ClientManagerName, ClientManagerMail" + " FROM clientsdata" + " where MaskRegion & " +
								RegionDD.SelectedItem.Value + ">0" + " and firmsegment=0" + " and LENGTH(ClientManagerMail)>0" +
								" and firmtype=0" + " and firmstatus=1" + " and firmcode in (select pd.FirmCode from" +
								" pricesdata as pd, pricesregionaldata as prd where regioncode=" + RegionDD.SelectedItem.Value +
								" and pd.enabled=1 and prd.enabled=1)" + " group by ClientManagerMail", "FirmEmail", DS1))
						{
							foreach (DataRow Row in DS1.Tables["FirmEmail"].Rows)
							{
								Func.Mail("Аналитическая Компания Инфорум <pharm@analit.net>",
								          "Новый клиент в системе \"АналитФАРМАЦИЯ\"",
								          MailFormat.Text,
								          "Добрый день. \n\nВ информационной системе \"АналитФАРМАЦИЯ\", участником которой является Ваша организация, зарегистрирован новый клиент: "
								          + ShortNameTB.Text + " в регионе(городе) "
								          + RegionDD.SelectedItem.Text + "."
								          +
								          "\nПожалуйста произведите настройки для данного клиента (Раздел \"Для зарегистрированных пользователей\" на сайте www.analit.net)."
								          + "С уважением," + "\nАналитическая компания \"Инфорум\", г. Воронеж"
								          + "\n4732-206000", "\"" + Row[0] + "\"<" + Row[1] + ">", null, Encoding.GetEncoding(1251));
							}
							Func.Mail("register@analit.net",
							          "\"Debug: " + FullNameTB.Text + "\" - Уведомления поставщиков",
							          MailFormat.Text, "Оператор: " + Session["UserName"]
							                           + "\nРегион: " + RegionDD.SelectedItem.Text + "\nLogin: "
							                           + LoginTB.Text + "\nКод: " + Session["Code"]
							                           + "\n\nСегмент: " + SegmentDD.SelectedItem.Text + "\nТип: "
							                           + TypeDD.SelectedItem.Text + "О Регистрации уведомлено поставщиков: "
							                           + Convert.ToString(DS1.Tables["FirmEmail"].Rows.Count - 1),
							          "RegisterList@subscribe.analit.net",
							          DS1.Tables["admin"].Rows[0]["email"].ToString(),
							          Encoding.UTF8);
						}
					}
					else
					{
						Func.Mail("register@analit.net",
						          "\"" + FullNameTB.Text + "\" - ошибка уведомления поставщиков",
						          MailFormat.Text, "Оператор: " + Session["UserName"] + "\nРегион: "
						                           + RegionDD.SelectedItem.Text + "\nLogin: " + LoginTB.Text
						                           + "\nКод: " + Session["Code"] + "\n\nСегмент: "
						                           + SegmentDD.SelectedItem.Text + "\nТип: " + TypeDD.SelectedItem.Text
						                           + "Ошибка: Ничего не получилось выбрать из базы",
						          "RegisterList@subscribe.analit.net",
						          DS1.Tables["admin"].Rows[0]["email"].ToString(), Encoding.UTF8);
					}
				}
				catch (Exception err)
				{
					Func.Mail("register@analit.net",
					          "\"" + FullNameTB.Text + "\" - ошибка уведомления поставщиков",
					          MailFormat.Text, "Оператор: " + Session["UserName"] + "\nРегион: "
					                           + RegionDD.SelectedItem.Text + "\nLogin: " + LoginTB.Text + "\nКод: "
					                           + Session["Code"] + "\n\nСегмент: " + SegmentDD.SelectedItem.Text
					                           + "\nТип: " + TypeDD.SelectedItem.Text + "Ошибка: " + err.Source + ": "
					                           + err.Message, "RegisterList@subscribe.analit.net",
					          DS1.Tables["admin"].Rows[0]["email"].ToString(), Encoding.UTF8);
				}
				try
				{
					Func.Mail("register@analit.net", "\"" + FullNameTB.Text + "\" - успешная регистрация",
					          MailFormat.Text, "Оператор: " + Session["UserName"] + "\nРегион: "
					                           + RegionDD.SelectedItem.Text + "\nLogin: " + LoginTB.Text
					                           + "\nКод: " + Session["Code"] + "\n\nСегмент: " + SegmentDD.SelectedItem.Text
					                           + "\nТип: " + TypeDD.SelectedItem.Text, "RegisterList@subscribe.analit.net",
					          DS1.Tables["admin"].Rows[0]["email"].ToString(), Encoding.UTF8);
					Func.Mail("\"" + FullNameTB.Text + "\" <" + EmailTB.Text + ">", "Sub", MailFormat.Text, "",
					          "FirmEmailList-on@subscribe.analit.net", null, Encoding.UTF8);
					if (!(TBClientManagerMail.Text == ""))
					{
						Func.Mail("\"" + TBClientManagerName.Text + "\" <" + TBClientManagerMail.Text + ">", "Sub", MailFormat.Text, "",
						          "ClientManagerList-on@subscribe.analit.net", null, Encoding.UTF8);
					}
					if (!(TBOrderManagerMail.Text == ""))
					{
						Func.Mail("\"" + TBOrderManagerName.Text + "\" <" + TBOrderManagerMail.Text + ">", "Sub", MailFormat.Text, "",
						          "OrderManagerList-on@subscribe.analit.net", null, Encoding.UTF8);
					}
					if (!(TBAccountantMail.Text == ""))
					{
						Func.Mail("\"" + TBAccountantName.Text + "\" <" + TBAccountantMail.Text + ">", "Sub", MailFormat.Text, "",
						          "AccountantList-on@subscribe.analit.net", null, Encoding.UTF8);
					}
				}
				catch (Exception err)
				{
					Func.Mail("register@analit.net", "\"" + FullNameTB.Text
					                                 + "\" - ошибка подписки поставщиков", MailFormat.Text,
					          "Оператор: " + Session["UserName"] + "\nРегион: "
					          + RegionDD.SelectedItem.Text + "\nLogin: " + LoginTB.Text + "\nКод: "
					          + Session["Code"] + "\n\nСегмент: " + SegmentDD.SelectedItem.Text
					          + "\nТип: " + TypeDD.SelectedItem.Text + "Ошибка: " + err.Source + ": "
					          + err.Message, "RegisterList@subscribe.analit.net",
					          DS1.Tables["admin"].Rows[0]["email"].ToString(), Encoding.UTF8);
				}
				Session["Name"] = FullNameTB.Text;
				Session["ShortName"] = ShortNameTB.Text;
				Session["Login"] = LoginTB.Text;
				Session["Password"] = PassTB.Text;
				Session["Tariff"] = TypeDD.SelectedItem.Text;
				Session["Register"] = true;
				if (IncludeCB.Checked)
				{
					Page.Controls.Clear();
					Label LB = new Label();
					LB.Text = "Регистрация завершена успешно.";
					LB.Font.Name = "Verdana";
					Page.Controls.Add(LB);
				}
				else
				{
					Response.Redirect("report.aspx");
				}
			}
			catch (Exception excL)
			{
				if (!((excL) is ThreadAbortException))
				{
					if (!(myMySqlDataReader.IsClosed))
					{
						myMySqlDataReader.Close();
					}
					Label3.Text = "Ошибка при регистрации клиента: " + excL.Message;
					mytrans.Rollback();
				}
			}
			finally
			{
				if (!(myMySqlDataReader.IsClosed))
				{
					myMySqlDataReader.Close();
				}
				if (myMySqlConnection.State == ConnectionState.Open)
				{
					myMySqlConnection.Close();
				}
				ms_Command.Dispose();
				myMySqlConnection.Dispose();
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
			Func.SelectTODS(" SELECT distinct PayerID, concat(PayerID, '. ', p.ShortName) PayerName"
			                + " FROM clientsdata as cd, accessright.showright, billing.payers p "
			                + " where p.payerid=cd.billingcode and cd.regioncode & showright.regionmask > 0 "
			                + " and showright.UserName='" + Session["UserName"]
			                + "' and FirmType=if(ShowRet+ShowOpt=2, FirmType, if(ShowRet=1, 1, 0)) "
			                + " and if(UseRegistrant=1, Registrant='" + Session["UserName"] + "', 1=1) "
			                + " and firmstatus=1 and billingstatus=1 " + " and p.ShortName like '%"
			                + PayerFTB.Text + "%' " + " order by p.shortname", "Payers", DS1);
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
			PayerPresentCB.Text = "Плательщик существует";
			PayerPresentCB.Visible = true;
			PayerPresentCB.Checked = false;
			if (IncludeCB.Checked)
			{
				IncludeCB.Text = "Подчинен клиенту:";
				PayerPresentCB.Visible = false;
				PayerFTB.Visible = false;
				FindPayerB.Visible = false;
				LoginTB.Enabled = false;
				Requiredfieldvalidator4.Enabled = false;
				RegionDD.Enabled = false;
				TypeDD.Enabled = false;
				SegmentDD.Enabled = false;
				InvCB.Enabled = false;
				ShowList.Enabled = false;
				WRList.Enabled = false;
				WRList2.Enabled = false;
				PayerDDL.Visible = false;
				PayerCountLB.Visible = false;
				IncludeSTB.Visible = true;
				IncludeSB.Visible = true;
			}
			else
			{
				LoginTB.Enabled = true;
				Requiredfieldvalidator4.Enabled = true;
				RegionDD.Enabled = true;
				TypeDD.Enabled = true;
				SegmentDD.Enabled = true;
				InvCB.Enabled = true;
				ShowList.Enabled = true;
				WRList.Enabled = true;
				WRList2.Enabled = true;
				IncludeCB.Text = "Подчиненный клиент";
				IncludeSTB.Visible = false;
				IncludeSB.Visible = false;
				IncludeSDD.Visible = false;
				IncludeCountLB.Visible = false;
			}
		}

		protected void IncludeSB_Click(object sender, EventArgs e)
		{
			Func.SelectTODS(" SELECT distinct cd.FirmCode, concat(cd.FirmCode, '. ', cd.ShortName) ShortName, cd.RegionCode" +
			                " FROM (accessright.showright, clientsdata as cd)"
			                + " left join includeregulation ir on ir.includeclientcode=cd.firmcode"
			                + " where cd.regioncode & showright.regionmask > 0"
			                + " and showright.UserName='" + Session["UserName"] + "'"
			                + " and FirmType=if(ShowRet+ShowOpt=2, FirmType, if(ShowRet=1, 1, 0)) "
			                + " and if(UseRegistrant=1, Registrant='" + Session["UserName"] + "', 1=1)"
			                + " and cd.ShortName like '%" + IncludeSTB.Text + "%' "
			                + " and FirmStatus=1" + " and billingstatus=1" + " And FirmType=1"
			                + " and ir.primaryclientcode is null" + " order by cd.shortname", "Includes", DS1);
			IncludeSDD.DataBind();
			IncludeCountLB.Text = "[" + IncludeSDD.Items.Count + "]";
			IncludeCountLB.Visible = true;
			RegionDD.SelectedValue = DS1.Tables["Includes"].Rows[0]["RegionCode"].ToString();
			SetWorkRegions(RegionDD.SelectedItem.Value);
			if (IncludeSDD.Items.Count > 0)
			{
				IncludeSDD.Visible = true;
				IncludeSTB.Visible = false;
				IncludeSB.Visible = false;
			}
		}

		private int CreateClientOnBilling()
		{
			ms_Command.CommandText =
				"insert into billing.payers(OldTariff, OldPayDate, Comment, PayerID, ShortName, BeforeNamePrefix) values(0, now(), 'Дата регистрации: " +
				DateTime.Now + "', null, ?ShortName, ?BeforeNamePrefix); ";
			ms_Command.CommandText += "SELECT LAST_INSERT_ID()";
			return Convert.ToInt32(ms_Command.ExecuteScalar());
		}

		private int CreateClientOnClientsData()
		{
			ms_Command.CommandText =
				"INSERT INTO usersettings.clientsdata (regionmask, MaskRegion, ShowRegionMask, FullName, ShortName, Phone, Fax, URL, FirmSegment, RegionCode, Adress, BussInfo, BussStop, FirmType, Mail, OrderManagerName, OrderManagerPhone, OrderManagerMail, ClientManagerName, ClientManagerPhone, ClientManagerMail, AccountantName, AccountantPhone, AccountantMail, FirmStatus, registrant, BillingCode, BillingStatus) ";
			if (!(IncludeCB.Checked))
			{
				ms_Command.CommandText +=
					" Values(0, ?maskregion, ?ShowRegionMask, ?FullName, ?ShortName, ?Phone, ?Fax, ?URL, ?FirmSegment, ?RegionCode, ?Adress, ?BussInfo, ?BussStop, ?FirmType, ?Mail, ?OrderManagerName, ?OrderManagerPhone, ?OrderManagerMail, ?ClientManagerName, ?ClientManagerPhone, ?ClientManagerMail, ?AccountantName, ?AccountantPhone, ?AccountantMail, 1, ?registrant, " +
					Session["DogN"] + ", 1); ";
			}
			else
			{
				ms_Command.CommandText +=
					" select 0, maskregion, ShowRegionMask, ?FullName, ?ShortName, ?Phone, ?Fax, ?URL, FirmSegment, RegionCode, ?Adress, ?BussInfo, ?BussStop, FirmType, ?Mail, ?OrderManagerName, ?OrderManagerPhone, ?OrderManagerMail, ?ClientManagerName, ?ClientManagerPhone, ?ClientManagerMail, ?AccountantName, ?AccountantPhone, ?AccountantMail, 1, ?registrant, BillingCode, BillingStatus" +
					" from usersettings.clientsdata where firmcode=" + IncludeSDD.SelectedValue + "; ";
			}
			ms_Command.CommandText += "SELECT LAST_INSERT_ID()";
			return Convert.ToInt32(ms_Command.ExecuteScalar());
		}

		private void CreateClientOnOSUserAccessRight()
		{
			ms_Command.CommandText =
				"INSERT INTO usersettings.osuseraccessright (ClientCode, AllowGetData, OSUserName) Values(?ClientCode, ?AllowGetData, ?OSUserName)";
			ms_Command.ExecuteNonQuery();
		}

		private void LogRegister()
		{
			ms_Command.CommandText = " insert into logs.register select null, now(), ?registrant, '" +
			                         HttpContext.Current.Request.UserHostAddress +
			                         "', ?ClientCode, ?FullName, ?FirmSegment, ?FirmType";
			ms_Command.ExecuteNonQuery();
		}

		private void CreateClientOnRCS_and_I(bool Invisible)
		{
			ms_Command.CommandText =
				"INSERT INTO usersettings.retclientsset (ClientCode, InvisibleOnFirm, WorkRegionMask, OrderRegionMask) Values(?ClientCode, ?InvisibleOnFirm, ?WorkMask, ?OrderMask); ";
			ms_Command.CommandText += " INSERT " + " INTO intersection" + " (" + " ClientCode, " + " regioncode, " +
			                          " pricecode, " + " InvisibleonFirm, " + " costcode" + " ) " + " SELECT DISTINCT " +
			                          " clientsdata2.firmcode, " + " regions.regioncode, " + " pc.showpricecode, " +
			                          " a.invisibleonfirm, " + " (" + " SELECT " + " costcode " + " FROM pricescosts pcc " +
			                          " WHERE basecost " + " AND showpricecode=pc.showpricecode" + " ) " + " FROM (clientsdata, " +
			                          " farm.regions, " + " pricescosts pc, " + " pricesdata) " + " LEFT JOIN " +
			                          " clientsdata AS clientsdata2 " + " ON clientsdata2.firmcode=?ClientCode " + " LEFT JOIN " +
			                          " intersection " + " ON intersection.pricecode=pc.showpricecode " +
			                          " AND intersection.regioncode=regions.regioncode " +
			                          " AND intersection.clientcode=clientsdata2.firmcode " + " LEFT JOIN " +
			                          " retclientsset AS a " + " ON a.clientcode=clientsdata2.firmcode " +
			                          " WHERE intersection.pricecode IS NULL " + " AND clientsdata.firmstatus=1 " +
			                          " AND clientsdata.firmsegment=clientsdata2.firmsegment " + " AND clientsdata.firmtype=0 " +
			                          " AND pricesdata.firmcode=clientsdata.firmcode " +
			                          " AND pricesdata.pricecode=pc.showpricecode " + " AND " + " ( " +
			                          " clientsdata.maskregion & regions.regioncode " + " ) " + " >0 " + " AND " + " ( " +
			                          " clientsdata2.maskregion & regions.regioncode " + " ) " + " >0;";
			if (!(Invisible))
			{
				ms_Command.CommandText += " insert into inscribe(ClientCode) values(?ClientCode); ";
			}
			ms_Command.ExecuteNonQuery();
		}

		private void CreatePriceRecords()
		{
			ms_Command.CommandText = "INSERT INTO pricesdata(Firmcode, PriceCode) values(?ClientCode, null); " +
			                         " set @NewPriceCode:=Last_Insert_ID(); insert into farm.formrules(firmcode) values(@NewPriceCode); " +
			                         " insert into farm.sources(FirmCode) values(@NewPriceCode);";
			ms_Command.CommandText += "Insert into PricesCosts(CostCode, PriceCode, BaseCost, ShowPriceCode) " +
			                          " Select @NewPriceCode, @NewPriceCode, 1, @NewPriceCode;" +
			                          " Insert into farm.costformrules(PC_CostCode) Select @NewPriceCode;";
			ms_Command.CommandText += " insert into regionaldata(regioncode, firmcode)" +
			                          " SELECT distinct regions.regioncode, clientsdata.firmcode" +
			                          " FROM (clientsdata, farm.regions, pricesdata)" +
			                          " left join regionaldata on regionaldata.firmcode=clientsdata.firmcode and regionaldata.regioncode= regions.regioncode" +
			                          " where pricesdata.firmcode=clientsdata.firmcode" + " and clientsdata.firmcode=?ClientCode" +
			                          " and (clientsdata.maskregion & regions.regioncode)>0" +
			                          " and regionaldata.firmcode is null;";
			ms_Command.CommandText += " insert into pricesregionaldata(regioncode, pricecode)" +
			                          " SELECT distinct regions.regioncode, pricesdata.pricecode" +
			                          " FROM (clientsdata, farm.regions, pricesdata, clientsdata as a)" +
			                          " left join pricesregionaldata on pricesregionaldata.pricecode=pricesdata.pricecode and pricesregionaldata.regioncode= regions.regioncode" +
			                          " where pricesdata.firmcode=clientsdata.firmcode" + " and clientsdata.firmcode=?ClientCode" +
			                          " and (clientsdata.maskregion & regions.regioncode)>0" +
			                          " and pricesregionaldata.pricecode is null;";
			ms_Command.CommandText +=
				" insert into intersection(clientcode, regioncode, pricecode, invisibleonclient, InvisibleonFirm, CostCode) " +
				" SELECT distinct clientsdata2.firmcode, regions.regioncode, pricesdata.pricecode," +
				" 0 as invisibleonclient, a.invisibleonfirm, pricesdata.pricecode" +
				" FROM (clientsdata, farm.regions, pricesdata, pricesregionaldata, clientsdata as clientsdata2, retclientsset as a)" +
				" LEFT JOIN intersection ON intersection.pricecode=pricesdata.pricecode and intersection.regioncode=regions.regioncode and intersection.clientcode=clientsdata2.firmcode" +
				" WHERE intersection.pricecode IS NULL and" +
				" clientsdata.firmstatus=1 and clientsdata.firmsegment=clientsdata2.firmsegment" +
				" and clientsdata.firmcode=?ClientCode" + " and clientsdata2.firmtype=1" + " and a.clientcode=clientsdata2.firmcode" +
				" and pricesdata.firmcode=clientsdata.firmcode" + " and pricesregionaldata.pricecode=pricesdata.pricecode" +
				" and pricesregionaldata.regioncode=regions.regioncode" + " and (clientsdata.maskregion & regions.regioncode)>0" +
				" and (clientsdata2.maskregion & regions.regioncode)>0;";
			ms_Command.ExecuteNonQuery();
		}

		private void CreateClientOnShowInclude(int PrimaryClientCode)
		{
			ms_Command.CommandText = "INSERT INTO showregulation" + "(PrimaryClientCode, ShowClientCode, Addition)" + " VALUES (" +
			                         PrimaryClientCode + ", ?ClientCode, ?ShortName);" + "INSERT INTO includeregulation" +
			                         "(ID, PrimaryClientCode, IncludeClientCode, Addition)" + "VALUES(NULL," + PrimaryClientCode +
			                         ", ?ClientCode, ?ShortName)";
			ms_Command.ExecuteNonQuery();
		}

		protected void IncludeSDD_SelectedIndexChanged(object sender, EventArgs e)
		{
			myMySqlConnection.Open();
			myMySqlDataReader =
				new MySqlCommand("select RegionCode from clientsdata where firmcode=" + IncludeSDD.SelectedValue, myMySqlConnection)
					.ExecuteReader();
			if (myMySqlDataReader.Read())
			{
				RegionDD.SelectedValue = myMySqlDataReader[0].ToString();
			}
			if (!(myMySqlDataReader.IsClosed))
			{
				myMySqlDataReader.Close();
			}
			myMySqlConnection.Close();
			SetWorkRegions(RegionDD.SelectedItem.Value);
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			InitializeComponent();

			if (Convert.ToInt32(Session["AccessGrant"]) != 1)
			{
				Response.Redirect("default.aspx");
			}
			Func.SelectTODS(
				"select regionaladmins.username, regions.regioncode, regions.region, regionaladmins.alowcreateretail, regionaladmins.alowcreatevendor, regionaladmins.alowchangesegment, regionaladmins.defaultsegment, AlowCreateInvisible, regionaladmins.email from accessright.regionaladmins, farm.regions where accessright.regionaladmins.regionmask & farm.regions.regioncode >0 and username='" +
				Session["UserName"] + "' order by region", "admin", DS1);
			if (DS1.Tables["admin"].Rows.Count < 1)
			{
				Session["strError"] = "Пользователь " + Session["UserName"] + " не найден!";
				Response.Redirect("error.aspx");
			}
			if (!(IsPostBack))
			{
				if (Convert.ToInt32(DS1.Tables["admin"].Rows[0]["AlowCreateInvisible"]) == 1)
				{
					InvCB.Visible = true;
				}
				RegionDD.DataBind();
				for (int i = 0; i <= RegionDD.Items.Count - 1; i++)
				{
					if (RegionDD.Items[i].Text == DS1.Tables["admin"].Rows[0][2])
					{
						RegionDD.SelectedIndex = i;
						goto exitForStatement0;
					}
				}
				exitForStatement0:
				;
				string iInt = DS1.Tables["admin"].Rows[0][1].ToString();
				SetWorkRegions(iInt);
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
	}
}