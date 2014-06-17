using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using AdminInterface.Controllers;
using AdminInterface.Models;
using Castle.Components.Binder;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Framework.Test;
using Common.Tools;
using Integration.ForTesting;
using NHibernate.Linq;
using NUnit.Framework;
using Rhino.Mocks;

namespace Integration.Controllers
{
	[TestFixture]
	public class RegisterFromClientInterfaceFixture : CommonUserControllerFixture
	{
		private string _json = "{\"Id\":0,\"Login\":\"testLoginRegister\",\"Enabled\":true,\"Name\":\"testComment\",\"SubmitOrders\":false,\"Auditor\":false,\"WorkRegionMask\":0,\"AuthorizationDate\":null,\"Client\":null,\"RootService\":{\"Name\":\"Протек-15\",\"Id\":5},\"SendWaybills\":false,\"SendRejects\":true,\"ShowSupplierCost\":true,\"InheritPricesFrom\":null,\"Payer\":{\"PayerID\":5,\"Name\": \"testPayer\",\"Reports\":null,\"Contacts\":null,\"ContactGroupOwner\":null},\"AvaliableAddresses\":[],\"ImpersonableUsers\":null,\"AssignedPermissions\":[{\"Id\":27,\"Name\":null,\"Shortcut\":null,\"AvailableFor\":0,\"Type\":0,\"AssignDefaultValue\":false},{\"Id\":29,\"Name\":null,\"Shortcut\":null,\"AvailableFor\":0,\"Type\":0,\"AssignDefaultValue\":false},{\"Id\":31,\"Name\":null,\"Shortcut\":null,\"AvailableFor\":0,\"Type\":0,\"AssignDefaultValue\":false},{\"Id\":33,\"Name\":null,\"Shortcut\":null,\"AvailableFor\":0,\"Type\":0,\"AssignDefaultValue\":false},{\"Id\":35,\"Name\":null,\"Shortcut\":null,\"AvailableFor\":0,\"Type\":0,\"AssignDefaultValue\":false},{\"Id\":37,\"Name\":null,\"Shortcut\":null,\"AvailableFor\":0,\"Type\":0,\"AssignDefaultValue\":false},{\"Id\":39,\"Name\":null,\"Shortcut\":null,\"AvailableFor\":0,\"Type\":0,\"AssignDefaultValue\":false},{\"Id\":41,\"Name\":null,\"Shortcut\":null,\"AvailableFor\":0,\"Type\":0,\"AssignDefaultValue\":false},{\"Id\":43,\"Name\":null,\"Shortcut\":null,\"AvailableFor\":0,\"Type\":0,\"AssignDefaultValue\":false},{\"Id\":45,\"Name\":null,\"Shortcut\":null,\"AvailableFor\":0,\"Type\":0,\"AssignDefaultValue\":false},{\"Id\":47,\"Name\":null,\"Shortcut\":null,\"AvailableFor\":0,\"Type\":0,\"AssignDefaultValue\":false}],\"RegionSettings\":[1,8,0,0,0,0,0,0],\"IsInheritPrices\":false,\"NameOrLogin\":\"testComment\",\"CanViewClientInterface\":true}";

		[Test]
		public void CollectionFromJSonText()
		{
			var user = new User();
			controller.BindObjectInstanceForUser(user, "User", _json);

			var regionMask = BitConverter.ToUInt64(user.RegionSettings.Select(r => Convert.ToByte(r)).ToArray(), 0);

			Assert.AreEqual(regionMask, 2049);
			Assert.AreEqual(user.Login, "testLoginRegister");
			Assert.AreEqual(user.Name, "testComment");
			Assert.AreEqual(user.AssignedPermissions.Count, 11);
			Assert.AreEqual(user.AssignedPermissions[0].Id, 27);
			Assert.AreEqual(user.RootService.Id, 5);
			Assert.AreEqual(user.RootService.Name, "Протек-15");
			Assert.IsNull(user.InheritPricesFrom);
		}

		[Test]
		public void Patch_json()
		{
			var result = UsersController.PatchJson(_json);
			Assert.That(result, Is.Not.StringContaining("PayerID"));
		}

		[Test]
		public void Add_from_json()
		{
			var tempLogin = Generator.Name();
			var supplier = DataMother.CreateSupplier();
			session.Save(supplier);

			var ojdJson = _json;
			_json = _json.Replace("\"Id\":5", string.Format("\"Id\":{0}", supplier.Id))
				.Replace("\"PayerID\":5", string.Format("\"PayerID\":{0}", supplier.Payer.Id))
				.Replace("testLoginRegister", tempLogin);

			Prepare();
			PrepareController(controller);

			Flush();
			controller.Add();

			var user = Registred();

			Assert.AreEqual(user.Login, tempLogin);
			Assert.AreEqual(user.Name, "testComment");
			_json = ojdJson;
		}

		protected override IMockRequest BuildRequest()
		{
			var requestTest = new MemoryStream(Encoding.UTF8.GetBytes(_json));
			var request = MockRepository.GenerateStub<IMockRequest>();
			request.Stub(r => r.InputStream).Return(requestTest);
			request.Stub(r => r.Params).Return(new NameValueCollection());
			request.Stub(r => r.ObtainParamsNode(ParamStore.Params)).Return(new CompositeNode("root"));
			return request;
		}
	}
}
