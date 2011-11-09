using System.Linq;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;

namespace AdminInterface.Models.Billing
{
	[ActiveRecord(DiscriminatorValue = "1")]
	public class AddressAccount : Account
	{
		public AddressAccount() {}

		public AddressAccount(Address address)
		{
			Address = address;
			_payment = Address.Client.HomeRegion.AddressPayment;
		}

		[BelongsTo("ObjectId")]
		public virtual Address Address { get; set; }

		public override Service Service
		{
			get
			{
				return Address.Client;
			}
		}

		public override Payer Payer
		{
			get { return Address.Payer; }
		}

		public override string Name
		{
			get { return Address.Name; }
		}

		public override LogObjectType ObjectType
		{
			get { return LogObjectType.Address; }
		}

		public override uint ObjectId
		{
			get { return Address.Id; }
		}

		public virtual bool HasPaidUsers
		{
			get
			{
				// ���-�� �������������, ������� �������� ���� ����� � ������� �������� �������� �� ���������, ������ ���� �� �������
				return (Address.AvaliableForUsers.Where(user => !user.Accounting.IsFree && user.Enabled).Count() > 0);
			}
		}

		public override bool ShouldPay()
		{
			return Address.Client.Enabled && Address.Enabled && HasPaidUsers && base.ShouldPay();
		}
	}
}