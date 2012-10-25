using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NHibernate;

namespace AdminInterface.ManagerReportsFilters
{
	public interface IFind<T>
	{
		IList<T> Find();
		ISession Session { get; set; }
	}
}