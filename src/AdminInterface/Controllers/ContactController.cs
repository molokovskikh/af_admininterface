using Castle.ActiveRecord;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Controllers;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;

namespace AdminInterface.Controllers
{
	[Layout("contact")]
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
			PopulateValidatorErrorSummary(contactGroup);
			if (ValidationHelper.IsInstanceHasValidationError(contactGroup)
				|| ValidationHelper.IsCollectionHasNotValideObject(contacts))
			{
				contactGroup.Contacts = CleanUp(contacts);
				PropertyBag["ValidationErrors"] = ValidationSummaryPerInstance;
				PropertyBag["billingCode"] = billingCode;
				PropertyBag["contactGroup"] = contactGroup;
				RenderView("NewContactGroup");
				return;
			}
			BillingInstance billingInstance = BillingInstance.Find(billingCode);
			contactGroup.Type = ContactGroupType.Custom;
			contactGroup.ContactGroupOwner = billingInstance.ContactGroupOwner;
			using (new TransactionScope())
			{
				UpdateContactForContactOwner(contacts, contactGroup);
				contactGroup.Save();
			}

			RenderView(@"..\Common\CloseWindow");
		}
	}
}
