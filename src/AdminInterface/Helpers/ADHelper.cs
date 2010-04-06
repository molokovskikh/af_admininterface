using System;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Text;
using AdminInterface.Models;
using System.Collections.Generic;

namespace AdminInterface.Helpers
{

	/*
	 * заметки на полях обращение к конкретному хосту 
	 * LDAP://acdcserv/OU=Пользователи,OU=Клиенты,DC=adc,DC=analit,DC=net 
	 * Поиск на конкретном хосте
	 * using (var searcher = new DirectorySearcher(new DirectoryEntry("LDAP://acdcserv")))
	 * searcher.Filter = string.Format("(&(objectClass=user)(sAMAccountName={0}))", login);
	 */
	[Flags]
	public enum AccountControl
	{
		NormalAccount = 512,
		DontExpirePassword = 65536,
	}

	public class ADUserInformation
	{
		public string Login;
		public bool IsLoginExists = false;
		public bool IsLocked;
		public bool IsDisabled;
		public DateTime? LastLogOnDate;
		public DateTime? BadPasswordDate;
		public DateTime? LastPasswordChange;
	}

	public class ADUserInformationCollection : List<ADUserInformation>
	{
		private int GetIndexByLogin(string login)
		{
			for(int i = 0; i < this.Count; i++)
				if(this[i].Login == login)
					return i;
			throw new ArgumentException("Не найдены настройки AD для пользователя " + login, "login");
		}

		public ADUserInformation this[string login]
		{
			get
			{
				return this[GetIndexByLogin(login)];
			}
			set
			{
				this[GetIndexByLogin(login)] = value;
			}
		}
	}

	public class ADHelper
	{
		private static readonly DateTime _badPasswordDateIfNotLogin = new DateTime(1601, 1, 1, 3, 0, 0);

		public static ADUserInformationCollection GetPartialUsersInformation(IEnumerable<User> users)
		{
			var result = new ADUserInformationCollection();
			var filter= new StringBuilder();
			foreach (var user in users)
			{
				filter.Append("(sAMAccountName=" + user.Login + ")");
				result.Add(new ADUserInformation() {Login = user.Login});
			}

			if(result.Count == 0)
				return null;
			if (result.Count > 1)
				filter.Insert(0, "|");
			Console.WriteLine("begin - {0:ss.fff}", DateTime.Now);
			
			using (var searcher = new DirectorySearcher(String.Format(@"(&(objectClass=user)({0}))", filter)))
			{				
				var searchResults = searcher.FindAll();
				foreach (SearchResult searchResult in searchResults)
				{
					var tempResult = new ADUserInformation();
					if (searchResult == null)
						continue;
					Console.WriteLine("{1} - {0:ss.fff}", DateTime.Now, searchResult.Properties["sAMAccountName"][0].ToString());
					tempResult.IsLoginExists = true;
					tempResult.Login = searchResult.Properties["sAMAccountName"][0].ToString();

					var directoryEntry = searchResult.GetDirectoryEntry();
					Console.WriteLine("{0:ss.fff}", DateTime.Now);
					tempResult.IsLocked = Convert.ToBoolean(directoryEntry.InvokeGet("IsAccountLocked"));
					tempResult.IsDisabled = Convert.ToBoolean(directoryEntry.InvokeGet("AccountDisabled"));
					Console.WriteLine("{0:ss.fff}", DateTime.Now);

					/* Более эта информация не нужна и тратить время на ее получение не хочется
					if (searchResult.Properties["lastLogon"].Count == 0)
						tempResult.LastLogOnDate = null;
					else
						tempResult.LastLogOnDate = DateTime.FromFileTime((long)searchResult.Properties["lastLogon"][0]);
					//ad инициализирует этим значением поле
					if (tempResult.LastLogOnDate == DateTime.Parse("01.01.1601 3:00:00"))
						tempResult.LastLogOnDate = null;

					if (searchResult.Properties["badPasswordTime"].Count == 0)
						tempResult.BadPasswordDate = null;
					else
						tempResult.BadPasswordDate = DateTime.FromFileTime((long)searchResult.Properties["badPasswordTime"][0]);
					if (tempResult.BadPasswordDate == _badPasswordDateIfNotLogin)
						tempResult.BadPasswordDate = null;

					if (searchResult.Properties["pwdLastSet"].Count == 0)
						tempResult.LastPasswordChange = null;
					else
						tempResult.LastPasswordChange = DateTime.FromFileTime((long)searchResult.Properties["pwdLastSet"][0]);*/

					result[tempResult.Login] = tempResult;
				}
			}
			return result;
		}

