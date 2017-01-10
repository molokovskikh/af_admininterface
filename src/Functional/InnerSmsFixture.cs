using System;
using System.Collections.Generic;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Security;
using Common.Tools;
using Common.Web.Ui.Models;
using Common.Web.Ui.NHibernateExtentions;
using Functional.ForTesting;
using NHibernate.Linq;
using NUnit.Framework;
using WatiN.Core;

namespace Functional
{
	[TestFixture]
	public class InnerSmsFixture : FunctionalFixture
	{

		[Test(Description = "Добавление нового смс-сообщения")]
		public void AddNewSmsMessage()
		{
			const string newNumber = "0123456789";
			const string newText = "Hallo world!";
			var currentUser = session.Query<Administrator>().FirstOrDefault();
			var admin = new Administrator
			{
				UserName = "admin" + new Random().Next(),
				ManagerName = " 1 Тестовый администратор",
				Email = "kvasovtest@analit.net",
				PhoneSupport = "4732-606000",
			};
			admin.RegionMask = ulong.MaxValue;
			admin.AllowedPermissions = Enum.GetValues(typeof(PermissionType))
				.Cast<PermissionType>()
				.Select(p => Permission.Find(p))
				.ToList();
			Save(admin);

			var messagesToDelete = session.Query<InnerSmsMessage>().ToList();
			session.DeleteEach(messagesToDelete);

			Open("SmsSender");
			AssertText(admin.Name);
			AssertText(admin.UserName);
			GlobalBrowser.Buttons.FirstOrDefault(s => s.Title == $"{admin.Name} ( {admin.UserName} )").Click();
			AssertText("Сообщений нет");
			ClickButton("Новое sms-сообщение");

			AssertText("Новое сообщение");
			AssertText("Получатель:"+ admin.Name.Trim());
			AssertText("Отправитель:" + currentUser.Name.Trim());

			browser.TextField(Find.ByName("address")).TypeText(newNumber);
			browser.TextField(Find.ByName("message")).TypeText(newText);

			ClickButton("Отправить");

			AssertText("режим дебага, отправка сообщений отключена");

			var newMessagesList = session.Query<InnerSmsMessage>().ToList();
			Assert.IsTrue(newMessagesList.Count == 1);
			Assert.IsTrue(newMessagesList[0].UserTo.Id == admin.Id);
			Assert.IsTrue(newMessagesList[0].UserFrom.Id == currentUser.Id);
			Assert.IsTrue(newMessagesList[0].Date.Date == SystemTime.Now().Date);
			Assert.IsTrue(newMessagesList[0].SmsIdFormat == "не отправлено");
			Assert.IsTrue(newMessagesList[0].TargetAddress == newNumber);
			Assert.IsTrue(newMessagesList[0].Message == newText);

			GlobalBrowser.Buttons.FirstOrDefault(s => s.Title == $"{newMessagesList[0].Id}").Click();

			AssertText($"Информация о сообщении № {newMessagesList[0].Id}");
			AssertText(admin.Name.Trim());
			AssertText(currentUser.Name.Trim());
			AssertText(SystemTime.Now().Date.ToShortDateString());
			AssertText("не отправлено");
			AssertText(newNumber);
			AssertText(newText);
			session.Delete(admin);
			session.Flush();
		}
	}
}