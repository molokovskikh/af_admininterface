using System;
using System.ComponentModel;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Suppliers;
using AdminInterface.Security;
using Castle.ActiveRecord;

namespace AdminInterface.Models.Billing
{
	[ActiveRecord(DiscriminatorValue = "0")]
	public class UserAccount : Account
	{
		public UserAccount() {}

		public UserAccount(User user)
		{
			User = user;
			if (user.RootService.GetType() == typeof(Supplier))
			{
				_payment = User.RootService.HomeRegion.SupplierUserPayment;
				_readyForAccounting = true;
				_beAccounted = true;
				Operator = SecurityContext.Administrator.UserName;
				WriteTime = DateTime.Now;
			}
			else
				_payment = User.RootService.HomeRegion.UserPayment;
		}

		[BelongsTo("ObjectId"), Description("Пользователь")]
		public virtual User User { get; set; }

		public override uint ObjectId
		{
			get { return User.Id; }
		}

		public override Service Service
		{
			get
			{
				return User.RootService;
			}
		}

		public override Payer Payer
		{
			get { return User.Payer; }
		}

		public override string Name
		{
			get { return User.Name; }
		}

		public override LogObjectType ObjectType
		{
			get { return LogObjectType.User; }
		}

		public override bool ShouldPay()
		{
			return !User.RootService.Disabled && User.Enabled && base.ShouldPay();
		}
	}
}