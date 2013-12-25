using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using AdminInterface.MonoRailExtentions;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Castle.Components.Validator;
using Castle.Core.Smtp;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Framework.Routing;
using Castle.MonoRail.Framework.Services;
using Castle.MonoRail.Framework.Test;
using Castle.MonoRail.TestSupport;
using Common.Tools;
using Common.Web.Ui.Controllers;
using Common.Web.Ui.MonoRailExtentions;
using NHibernate;
using NUnit.Framework;
using Rhino.Mocks;
using Test.Support;

namespace Integration.ForTesting
{
	public class ControllerFixture : Common.Web.Ui.Test.Controllers.ControllerFixture
	{
		[SetUp]
		public void Setup()
		{
			ForTest.InitializeMailer();
		}
	}
}