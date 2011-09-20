using System;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Security;
using AdminInterface.Models.Suppliers;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Castle.ActiveRecord.Linq;
using Common.MySql;
using Common.Web.Ui.Helpers;

namespace AdminInterface.Models.Billing
{
	[ActiveRecord(DiscriminatorValue = "0")]
	public class UserAccounting : Accounting
	{
		public UserAccounting() {}

		public UserAccounting(User user)
		{
			Payment = 800;
			User = user;
		}

		[OneToOne(PropertyRef = "Accounting")]
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

	[ActiveRecord(DiscriminatorValue = "1")]
	public class AddressAccounting : Accounting
	{
		[OneToOne(PropertyRef = "Accounting")]
		public virtual Address Address { get; set; }

		public AddressAccounting(Address address)
		{
			Address = address;
			Payment = 200;
		}

		public AddressAccounting() {}

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
				// Кол-во пользователей, которым доступен этот адрес и которые включены работают НЕ бесплатно, должно быть НЕ нулевым
				return (Address.AvaliableForUsers.Where(user => !user.Accounting.IsFree && user.Enabled).Count() > 0);
			}
		}

		public override bool ShouldPay()
		{
			return Address.Client.Enabled && Address.Enabled && HasPaidUsers && base.ShouldPay();
		}
	}

	[ActiveRecord(DiscriminatorValue = "2")]
	public class ReportAccounting : Accounting
	{
		[BelongsTo("ObjectId", Cascade = CascadeEnum.All)]
		public virtual Report Report { get; set; }

		public ReportAccounting(Report report)
		{
			Report = report;
			ReadyForAcounting = true;
		}

		public ReportAccounting() {}

		public override Payer Payer
		{
			get { return Report.Payer; }
		}

		public override string Name
		{
			get { return Report.Comment; }
		}

		public override LogObjectType ObjectType
		{
			get { return LogObjectType.Report; }
		}

		public override uint ObjectId
		{
			get { return Report.Id; }
		}

		public override bool ShouldPay()
		{
			return Report.Allow && base.ShouldPay();
		}

		public override bool Status
		{
			get
			{
				return Report.Allow;
			}
			set
			{
				Report.Allow = value;
			}
		}
	}

	[ActiveRecord("Accounting", DiscriminatorColumn = "Type", DiscriminatorValue = "10000", Schema = "Billing", Lazy = true), Auditable]
	public abstract class Accounting : ActiveRecordLinqBase<Accounting>, IAuditable
	{
		[PrimaryKey]
		public virtual uint Id { get; set; }

		[Property]
		public virtual DateTime? WriteTime { get; set; }

		[Property]
		public virtual string Operator { get; set; }

		[Property, Auditable("Платеж")]
		public virtual decimal Payment { get; set; }

		[Property]
		public virtual bool BeAccounted { get; set;  }

		[Property]
		public virtual bool ReadyForAcounting { get; set; }

		[Property]
		public virtual int InvoiceGroup { get; set; }

		[Property]
		public virtual bool IsFree { get; set; }

		public virtual uint PayerId
		{
			get
			{
				return Payer.Id;
			}
		}

		public abstract Payer Payer { get; }

		public abstract string Name { get;}

		public abstract LogObjectType ObjectType { get; }

		public abstract uint ObjectId { get; }

		public virtual string Type
		{
			get
			{
				return BindingHelper.GetDescription(ObjectType);
			}
		}

		public virtual Service Service
		{
			get { return null; }
		}

		public virtual bool Status
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public virtual void Accounted()
		{
			Operator = SecurityContext.Administrator.UserName;
			WriteTime = DateTime.Now;
			BeAccounted = true;
		}

		public virtual bool ShouldPay()
		{
			return (BeAccounted || ReadyForAcounting) && !IsFree;
		}

		public static IEnumerable<Accounting> GetReadyForAccounting(Pager pager)
		{
			var readyForAccounting = Queryable.Where(a => a.ReadyForAcounting && !a.BeAccounted);

			pager.Total = readyForAccounting.Count();
			return pager.DoPage(readyForAccounting).ToList();
		}

		public virtual IAuditRecord GetAuditRecord()
		{
			return new PayerAuditRecord(Payer, this);
		}
	}

	public class Pager
	{
		public int Page;
		public int PageSize = 30;

		public int Total;

		public int TotalPages
		{
			get
			{
				var result = Total / PageSize;
				if (Total % PageSize > 0)
					result++;
				return result;
			}
		}

		public Pager()
		{}

		public Pager(int? page, int pageSize)
		{
			if (page != null)
				Page = page.Value;
			PageSize = pageSize;
		}

		public IEnumerable<T> DoPage<T>(IEnumerable<T> enumerable)
		{
			return enumerable
				.Skip(Page*PageSize)
				.Take(PageSize);
		}
	}
}
