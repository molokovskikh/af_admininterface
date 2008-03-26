using System;
using System.Configuration;
using System.Data;
using System.Net.Mail;
using System.Text;
using MySql.Data.MySqlClient;
using MailMessage=System.Net.Mail.MailMessage;
using Microsoft.Practices.EnterpriseLibrary.Logging;

namespace AddUser
{
	public class Func
	{
		public static void Mail(string from, string fromDisplayName, string subject, bool isBodyHtml, 
			string body, string to, string toDisplayName, string bcc, Encoding encoding)
		{
			try
			{
				if (!String.IsNullOrEmpty(to))
				{
#if DEBUG
					to = "r.kvasov@analit.net";
					bcc = "";
#endif
					MailMessage message = new MailMessage();
					message.From = new MailAddress(from, fromDisplayName, encoding);
					message.IsBodyHtml = isBodyHtml;
					message.Subject = subject;
					message.SubjectEncoding = encoding;
					message.Body = body;
					message.BodyEncoding = encoding;
					if (!String.IsNullOrEmpty(bcc))
						message.Bcc.Add(bcc);

					foreach (string toAddress in to.Split(",".ToCharArray()))
						message.To.Add(new MailAddress(toAddress, toDisplayName, encoding));

					SmtpClient client = new SmtpClient("mail.adc.analit.net");
					client.Send(message);
				}
			}
			catch(Exception ex)
			{
				Logger.Write(Utils.ExceptionToString(ex), "Error");
			}
 
		}

		public static string GeneratePassword()
		{
			string  availableChars = "23456789qwertyuiopasdfghjkzxcvbnmQWERTYUOPASDFGHJKLZXCVBNM";
			string password = String.Empty;
			Random random = new Random();
			while(password.Length < 8)
				password += availableChars[random.Next(0, availableChars.Length - 1)];
			return password;
		}

		public static bool SelectTODS(string SQLQuery, string Table, DataSet DS, MySqlCommand MySQLCommand, string CommandAdd)
		{
			MySqlCommand myMySqlCommand = new MySqlCommand();
			MySqlConnection myMySqlConnection = new MySqlConnection(Literals.GetConnectionString());
			MySqlCommand Комманда = new MySqlCommand();
			MySqlDataAdapter myMySqlDataAdapter = new MySqlDataAdapter();
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
				Logger.Write(Utils.ExceptionToString(ex), "Error");
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
			MySqlConnection myMySqlConnection = new MySqlConnection(Literals.GetConnectionString());
			MySqlCommand Комманда = new MySqlCommand();
			MySqlDataAdapter myMySqlDataAdapter = new MySqlDataAdapter();
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
				Logger.Write(Utils.ExceptionToString(ex), "Error");
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
			MySqlConnection myMySqlConnection = new MySqlConnection(Literals.GetConnectionString());
			MySqlCommand Комманда = new MySqlCommand();
			MySqlDataAdapter myMySqlDataAdapter = new MySqlDataAdapter();
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
				Logger.Write(Utils.ExceptionToString(ex), "Error");
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