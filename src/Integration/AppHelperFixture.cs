﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AdminInterface.Controllers;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.MonoRailExtentions;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Framework.Adapters;
using Castle.MonoRail.Framework.Helpers;
using Castle.MonoRail.Framework.Routing;
using Castle.MonoRail.Framework.Services;
using Castle.MonoRail.Framework.Test;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.MonoRailExtentions;
using NHibernate;
using NUnit.Framework;
using AppHelper = AdminInterface.Helpers.AppHelper;
using DescriptionAttribute=System.ComponentModel.DescriptionAttribute;

namespace Integration
{
	[TestFixture]
	public class AppHelperFixture
	{
		private AppHelper helper;
		private StubEngineContext context;

		private TestFilter filter;
		private RoutingEngine engine;
		private IDictionary propertyBag;

		[SetUp]
		public void Setup()
		{
			helper = new AdminInterface.Helpers.AppHelper();
			var area = "";
			var controllerName = "home";
			var actionName = "index";
			var appPath = "";
			var urlInfo = new UrlInfo(area, controllerName, actionName, appPath, "");
			context = new StubEngineContext(urlInfo) {
				CurrentControllerContext = new ControllerContext(area, controllerName, actionName, null)
			};
			propertyBag = context.CurrentControllerContext.PropertyBag;

			engine = new RoutingEngine();
			engine.Add(new PatternRoute("/<controller>/<action>"));
			engine.Add(new PatternRoute("/<controller>/<id>/[action]"));
			var match = engine.FindMatch("home/index", new RouteContext(context.Request, null, appPath, null));
			context.CurrentControllerContext.RouteMatch = match;

			PrepareHelper(helper);

			filter = new TestFilter();
			propertyBag["filter"] = filter;
		}

		private void PrepareHelper(AbstractHelper helper)
		{
			helper.UrlHelper = new UrlHelper(context) {
				UrlBuilder = new DefaultUrlBuilder {
					RoutingEngine = engine,
					ServerUtil = new StubServerUtility(),
					UseExtensions = false,
				},
			};
			helper.SetContext(context);
			helper.SetController(null, context.CurrentControllerContext);
		}

