using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;
using Common.Tools;
using Common.Web.Ui.ActiveRecordExtentions;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;

namespace AdminInterface.Changer
{
	public class HelpSupplierContact
	{
		public void ChangeRegionalDataFurHelpSuppliers()
		{
			using (var transaction = new TransactionScope(OnDispose.Rollback)) {
				var rds = ArHelper.WithSession(session => session.CreateSQLQuery(@"
	SELECT rd.* FROM usersettings.regionaldata rd
	join customers.Suppliers s on rd.firmCode = s.id
	join farm.Regions r on s.HomeRegion = r.RegionCode
	where r.Retail and rd.ContactInfo <> '';")
					.AddEntity(typeof(RegionalData)).List<RegionalData>());

				foreach (var regionalData in rds) {
					var list = regionalData.ContactInfo.Split('\r').ToList();
					if (list.Count < 14) {
						foreach (var i in Enumerable.Range(1, 14 - list.Count)) {
							list.Add(string.Empty);
						}
					}
					var operativeInfo = list[0].Trim() + "\r\n" + list[1].Trim() + "\r\n";

					for (int i = 6; i < 14; i++) {
						var val = list[i].Trim();
						if (!string.IsNullOrEmpty(val))
							val += "\r\n";
						operativeInfo += val;
					}
					var contactInfo = list[4].Trim() + "\r\n" + list[5].Trim();

					var supp = regionalData.Supplier;
					supp.Address = list[3].Trim();
					ActiveRecordMediator.Save(supp);
					list[2] = list[2].Trim();
					if (!Regex.IsMatch(list[2], @"^(\d{3,4})-(\d{6,7})(\*\d{3})?$")) {
						var nums = list[2].Split(',');
						if (nums.Length > 1)
							list[2] = nums.First();
						var matches = Regex.Matches(list[2], "\\d+")
							.Cast<Match>()
							.Select(x => int.Parse(x.Value))
							.ToArray();
						var tel = matches.Implode(string.Empty);
						if (tel.Length == 7)
							list[2] = "4732-" + tel;
						else if (tel.Length == 6)
							list[2] = "4732-2" + tel;
						else if (list[2].Contains("4732"))
							list[2] = list[2].Replace("4732", "4732-");
						else {
							operativeInfo += "\r\n" + list[2];
							list[2] = string.Empty;
						}
					}
					if (!string.IsNullOrEmpty(list[2])) {
						var generaleGroup = supp.ContactGroupOwner.Group(ContactGroupType.General);
						if (generaleGroup == null) {
							var group = supp.ContactGroupOwner.AddContactGroup(ContactGroupType.General);
							group.AddContact(ContactType.Phone, list[2]);
							group.Save();
						}
						else {
							var contact = generaleGroup.Contacts.FirstOrDefault(c => c.Type == ContactType.Phone);
							if (contact != null) {
								contact.ContactText = string.IsNullOrEmpty(contact.ContactText) ? list[2] : contact.ContactText;
								contact.Save();
							}
							else {
								generaleGroup.AddContact(ContactType.Phone, list[2]);
								generaleGroup.Save();
							}
						}
					}
					regionalData.ContactInfo = contactInfo;
					regionalData.OperativeInfo = operativeInfo;
					ActiveRecordMediator<RegionalData>.Update(regionalData);
				}
				transaction.VoteCommit();
			}
		}
	}
}