using System;
using System.DirectoryServices;
using AdminInterface.Helpers;
using NUnit.Framework;
using System.Collections.Generic;
using NUnit.Framework.SyntaxHelpers;

namespace AdminInterface.Test.Helpers
{
	[TestFixture]
	public class ADHelperFixture
	{
		public static void Log(DirectoryEntry entry)
		{
			Console.WriteLine(entry.Name);
			foreach (var property in entry.Properties.PropertyNames)
			{
				Console.WriteLine(String.Format("{0}={1}", property, String.Join("; ", GetValueArray(property.ToString(), entry))));
			}

			Console.WriteLine("______________________________");
		}

		public static string[] GetValueArray(string propertyName, DirectoryEntry entry)
		{
			var values = new List<string>();
			foreach (var value in entry.Properties[propertyName])
			{
				values.Add(value.ToString());
			}
			return values.ToArray();
		}

		private static DirectoryEntry FindDirectoryEntry(string login)
		{
			using (var searcher = new DirectorySearcher(String.Format(@"(CN={0})", login)))
			{
				var searchResult = searcher.FindOne();
				if (searchResult != null)
					return searcher.FindOne().GetDirectoryEntry();
				return null;
			}
		}


		[Test]
		public void IsLoginExistsTest()
		{
			Assert.That(ADHelper.IsLoginExists("kvasov"), Is.True);
			Assert.That(ADHelper.IsLoginExists("loasdvhuq34y89rhawf"), Is.False);
		}

		[Test]
		public void GetPasswordExpirationDate()
		{
			Assert.That(ADHelper.GetPasswordExpirationDate("kvasov"), Is.GreaterThan(DateTime.Now));
		}

		[Test]
		public void IsLockedTest()
		{
			Assert.That(ADHelper.IsLocked("kvasov"), Is.False);
		}

		[Test]
		public void IsDisabledTest()
		{
			Assert.That(ADHelper.IsDisabled("kvasov"), Is.False);
		}

		[Test]
		public void LastLogOnDate()
		{
			Assert.That(ADHelper.GetLastLogOnDate("kvasov"), Is.GreaterThan(DateTime.MinValue));
		}

		[Test]
		public void Test()
		{
			Log(FindDirectoryEntry("Пользователи офиса"));
		}

		[Test]
		public void Enable_disable_test()
		{
			using (var testUser = new TestUser())
			{
				var login = testUser.Login;
				Assert.That(ADHelper.IsDisabled(login), Is.False);
				ADHelper.Disable(login);
				Assert.That(ADHelper.IsDisabled(login), Is.True);
				ADHelper.Enable(login);
				Assert.That(ADHelper.IsDisabled(login), Is.False);
			}
		}
	}

	public class TestUser : IDisposable
	{
		private static DirectoryEntry FindDirectoryEntry(string login)
		{
			using (var searcher = new DirectorySearcher(String.Format(@"(&(objectClass=user)(CN={0}))", login)))
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

		public static bool IsLoginExists(string login)
		{
			return FindDirectoryEntry(login) != null;
		}

		public static void Delete(string login)
		{
			var entry = GetDirectoryEntry(login);
			entry.DeleteTree();
			entry.CommitChanges();
		}

		public TestUser()
		{
			Login = "test456";
			if (IsLoginExists(Login))
				Delete(Login);
			var password = "12345678";
			var root = new DirectoryEntry("LDAP://OU=Пользователи,OU=Клиенты,DC=adc,DC=analit,DC=net");
			var user = root.Children.Add("CN=" + Login, "user");
			user.Properties["samAccountName"].Value = Login;
			user.Properties["userWorkstations"].Add("acdcserv");
			user.CommitChanges();
			user.Invoke("SetPassword", password);
			user.Properties["userAccountControl"].Value = 66048;
			user.CommitChanges();
			root.CommitChanges();
		}

		public string Login { get; set; }

		public void Dispose()
		{
			Delete(Login);
			if (IsLoginExists(Login))
				throw new Exception("тестовая учетная запись {0} не удалена");
		}
	}
}
