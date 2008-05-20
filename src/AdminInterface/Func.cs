using System;
using System.Data;
using System.Net.Mail;
using System.Text;
using log4net;
using LumiSoft.Net.SMTP.Client;
using MySql.Data.MySqlClient;

namespace AddUser
{
	public class Func
	{
		private static readonly ILog _log = LogManager.GetLogger(typeof (Func));

		public static void Mail(string from, string fromDisplayName, string subject, bool isBodyHtml,
		                        string body, string to, string toDisplayName, string bcc)
		{
			try
			{
				if (!String.IsNullOrEmpty(to))
				{
#if DEBUG
					to = "r.kvasov@analit.net";
					bcc = "";
#endif
					var message = new MailMessage
					              	{
										From = new MailAddress(from, fromDisplayName, Encoding.UTF8),
					              		IsBodyHtml = isBodyHtml,
					              		Subject = subject,
					              		SubjectEncoding = Encoding.UTF8,
					              		Body = body,
										BodyEncoding = Encoding.UTF8,
					              	};
					if (!String.IsNullOrEmpty(bcc))
						message.Bcc.Add(bcc);

					foreach (var toAddress in to.Split(",".ToCharArray()))
						message.To.Add(new MailAddress(toAddress, toDisplayName, Encoding.UTF8));

					var client = new SmtpClient("mail.adc.analit.net");
					client.Send(message);
				}
			}
			catch (Exception ex)
			{
				_log.Error(Utils.ExceptionToString(ex));
			}
		}



		public static string GeneratePassword()
		{
			var availableChars = "23456789qwertyuiopasdfghjkzxcvbnmQWERTYUOPASDFGHJKLZXCVBNM";
			var password = String.Empty;
			var random = new Random();
			while (password.Length < 8)
				password += availableChars[random.Next(0, availableChars.Length - 1)];
			return password;
		}

		public static bool SelectTODS(string SQLQuery, string Table, DataSet DS, MySqlCommand MySQLCommand, string CommandAdd)
		{
			var myMySqlCommand = new MySqlCommand();
			var myMySqlConnection = new MySqlConnection(Literals.GetConnectionString());
			var Комманда = new MySqlCommand();
			var myMySqlDataAdapter = new MySqlDataAdapter();
			try
			{
				myMySqlConnection.Open();
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
			catch (Exception ex)
			{
				_log.Error(Utils.ExceptionToString(ex));
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
			var myMySqlCommand = new MySqlCommand();
			var myMySqlConnection = new MySqlConnection(Literals.GetConnectionString());
			var Комманда = new MySqlCommand();
			var myMySqlDataAdapter = new MySqlDataAdapter();
			try
			{
				myMySqlConnection.Open();
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
			catch (Exception ex)
			{
				_log.Error(Utils.ExceptionToString(ex));
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
			var myMySqlCommand = new MySqlCommand();
			var myMySqlConnection = new MySqlConnection(Literals.GetConnectionString());
			var Комманда = new MySqlCommand();
			var myMySqlDataAdapter = new MySqlDataAdapter();
			try
			{
				myMySqlConnection.Open();
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
			catch (Exception ex)
			{
				_log.Error(Utils.ExceptionToString(ex));
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
				return false;
			return true;
		}

		public static int Send(MailMessage message)
		{
			try
			{
				var smtpid = SmtpClientEx.QuickSendSmartHostSMTPID("mail.adc.analit.net", "", "", message);
				if (smtpid == null)
					return 0;
			}
			catch (Exception ex)
			{
				_log.Error(Utils.ExceptionToString(ex));
			}
			return 0;
		}
	}
}