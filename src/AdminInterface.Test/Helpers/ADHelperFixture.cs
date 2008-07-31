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
			using (var searcher = new DirectorySearcher(String.Format(@"(name={0})", login)))
			{
				var searchResult = searcher.FindOne();
				if (searchResult != null)
					return searcher.FindOne().GetDirectoryEntry();
				return null;
			}
		}

		private static DateTime CreatedAt(string login)
		{
			using (var searcher = new DirectorySearcher(string.Format("(&(objectClass=user)(sAMAccountName={0}))", login)))
			{
				return Convert.ToDateTime(searcher.FindOne().Properties["whenCreated"][0]);
			}
		}

		[Test]
		public void Show()
		{
			var entry = FindDirectoryEntry("Kvasov");
			Log(entry);
		}

		[Test]
		public void Block_user()
		{
			using (var user = new TestADUser())
			{
				ADHelper.Block(user.Login);
				Assert.That(ADHelper.IsLocked(user.Login));
			}
		}

		[Test]
		public void IsUserBelongToOfficeContainer()
		{
			using (var user = new TestADUser())
				Assert.That(ADHelper.IsBelongsToOfficeContainer(user.Login), Is.False);

			using (var user = new TestADUser("LDAP://OU=Офис,DC=adc,DC=analit,DC=net"))
				Assert.That(ADHelper.IsBelongsToOfficeContainer(user.Login), Is.True);
		}

		[Test] 
		public void BadPasswordTimeTest()
		{
			using (var user = new TestADUser())
			{
				Assert.That(ADHelper.GetBadPasswordDate(user.Login), Is.Null);	
				try
				{
					var directoryEntity = FindDirectoryEntry(user.Login);
					var directoryEntry = new DirectoryEntry(directoryEntity.Path, user.Login, "1234");
					Console.WriteLine(directoryEntry.NativeObject.ToString());
					Assert.Fail("странно но пароль почему то подошел");
				}
				catch {}
				Assert.That(ADHelper.GetBadPasswordDate(user.Login), Is.GreaterThanOrEqualTo(CreatedAt(user.Login)));	
			}
		}

		[Test]
		public void Enable_disable_test()
		{
			using (var testUser = new TestADUser())
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

	public class TestADUser : IDisposable
	{
		private static DirectoryEntry FindDirectoryEntry(string login)
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

		public TestADUser() : this("LDAP://OU=Пользователи,OU=Клиенты,DC=adc,DC=analit,DC=net")
		{}

		public TestADUser(string container) : this("test" + new Random().Next(), container)
		{}

		public TestADUser(string userName, string container) 
		{
			Login = userName;
			if (IsLoginExists(Login))
				Delete(Login);
			var password = "12345678";
			var root = new DirectoryEntry(container);
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