		public static ADUserInformation GetADUserInformation(string login)
		{
			try
			{
				var result = new ADUserInformation {
                    Login = login,
                    IsLoginExists = IsLoginExists(login),
				};
				if (result.IsLoginExists)
				{
					result.BadPasswordDate = GetBadPasswordDate(login);
					result.BadPasswordDate = GetBadPasswordDate(login);
					result.IsDisabled = IsDisabled(login);
					result.IsLocked = IsLocked(login);
					result.LastLogOnDate = GetLastLogOnDate(login);
					result.LastPasswordChange = GetLastPasswordChange(login);
					result.IsDisabled = IsDisabled(login);
				}
				return result;
			}
			catch (Exception)
			{
				return null;
			}
		}

		public static void CreateUserInAD(string login, string password, uint clientCode)
		{
#if !DEBUG
			var root = new DirectoryEntry("LDAP://acdcserv/OU=Пользователи,OU=Клиенты,DC=adc,DC=analit,DC=net");
			var userGroup = new DirectoryEntry("LDAP://acdcserv/CN=Базовая группа клиентов - получателей данных,OU=Группы,OU=Клиенты,DC=adc,DC=analit,DC=net");
			var user = root.Children.Add("CN=" + login, "user");
			user.Properties["samAccountName"].Value = login;
			user.Properties["userWorkstations"].Add("acdcserv,solo");
			user.Properties["description"].Value = clientCode.ToString();
			user.CommitChanges();
			user.Invoke("SetPassword", password);
			user.Properties["userAccountControl"].Value = 66048;
			user.CommitChanges();
			userGroup.Invoke("Add", user.Path);
			userGroup.CommitChanges();
			root.CommitChanges();
#endif
		}

		public static bool IsLoginExists(string login)
		{
#if !DEBUG
			return FindDirectoryEntry(login) != null;
#else
			return true;
#endif
		}

		public static void ChangePassword(string login, string password)
		{
#if !DEBUG
			var entry = GetDirectoryEntry(login);
			GetDirectoryEntry(login).Invoke("SetPassword", password);
			entry.CommitChanges();
#endif
		}

		public static DateTime GetPasswordExpirationDate(string login)
		{
			using (var searcher = new DirectorySearcher(String.Format(@"(&(objectClass=user)(sAMAccountName={0}))", login)))
				return DateTime.FromFileTime((long)searcher.FindOne().Properties["pwdLastSet"][0]) + GetMaxPasswordAge();
		}

		public static bool IsLocked(string login)
		{
#if !DEBUG
			return Convert.ToBoolean(GetDirectoryEntry(login).InvokeGet("IsAccountLocked")); 
#else
			return false;
#endif
		}

		public static void Unlock(string login)
		{
			var entry = GetDirectoryEntry(login);
			entry.InvokeSet("IsAccountLocked", false);
			entry.CommitChanges();
		}

		public static bool IsDisabled(string login)
		{
#if !DEBUG
			return Convert.ToBoolean(GetDirectoryEntry(login).InvokeGet("AccountDisabled")); 
#else
			return false;
#endif
		}

		public static byte[] LogonHours()
		{
			var hours = new byte[] {
				0, 0, 0,
				224, 255, 3,
				224, 255, 3,
				224, 255, 3,
				224, 255, 3,
				224, 255, 3,
				0, 0, 0,
			};
			return hours;
		}

		public static DirectoryEntry FindDirectoryEntry(string login)
		{
			using (var searcher = new DirectorySearcher(String.Format(@"(&(objectClass=user)(sAMAccountName={0}))", login)))
			{
				var searchResult = searcher.FindOne();
				if (searchResult != null)
					return searcher.FindOne().GetDirectoryEntry();
				return null;
			}
		}

		private static DirectoryEntry GetDirectoryEntry(string login)
		{
			var entry = FindDirectoryEntry(login);
			if (entry == null)
				throw new Exception(String.Format("Учетная запись Active Directory {0} не найдена", login));
			return entry;
		}

