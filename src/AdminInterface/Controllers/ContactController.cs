using AdminInterface.Security;
using Castle.ActiveRecord;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Controllers;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;

namespace AdminInterface.Controllers
{
	[Layout("contact")]
	[Security]
	public class ContactController : AbstractContactController
	{
		public void NewContactGroup(uint billingCode)
		{
			PropertyBag["billingCode"] = billingCode;
			PropertyBag["contactGroup"] = new ContactGroup();
		}

		public void AddContactGroup(uint billingCode,
		                            [DataBind("ContactGroup")] ContactGroup contactGroup,
		                            [DataBind("Contacts")] Contact[] contacts)
		{

			PopulateValidatorErrorSummary(contactGroup, Binder.GetValidationSummary(contactGroup));
			if (ValidationHelper.IsInstanceHasValidationError(contactGroup)
			    || ValidationHelper.IsCollectionHasNotValideObject(contacts))
			{
				contactGroup.Contacts = CleanUp(contacts);
				PropertyBag["billingCode"] = billingCode;
				PropertyBag["contactGroup"] = contactGroup;
				RenderView("NewContactGroup");
				return;
			}
			var billingInstance = Payer.Find(billingCode);
			contactGroup.Type = ContactGroupType.Custom;
			contactGroup.ContactGroupOwner = billingInstance.ContactGroupOwner;
			using (new TransactionScope())
			{
				UpdateContactForContactOwner(contacts, contactGroup);
				contactGroup.Save();
			}

			RenderView(@"..\Common\CloseWindow");
		}

		public override void AddPerson(uint contactGroupId,
		                               [DataBind("CurrentPerson")] Person person,
		                               [DataBind("Contacts")] Contact[] contacts)
		{
			base.AddPerson(contactGroupId, person, contacts);
			if (Response.StatusCode == 302)
				RedirectToAction("CloseWindow");
		}

		public override void UpdateContactGroup(uint contactGroupId,
		                                        [DataBind("Contacts")] Contact[] contacts)
		{
			base.UpdateContactGroup(contactGroupId, contacts);
			if (Response.StatusCode == 302)
				RedirectToAction("CloseWindow");
		}

		public override void UpdatePerson([DataBind("CurrentPerson")] Person person,
		                                  [DataBind("Contacts")] Contact[] contacts)
		{
			base.UpdatePerson(person, contacts);
			if (Response.StatusCode == 302)
				RedirectToAction("CloseWindow");
		}

		public void CloseWindow()
		{
			RenderView(@"..\Common\CloseWindow");
		}
	}
}