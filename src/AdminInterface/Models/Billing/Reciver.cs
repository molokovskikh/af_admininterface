using Castle.ActiveRecord;

namespace AdminInterface.Models.Billing
{
	[ActiveRecord("Billing.Recivers")]
	public class Reciver : ActiveRecordBase<Reciver>
	{
		[PrimaryKey]
		public uint Id { get; set; }

		[Property]
		public string Name { get; set; }

		[Property]
		public string FullName { get; set; }

		[Property]
		public string Address { get; set; }

		[Property]
		public string Phone { get; set; }

		[Property]
		public string Accountant { get; set; }

		[Property]
		public string Boss { get; set; }

		[Property]
		public string INN { get; set; }

		[Property]
		public string BIK { get; set; }

		[Property]
		public string KPP { get; set; }

		[Property]
		public string AccountNumber { get; set; }

		[Property]
		public string BankName { get; set; }

		[Property]
		public string BankAccountNumber { get; set; }
	}
}
