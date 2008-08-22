using Castle.Components.Validator;
using Castle.ActiveRecord;

namespace Common.Web.Ui.Models
{
	[ActiveRecord("contacts.Persons")]
	public class Person : ContactOwner
	{
		public Person()
		{ }

		public Person(ContactGroup contactGroup)
		{
			ContactGroup = contactGroup;
			contactGroup.Persons.Add(this);
		}

		[JoinedKey("Id")]
		protected uint PersonId { get; set; }

		[Property, ValidateNonEmpty("Имя не может быть пустым")]
		public string Name { get; set; }

		[BelongsTo(Column = "ContactGroupId")]
		public ContactGroup ContactGroup { get; set; }

		public static Person Find(uint id)
		{
			return (Person) FindByPrimaryKey(typeof (Person), id);
		}
	}
}
