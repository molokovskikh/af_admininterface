using System;
using System.ComponentModel;
using System.Linq;

namespace AdminInterface.Models.Billing
{
	public enum AccountingSearchBy
	{
		[Description("Автоматически")] Auto,
		[Description("По адресу")] ByAddress,
		[Description("По пользователю")] ByUser,
		[Description("По клиенту")] ByClient,
		[Description("По плательщику")] ByPayer,
	}

	public class AccountingSearchProperties
	{
		public AccountingSearchProperties()
		{
			BeginDate = DateTime.Today.AddDays(-1);
			EndDate = DateTime.Today;
			SearchText = String.Empty;
		}

		public string SearchText { get; set; }

		public DateTime? BeginDate { get; set; }

		public DateTime? EndDate { get; set; }

		public AccountingSearchBy SearchBy { get; set; }

		public bool IsAutoSearch
		{
			get { return (SearchBy == AccountingSearchBy.Auto); }
		}

		public bool IsSearchByAddress
		{
			get { return (SearchBy == AccountingSearchBy.ByAddress); }
		}

		public bool IsSearchByUser
		{
			get { return (SearchBy == AccountingSearchBy.ByUser); }
		}

		public bool IsSearchByClient
		{
			get { return (SearchBy == AccountingSearchBy.ByClient); }
		}

		public bool IsSearchByPayer
		{
			get { return (SearchBy == AccountingSearchBy.ByPayer); }
		}

		public IQueryable<Accounting> Apply(IOrderedQueryable<Accounting> queryable)
		{
			IQueryable<Accounting> query = null;
/*			if (SearchBy == AccountingSearchBy.ByUser)
			{
				queryable = UserAccounting.Queryable;
				query = ByUser(queryable);
			}
			if (SearchBy == AccountingSearchBy.ByAddress)
			{
				query = AddressAccounting.Queryable;
				query = ByAddress(queryable);
			}
			if (SearchBy == AccountingSearchBy.ByClient)
			{
				query = queryable.Where(a => 
					((AddressAccounting)a).Address.Client.FullName.Contains(SearchText) ||
					((AddressAccounting)a).Address.Client.Name.Contains(SearchText) ||
					((UserAccounting)a).User.Client.FullName.Contains(SearchText) || 
					((UserAccounting)a).User.Client.Name.Contains(SearchText));
				uint id;
				if (uint.TryParse(SearchText, out id))
					query = query.Where(a => 
						((AddressAccounting)a).Address.Client.Id == id ||
						((UserAccounting)a).User.Client.Id == id);
			}
			if (SearchBy == AccountingSearchBy.ByPayer)
			{
				query = ByPayer(queryable);
			}
			if (SearchBy == AccountingSearchBy.Auto)
			{
				query = ByPayer(queryable);
				query = ByAddress(query);
				query = ByUser(query);
			}
			query = query.Where(a => a.WriteTime >= BeginDate && a.WriteTime <= EndDate && a.BeAccounted);*/
			return query;
		}

/*		private Expression<Func<Accounting, bool>> ByPayer(IQueryable<Accounting> queryable)
		{
			Expression<Func<Accounting, bool>> query = a => 
				((AddressAccounting)a).Address.Client.Payer.JuridicalName.Contains(SearchText) ||
				((AddressAccounting)a).Address.Client.Payer.ShortName.Contains(SearchText) ||
				((UserAccounting)a).User.Client.Payer.JuridicalName.Contains(SearchText) ||
				((UserAccounting)a).User.Client.Payer.ShortName.Contains(SearchText);
			uint id;
			if (uint.TryParse(SearchText, out id))
				query = query.Where(a => ((AddressAccounting)a).Address.Client.Payer.PayerID == id ||
					((UserAccounting)a).User.Client.Payer.PayerID == id);

			return query;
		}*/

		private IQueryable<Accounting> ByAddress(IQueryable<Accounting> queryable)
		{
			var query = queryable.Where(a => ((AddressAccounting)a).Address.Value.Contains(SearchText));
			uint id;
			if (uint.TryParse(SearchText, out id))
				query = query.Where(a => ((AddressAccounting)a).Address.Id == id);
			return query;
		}

		private IQueryable<Accounting> ByUser(IQueryable<Accounting> queryable)
		{
			var query = queryable.Where(a => ((UserAccounting)a).User.Name.Contains(SearchText));
			uint id;
			if (uint.TryParse(SearchText, out id))
				query = query.Where(a => ((UserAccounting)a).User.Id == id);
			return query;
		}
	}
}