﻿using AdminInterface.Models.Billing;
using AdminInterface.MonoRailExtentions;
using Castle.ActiveRecord;

namespace AdminInterface.Controllers
{
	public class BalanceOperationsController : AdminInterfaceController
	{
		public void Show(uint id)
		{
			var operation = DbSession.Load<BalanceOperation>(id);
			PropertyBag["operation"] = operation;
		}

		public void Edit(uint id)
		{
			var operation = DbSession.Load<BalanceOperation>(id);
			PropertyBag["operation"] = operation;

			if (IsPost) {
				BindObjectInstance(operation, "operation");
				if (IsValid(operation)) {
					Notify("Сохранено");
					operation.Save();
					RedirectToReferrer();
				}
			}
		}

		public void Delete(uint id)
		{
			var operation = DbSession.Load<BalanceOperation>(id);
			operation.Delete();

			Notify("Удалено");
			RedirectTo(operation.Payer);
		}
	}
}