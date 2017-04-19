using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using AdminInterface.Models;
using NHibernate;
using NHibernate.Linq;

namespace AdminInterface.ViewModels
{
	public class MarkupsSynchronizationView
	{
		[Description("Синхронизация наценок")]
		public virtual bool ViewMarkupsSynchronization { get; set; }

		public virtual List<MarkupGlobalConfig> UpdateSynchronizationIfNeeds(ISession dbsession, Client client)
		{
			List<MarkupGlobalConfig> result = null;
			if (client.MarkupsSynchronization == false && ViewMarkupsSynchronization &&
				dbsession.Query<MarkupGlobalConfig>().Count(s => s.Client.Id == client.Id) == 0) {
				result = MarkupGlobalConfig.Defaults(client);
			}
			if (client.MarkupsSynchronization != ViewMarkupsSynchronization) {
				client.MarkupsSynchronization = ViewMarkupsSynchronization;
			}
			return result;
		}
	}
}