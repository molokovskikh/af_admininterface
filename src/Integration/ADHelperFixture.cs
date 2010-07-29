using System;
using System.DirectoryServices;
using AdminInterface.Helpers;
using AdminInterface.Models.Security;
using NUnit.Framework;
using System.Collections.Generic;

namespace Integration
{
	[TestFixture]
	[Ignore("не работают т.к. нет административных прав")]
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


		private void LogAll(IEnumerable<DirectoryEntry> entries)
		{
			foreach (var entry in entries)
				Log(entry);
		}

		private static IEnumerable<DirectoryEntry> FindAll(string login)
		{
			using (var searcher = new DirectorySearcher(String.Format(@"(name={0})", login)))
			{
				foreach(SearchResult result in searcher.FindAll())
				{
					yield return result.GetDirectoryEntry();
				}
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
		public void c()
		{
			var entyty = FindDirectoryEntry("KvasovT");
			Log(entyty);
			//Console.WriteLine(entyty.Properties["userAccountControl"][0]);
		}

		[Test]
		public void test()
		{
			var adm = new Administrator();
			adm.UserName = "KvasovT";
			adm.CreateUserInAd("123456789");
		}

		[Test]
		public void Set_logon_hours()
		{
			var hours = new bool[] {
				false, false, false, true, true, true, true, true, true, true, false, false,
				false, false, false, false, true, true, true, true, true, true, true, false,
				false, false, false, false, true, true, true, true, true, true, true, false,
				false, false, false, false, true, true, true, true, true, true, true, false,
				false, false, false, false, true, true, true, true, true, true, false, false,
				false, false, false, false, false, true, true, true, false, false, false, false,
				false, false, false, false, false, false, false, false, false, false, false, false,
			};
			Administrator.SetLogonHours("KvasovT", hours);

			var weekRealHours = ADHelper.GetLogonHours("KvasovT");
			var index = 0;
			for (var i = 0; i < 7; i++)
				for (var j = 0; j < 24; j++)
					if (j % 2 == 0)
						Assert.That(hours[index++], Is.EqualTo(weekRealHours[i, j]));
		}

		[Test]
		public void Add_accessible_computer()
		{
			var computers = new [] { "FMS", "SOLO" };

			var entry = FindDirectoryEntry("KvasovT");
			if (entry.Properties["userWorkstations"].Count > 0)
			{
				entry.Properties["userWorkstations"].Remove(entry.Properties["userWorkstations"][0]);
				entry.CommitChanges();
			}

			var accessibleComputers = ADHelper.GetAccessibleComputers("KvasovT");
			Assert.That(accessibleComputers.Count, Is.EqualTo(0));

			var count = 0;
			foreach (var computer in computers)
			{
				ADHelper.AddAccessibleComputer("KvasovT", computer);
				count++;
				accessibleComputers = ADHelper.GetAccessibleComputers("KvasovT");
				Assert.That(accessibleComputers.Count, Is.EqualTo(count));
				Assert.IsTrue(accessibleComputers.Contains(computer));
			}

			// Пробуем добавить FMS, но он не должен добавиться т.к. доступ к нему уже есть
			ADHelper.AddAccessibleComputer("KvasovT", computers[0]);
			accessibleComputers = ADHelper.GetAccessibleComputers("KvasovT");
			Assert.That(accessibleComputers.Count, Is.EqualTo(count));
		}

		[Test]
		public void Rename_user()
		{
			var testLogin = "KvasovT";
			var newTestLogin = (new Random().Next(30000, 40000)).ToString();

			var user = ADHelper.FindDirectoryEntry(testLogin);
			Assert.IsNotNull(user, "Не найдена запись в AD, хотя должна быть");

			ADHelper.RenameUser(testLogin, newTestLogin);

			// Переименовываем и проверяем что по старому имени ничего не найдем, а по новому найдем
			user = ADHelper.FindDirectoryEntry(testLogin);
			Assert.IsNull(user, "Нашли запись в AD, хотя не должны были");
			user = ADHelper.FindDirectoryEntry(newTestLogin);
			Assert.IsNotNull(user, "Не найдена запись в AD, хотя должна быть");

			// Переименовываем обратно
			ADHelper.RenameUser(newTestLogin, testLogin);
			user = ADHelper.FindDirectoryEntry(testLogin);
			Assert.IsNotNull(user, "Не найдена запись в AD, хотя должна быть");
		}		

		[Test]
		public void Get_last_password_change()
		{
			var begin = DateTime.Now;
			using (var user = new TestADUser())
			{
				ADHelper.ChangePassword(user.Login, "123456789");
				Assert.That(ADHelper.GetLastPasswordChange(user.Login), Is.GreaterThanOrEqualTo(begin));
			}
		}

		[Test]
		public void If_login_exists_but_not_belog_to_admin_group_than_add_user_to_admin_group()
		{
			using (var user = new TestADUser())
			{
				var adm = new Administrator { UserName = user.Login, ManagerName = "test" };
				adm.CreateUserInAd("123456789");
				Assert.That(TestADUser.IsMemberOf(adm.UserName,
				                                  new DirectoryEntry("LDAP://CN=Региональные администраторы,OU=Группы,OU=Клиенты,DC=adc,DC=analit,DC=net")),
				            Is.True);
			}
		}

		[Test]
		public void Create_login_if_not_exists()
		{
			var login = "test" + new Random().Next();
			try
			{
				var adm = new Administrator { UserName = login, ManagerName = "test" };
				adm.CreateUserInAd("123456789");
				Assert.That(TestADUser.IsLoginExists(login));
				Assert.That(TestADUser.IsMemberOf(login,
				                                  new DirectoryEntry("LDAP://CN=Региональные администраторы,OU=Группы,OU=Клиенты,DC=adc,DC=analit,DC=net")),
							Is.True);
			}
			catch (Exception)
			{
				TestADUser.Delete(login);
				throw;
			}
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
			entry = new DirectoryEntry(entry.Path);
			entry.DeleteTree();
			entry.CommitChanges();
		}

		public static bool IsMemberOf(string login, DirectoryEntry entry)
		{
			var loginEntry = FindDirectoryEntry(login);
			foreach (var property in loginEntry.Properties["memberOf"])
			{
				if (property.ToString() == entry.Path.Replace("LDAP://", ""))
					return true;
			}
			return false;
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
