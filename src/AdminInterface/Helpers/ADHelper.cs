using System;
using System.Collections;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Text;
using AdminInterface.Models;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using AdminInterface.Security;
using Common.Tools;
using Common.Web.Ui.Helpers;
using log4net;

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

		public bool IsLoginExists;

		[Style]
		public bool IsLocked { get; set; }

		[Style]
		public bool DisabledInAd { get; set; }

		[Style]
		public bool NotExistsUser
		{
			get { return !IsLoginExists; }
		}

		public DateTime? LastLogOnDate;
		public DateTime? BadPasswordDate;
		public DateTime? LastPasswordChange;

		public void CalculateLastLogon(DateTime? value)
		{
			if (value.GetValueOrDefault() > LastLogOnDate.GetValueOrDefault())
				LastLogOnDate = value;
		}
	}

	public class ADUserInformationCollection : List<ADUserInformation>
	{
		private int GetIndexByLogin(string login)
		{
			for(int i = 0; i < Count; i++)
				if(String.Equals(this[i].Login, login, StringComparison.InvariantCultureIgnoreCase))
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

	public interface IUserStorage
	{
		bool CreateAdmin(string userName, string managerName, string password);
		IList<string> GetDomainComputers();
		bool IsLoginExists(string userName);
		bool[,] GetLogonHours(string userName);
		IList<string> GetAccessibleComputers(string userName);
		void SetAccessibleComputer(string userName, IEnumerable<string> workstations);
	}

	public class MemoryUserStorage : IUserStorage
	{
		public static Dictionary<string, Dictionary<string, object>> users
			= new Dictionary<string, Dictionary<string, object>>(StringComparer.InvariantCultureIgnoreCase);

		public bool CreateAdmin(string userName, string managerName, string password)
		{
			if (IsLoginExists(userName))
				return false;

			users.Add(userName, new Dictionary<string, object> {
				{"computers", new List<string>()},
				{"logonHours", new bool[7, 24]}
			});

			return true;
		}

		public IList<string> GetDomainComputers()
		{
			return new List<string> {
				"cmp1", "cmp2"
			};
		}

		public bool IsLoginExists(string userName)
		{
			return users.ContainsKey(userName);
		}

		public bool[,] GetLogonHours(string userName)
		{
			if (!IsLoginExists(userName))
				return new bool[7, 24];
			return (bool[,])users[userName]["logonHours"];
		}

		public IList<string> GetAccessibleComputers(string userName)
		{
			if (!IsLoginExists(userName))
				return Enumerable.Empty<string>().ToList();
			return (IList<string>)users[userName]["computers"];
		}

		public void SetAccessibleComputer(string userName, IEnumerable<string> workstations)
		{
			if (!IsLoginExists(userName))
				return;
			users[userName]["computers"] = workstations.ToList();
		}
	}

	public class ActiveDirectoryUserStorage : IUserStorage
	{
		public bool CreateAdmin(string userName, string managerName, string password)
		{
			var entry = ADHelper.FindDirectoryEntry(userName);

			var adminGroupPath = "LDAP://CN=Региональные администраторы,OU=Группы,OU=Клиенты,DC=adc,DC=analit,DC=net";
			var root = new DirectoryEntry("LDAP://OU=Офис,DC=adc,DC=analit,DC=net");

			if (entry != null)
			{
				entry.Properties["userAccountControl"][0] = AccountControl.NormalAccount;
				// установить pwdLastSet в текущую дату
				entry.Properties["pwdLastSet"][0] = -1;
				// сменить пароль
				entry.Invoke("SetPassword", password);
				entry.CommitChanges();

				var member = entry.Properties["memberOf"]
					.OfType<string>()
					.FirstOrDefault(mebmer => mebmer.Equals(adminGroupPath));

				if (String.IsNullOrEmpty(member))
				{
					var adminGroup = new DirectoryEntry(adminGroupPath);
					adminGroup.Invoke("Add", entry.Path);
					adminGroup.CommitChanges();
				}
				entry.MoveTo(root);
				entry.CommitChanges();
				return false;
			}

			var userGroup = new DirectoryEntry("LDAP://CN=Пользователи офиса,OU=Уровни доступа,OU=Офис,DC=adc,DC=analit,DC=net");
			var adminGroup1 = new DirectoryEntry(adminGroupPath);
			var user = root.Children.Add("CN=" + userName, "user");
			user.Properties["samAccountName"].Value = userName;
			if (!String.IsNullOrEmpty(managerName.Trim()))
				user.Properties["sn"].Value = managerName;
			user.Properties["logonHours"].Value = ADHelper.LogonHours();
			user.CommitChanges();
			user.Properties["userAccountControl"][0] = AccountControl.NormalAccount;
			user.CommitChanges();
			user.Invoke("SetPassword", password);
			user.CommitChanges();

			userGroup.Invoke("Add", user.Path);
			userGroup.CommitChanges();

			adminGroup1.Invoke("Add", user.Path);
			adminGroup1.CommitChanges();

			root.CommitChanges();
			return true;
		}

		public IList<string> GetDomainComputers()
		{
			return ADHelper.GetDomainComputers();
		}

		public bool IsLoginExists(string userName)
		{
			return ADHelper.IsLoginExists(userName);
		}

		public bool[,] GetLogonHours(string userName)
		{
			return ADHelper.GetLogonHours(userName);
		}

		public IList<string> GetAccessibleComputers(string userName)
		{
			return ADHelper.GetAccessibleComputers(userName);
		}

		public void SetAccessibleComputer(string userName, IEnumerable<string> workstations)
		{
			ADHelper.SetAccessibleComputer(userName, workstations);
		}
	}

	public class ADHelper
	{
		private static ILog log = LogManager.GetLogger(typeof(ADHelper));

		private static readonly DateTime _adDateInitValue = new DateTime(1601, 1, 1, 3, 0, 0);

		public static IUserStorage Storage = new ActiveDirectoryUserStorage();

		public static ADUserInformationCollection GetPartialUsersInformation(IEnumerable<User> users)
		{
			return GetPartialUsersInformation(users.Select(u => u.Login));
		}

		public static ADUserInformationCollection GetPartialUsersInformation(IEnumerable<string> logins)
		{
			var result = new ADUserInformationCollection();
			result.AddRange(logins.Select(user => new ADUserInformation {Login = user}));
			try
			{
				var filter = new StringBuilder();
				foreach (var login in logins)
					filter.Append("(sAMAccountName=" + login + ")");
				return GetInformation(result, filter);
			}
			catch (Exception ex)
			{
				log.Error(String.Format("Не смогли получить информацию о пользователе AD. Admin={0}, Users=({1})",
						SecurityContext.Administrator.UserName, logins.Implode()), ex);
			}
			return result;
		}

		private static ADUserInformationCollection GetInformation(ADUserInformationCollection usersInfo, StringBuilder filter)
		{
			if (usersInfo.Count == 0)
				return null;
			if (usersInfo.Count > 1)
				filter.Insert(0, "|");

			using (var searcher = new DirectorySearcher(String.Format(@"(&(objectClass=user)({0}))", filter)))
			{
				var searchResults = searcher.FindAll();
				foreach (SearchResult searchResult in searchResults)
				{
					var info = GetInformationBySearchResult(searchResult);
					if (info == null)
						continue;
					usersInfo[info.Login] = info;
				}
			}
			return usersInfo;
		}

		private static ADUserInformation GetInformationBySearchResult(SearchResult searchResult)
		{
			var tempResult = new ADUserInformation();
			if (searchResult == null)
				return null;
			tempResult.IsLoginExists = true;
			tempResult.Login = searchResult.Properties["sAMAccountName"][0].ToString();

			var directoryEntry = searchResult.GetDirectoryEntry();
			tempResult.IsLocked = Convert.ToBoolean(directoryEntry.InvokeGet("IsAccountLocked"));
			tempResult.DisabledInAd = Convert.ToBoolean(directoryEntry.InvokeGet("AccountDisabled"));

			return tempResult;
		}

		public static ADUserInformation GetADUserInformation(User user)
		{
			var login = user.Login;
			var result = new ADUserInformation {
				LastLogOnDate = user.Logs.LastLogon
			};
			try
			{
				result.Login = login;
				result.IsLoginExists = IsLoginExists(login);
				if (result.IsLoginExists) {
					result.BadPasswordDate = GetBadPasswordDate(login);
					result.DisabledInAd = IsDisabled(login);
					result.IsLocked = IsLocked(login);
					result.CalculateLastLogon(GetLastLogOnDate(login));
					result.LastPasswordChange = GetLastPasswordChange(login);
					result.DisabledInAd = IsDisabled(login);
				}
				return result;
			}
			catch (Exception)
			{
				return result;
			}
		}

		public static void CreateUserInAD(string login, string password, uint clientCode)
		{
#if !DEBUG
			var root = new DirectoryEntry("LDAP://acdcserv/OU=Пользователи,OU=Клиенты,DC=adc,DC=analit,DC=net");
			var userGroup = new DirectoryEntry("LDAP://acdcserv/CN=Базовая группа клиентов - получателей данных,OU=Группы,OU=Клиенты,DC=adc,DC=analit,DC=net");
			var user = root.Children.Add("CN=" + login, "user");
			user.Properties["samAccountName"].Value = login;
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

		public static void RenameUser(string oldLogin, string newLogin)
		{
#if !DEBUG
			var user = FindDirectoryEntry(oldLogin);
			var parent = new DirectoryEntry(user.Path.Replace(String.Format("CN={0},", oldLogin), String.Empty));
			user.MoveTo(parent, String.Format("CN={0}", newLogin));
			user.Properties["samAccountName"].Value = newLogin;
			user.CommitChanges();
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

		public static bool[,] GetLogonHours(string login)
		{
			var entry = GetDirectoryEntry(login);
			var week = new List<bool>();
			var logonHours = (byte[]) entry.Properties["logonHours"].Value;
			if (logonHours == null)
			{
				var matrix = new bool[7, 24];
				for (var i = 0; i < 7; i++)
					for (var j = 0; j < 24; j++)
						matrix[i, j] = true;
				return matrix;
			}

			for (var i = 0; i < logonHours.Length; i += 3)
			{
				for (var j = 0; j < 3; j++)
				{
					var currByte = logonHours[j + i];
					var hoursBoolIndex = 7;
					var hourBools = new bool[8];

					for (var k = 128; k > 0; k /= 2)
					{
						hourBools[hoursBoolIndex] = ((currByte & k) != 0);
						hoursBoolIndex--;
					}

					for (var x = 0; x < hourBools.Length; x++)
						week.Add(hourBools[x]);
				}
			}

			var logonHoursMatrix = new bool[7, 24];
			var hourIndex = 21;

			for (var i = 0; i < 7; i++)
				for (var j = 0; j < 24; j++)
				{
					logonHoursMatrix[i, j] = week[hourIndex % 168];
					hourIndex++;
				}
			return logonHoursMatrix;
		}

		public static void SetLogonHours(string login, bool[,] logonHoursMatrix)
		{
			var hourIndex = 21;
			var week = new bool[168]; // массив часов в неделе

			for (var i = 0; i < 7; i++)
				for (var j = 0; j < 24; j++)
				{
					week[hourIndex % 168] = logonHoursMatrix[i, j];
					hourIndex++;
				}

			var totalBytes = new List<byte>();
			for (var i = 0; i < 3 * 7; i += 3)
			{
				var bytes = new List<byte>();
				for (var j = 0; j < 3; j++)
				{
					var hourBools = new bool[8];
					var index = 0;
					for (var k = 7; k >= 0; k--)
						hourBools[index++] = week[(i + j)*8 + k];
					bytes.Add(ConvertToByte(hourBools));
				}
				totalBytes.AddRange(bytes);
			}
#if !DEBUG
			var entry = GetDirectoryEntry(login);
			entry.Properties["logonHours"].Value = totalBytes.ToArray();
			entry.CommitChanges();
#endif
		}

		public static byte ConvertToByte(bool[] bits)
		{
			if (bits.Length != 8)
				throw new ArgumentException("Неправильное количество битов");

			byte b = 0;
			var index = 0;

			for (byte i = 128; i > 0; i /= 2)
				if (bits[index++])
					b += i;
			return b;
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

		public static IList<string> GetDomainComputers()
		{
			var computers = new List<string>();
			using (var searcher = new DirectorySearcher(String.Format(@"(&(objectClass=computer))")))
			{
				var searchResult = searcher.FindAll();
				if (searchResult == null)
					return computers;
				foreach (SearchResult res in searchResult)
					computers.Add(res.Properties["name"][0].ToString().ToUpper());
			}
			computers.Sort();
			return computers;
		}


		public static IList<string> GetAccessibleComputers(string login)
		{
			var computers = new List<string>();
			var user = GetDirectoryEntry(login);
			var workstations = user.Properties["userWorkstations"];

			foreach (var workstation in workstations)
				computers.AddRange(workstation.ToString().ToUpper().Split(','));
			return computers;
		}

		public static void SetAccessibleComputer(string login, IEnumerable<string> workstations)
		{
			var entry = GetDirectoryEntry(login);
			entry.Properties["userWorkstations"][0] = workstations.Implode(",");
			entry.CommitChanges();
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
			return ReadNotReplicatedDate(login, "lastLogon");
		}

		public static void Disable(string login)
		{
#if !DEBUG
			var entity = GetDirectoryEntry(login);
			entity.InvokeSet("AccountDisabled", true);
			entity.CommitChanges();
#endif
		}

		public static void Enable(string login)
		{
#if !DEBUG
			var entiry = GetDirectoryEntry(login);
			entiry.InvokeSet("AccountDisabled", false);
			entiry.CommitChanges();
#endif
		}

		public static DateTime? GetBadPasswordDate(string login)
		{
			return ReadNotReplicatedDate(login, "badPasswordTime");
		}

		private static DateTime? ReadNotReplicatedDate(string login, string name)
		{
			DateTime? resultDate = null;
			var controllers = GetDomainControllers();
			foreach (var serverName in controllers)
			{
				try
				{
					using (var searcher = new DirectorySearcher(new DirectoryEntry(String.Format("LDAP://{0}", serverName))))
					{
						searcher.Filter = string.Format("(&(objectClass=user)(sAMAccountName={0}))", login);
						var result = searcher.FindOne();
						if (result == null || result.Properties[name].Count == 0)
							continue;
						var date = DateTime.FromFileTime((long) searcher.FindOne().Properties[name][0]);
						if (date == _adDateInitValue)
							continue;
						if (!resultDate.HasValue || date.CompareTo(resultDate.Value) > 0)
							resultDate = date;
					}
				}
				catch(Exception e)
				{
					log.Warn(String.Format("Ошибка при запросе к контролеру домена {0}", serverName), e);
				}
			}
			return resultDate;
		}

		public static bool IsBelongsToOfficeContainer(string login)
		{
#if !DEBUG
			var entry = FindDirectoryEntry(login);
			if (entry == null)
				return false;
			return entry.Path.Contains("OU=Офис");
#else
			return false;
#endif
		}

		public static DateTime? GetLastPasswordChange(string login)
		{
			return ReadNotReplicatedDate(login, "pwdLastSet");
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
