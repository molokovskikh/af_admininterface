using System;
using System.Configuration;
using System.Data;
using System.Runtime.InteropServices;
using System.Text;
using System.Web.Mail;
using MySql.Data.MySqlClient;

namespace AddUser
{
	public class Func
	{
		public static void Mail(string From, string Subject, MailFormat BodyFormat, string Body, string MessageTo,
		                        string MessageBCC, Encoding Encoding)
		{
			try
			{
				MailMessage message = new MailMessage();
				message.From = From;
				message.Subject = Subject;
				message.BodyFormat = BodyFormat;
				message.Body = Body;
				message.Bcc = MessageBCC;
				message.To = MessageTo;
				message.BodyEncoding = Encoding;
				SmtpMail.SmtpServer = "box.analit.net";
				SmtpMail.Send(message);
			}
			catch
			{
			}
		}

		public static string GeneratePassword()
		{
			Random random = new Random();
			Int16 r;
			string PassStr = String.Empty;
			try
			{
				for (int i = 0; i <= 7; i++)
				{
					r = Convert.ToInt16((62*Convert.ToSingle(random.NextDouble())) + 1);
					if ((r >= 0) && (r < 26))
					{
						if ((65 + r) == 73)
							r = Convert.ToInt16(r + 1);
						PassStr = PassStr + (char) (65 + r);
					}
					if ((r >= 26) && (r < 52))
					{
						if ((71 + Convert.ToInt32(r)) == 108)
							r = Convert.ToInt16(r + 1);
						PassStr = PassStr + (char) (71 + r);
					}
					if (r >= 52)
					{
						PassStr = PassStr + (char) (48 + Convert.ToInt32(Convert.ToSingle(random.NextDouble())*9));
					}
				}
				if (PassStr.Length < 8)
				{
					r = Convert.ToInt16(97 + (DateTime.Now.Second/3));
					if (Convert.ToInt32(r) == 108)
					{
						r = Convert.ToInt16(r + 1);
					}
					PassStr = PassStr + (char) r;
				}
				return PassStr;
			}
			catch
			{
				return "[Ошибка формирования пароля]";
			}
		}

		public static bool SelectTODS(string SQLQuery, string Table, DataSet DS, MySqlCommand MySQLCommand, string CommandAdd)
		{
			MySqlCommand myMySqlCommand = new MySqlCommand();
			MySqlConnection myMySqlConnection = new MySqlConnection(ConfigurationManager.AppSettings["ConnectionString"]);
			MySqlCommand Комманда = new MySqlCommand();
			MySqlDataAdapter myMySqlDataAdapter = new MySqlDataAdapter();
			try
			{
				myMySqlConnection.Open();
			}
			catch
			{
				return false;
			}
			try
			{
				if (!(MySQLCommand == null))
				{
					myMySqlCommand = MySQLCommand;
				}
				myMySqlCommand.CommandText = SQLQuery + CommandAdd;
				myMySqlCommand.Connection = myMySqlConnection;
				myMySqlDataAdapter.SelectCommand = myMySqlCommand;
				myMySqlDataAdapter.Fill(DS, Table);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
			finally
			{
				Комманда.Dispose();
				myMySqlDataAdapter.Dispose();
				myMySqlConnection.Close();
				myMySqlConnection.Dispose();
			}
		}


		public static bool SelectTODS(string SQLQuery, string Table, DataSet DS, MySqlCommand MySQLCommand)
		{
			string CommandAdd = String.Empty;
			MySqlCommand myMySqlCommand = new MySqlCommand();
			MySqlConnection myMySqlConnection = new MySqlConnection(ConfigurationManager.AppSettings["ConnectionString"]);
			MySqlCommand Комманда = new MySqlCommand();
			MySqlDataAdapter myMySqlDataAdapter = new MySqlDataAdapter();
			try
			{
				myMySqlConnection.Open();
			}
			catch
			{
				return false;
			}
			try
			{
				if (!(MySQLCommand == null))
				{
					myMySqlCommand = MySQLCommand;
				}
				myMySqlCommand.CommandText = SQLQuery + CommandAdd;
				myMySqlCommand.Connection = myMySqlConnection;
				myMySqlDataAdapter.SelectCommand = myMySqlCommand;
				myMySqlDataAdapter.Fill(DS, Table);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
			finally
			{
				Комманда.Dispose();
				myMySqlDataAdapter.Dispose();
				myMySqlConnection.Close();
				myMySqlConnection.Dispose();
			}
		}

		public static bool SelectTODS(string SQLQuery, string Table, DataSet DS)
		{
			MySqlCommand MySQLCommand = null;
			string CommandAdd = String.Empty;
			MySqlCommand myMySqlCommand = new MySqlCommand();
			MySqlConnection myMySqlConnection = new MySqlConnection(ConfigurationManager.AppSettings["ConnectionString"]);
			MySqlCommand Комманда = new MySqlCommand();
			MySqlDataAdapter myMySqlDataAdapter = new MySqlDataAdapter();
			try
			{
				myMySqlConnection.Open();
			}
			catch
			{
				return false;
			}
			try
			{
				if (!(MySQLCommand == null))
				{
					myMySqlCommand = MySQLCommand;
				}
				myMySqlCommand.CommandText = SQLQuery + CommandAdd;
				myMySqlCommand.Connection = myMySqlConnection;
				myMySqlDataAdapter.SelectCommand = myMySqlCommand;
				myMySqlDataAdapter.Fill(DS, Table);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
			finally
			{
				Комманда.Dispose();
				myMySqlDataAdapter.Dispose();
				myMySqlConnection.Close();
				myMySqlConnection.Dispose();
			}
		}

		public static decimal GetDecimal(string InputStr)
		{
			if (InputStr.Length < 1)
				return 0;
			else
			{
				try
				{
					return Convert.ToDecimal(InputStr);
				}
				catch
				{
					new Exception(
						String.Format(
							"Не верно указанна скидка {0}. Для указания десятичных долей используйте знаки \".\" или \",\", к примеру 5.54",
							InputStr));
				}
			}
			return 0;
		}


		public static string GetFirmType(int FrimTypeCode)
		{
			string FirmTypeStr;
			if (FrimTypeCode == 0)
				FirmTypeStr = "Поставщик";
			else
				FirmTypeStr = "Аптека";
			return FirmTypeStr;
		}

		public bool CalcDays(DateTime CurContDate)
		{
			if (DateTime.Now.Subtract(CurContDate).TotalDays > 3)
			{
				return false;
			}
			return true;
		}
	}
}