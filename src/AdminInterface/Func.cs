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
#if !DEBUG
			try
			{
				MailMessage message = new MailMessage();
				message.From = new MailAddress(from, fromDisplayName, encoding);
				message.IsBodyHtml = isBodyHtml;
				message.Subject = subject;
				message.SubjectEncoding = encoding;
				message.Body = body;
				message.BodyEncoding = encoding;				
				if (!String.IsNullOrEmpty(bcc))
					message.Bcc.Add(bcc);

				foreach(string toAddress in to.Split(",".ToCharArray()))
					message.To.Add(new MailAddress(toAddress, toDisplayName, encoding));

				SmtpClient client = new SmtpClient("box.analit.net");
				client.Send(message);
			}
			catch(Exception ex)
			{
				Logger.Write(Utils.ExceptionToString(ex), "Error");
			}
#endif 
		}

		public static string GeneratePassword()
		{
			Random random = new Random();
			Int16 r;
			string PassStr = String.Empty;
			for (int i = 0; i <= 7; i++)
			{
				r = Convert.ToInt16((62 * Convert.ToSingle(random.NextDouble())) + 1);
				if ((r >= 0) && (r < 26))
				{
					if ((65 + r) == 73)
						r = Convert.ToInt16(r + 1);
					PassStr = PassStr + (char)(65 + r);
				}
				if ((r >= 26) && (r < 52))
				{
					if ((71 + Convert.ToInt32(r)) == 108)
						r = Convert.ToInt16(r + 1);
					PassStr = PassStr + (char)(71 + r);
				}
				if (r >= 52)
				{
					PassStr = PassStr + (char)(48 + Convert.ToInt32(Convert.ToSingle(random.NextDouble()) * 9));
				}
			}
			if (PassStr.Length < 8)
			{
				r = Convert.ToInt16(97 + (DateTime.Now.Second / 3));
				if (Convert.ToInt32(r) == 108)
				{
					r = Convert.ToInt16(r + 1);
				}
				PassStr = PassStr + (char)r;
			}
			return PassStr;
		}

		public static bool SelectTODS(string SQLQuery, string Table, DataSet DS, MySqlCommand MySQLCommand, string CommandAdd)
		{
			MySqlCommand myMySqlCommand = new MySqlCommand();
			MySqlConnection myMySqlConnection = new MySqlConnection(Literals.GetConnectionString());
			MySqlCommand �������� = new MySqlCommand();
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
				��������.Dispose();
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
			MySqlCommand �������� = new MySqlCommand();
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
				��������.Dispose();
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
			MySqlCommand �������� = new MySqlCommand();
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
				��������.Dispose();
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
							"�� ����� �������� ������ {0}. ��� �������� ���������� ����� ����������� ����� \".\" ��� \",\", � ������� 5.54",
							InputStr));
				}
			}
			return 0;
		}


		public static string GetFirmType(int FrimTypeCode)
		{
			string FirmTypeStr;
			if (FrimTypeCode == 0)
				FirmTypeStr = "���������";
			else
				FirmTypeStr = "������";
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