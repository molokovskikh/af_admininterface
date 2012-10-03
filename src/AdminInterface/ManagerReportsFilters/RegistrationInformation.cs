using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using AdminInterface.Controllers;
using AdminInterface.Models;
using AdminInterface.Models.Suppliers;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.MonoRailExtentions;

namespace AdminInterface.ManagerReportsFilters
{
	public enum RegistrationFinderType
	{
		[Description("Пользователям")] Users,
		[Description("Адресам")] Addresses
	}

	public class RegistrationInformation : IUrlContributor
	{
		public uint Id { get; set; }

		private string _name;

		public string Name
		{
			get
			{
				if (_name == null)
					return Id.ToString();
				return _name;
			}
			set { _name = value; }
		}
		public bool IsUpdate { get; set; }
		public DateTime RegistrationDate { get; set; }
		public bool AdressEnabled { get; set; }
		public ClientStatus ClientEnabled { get; set; }
		public bool UserEnabled { get; set; }
		public bool ServiceDisabled { get; set; }

		public uint ClientId { get; set; }
		public string ClientName { get; set; }

		public int UserCount { get; set; }
		public string UserNames { get; set; }
		public string RegionName { get; set; }

		public ServiceType ClientType { get; set; }

		public RegistrationFinderType ObjectType;

		[Style]
		public bool DisabledByBilling
		{
			get
			{
				return (ObjectType == RegistrationFinderType.Users && (!UserEnabled || ServiceDisabled)) ||
					(ObjectType == RegistrationFinderType.Addresses && (!AdressEnabled || ClientEnabled == ClientStatus.Off));
			}
		}

		[Style]
		public bool SingleUser
		{
			get { return UserCount == 1; }
		}

		public IDictionary GetQueryString()
		{
			return new Dictionary<string, string> {
				{ "controller", ObjectType.ToString() },
				{ "action", "edit" },
				{ "id", Id.ToString() },
			};
		}

		public bool IsDrugstore
		{
			get { return ClientType == ServiceType.Drugstore; }
		}
	}
}