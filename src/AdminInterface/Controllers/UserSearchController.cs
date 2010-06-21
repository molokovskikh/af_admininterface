using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using AdminInterface.Helpers;
using AdminInterface.Models.Security;
using AdminInterface.Security;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Helpers;
using System.ComponentModel;
using AdminInterface.Models;

namespace AdminInterface.Controllers
{
	[
		Layout("GeneralWithJQuery"),
		Helper(typeof(BindingHelper)), 
        Helper(typeof(ViewHelper)),
		Helper(typeof(ADHelper)),
		Helper(typeof(LinkHelper)),
		Secure(PermissionType.ViewDrugstore, Required = Required.AnyOf),
		Filter(ExecuteWhen.BeforeAction, typeof(SecurityActivationFilter))
	]
	public class UserSearchController : ARSmartDispatcherController
	{
		private const uint PageSize = 30;

		public void SearchBy([DataBind("SearchBy")] UserSearchProperties searchProperties)
		{
		    var searchResults = UserSearchItem.SearchBy(SecurityContext.Administrator, searchProperties, "UserName", "Ascending");

			if (searchResults.Count.Equals(1) && !String.IsNullOrEmpty(searchResults.First().Login))
			{
				var virtualDir = Context.UrlInfo.AppVirtualDir;
				if (!virtualDir.StartsWith("/"))
					virtualDir = "/" + virtualDir;
				if (virtualDir.EndsWith("/"))
					virtualDir = virtualDir.Remove(virtualDir.Length - 1, 1);
				RedirectToUrl(String.Format(virtualDir + "/Users/{0}/edit", searchResults.First().Login));
			}

			PropertyBag["SearchResults"] = searchResults;
			PropertyBag["FindBy"] = searchProperties;
			PropertyBag["regions"] = Region.GetAllRegions();
		    PropertyBag["SortDirection"] = "Ascending";
			PropertyBag["SortColumnName"] = "UserName";

		    PropertyBag["rowsCount"] = searchResults.Count;
		    PropertyBag["pageSize"] = PageSize;
		    PropertyBag["currentPage"] = 0;
		}

		public void Search()
		{
			var searchProperties = new UserSearchProperties { SearchBy = SearchUserBy.Auto };
			PropertyBag["FindBy"] = searchProperties;
			PropertyBag["regions"] = Region.GetAllRegions();
        }

        public void OrderBy([DataBind("SearchBy")] UserSearchProperties searchProperties,
							string sortDirection,
							string sortColumnName,
                            int rowsCount)
        {
            var searchResults = (List<UserSearchItem>)(UserSearchItem.SearchBy(SecurityContext.Administrator,
				searchProperties, sortColumnName, sortDirection));

            PropertyBag["SearchResults"] = searchResults;
            PropertyBag["FindBy"] = searchProperties;
            PropertyBag["regions"] = Region.GetAllRegions();
            PropertyBag["SortColumnName"] = sortColumnName;
        	PropertyBag["SortDirection"] = sortDirection;
            PropertyBag["rowsCount"] = rowsCount;
            RenderView("SearchBy");
        }
	}
}
