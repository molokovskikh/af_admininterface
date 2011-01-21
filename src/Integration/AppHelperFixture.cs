using System;
using System.Collections.Generic;
using System.Linq;
using AdminInterface.Helpers;
using AdminInterface.Models;
using Castle.ActiveRecord;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Framework.Adapters;
using Castle.MonoRail.Framework.Test;
using NHibernate;
using NUnit.Framework;

namespace Integration
{
	[TestFixture]
	public class AppHelperFixture
	{
		private AppHelper helper;
		private StubEngineContext context;

		[SetUp]
		public void Setup()
		{
			helper = new AppHelper();
			var urlInfo = new UrlInfo("test", "test", "test", "/", "");
			context = new StubEngineContext(urlInfo);
			context.CurrentControllerContext = new ControllerContext();
			helper.SetContext(context);
			helper.SetController(null, context.CurrentControllerContext);
		}

		[Test]
		public void Link_to_for_lazy()
		{
			using (new SessionScope())
			{
				var user = User.Queryable.Take(10).First();
				Assert.That(NHibernateUtil.IsInitialized(user.Client), Is.False);
				var linkTo = helper.LinkTo(user.Client);
				Assert.That(linkTo, Is.EqualTo(String.Format(@"<a class='' href='/Clients/{0}'>{1}</a>", user.Client.Id, user.Client.Name)));
			}
		}

		public class TestFilter
		{
			public bool SomeBool { get; set; }
			public string TextField { get; set; }
		}

		[Test]
		public void Checkbox_edit()
		{
			context.CurrentControllerContext.PropertyBag["filter"] = new TestFilter();

			var result = helper.FilterFor("Тест", "filter.SomeBool");
			Assert.That(result, Is.StringContaining("<tr><td class='filter-label'>Тест</td><td colspan=2><input type=\"checkbox\" id=\"filter_SomeBool\" name=\"filter.SomeBool\" value=\"true\" /><input type=\"hidden\" id=\"filter_SomeBoolH\" name=\"filter.SomeBool\" value=\"false\" /></td></tr>"));
		}

		[Test]
		public void Edit_for_text()
		{
			context.CurrentControllerContext.PropertyBag["filter"] = new TestFilter();

			var result = helper.Edit("filter.TextField");
			Assert.That(result, Is.EqualTo("<input type=\"text\" id=\"filter_TextField\" name=\"filter.TextField\" value=\"\" />"));
		}
	}
}