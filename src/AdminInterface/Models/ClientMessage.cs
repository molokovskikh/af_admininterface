using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Castle.ActiveRecord;
using Castle.Components.Validator;

namespace AdminInterface.Model
{
	[ActiveRecord(Table = "Usersettings.retclientsset")]
	public class ClientMessage : ActiveRecordValidationBase<ClientMessage>
	{
		private uint _showCount;
		private string _message;
		private uint _clientCode;

		[Property]
		[ValidateNonEmpty("Сообщение не может быть пустым")]
		public string Message
		{
			get { return _message; }
			set { _message = value; }
		}

		[Property]
		[ValidateRange(1, 10, "Количество показов долюно быть больше 1 но меньше 10")]
		public uint ShowMessageCount
		{
			get { return _showCount; }
			set { _showCount = value; }
		}

		[PrimaryKey]
		public uint ClientCode
		{
			get { return _clientCode; }
			set { _clientCode = value; }
		}

		public bool IsContainsNotShowedMessage()
		{
			return _showCount > 0;
		}
	}
}
