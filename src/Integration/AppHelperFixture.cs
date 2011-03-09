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
using DescriptionAttribute=System.ComponentModel.DescriptionAttribute;

namespace Integration
{
	[TestFixture]
	public class AppHelperFixture
	{
		private AppHelper helper;
		private StubEngineContext context;

		private TestFilter filter;

		[SetUp]
		public void Setup()
		{
			helper = new AppHelper();
			var urlInfo = new UrlInfo("test", "test", "test", "/", "");
			context = new StubEngineContext(urlInfo);
			context.CurrentControllerContext = new ControllerContext();
			helper.SetContext(context);
			helper.SetController(null, context.CurrentControllerContext);

			filter = new TestFilter();
			context.CurrentControllerContext.PropertyBag["filter"] = filter;
		}

		[Test]
		public void Link_to_for_lazy()
		{
			using (new SessionScope())
			{
				var user = User.Queryable.Take(10).First();
				Assert.That(NHibernateUtil.IsInitialized(user.Client), Is.False);
				var linkTo = helper.LinkTo(user.Client);
				Assert.That(linkTo, Is.EqualTo(String.Format(@"<a class='' href='/Client/{0}'>{1}</a>", user.Client.Id, user.Client.Name)));
			}
		}

		public enum TestEnum
		{
			[Description("Value1")] Value1,
			[Description("Value2")] Value2
		}

		public class TestFilter
		{
			public bool SomeBool { get; set; }
			public string TextField { get; set; }
			public Multivalue Multivalue { get; set; }
			public TestEnum Enum { get; set; }
		}

		public class Multivalue
		{
			public uint Id { get; set; }
			public string Name { get; set; }
			public static IList<Multivalue> All()
			{
				return new List<Multivalue> {
					new Multivalue {
						Id = 1,
						Name = "test"
					}
				};
			}
		}

		[Test]
		public void Checkbox_edit()
		{
			var result = helper.FilterFor("Тест", "filter.SomeBool");
			Assert.That(result, Is.StringContaining("<tr><td class='filter-label'>Тест</td><td colspan=2><input type=\"checkbox\" id=\"filter_SomeBool\" name=\"filter.SomeBool\" value=\"true\" /><input type=\"hidden\" id=\"filter_SomeBoolH\" name=\"filter.SomeBool\" value=\"false\" /></td></tr>"));
		}

		[Test]
		public void Edit_for_text()
		{
			var result = helper.Edit("filter.TextField");
			Assert.That(result, Is.EqualTo("<input type=\"text\" id=\"filter_TextField\" name=\"filter.TextField\" value=\"\" />"));
		}

		[Test]
		public void Selected_value_for_enum()
		{
			filter.Enum = TestEnum.Value2;

			var result = helper.FilterFor("filter.Enum");
			Assert.That(result, Is.EqualTo("<tr><td class='filter-label'></td><td colspan=2>"
				+ "<select name='filter.Enum'>"
				+ "<option value=0>Value1</option>"
				+ "<option value=1 selected>Value2</option>"
				+ "</select>"
				+ "</td></tr>"));
		}

		[Test]
		public void Select_edit()
		{
			var result = helper.FilterFor("filter.Multivalue");
			Assert.That(result, Is.EqualTo("<tr><td class='filter-label'></td><td colspan=2>"
				+ "<select name='filter.Multivalue.Id'>"
				+ "<option>Все</option><option value=1>test</option></select>" 
				+ "</td></tr>"));
		}

		[Test]
		public void Return_null_if_description_not_found()
		{
			Assert.That(helper.GetLabel("filter.TextField"), Is.Null);
		}
	}
}