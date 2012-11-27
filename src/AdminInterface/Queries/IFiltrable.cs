using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NHibernate;

namespace AdminInterface.ManagerReportsFilters
{
	public interface IFiltrable<TItem>
	{
		IList<TItem> Find();
		ISession Session { get; set; }
		bool LoadDefault { get; set; }
	}
}