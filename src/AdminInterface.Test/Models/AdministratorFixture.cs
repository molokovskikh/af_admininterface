using System.Collections.Generic;
using AdminInterface.Filters;
using AdminInterface.Models;
using Common.Web.Ui.Models;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace AdminInterface.Test.Models
{
	[TestFixture]
	public class AdministratorFixture
	{
		private Administrator _adm;

		[SetUp]
		public void Setup()
		{
			_adm = new Administrator
			       	{
			       		AllowedPermissions = new List<Permission>(),
			       	};
		}

		[Test]
		public void ClientTypeFilterTest()
		{
			var adm = new Administrator
			          	{
			          		AllowedPermissions = new List<Permission> { new Permission { Type = PermissionType.ViewDrugstore } }
			          	};
			Assert.That(adm.ClientTypeFilter("cd"), Is.EqualTo(" and cd.FirmType = 1 "));

			adm.AllowedPermissions.Clear();

			adm.AllowedPermissions.Add(new Permission { Type = PermissionType.ViewSuppliers });
			Assert.That(adm.ClientTypeFilter("cd"), Is.EqualTo(" and cd.FirmType = 0 "));

			adm.AllowedPermissions.Add(new Permission { Type = PermissionType.ViewDrugstore });
			Assert.That(adm.ClientTypeFilter("cd"), Is.EqualTo(" "));
		}

		[Test]
		public void Do_not_create_alias_if_not_set()
		{
			var adm = new Administrator
			{
				AllowedPermissions = new List<Permission> { new Permission { Type = PermissionType.ViewDrugstore } }
			};
			Assert.That(adm.ClientTypeFilter(null), Is.EqualTo(" and FirmType = 1 "));

			adm.AllowedPermissions.Clear();

			adm.AllowedPermissions.Add(new Permission { Type = PermissionType.ViewSuppliers });
			Assert.That(adm.ClientTypeFilter(null), Is.EqualTo(" and FirmType = 0 "));
		}

		[Test]
		[ExpectedException(ExceptionType = typeof(NotHavePermissionException))]
		public void Throw_exception_if_nothind_allow()
		{
			var adm = new Administrator
			          	{
			          		AllowedPermissions = new List<Permission>()
			          	};
			adm.ClientTypeFilter("cd");
		}

		[Test]
		public void CheckPermisionsTest()
		{
			var adm = new Administrator
			           	{
							AllowedPermissions = new List<Permission>
							                     	{
							                     		new Permission { Type = PermissionType.BillingPermision },
														new Permission { Type = PermissionType.ManageAdministrators },
							                     	}
			           	};
			adm.CheckPermisions(PermissionType.BillingPermision, PermissionType.ManageAdministrators);
		}

		[Test]
		[ExpectedException(ExceptionType = typeof(NotHavePermissionException))]
		public void CheckFailPermisions()
		{
			var adm = new Administrator
			{
				AllowedPermissions = new List<Permission>
							                     	{
							                     		new Permission { Type = PermissionType.BillingPermision },
							                     	}
			};
			adm.CheckPermisions(PermissionType.BillingPermision, PermissionType.ManageAdministrators);
		}

		[Test]
		public void CheckAnyOfPermissions()
		{
			var adm = new Administrator
			          	{
			          		AllowedPermissions = new List<Permission>
			          		                     	{
			          		                     		new Permission { Type = PermissionType.BillingPermision },
			          		                     	}
			          	};
			adm.CheckAnyOfPermissions(PermissionType.BillingPermision, PermissionType.ManageAdministrators);
		}

		[Test]
		[ExpectedException(ExceptionType = typeof(NotHavePermissionException))]
		public void CheckFailAnyOfPermissions()
		{
			var adm = new Administrator
			{
				AllowedPermissions = new List<Permission>()
			};
			adm.CheckAnyOfPermissions(PermissionType.BillingPermision, PermissionType.ManageAdministrators);
		}

		[Test]
		[ExpectedException(ExceptionType = typeof(NotHavePermissionException))]
		public void FailCheckForDrugstorePermissionForViewClient()
		{
			_adm.AllowedPermissions.Add(new Permission { Type = PermissionType.ViewSuppliers });
			_adm.CheckClientType(ClientType.Drugstore);
		}

		[Test]
		[ExpectedException(ExceptionType = typeof(NotHavePermissionException))]
		public void FailCheckForSupplierPermissionForViewClient()
		{
			_adm.AllowedPermissions.Add(new Permission { Type = PermissionType.ViewDrugstore });
			_adm.CheckClientType(ClientType.Supplier);
		}

		[Test]
		[ExpectedException(ExceptionType = typeof(NotHavePermissionException))]
		public void FailCheckClientHomeRegion()
		{
			_adm.RegionMask = 1;
			_adm.CheckClientHomeRegion(2);
		}

		[Test]
		public void CheckPermissionForViewClient()
		{
			_adm.RegionMask = 1;
			_adm.CheckClientHomeRegion(1);
		}

		[Test]
		public void CheckForSupplierPermissionForViewClient()
		{
			_adm.AllowedPermissions.Add(new Permission { Type = PermissionType.ViewSuppliers });
			_adm.CheckClientType(ClientType.Supplier);
		}

		[Test]
		public void HavePermissions()
		{
			Assert.That(_adm.HavePermisions(PermissionType.ManageDrugstore), Is.False);
			_adm.AllowedPermissions.Add(new Permission { Type = PermissionType.ManageDrugstore });
			Assert.That(_adm.HavePermisions(PermissionType.ManageDrugstore), Is.True);

			Assert.That(_adm.HavePermisions(PermissionType.ManageDrugstore, PermissionType.RegisterDrugstore), Is.False);
			_adm.AllowedPermissions.Add(new Permission { Type = PermissionType.RegisterDrugstore });
			Assert.That(_adm.HavePermisions(PermissionType.ManageDrugstore, PermissionType.RegisterDrugstore), Is.True);
		}

		[Test]
		public void HaveAnyOfPermissions()
		{
			Assert.That(_adm.HaveAnyOfPermissions(PermissionType.ManageDrugstore), Is.False);
			_adm.AllowedPermissions.Add(new Permission { Type = PermissionType.ManageDrugstore });
			Assert.That(_adm.HaveAnyOfPermissions(PermissionType.ManageDrugstore), Is.True);

			Assert.That(_adm.HaveAnyOfPermissions(PermissionType.ManageDrugstore, PermissionType.RegisterDrugstore), Is.True);
			_adm.AllowedPermissions.Add(new Permission { Type = PermissionType.RegisterDrugstore });
			Assert.That(_adm.HaveAnyOfPermissions(PermissionType.ManageDrugstore, PermissionType.RegisterDrugstore), Is.True);
		}

		[Test]
		public void AllowChangePassword()
		{
			Assert.That(_adm.AlowChangePassword, Is.False);
			_adm.AllowedPermissions.Add(new Permission {Type = PermissionType.ChangePassword });
			Assert.That(_adm.AlowChangePassword, Is.True);
		}
	}
}