		private static TimeSpan GetMaxPasswordAge()
		{
			using (var searcher = new DirectorySearcher("(objectClass=domainDNS)"))
				return TimeSpan.FromTicks(Math.Abs((long) searcher.FindOne().Properties["maxPwdAge"][0]));
		}

		public static void Block(string login)
		{
			for (var i = 0; i < 10; i++)
			{
				try
				{
					var en = new DirectoryEntry("LDAP://DC=adc,DC=analit,DC=net", login, "123");
					var n = en.NativeObject;
				}
				catch {}
			}
		}

		public static void Delete(string login)
		{
#if !DEBUG
			var entry = GetDirectoryEntry(login);
			entry.DeleteTree();
			entry.CommitChanges();
#endif
		}

		public static DateTime? GetLastLogOnDate(string login)
		{
			DateTime? resultDate = null;
			var controllers = GetDomainControllers();
			foreach (var serverName in controllers)
			{
				using (var searcher = new DirectorySearcher(new DirectoryEntry(String.Format("LDAP://{0}", serverName))))
				{
					searcher.Filter = String.Format("(&(objectClass=user)(sAMAccountName={0}))", login);
					var result = searcher.FindOne();
					if ((result == null) || (result.Properties["lastLogon"].Count == 0))
						continue;
					var lastLogon = DateTime.FromFileTime((long)searcher.FindOne().Properties["lastLogon"][0]);
					//ad инициализирует этим значением поле
					if (lastLogon == DateTime.Parse("01.01.1601 3:00:00"))
						continue;
					if (!resultDate.HasValue || (lastLogon.CompareTo(resultDate.Value) > 0))
						resultDate = lastLogon;
				}
			}
			return resultDate;
		}

		public static void Disable(string login)
		{
			var entity = GetDirectoryEntry(login);
			entity.InvokeSet("AccountDisabled", true);
			entity.CommitChanges();
		}

		public static void Enable(string login)
		{
			var entiry = GetDirectoryEntry(login);
			entiry.InvokeSet("AccountDisabled", false);
			entiry.CommitChanges();
		}

		public static DateTime? GetBadPasswordDate(string login)
		{
			DateTime? resultDate = null;
			var controllers = GetDomainControllers();
			foreach (var serverName in controllers)
			{
				using (var searcher = new DirectorySearcher(new DirectoryEntry(String.Format("LDAP://{0}", serverName))))
				{
					searcher.Filter = string.Format("(&(objectClass=user)(sAMAccountName={0}))", login);
					var result = searcher.FindOne();
					if ((result == null) || (result.Properties["badPasswordTime"].Count == 0))
						continue;
					var date = DateTime.FromFileTime((long) searcher.FindOne().Properties["badPasswordTime"][0]);
					if (date == _badPasswordDateIfNotLogin)
						continue;
					if (!resultDate.HasValue || (date.CompareTo(resultDate.Value) > 0))
						resultDate = date;
				}
			}
			return resultDate;
		}

		public static bool IsBelongsToOfficeContainer(string login)
		{
#if !DEBUG
			var entry = FindDirectoryEntry(login);
			return entry.Path.Contains("OU=Офис");
#else
			return false;
#endif
		}

		public static DateTime? GetLastPasswordChange(string login)
		{
			DateTime? resultDate = null;
			var controllers = GetDomainControllers();
			foreach (var serverName in controllers)
			{
				using (var searcher = new DirectorySearcher(new DirectoryEntry(String.Format("LDAP://{0}", serverName))))
				{
					searcher.Filter = string.Format("(&(objectClass=user)(sAMAccountName={0}))", login);
                    var result = searcher.FindOne();
                    if ((result == null) || (result.Properties["pwdLastSet"].Count == 0))
						continue;
					var date = DateTime.FromFileTime((long) searcher.FindOne().Properties["pwdLastSet"][0]);
					if (!resultDate.HasValue || (date.CompareTo(resultDate.Value) > 0))
						resultDate = date;
				}
			}
			return resultDate;
		}

		public static IList<string> GetDomainControllers()
		{
            var controlers = Forest.GetCurrentForest().Domains[0].DomainControllers;
			var names = new List<string>();
			foreach (DomainController srv in controlers)
				names.Add(srv.Name);
			return names;
		}
	}
}
