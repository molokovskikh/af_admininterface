using System;
using AdminInterface.Models.Security;
using AdminInterface.Models.Telephony;
using Functional.ForTesting;
using NUnit.Framework;
using Test.Support.Web;
using WatiN.Core;

namespace Functional
{
	[TestFixture, Ignore("Временно до починки")]
	public class CallbackRulesFixture : WatinFixture2
	{
		private const string CallbackLinkText = "Правила обратного звонка";

		[TestFixtureSetUp]
		public void SetupFixture()
		{
			var adm = Administrator.GetByName("kvasov");
			if (!adm.HavePermision(PermissionType.ManageCallbacks)) {
				adm.AllowedPermissions.Add(Permission.Find(PermissionType.ManageCallbacks));
				session.Save(adm);
			}

			FixtureMapping
				.For<Callback>()
				.To(c => c.Comment, "Комментарий")
				.To(c => c.CallerPhone, "Номер телефона")
				.To(c => c.DueDate == null ? null : c.DueDate.Value.ToShortDateString(), "Дату блокировки")
				.To(c => As.Checkbox(c.CheckDate), "Проверять дату блокировки")
				.To(c => As.Checkbox(c.Enabled), "Включен");
		}

		[SetUp]
		public void Setup()
		{
			Callback.DeleteAll();
		}

		[Test]
		public void Main_form_should_have_link_to_rejected_emails_where_should_be_emails_for_current_day()
		{
			var callback1 = new Callback { CallerPhone = "4732606000", Comment = "Офис", Enabled = false };
			callback1.Save();
			var callback2 = new Callback { CallerPhone = "9202299222", Comment = "Епихин" };
			callback2.SaveAndFlush();

			using (var browser = new IE(BuildTestUrl("default.aspx"))) {
				ClickLink(CallbackLinkText);
				browser.AssertThatTableContains(callback2, callback1);
			}
		}

		[Test]
		public void On_delete_button_click_call_back_rule_should_be_deleted()
		{
			var callback1 = new Callback { CallerPhone = "4732606000", Comment = "Офис" };
			callback1.Save();
			var callback2 = new Callback { CallerPhone = "9202299222", Comment = "Епихин" };
			callback2.SaveAndFlush();

			using (var browser = new IE(BuildTestUrl("default.aspx"))) {
				ClickLink(CallbackLinkText);
				browser.FindRow(callback1).Link(Find.ByText("Удалить")).Click();
				browser.AssertThatTableContains(callback2);
			}
		}

		[Test]
		public void Update_link_should_enter_to_edit_page()
		{
			var callback1 = new Callback { CallerPhone = "4732606000", Comment = "Офис" };
			callback1.Save();

			using (var browser = new IE(BuildTestUrl("default.aspx"))) {
				ClickLink(CallbackLinkText);
				browser.FindRow(callback1).Link(Find.ByText("Редактировать")).Click();
				browser.Input<Callback>(callback => callback.DueDate, DateTime.Today.AddMonths(-1));
				ClickButton("Сохранить");

				callback1.Refresh();
				Assert.That(callback1.DueDate, Is.EqualTo(DateTime.Today.AddMonths(-1)));
				browser.AssertThatTableContains(callback1);
			}
		}

		[Test]
		public void Enabled_and_check_date_can_ne_updated_in_summary_table()
		{
			var callback1 = new Callback { CallerPhone = "4732606000", Comment = "Офис" };
			callback1.Save();
			var callback2 = new Callback { CallerPhone = "9202299222", Comment = "Епихин" };
			callback2.SaveAndFlush();

			using (var browser = new IE(BuildTestUrl("default.aspx"))) {
				ClickLink(CallbackLinkText);
				browser.FindRow(callback1).Input<Callback[]>(callbacks => callbacks[1].CheckDate, true);
				browser.FindRow(callback2).Input<Callback[]>(callbacks => callbacks[0].Enabled, true);
				ClickButton("Сохранить");

				callback1.Refresh();
				callback2.Refresh();
				Assert.That(callback1.Comment, Is.EqualTo("Офис"));
				Assert.That(callback1.CheckDate, Is.True);
				Assert.That(callback1.Enabled, Is.False);

				Assert.That(callback2.Comment, Is.EqualTo("Епихин"));
				Assert.That(callback2.CheckDate, Is.False);
				Assert.That(callback2.Enabled, Is.True);
				browser.AssertThatTableContains(callback2, callback1);
			}
		}

		[Test]
		public void After_update_should_show_confirm_message()
		{
			var callback = new Callback { CallerPhone = "4732606000", Comment = "Офис" };
			callback.Save();

			using (var browser = new IE(BuildTestUrl("default.aspx"))) {
				ClickLink(CallbackLinkText);
				browser.FindRow(callback).Input<Callback[]>(callbacks => callbacks[0].CheckDate, true);
				ClickButton("Сохранить");

				Assert.That(browser.ContainsText("Обновление успешно завершено."), Is.True, "Нет сообщения об успешном сохранении");
			}
		}
	}
}