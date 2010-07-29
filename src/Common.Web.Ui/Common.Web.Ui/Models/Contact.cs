using System.ComponentModel;
using Castle.ActiveRecord;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models.Validators;

namespace Common.Web.Ui.Models
{
	public enum ContactType
	{
		[Description("E-mail")] Email = 0,
		[Description("�������")] Phone = 1,
		[Description("�������� �����")] MailingAddress = 2,
		[Description("����")] Fax = 3,
	}

	[ActiveRecord("contacts", Schema = "contacts")]
	public class Contact : ActiveRecordValidationBase<Contact>
	{
		private ContactOwner _contactOwner;

		[PrimaryKey]
		public uint Id { get; set; }

		[Property]
		public ContactType Type { get; set; }

		[Property, ContactTextValidation("�� ������ ���������� ����������")]
		public string ContactText { get; set; }

		[Property]
		public string Comment { get; set; }

		[BelongsTo("ContactOwnerId")]
		public ContactOwner ContactOwner
		{
			get { return _contactOwner; }
			set
			{
				value.Contacts.Add(this);
				_contactOwner = value;
			}
		}

		public Contact()
		{}

		public Contact(ContactType type, string contactText)
		{
			Type = type;

			if (type == ContactType.Email || type == ContactType.Phone)
				contactText = (contactText ?? "").Trim();

			ContactText = contactText;
		}

		public Contact(ContactOwner contactOwner)
		{
			_contactOwner = contactOwner;
			contactOwner.Contacts.Add(this);
		}

		//��� ����������� � ������� ������������������� �.�. ����� ��� ��� ������� ����� ��� ����������� 
		//�� � ��������� ����� ���������� ������ �������� ����� ��� ������������ � ���� ��� �� �������
		//���� ��� ��� ����������� ������������ � ����������� ��� �� ���������
		//protected override bool OnFlushDirty(object id, IDictionary previousState, IDictionary currentState, NHibernate.Type.IType[] types)
		//{
		//    ContactLogEntity contactLog = CreateContactLog(OperationType.Update);
		//    contactLog.ContactText = previousState["ContactText"].ToString();
		//    contactLog.Save();
		//    return base.OnFlushDirty(id, previousState, currentState, types);
		//}

		//protected override bool BeforeSave(IDictionary state)
		//{
		//    CreateContactLog(OperationType.Add).Save();
		//    return base.BeforeSave(state);
		//}

		//protected override void BeforeDelete(IDictionary state)
		//{
		//    CreateContactLog(OperationType.Delete).Save();
		//    base.BeforeDelete(state);
		//}
	}
}
