using Castle.ActiveRecord;
using System.Collections.Generic;
using System.ComponentModel;
using Castle.Components.Validator;

namespace Common.Web.Ui.Models
{
	public enum ContactGroupType
	{
		[Description("Общая информация")] General = 0,
		[Description("Администратор клиентов в автоматизированной системе")] ClientManagers = 1,
		[Description("Менеджер прайс листов(заказов)")] OrderManagers = 2,
		[Description("Бухгалтер по расчетам с АК \"Инфорум\"")] AccountantManagers = 3,
		[Description("Контактная информация для биллинга")] Billing = 4,
		Custom = 5
	}

	[ActiveRecord("contacts.contact_groups")]
	public class ContactGroup : ContactOwner
	{
		[JoinedKey("Id")]
		protected uint ContactGroupId { get; set; }

		[Property, ValidateNonEmpty("Название не может быть пустым")]
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
