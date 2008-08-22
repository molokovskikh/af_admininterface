using Castle.ActiveRecord;
using System.Collections.Generic;
using System.ComponentModel;
using Castle.Components.Validator;

namespace Common.Web.Ui.Models
{
	public enum ContactGroupType
	{
		[Description("����� ����������")] General = 0,
		[Description("������������� �������� � ������������������ �������")] ClientManagers = 1,
		[Description("�������� ����� ������(�������)")] OrderManagers = 2,
		[Description("��������� �� �������� � �� \"�������\"")] AccountantManagers = 3,
		[Description("���������� ���������� ��� ��������")] Billing = 4,
		Custom = 5
	}

	[ActiveRecord("contacts.contact_groups")]
	public class ContactGroup : ContactOwner
	{
		[JoinedKey("Id")]
		protected uint ContactGroupId { get; set; }

		[Property, ValidateNonEmpty("�������� �� ����� ���� ������")]
		public string Name { get; set; }

		[Property]
		public ContactGroupType Type { get; set; }

		[HasMany(ColumnKey = "ContactGroupId", Lazy = true, Inverse = true, OrderBy = "Name")]
		public IList<Person> Persons { get; set; }

		[BelongsTo(Column = "ContactGroupOwnerId")]
		public ContactGroupOwner ContactGroupOwner { get; set; }

		public bool ShowMailingAddress
		{
			get { return Type == ContactGroupType.Billing; }
		}

		public static ContactGroup Find(uint id)
		{
			return (ContactGroup)FindByPrimaryKey(typeof (ContactGroup), id);
		}
	}
}
