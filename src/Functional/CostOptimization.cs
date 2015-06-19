using System;
using System.Linq;
using AdminInterface.Models;
using Common.Web.Ui.NHibernateExtentions;
using Functional.ForTesting;
using NHibernate.Linq;
using NUnit.Framework;
using Test.Support.Selenium;
using Test.Support.Suppliers;

namespace Functional
{
	[TestFixture]
	public class CostOptimization : AdmSeleniumFixture
	{
		[Test]
		public void Create_exclude()
		{
			TestSupplier.CreateNaked(session);
			session.DeleteEach(session.Query<CostOptimizationForbiddenConcurrent>());

			Click("Оптимизация цен - Исключения");
			AssertText("Оптимизация цен - Исключения");
			Select2SelectFirst("#supplier");
			Css("#add-supplier input[type=submit]").Click();
			AssertText("Сохранено");

			var concurrents = session.Query<CostOptimizationForbiddenConcurrent>().ToArray();
			Assert.AreEqual(1, concurrents.Length);
		}

		private void Select2SelectFirst(string css)
		{
			Eval(String.Format("$(\"{0}\").data().select2.open()", css));
			WaitAjax();
			WaitForCss(".select2-result");
			Css(".select2-result").Click();
		}
	}
}