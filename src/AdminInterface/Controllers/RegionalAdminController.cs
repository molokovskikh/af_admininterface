using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using AdminInterface.Mailers;
using AdminInterface.Models;
using AdminInterface.MonoRailExtentions;
using AdminInterface.Security;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using AdminInterface.Models.Security;
using AdminInterface.Helpers;
using Castle.ActiveRecord;
using Common.Tools;
using Common.Web.Ui.Helpers;
using System.Collections;
using Common.Web.Ui.Models;

namespace AdminInterface.Controllers
{
	[
		Helper(typeof(BindingHelper)),
		Helper(typeof(ViewHelper)),
		Secure(PermissionType.ManageAdministrators)
	]
	public class RegionalAdminController : AdminInterfaceController
	{
		private string[] _weekDays = new[] { "Понедельник", "Вторник", "Среда", "Четверг", "Пятница", "Суббота", "Воскресенье" };

		private int[] _defaultLogonHours = new[] {
			0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0,
			0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0,
			0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0,
			0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0,
			0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		};

		[AccessibleThrough(Verb.Get)]
		public void Add()
		{
			var regions = Region.FindAll().OrderBy(region => region.Name).ToArray();
			PropertyBag["administrator"] = new Administrator();
			PropertyBag["permissions"] = Permission.FindAll();
			PropertyBag["regions"] = regions;
			PropertyBag["days"] = _weekDays;
			PropertyBag["logonHours"] = _defaultLogonHours;
		}

		[AccessibleThrough(Verb.Post)]
		public void Add([DataBind("administrator")] Administrator administrator,
			[DataBind("accessibleRegions")] RegionSettings[] accessibleRegions,
			[DataBind("logonHours")] bool[] weekLogonHours)
		{
			var admin = new Administrator {
				UserName = administrator.UserName,
				Department = administrator.Department,
				Email = String.IsNullOrEmpty(administrator.Email) ? String.Empty : administrator.Email,
				InternalPhone = administrator.InternalPhone,
				ManagerName = administrator.ManagerName,
				PhoneSupport = administrator.PhoneSupport,
				AllowedPermissions = new List<Permission>(),
			};
			if (administrator.AllowedPermissions != null)
				foreach (var permission in administrator.AllowedPermissions)
					admin.AllowedPermissions.Add(permission);
			var countAccessibleRegions = 0;
			foreach (var region in accessibleRegions) {
				if (region.IsAvaliableForBrowse) {
					admin.RegionMask |= Convert.ToUInt64(region.Id);
					countAccessibleRegions++;
				}
				else
					admin.RegionMask &= ~Convert.ToUInt64(region.Id);
			}
			if (countAccessibleRegions == accessibleRegions.Count())
				admin.RegionMask = UInt64.MaxValue;
			DbSession.Save(admin);
			var isLoginCreated = CreateUserInAD(admin);

			UpdateAd(administrator, weekLogonHours);

			Mailer.RegionalAdminCreated(admin);

			if (isLoginCreated)
				RedirectToUrl("~/OfficeUserRegistrationReport.aspx");
			else
				RedirectToUrl("~/ViewAdministrators.aspx");
		}

		public void Edit(uint id)
		{
			var regions = Region.FindAll().OrderBy(region => region.Name).ToArray();
			var admin = Administrator.GetById(id);
			PropertyBag["administrator"] = admin;
			PropertyBag["permissions"] = Permission.FindAll();
			PropertyBag["regions"] = regions;
			PropertyBag["days"] = _weekDays;

			var logonHours = new List<bool>();
			foreach (var hour in Storage.GetLogonHours(admin.UserName))
				logonHours.Add(hour);
			PropertyBag["logonHours"] = logonHours;
		}

		public void Update([ARDataBind("administrator", AutoLoad = AutoLoadBehavior.NullIfInvalidKey)] Administrator administrator,
			[DataBind("accessibleRegions")] RegionSettings[] accessibleRegions,
			[DataBind("logonHours")] bool[] weekLogonHours)
		{
			var countAccessibleRegions = 0;
			foreach (var region in accessibleRegions) {
				if (region.IsAvaliableForBrowse) {
					administrator.RegionMask |= Convert.ToUInt64(region.Id);
					countAccessibleRegions++;
				}
				else
					administrator.RegionMask &= ~Convert.ToUInt64(region.Id);
			}
			if (countAccessibleRegions == accessibleRegions.Count())
				administrator.RegionMask = UInt64.MaxValue;

			DbSession.Update(administrator);
			UpdateAd(administrator, weekLogonHours);
			Notify("Сохранено");
			RedirectUsingRoute("RegionalAdmin", "Edit", new { id = administrator.Id });
		}

		private void UpdateAd(Administrator administrator, bool[] weekLogonHours)
		{
			Administrator.SetLogonHours(administrator.UserName, weekLogonHours);
		}

		public void GetDefaultPermissions(string departmentDescription)
		{
			CancelLayout();
			CancelView();

			var responseString = String.Empty;
			foreach (var permission in Permission.FindAll()) {
				if (permission.IsDefaultFor(departmentDescription))
					responseString += String.Format("{0},", permission.Id);
			}
			responseString = responseString.Remove(responseString.Length - 1);
			var bytes = new ASCIIEncoding().GetBytes(responseString);
			//отдаем строку, в которой через запятую указаны идентификаторы тех прав доступа,
			//которые назначаются по умолчанию для нанного подразделения
			Response.OutputStream.Write(bytes, 0, bytes.Length);
		}

		private bool CreateUserInAD(Administrator administrator)
		{
			var isExist = Storage.IsLoginExists(administrator.UserName);
			if (isExist)
				return false;

			var password = UserCommon.GeneratePassword();
			var isLoginCreated = Storage.CreateAdmin(administrator.UserName, administrator.ManagerName, password);

			if (!isLoginCreated)
				return false;
			Session["Password"] = password;
			Session["FIO"] = administrator.ManagerName;
			Session["Login"] = administrator.UserName;

			return true;
		}
	}
}