		[Test]
		public void Link_to_for_lazy()
		{
			using (new SessionScope())
			{
				var user = User.Queryable.Where(u => u.Payer != null).First();
				Assert.That(NHibernateUtil.IsInitialized(user.Payer), Is.False);
				var linkTo = helper.LinkTo(user.Payer);
				Assert.That(linkTo, Is.EqualTo(String.Format(@"<a  href=""/Payers/{0}"">{1}</a>", user.Payer.Id, user.Payer.Name)));
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
			public DateTime Date { get; set; }

			public DatePeriod Period { get; set; }

			public Entity Entity { get; set; }
		}

		public class Entity
		{
			public uint Id { get; set; }

			public string Name { get; set; }
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
		public void Filter_for_period()
		{
			filter.Period = new DatePeriod();
			var result = helper.FilterFor("filter.Period");
			Assert.That(result, Is.StringContaining("calendar"));
		}

		[Test]
		public void Filter_for_text()
		{
			var result = helper.FilterFor("filter.TextField");
			Assert.That(result, Is.EqualTo("<tr><td class='filter-label'>Введите текст для поиска:</td><td colspan=2>"
				+"<input type=\"text\" id=\"filter_TextField\" name=\"filter.TextField\" value=\"\" /></td></tr>"));
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

		[Test]
		public void Build_sortable_url()
		{
			var link = helper.Sortable("test", "test");
			Assert.That(link, Is.EqualTo("<a href='/home/index?SortBy=test&Direction=asc' class='sort_link'>test</a>"));
		}

		[Test]
		public void Build_sortable_url_for_routing()
		{
			engine.Add(new PatternRoute("/<controller>/[id]/<action>").Restrict("id").ValidInteger);
			var match = engine.FindMatch("users/1/edit", new RouteContext(context.Request, null, "/", null));
			context.CurrentControllerContext.RouteMatch = match;

			var link = helper.Sortable("test", "test");
			Assert.That(link, Is.EqualTo("<a href='/users/1/edit?SortBy=test&Direction=asc' class='sort_link'>test</a>"));
		}

		[Test]
		public void Edit_date()
		{
			var edit = helper.Edit("filter.Date");
			Assert.That(edit, Is.EqualTo("<input type=text name='filter.Date' class='required validate-date input-date' value='01.01.0001'><input type=button class=CalendarInput>"));
		}

		[Test]
		public void Select_with_values()
		{
			var edit = helper.Edit("filter.Entity", new List<Entity> {
				new Entity{Id = 1, Name = "Test1"},
				new Entity{Id = 2, Name = "Test2"}
			});
			Assert.That(edit, Is.EqualTo("<select name='filter.Entity.Id'>"
				+ "<option value=1>Test1</option>"
				+ "<option value=2>Test2</option>"
				+ "</select>"));
		}

		public class TestSortable : Sortable, SortableContributor
		{
			public int Filter { get; set; }

			public string GetUri()
			{
				return "filter.Filter=" + Filter;
			}
		}

		[Test]
		public void Sortable_support()
		{
			propertyBag["filter"] = new TestSortable {
				SortBy = "test",
				SortDirection = "asc",
				Filter = 1
			};
			var link = helper.Sortable("test", "test");
			Assert.That(link, Is.EqualTo("<a href='/home/index?filter.Filter=1&filter.SortBy=test&filter.SortDirection=desc' class='sort desc'>test</a>"));
		}

		public class UrlContributor : IUrlContributor
		{
			public string Name { get; set; }
			public IDictionary GetQueryString()
			{
				return new Dictionary<string, string> {
					{"controller", "client"},
					{"action", "index"},
				};
			}
		}

		[Test]
		public void Get_link_from_url_contributor()
		{
			engine.Add(new PatternRoute("/<controller>/[id]/<action>").Restrict("id").ValidInteger);
			var link = helper.LinkTo(new UrlContributor{Name = "bad bad test"});
			Assert.That(link, Is.EqualTo("<a  href=\"/clients/index\">bad bad test</a>"));
		}

		[Test]
		public void Style_link()
		{
			var link = helper.LinkTo(new Address{Enabled = false, Value = "Test"});
			Assert.That(link, Is.EqualTo("<a class=\"DisabledByBilling\"  href=\"/Addresses/0\">Test</a>"));
		}

		[Test]
		public void Link_with_parameters()
		{
			var link = helper.LinkTo(new Address{Enabled = true, Value = "Test"}, "Test", "Index", new Dictionary<string, object>{{"tab", "1"}});
			Assert.That(link, Is.EqualTo("<a  href=\"/Addresses/0/Index?tab=1\">Test</a>"));
		}

		[Test]
		public void Check_for_minified_assets()
		{
			if (!Directory.Exists("Assets"))
				Directory.CreateDirectory("Assets");
			if (!Directory.Exists(@"Assets\Javascripts"))
				Directory.CreateDirectory(@"Assets\Javascripts");
			File.WriteAllText(@"Assets\Javascripts\test.min.js", "");

			var asset = helper.Asset("test.js");
			Assert.That(asset, Is.StringContaining("test.min.js"));
		}

		[Test]
		public void Asset_for_coffee_Script()
		{
			if (!Directory.Exists("Assets"))
				Directory.CreateDirectory("Assets");
			if (!Directory.Exists(@"Assets\Javascripts"))
				Directory.CreateDirectory(@"Assets\Javascripts");

			File.Delete(@"Assets\Javascripts\test.min.js");
			File.WriteAllText(@"Assets\Javascripts\test.js", "");
			File.WriteAllText(@"Assets\Javascripts\test.coffee", "");

			var asset = helper.Asset("test.coffee");
			Assert.That(asset, Is.StringStarting("<script type='text/javascript' src='/Assets/Javascripts/test.js"));
		}
	}
}