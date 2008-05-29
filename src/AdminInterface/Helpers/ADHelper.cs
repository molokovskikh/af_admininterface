﻿using System;
using System.DirectoryServices;
using AdminInterface.Models;

namespace AdminInterface.Helpers
{
	public class ADHelper
	{
		public static void CreateUserInAD(string login, string password , string clientCode)
		{
#if !DEBUG
			var root = new DirectoryEntry("LDAP://OU=Пользователи,OU=Клиенты,DC=adc,DC=analit,DC=net");
			var userGroup = new DirectoryEntry("LDAP://CN=Базовая группа клиентов - получателей данных,OU=Группы,OU=Клиенты,DC=adc,DC=analit,DC=net");
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
			using (var searcher = new DirectorySearcher(String.Format(@"(&(objectClass=user)(CN={0}))", login)))
				return DateTime.FromFileTime((long)searcher.FindOne().Properties["pwdLastSet"][0]) + GetMaxPasswordAge();
		}

		public static bool IsLocked(string login)
		{
			return Convert.ToBoolean(GetDirectoryEntry(login).InvokeGet("IsAccountLocked")); 
		}

		public static void Unlock(string login)
		{
#if !DEBUG
			var entry = GetDirectoryEntry(login);
			entry.InvokeSet("IsAccountLocked", false);
			entry.CommitChanges();
#endif
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

		private static DirectoryEntry FindDirectoryEntry(string login)
		{
			using(var searcher = new DirectorySearcher(String.Format(@"(&(objectClass=user)(CN={0}))", login)))
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
#if !DEBUG
			var entry = GetDirectoryEntry(login);
			entry.InvokeSet("IsAccountLocked", true);
			entry.CommitChanges();
#endif
		}

		public static void Delete(string login)
		{
#if !DEBUG
			var entry = GetDirectoryEntry(login);
			entry.DeleteTree();
			entry.CaommitChanges();
#endif
		}

		public static DateTime? GetLastLogOnDate(string login)
		{
			using (var searcher = new DirectorySearcher(string.Format("(&(objectClass=user)(CN={0}))", login)))
			{
				var result = searcher.FindOne();
				if ((result == null) || (result.Properties["lastLogon"].Count == 0))
					return null;
				return DateTime.FromFileTime((long)searcher.FindOne().Properties["lastLogon"][0]);
			}
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

		public static void CreateAdministratorInAd(Administrator administrator)
		{
#if !DEBUG
			var root = new DirectoryEntry("LDAP://OU=Региональные администраторы,OU=Управляющие,DC=adc,DC=analit,DC=net");
			var userGroup = new DirectoryEntry("LDAP://CN=Пользователи офиса,OU=Уровни доступа,OU=Офис,DC=adc,DC=analit,DC=net");
			var user = root.Children.Add("CN=" + administrator.Login, "user");
			user.Properties["samAccountName"].Value = administrator.Login;
			if (!String.IsNullOrEmpty(administrator.FIO.Trim()))
				user.Properties["sn"].Value = administrator.FIO;
			user.Properties["logonHours"].Value = ADHelper.LogonHours();
			user.CommitChanges();
			user.Invoke("SetPassword", password);
			user.CommitChanges();
			userGroup.Invoke("Add", user.Path);
			userGroup.CommitChanges();
			root.CommitChanges();
#endif
		}
	}
}
