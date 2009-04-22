using System;
using System.DirectoryServices;

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

	public class ADHelper
	{
		private static readonly DateTime _badPasswordDateIfNotLogin = new DateTime(1601, 1, 1, 3, 0, 0); 

		public static void CreateUserInAD(string login, string password , string clientCode)
		{
#if !DEBUG
			var root = new DirectoryEntry("LDAP://acdcserv/OU=Пользователи,OU=Клиенты,DC=adc,DC=analit,DC=net");
			var userGroup = new DirectoryEntry("LDAP://acdcserv/CN=Базовая группа клиентов - получателей данных,OU=Группы,OU=Клиенты,DC=adc,DC=analit,DC=net");
			var user = root.Children.Add("CN=" + login, "user");
			user.Properties["samAccountName"].Value = login;
			user.Properties["userWorkstations"].Add("acdcserv");
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
			return FindDirectoryEntry(login) != null;
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
			return Convert.ToBoolean(GetDirectoryEntry(login).InvokeGet("IsAccountLocked")); 
		}

		public static void Unlock(string login)
		{
			var entry = GetDirectoryEntry(login);
			entry.InvokeSet("IsAccountLocked", false);
			entry.CommitChanges();
		}

		public static bool IsDisabled(string login)
		{
			return Convert.ToBoolean(GetDirectoryEntry(login).InvokeGet("AccountDisabled")); 
		}

		public static byte[] LogonHours()
		{
			var hours = new byte[]
			            	{
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
			using (var searcher = new DirectorySearcher(new DirectoryEntry("LDAP://acdcserv")))
			{
				searcher.Filter = String.Format("(&(objectClass=user)(sAMAccountName={0}))", login);
				var result = searcher.FindOne();
				if ((result == null) || (result.Properties["lastLogon"].Count == 0))
					return null;
				var lastLogon = DateTime.FromFileTime((long)searcher.FindOne().Properties["lastLogon"][0]);
				//ad инициализирует этим значением поле
				if (lastLogon == DateTime.Parse("01.01.1601 3:00:00"))
					return null;
				return lastLogon;
			}
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
			using (var searcher = new DirectorySearcher(string.Format("(&(objectClass=user)(sAMAccountName={0}))", login)))
			{
				var result = searcher.FindOne();
				if ((result == null) || (result.Properties["badPasswordTime"].Count == 0))
					return null;

				var badPassworDate = DateTime.FromFileTime((long)searcher.FindOne().Properties["badPasswordTime"][0]);
				if (badPassworDate == _badPasswordDateIfNotLogin)
					return null;

				return badPassworDate;
			}
		}

		public static bool IsBelongsToOfficeContainer(string login)
		{
			var entry = FindDirectoryEntry(login);
			return entry.Path.Contains("OU=Офис");
		}

		public static DateTime? GetLastPasswordChange(string login)
		{
			using (var searcher = new DirectorySearcher(string.Format("(&(objectClass=user)(sAMAccountName={0}))", login)))
			{
				var result = searcher.FindOne();
				if ((result == null) || (result.Properties["pwdLastSet"].Count == 0))
					return null;
				return DateTime.FromFileTime((long)searcher.FindOne().Properties["pwdLastSet"][0]);
			}
		}
	}
}
