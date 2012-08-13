using System;
using System.ComponentModel;
using AdminInterface.Models.Audit;
using Castle.ActiveRecord;
using Castle.Components.Validator;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models.Audit;

namespace AdminInterface.Models
{
	[ActiveRecord(Schema = "Usersettings"), Auditable, Description("Новость")]
	public class News
	{
		[PrimaryKey]
		public virtual uint Id { get; set; }

		[Property, Description("Дата публикации"), NotifyNews]
		public virtual DateTime PublicationDate { get; set; }

		//для возможности редактировать на форме
		//тк календарь будет отсекать значение времени при редактировании
		[Description("Время публикации")]
		public virtual TimeSpan PublicationTime
		{
			get
			{
				return PublicationDate.TimeOfDay;
			}
			set
			{
				PublicationDate = PublicationDate
					.Add(-PublicationDate.TimeOfDay)
					.Add(value);
			}
		}

		[Property, Description("Заголовок"), ValidateNonEmpty, NotifyNews]
		public virtual string Header { get; set; }

		[Property, Description("Тело новости"), NotifyNews]
		public virtual string Body { get; set; }

		[Property]
		public bool Deleted { get; set; }

		public string Name { get {return Header; }}

		[Style]
		public virtual bool HiddenNews
		{
			get{ return Deleted; }
		}
	}
}