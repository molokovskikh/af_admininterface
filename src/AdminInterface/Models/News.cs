using System;
using System.ComponentModel;
using Castle.ActiveRecord;
using Castle.Components.Validator;

namespace AdminInterface.Models
{
	[ActiveRecord(Schema = "Usersettings")]
	public class News
	{
		[PrimaryKey]
		public virtual uint Id { get; set; }

		[Property, Description("Дата публикации")]
		public virtual DateTime PublicationDate { get; set; }

		[Property, Description("Заголовок"), ValidateNonEmpty]
		public virtual string Header { get; set; }

		[Property]
		public virtual string Body { get; set; }

		[Property]
		public bool Deleted { get; set; }
	}
}