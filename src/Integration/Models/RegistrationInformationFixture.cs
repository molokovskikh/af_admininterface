using System.Linq;
using AdminInterface.Controllers;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.NHibernateExtentions;
using Integration.ForTesting;
using NUnit.Framework;

namespace Integration.Models
{
	[TestFixture]
	public class RegistrationInformationFixture : IntegrationFixture
	{
		[Test]
		public void BaseTest()
		{
			var filter = new UserFinderFilter();
			var criteria = filter.GetCriteria();
			filter.ApplySort(criteria);
			var client = ArHelper.WithSession(s => criteria.GetExecutableCriteria(s).ToList<RegistrationInformation>()).ToList();
		}
	}
}
