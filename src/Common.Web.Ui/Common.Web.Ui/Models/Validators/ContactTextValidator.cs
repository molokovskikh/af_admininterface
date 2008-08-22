using Castle.Components.Validator;
using System.Text.RegularExpressions;

namespace Common.Web.Ui.Models.Validators
{
	public class ContactTextValidator : AbstractValidator
	{
		public override bool IsValid(object instance, object fieldValue)
		{
			Contact contact = (Contact) instance;
			if (fieldValue == null)
				return true;
			Regex regex;
			switch(contact.Type)
			{
				case ContactType.Email:
					regex = new Regex(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
					break;
				case ContactType.Phone:
					regex = new Regex(@"^(\d{3,4})-(\d{6,7})(\*\d{3})?$");
					break;
				default:
					return true;
			}
			return regex.Match(fieldValue.ToString()).Success;
		}

		public override bool SupportsBrowserValidation
		{
			get { return false; }
		}
	}
}
