using System;
using System.Collections.Generic;
using System.Linq;
using Castle.ActiveRecord;
using Common.Web.Ui.Helpers;
using NHibernate.Criterion;

namespace AdminInterface.Models.Security
{
	public enum PermissionType
	{
		ViewDrugstore = 1,
		ManageDrugstore = 2,
		RegisterDrugstore = 7,
		DrugstoreInterface = 12,

		ViewSuppliers = 4,
		ManageSuppliers = 5,
		RegisterSupplier = 6,
		SupplierInterface = 11,

		ManageAdministrators = 3,
		MonitorUpdates = 8,
		Billing = 9,
		CopySynonyms = 10,
		ChangePassword = 13,
		RegisterInvisible = 14,
		SendNotification = 15,
		CanRegisterClientWhoWorkForFree = 16,
		ManageCallbacks = 17,
		EditSettings = 19,

		ConfigurerEditProducers = 18,
		CallHistory = 20,
		ChangePayment = 22
	}

	[ActiveRecord(Schema = "accessright", Lazy = false)]
	public class Permission
	{
		[PrimaryKey("Id")]
		public uint Id { get; set; }

		[Property]
		public string Name { get; set; }

		[Property]
		public PermissionType Type { get; set; }

		[Property]
		public string Shortcut { get; set; }

		public static Permission Find(PermissionType permissionType)
		{
			return ActiveRecordMediator<Permission>.FindFirst(Expression.Eq("Type", permissionType));
		}

		public static IList<Permission> FindAll()
		{
			return ActiveRecordMediator<Permission>.FindAll(new [] { Order.Asc("Name") });
		}

		public bool HaveAccessTo(string controller, string action)
		{
			if (controller.ToLower() == "register" && action.ToLower() == "registerpayer")
				return Type == PermissionType.RegisterDrugstore
					|| Type == PermissionType.RegisterSupplier
					|| Type == PermissionType.Billing;

			if (Type == PermissionType.Billing)
			{
				var billingControllers = new [] {
					"payments",
					"invoices",
					"billing",
					"payers",
					"recipients",
					"acts",
					"advertisings",
					"references",
					"accounts",
					"regions"
				};
				return billingControllers.Any(c => c == controller.ToLower());
			}
			if (Type == PermissionType.ViewDrugstore)
			{
				var drugstore = new [] {
					"clients", "users", "addresses", "mails"
				};
				return drugstore.Any(c => c == controller.ToLower());
			}
			if (Type == PermissionType.RegisterSupplier)
			{
				if (controller.ToLower() == "register"
					&& (action.ToLower() == "RegisterSupplier".ToLower() || action.ToLower() == "SearchPayers".ToLower()))
					return true;
			}
			if (Type == PermissionType.ViewSuppliers)
			{
				var controllers = new [] {
					"suppliers",
					"users",
					"mails",
				};
				return controllers.Any(c => c == controller.ToLower());
			}
			if (Type == PermissionType.ViewSuppliers)
			{
				var controllers = new [] {
					"clients",
					"users",
					"addresses",
				};
				return controllers.Any(c => c == controller.ToLower());
			}
			if (Type == PermissionType.RegisterDrugstore)
			{
				if (controller.ToLower() == "register"
					&& (action.ToLower() == "RegisterClient".ToLower()
						|| action.ToLower() == "SearchSuppliers".ToLower()
						|| action.ToLower() == "SearchPayers".ToLower()))
					return true;
			}
			return false;
		}

		public bool IsDefaultFor(string departmentDescription)
		{
			var allExceptProcessingAndBilling = new List<Department> {
				Department.Administration, Department.IT, Department.Support, Department.Manager
			};
			var allExceptProcessing = new List<Department> { Department.Billing };
			allExceptProcessing.AddRange(allExceptProcessingAndBilling);

			var department = Department.Administration;
			foreach (var item in BindingHelper.GetDescriptionsDictionary(typeof (Department)))
			{
				if (departmentDescription.Equals(item.Value))
				{
					department = (Department) Enum.ToObject(typeof (Department), item.Key);
					break;
				}
			}

			switch (Type)
			{
				case PermissionType.ConfigurerEditProducers:
					return (department == Department.Administration);
				case PermissionType.CanRegisterClientWhoWorkForFree:
					return (new List<Department> {
						Department.Administration, 
						Department.Manager
					}).Contains(department);
				case PermissionType.ViewDrugstore:
					return true;
				case PermissionType.ViewSuppliers:
					return true;
				case PermissionType.SendNotification:
					return true;
				case PermissionType.Billing:
					return allExceptProcessing.Contains(department);
				case PermissionType.CallHistory:
					return department == Department.Administration;
				case PermissionType.ChangePassword:
					return allExceptProcessing.Contains(department);
				case PermissionType.CopySynonyms:
					return allExceptProcessing.Contains(department);
				case PermissionType.MonitorUpdates:
					return true;
				case PermissionType.ManageDrugstore:
					return allExceptProcessing.Contains(department);
				case PermissionType.ManageSuppliers:
					return allExceptProcessing.Contains(department);
				case PermissionType.RegisterInvisible:
					return allExceptProcessingAndBilling.Contains(department);
				case PermissionType.RegisterDrugstore:
					return allExceptProcessingAndBilling.Contains(department);
				case PermissionType.RegisterSupplier:
					return allExceptProcessingAndBilling.Contains(department);
				case PermissionType.DrugstoreInterface:
					return allExceptProcessingAndBilling.Contains(department);
				case PermissionType.SupplierInterface:
					return department == Department.Administration;
				case PermissionType.ManageAdministrators:
					return department == Department.Administration;
				case PermissionType.ManageCallbacks:
					return department == Department.Administration;
				case PermissionType.EditSettings:
					return department == Department.Administration;
				default:
					return department == Department.Administration;
			}
		}
	}
}