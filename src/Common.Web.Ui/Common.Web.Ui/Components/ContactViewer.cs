using System.IO;
using Castle.MonoRail.Framework;
using System.Collections.Generic;
using Common.Web.Ui.Models;
using System;
using Common.Web.Ui.Helpers;
using System.Text;

namespace Common.Web.Ui.Components
{
	public class ContactViewer : ViewComponent
	{
		public override bool SupportsSection(string name)
		{
			return name == "Item" || name == "ContactGroupHeader" || name == "ContactGroupHeaderLink" || name == "Person";
		}

		public override void Render()
		{
			IEnumerable<ContactGroup> contactGroups = (IEnumerable<ContactGroup>)ComponentParams["ContactGroups"];
			if (!Context.HasSection("Item"))
			{
				RenderHeader();
			}
			foreach (ContactGroup contactGroup in contactGroups)
			{
				if (contactGroup.Specialized)
					continue;
				RenderGroupHeader(contactGroup);
				if (contactGroup.Contacts.Count == 0 && contactGroup.Persons.Count == 0)
				{
					RenderEmptyBlock();
				}
				else 
				{
					RenderContacts(contactGroup.Contacts);
					foreach (Person person in contactGroup.Persons)
						RenderPerosn(person);
				}
			}
			if (!Context.HasSection("Item"))
			{
				RenderFooter();
			}
		}

		private void RenderEmptyBlock()
		{
			RenderText("<tr><td colspan='2' class='ContactViewerEmptyBlock'>Контактная информация не задана</tr></td>");
		}

		private void RenderHeader()
		{
			RenderText("<table width='100%' cellpadding='0' cellspacing='0'>");
		}

		private void RenderGroupHeader(ContactGroup contactGroup)
		{
			string groupNameLink;
			if (!Context.HasSection("ContactGroupHeaderLink"))
			{
				groupNameLink = String.Format("<a href='../Contact/EditContactGroup.rails?contactGroupId={0}'>{1}</a>",
				                              contactGroup.Id,
				                              contactGroup.Name);
			}
			else
			{
				StringWriter stringWriter = new StringWriter();
				PropertyBag["ContactGroupId"] = contactGroup.Id;
				PropertyBag["ContactGroupName"] = contactGroup.Name;
				RenderSection("ContactGroupHeaderLink", stringWriter);
				groupNameLink = stringWriter.ToString();
			}

			if (Context.HasSection("ContactGroupHeader"))
			{
					PropertyBag["ContactGroupName"] = groupNameLink;
					RenderSection("ContactGroupHeader");
			}
			else
			{
					RenderText(String.Format(@"
	<tr>
		<td class='ContactViewerGroupHeader' colspan='2'>
			{0}
		</td>
	</tr>", groupNameLink));
			}
		}

		private void RenderFooter()
		{
			RenderText("</table>");
		}

		private void RenderPerosn(Person person)
		{
			string personSection;
			if (Context.HasSection("Person"))
			{
				var stringWriter = new StringWriter();
				PropertyBag["PersonId"] = person.Id;
				PropertyBag["PersonName"] = person.Name;
				RenderSection("Person", stringWriter);
				personSection = stringWriter.ToString();
			}
			else
				personSection = String.Format("<a href='../Contact/EditPerson.rails?personId={0}'>{1}</a>",
						person.Id,
						person.Name);

			RenderItem("Ф.И.О.", personSection);

			RenderContacts(person.Contacts);
		}

		private void RenderContacts(IEnumerable<Contact> contacts)
		{
			foreach (var valueAndDescription in BindingHelper.GetDescriptionsDictionary(typeof(ContactType)))
			{
				var summaryContact = ContactContactTextByContactType(contacts, (ContactType) valueAndDescription.Key);
				if (!String.IsNullOrEmpty(summaryContact))
					RenderItem(valueAndDescription.Value, summaryContact);
			}
		}

		private void RenderItem(string label, string value)
		{
			if (Context.HasSection("Item"))
			{
				PropertyBag["Label"] = label;
				PropertyBag["Value"] = value;
				RenderSection("Item");
			}
			else
			{
				RenderText(String.Format(@"
<tr>
	<td class='ContactViewerContactLabel'>
		{0}:
	</td>
	<td class='ContactViewerContactText'>
		{1}
	</td>
</tr>", label, value));
			}
		}


		private static string ContactContactTextByContactType(IEnumerable<Contact> contacts, ContactType type)
		{
			var result = new StringBuilder();
			foreach (var contact in contacts)
				if (contact.Type == type)
					result.Append(GetContactText(contact) + ", ");
			if (result.Length > 0)
				result.Remove(result.Length - 2, 2);
			return result.ToString();
		}

		private static string GetContactText(Contact contact)
		{
			string result;
			switch (contact.Type)
			{
				case ContactType.Email:
					result = String.Format("<a href=\"mailto:{0}\">{0}</a>", contact.ContactText);
					break;
				case ContactType.Phone:
					var sipPhone = contact.ContactText;
					if (sipPhone.IndexOf('*') > -1)
						sipPhone = sipPhone.Remove(contact.ContactText.IndexOf('*'));
					result = String.Format("<a href=\"sip:8{0}\">{1}</a>",
					                       sipPhone.Replace("-", "").Replace(" ", "").Replace(")", "").Replace("(", ""),
					                       contact.ContactText);
					break;
				default:
					result = contact.ContactText;
					break;
			}

			if (!String.IsNullOrEmpty(contact.Comment))
				result += " - " + contact.Comment;

			return result;
		}
	}
}
