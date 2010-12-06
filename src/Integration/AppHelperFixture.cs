using System;
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
		[Test]
		public void Link_to()
		{
			var helper = new AppHelper();
			var urlInfo = new UrlInfo("test", "test", "test", "/", "");
			helper.SetContext(new StubEngineContext(urlInfo));
			using (new SessionScope())
			{
				var user = User.Queryable.Take(10).First();
				Assert.That(NHibernateUtil.IsInitialized(user.Client), Is.False);
				var linkTo = helper.LinkTo(user.Client);
				Assert.That(linkTo, Is.EqualTo(String.Format(@"<a class='' href='/Clients/{0}'>{1}</a>", user.Client.Id, user.Client.Name)));
			}
		}
	}
}