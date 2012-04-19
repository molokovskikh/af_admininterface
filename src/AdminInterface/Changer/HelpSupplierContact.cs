﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;
using Common.Tools;
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
	SELECT rd.* FROM regionaldata rd
	join customers.Suppliers s on rd.firmCode = s.id
	join farm.Regions r on s.HomeRegion = r.RegionCode
	where r.Retail and ContactInfo <> '' ").AddEntity(typeof(RegionalData)).List<RegionalData>());

				foreach (var regionalData in rds) {
					var list = regionalData.ContactInfo.Split('\r').ToList();
					if (list.Count < 14)
					{
						foreach (var i in Enumerable.Range(1, 14 - list.Count))
						{
							list.Add(string.Empty);
						}
					}
					var operativeInfo = string.Empty;

					for (int i = 6; i < 14; i++) {
						var val = list[i].Trim();
						if (!string.IsNullOrEmpty(val))
							val += "\r\n";
						operativeInfo += val;
					}
					var contactInfo = list[4].Trim() + "\r\n" + list[5].Trim();

					regionalData.ContactInfo = contactInfo;
					regionalData.OperativeInfo = operativeInfo;
					list[2] = list[2].Trim();
					if (!Regex.IsMatch(list[2], @"^(\d{3,4})-(\d{6,7})(\*\d{3})?$"))
					{
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
						else
							if (tel.Length == 6)
							list[2] = "4732-2" + tel;
								else
								list[2] = string.Empty;
					}
					if (!string.IsNullOrEmpty(list[2])) { 
						var supp = regionalData.Supplier;
						var generaleGroup = supp.ContactGroupOwner.Group(ContactGroupType.General);
						if (generaleGroup == null) {
							var group = supp.ContactGroupOwner.AddContactGroup(ContactGroupType.General);
							group.AddContact(ContactType.Phone, list[2]);
							var contact = group.Contacts.FirstOrDefault();
							contact.Comment = list[3].Trim();
							contact.Save();
							group.Save();
						}
						else {
							var contact = generaleGroup.Contacts.FirstOrDefault(c => c.Type == ContactType.Phone);
							if (contact != null) {
								contact.ContactText = string.IsNullOrEmpty(contact.ContactText) ? list[2] : contact.ContactText;
								contact.Comment = string.IsNullOrEmpty(contact.Comment) ? list[3].Trim() : contact.Comment;
								contact.Save();
							}
							else {
								generaleGroup.AddContact(ContactType.Phone, list[2]);
								var temp_contact = generaleGroup.Contacts.FirstOrDefault();
								temp_contact.Comment = list[3].Trim();
								temp_contact.Save();
								generaleGroup.Save();
							}
						}
					}
					ActiveRecordMediator<RegionalData>.Update(regionalData);
				}
				transaction.VoteCommit();
			}
		}
	}
}