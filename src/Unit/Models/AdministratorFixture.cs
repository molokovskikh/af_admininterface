using System.Collections.Generic;
using AdminInterface.Models;
using AdminInterface.Models.Security;
using AdminInterface.Models.Suppliers;
using AdminInterface.Security;
using Common.Web.Ui.Models;
using NUnit.Framework;

namespace Unit.Models
{
	[TestFixture]
	public class AdministratorFixture
	{
		private Administrator _adm;

		[SetUp]
		public void Setup()
		{
			_adm = new Administrator {
				AllowedPermissions = new List<Permission>(),
			};
		}

		[Test]
		public void ClientTypeFilterTest()
		{
			var adm = new Administrator {
				AllowedPermissions = new List<Permission> { new Permission { Type = PermissionType.ViewDrugstore } }
			};
			Assert.That(adm.GetClientFilterByType("cd"), Is.EqualTo(" and cd.FirmType = 1 "));

			adm.AllowedPermissions.Clear();

			adm.AllowedPermissions.Add(new Permission { Type = PermissionType.ViewSuppliers });
			Assert.That(adm.GetClientFilterByType("cd"), Is.EqualTo(" and cd.FirmType = 0 "));

			adm.AllowedPermissions.Add(new Permission { Type = PermissionType.ViewDrugstore });
			Assert.That(adm.GetClientFilterByType("cd"), Is.EqualTo(" "));
		}

		[Test]
		public void Do_not_create_alias_if_not_set()
		{
			var adm = new Administrator {
				AllowedPermissions = new List<Permission> { new Permission { Type = PermissionType.ViewDrugstore } }
			};
			Assert.That(adm.GetClientFilterByType(null), Is.EqualTo(" and FirmType = 1 "));

			adm.AllowedPermissions.Clear();

			adm.AllowedPermissions.Add(new Permission { Type = PermissionType.ViewSuppliers });
			Assert.That(adm.GetClientFilterByType(null), Is.EqualTo(" and FirmType = 0 "));
		}

		[Test]
		public void Throw_exception_if_nothind_allow()
		{
			var adm = new Administrator {
				AllowedPermissions = new List<Permission>()
			};
			Assert.That(() => adm.GetClientFilterByType("cd"),
				Throws.InstanceOf<NotHavePermissionException>());
		}

		[Test]
		public void CheckPermisionsTest()
		{
			var adm = new Administrator {
				AllowedPermissions = new List<Permission> {
					new Permission { Type = PermissionType.Billing },
					new Permission { Type = PermissionType.ManageAdministrators },
				}
			};
			adm.CheckPermisions(PermissionType.Billing, PermissionType.ManageAdministrators);
		}

		[Test]
		public void CheckFailPermisions()
		{
			var adm = new Administrator {
				AllowedPermissions = new List<Permission> {
					new Permission { Type = PermissionType.Billing },
				}
			};
			Assert.That(() => adm.CheckPermisions(PermissionType.Billing, PermissionType.ManageAdministrators),
				Throws.InstanceOf<NotHavePermissionException>());
		}

		[Test]
		public void CheckAnyOfPermissions()
		{
			var adm = new Administrator {
				AllowedPermissions = new List<Permission> {
					new Permission { Type = PermissionType.Billing },
				}
			};
			adm.CheckAnyOfPermissions(PermissionType.Billing, PermissionType.ManageAdministrators);
		}

		[Test]
		public void CheckFailAnyOfPermissions()
		{
			var adm = new Administrator {
				AllowedPermissions = new List<Permission>()
			};
			Assert.That(() => adm.CheckAnyOfPermissions(PermissionType.Billing, PermissionType.ManageAdministrators),
				Throws.InstanceOf<NotHavePermissionException>());
		}

		[Test]
		public void FailCheckForDrugstorePermissionForViewClient()
		{
			_adm.AllowedPermissions.Add(new Permission { Type = PermissionType.ViewSuppliers });
			Assert.That(() => _adm.CheckType(ServiceType.Drugstore),
				Throws.InstanceOf<NotHavePermissionException>());
		}

		[Test]
		public void FailCheckForSupplierPermissionForViewClient()
		{
			_adm.AllowedPermissions.Add(new Permission { Type = PermissionType.ViewDrugstore });

			Assert.That(() => _adm.CheckType(ServiceType.Supplier),
				Throws.InstanceOf<NotHavePermissionException>());
		}

		[Test]
		public void FailCheckClientHomeRegion()
		{
			_adm.RegionMask = 1;
			Assert.That(() => _adm.CheckRegion(2),
				Throws.InstanceOf<NotHavePermissionException>());
		}

		[Test]
		public void CheckPermissionForViewClient()
		{
			_adm.RegionMask = 1;
			_adm.CheckRegion(1);
		}

		[Test]
		public void CheckForSupplierPermissionForViewClient()
		{
			_adm.AllowedPermissions.Add(new Permission { Type = PermissionType.ViewSuppliers });
			_adm.CheckType(ServiceType.Supplier);
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
			_adm.AllowedPermissions.Add(new Permission { Type = PermissionType.ChangePassword });
			Assert.That(_adm.AlowChangePassword, Is.True);
		}

		[Test]
		public void CanRegisterClientWhoWorkForFree()
		{
			Assert.That(_adm.CanRegisterClientWhoWorkForFree, Is.False);
			_adm.AllowedPermissions.Add(new Permission { Type = PermissionType.CanRegisterClientWhoWorkForFree });
			Assert.That(_adm.CanRegisterClientWhoWorkForFree, Is.True);
		}

		[Test]
		public void Check_client_permission()
		{
			var client = new Client {
				HomeRegion = new Region { Id = 1 },
				Type = ServiceType.Drugstore,
			};
			_adm.RegionMask = 1;
			_adm.AllowedPermissions.Add(new Permission { Type = PermissionType.ViewDrugstore });
			_adm.CheckClientPermission(client);
		}

		[Test]
		public void In_client_check_home_region_must_be_checked()
		{
			var client = new Client {
				HomeRegion = new Region { Id = 1 },
				Type = ServiceType.Drugstore,
			};
			_adm.AllowedPermissions.Add(new Permission { Type = PermissionType.ViewDrugstore });
			Assert.Throws<NotHavePermissionException>(() => _adm.CheckClientPermission(client));
		}

		[Test]
		public void In_client_check_client_type_must_be_checked()
		{
			var client = new Client {
				HomeRegion = new Region { Id = 1 },
				Type = ServiceType.Drugstore,
			};
			_adm.RegionMask = 1;
			Assert.Throws<NotHavePermissionException>(() => _adm.CheckClientPermission(client));
		}
	}
}