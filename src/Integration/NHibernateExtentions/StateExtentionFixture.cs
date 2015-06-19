using AdminInterface.Models;
using AdminInterface.NHibernateExtentions;
using Castle.ActiveRecord;
using Common.Web.Ui.NHibernateExtentions;
using Integration.ForTesting;
using NUnit.Framework;

namespace Integration.NHibernateExtentions
{
	[TestFixture]
	public class StateExtentionFixture : AdmIntegrationFixture
	{
		[Test]
		public void If_property_changed_should_be_true()
		{
			var client = DataMother.TestClient();
			var oldValue = client.Name;
			client.Name = "123";
			Assert.That(client.IsChanged(c => c.Name), Is.True);
			Assert.That(client.OldValue(c => c.Name), Is.EqualTo(oldValue));
		}

		[Test]
		public void Update_property()
		{
			var client = DataMother.TestClient();
			var loadedClient = session.Load<Client>(client.Id);
			var oldValue = loadedClient.Name;
			loadedClient.Name = "123";
			Assert.That(loadedClient.IsChanged(c => c.Name), Is.True);
			Assert.That(loadedClient.OldValue(c => c.Name), Is.EqualTo(oldValue));
		}
	}
}