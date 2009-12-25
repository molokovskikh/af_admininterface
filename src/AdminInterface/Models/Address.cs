using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
#if !DEBUG
using System.Security.AccessControl;
#endif
using Castle.ActiveRecord;
using Common.MySql;
using Common.Web.Ui.Helpers;
using log4net;
using Common.Web.Ui.Models;

namespace AdminInterface.Models
{
	public class AddressContactInfo
	{
		public string ContactText { get; set; }
		public ContactType Type { get; set; }
		public bool Deleted { get; set; }
		public int Id { get; set; }
	}

	[ActiveRecord("Addresses", Schema = "Future")]
	public class Address : ActiveRecordBase<Address>
	{
		[PrimaryKey]
		public uint Id { get; set; }

		[Property("Address")]
		public string Value { get; set; }

		[BelongsTo("ClientId")]
		public Client Client { get; set; }

		[BelongsTo("ContactGroupId")]
		public ContactGroup ContactGroup { get; set; }

		[HasAndBelongsToMany(typeof (User),
			Lazy = true,
			ColumnKey = "AddressId",
			Table = "future.UserAddresses",
			ColumnRef = "UserId")]
		public virtual IList<User> AvaliableForUsers { get; set; }

		public virtual bool AvaliableFor(User user)
		{
			return AvaliableForUsers.Any(u => u.Id == user.Id);
		}

		public virtual void MaitainIntersection()
		{
			ArHelper.WithSession(s => {
				s.CreateSQLQuery(@"
insert into Future.AddressIntersection(AddressId, IntersectionId)
select a.Id, i.Id
from Future.Intersection i
	join Future.Addresses a on a.ClientId = i.ClientId
	left join Future.AddressIntersection ai on ai.AddressId = a.Id and ai.IntersectionId = i.Id
where a.Id = :addressId")
					.SetParameter("addressId", Id)
					.ExecuteUpdate();
			});
		}

		public virtual void CreateFtpDirectory()
		{
			var ftpRoot = ConfigurationManager.AppSettings["FtpRoot"];
			var clientRoot = Path.Combine(ftpRoot, Id.ToString());
			try
			{
				Directory.CreateDirectory(clientRoot);

				Directory.CreateDirectory(Path.Combine(clientRoot, "Orders"));
				Directory.CreateDirectory(Path.Combine(clientRoot, "Docs"));
				Directory.CreateDirectory(Path.Combine(clientRoot, "Rejects"));
				Directory.CreateDirectory(Path.Combine(clientRoot, "Waybills"));
				foreach (var user in Client.Users)
					SetAccessControl(user.Login);
			}
			catch(Exception e)
			{
				LogManager.GetLogger(GetType()).Error(String.Format(@"
Ошибка при создании папки на ftp для клиента, иди и создавай руками
Нужно создать папку {0}
А так же создать под папки Orders, Docs, Rejects, Waybills
Дать логину {1} право читать, писать и получать список директорий и удалять под директории в папке Orders",
					clientRoot, Client.Users.First().Login), e);
			}
		}

		public void SetAccessControl(string username)
		{
#if !DEBUG
			var ftpRoot = ConfigurationManager.AppSettings["FtpRoot"];
			var clientRoot = Path.Combine(ftpRoot, Id.ToString());

			username = String.Format(@"ANALIT\{0}", username);
			var rootDirectorySecurity = Directory.GetAccessControl(clientRoot);
			rootDirectorySecurity.AddAccessRule(new FileSystemAccessRule(username,
				FileSystemRights.Read,
				InheritanceFlags.ContainerInherit |
					InheritanceFlags.ObjectInherit,
				PropagationFlags.None,
				AccessControlType.Allow));
			rootDirectorySecurity.AddAccessRule(new FileSystemAccessRule(username,
				FileSystemRights.Write,
				InheritanceFlags.ContainerInherit |
					InheritanceFlags.ObjectInherit,
				PropagationFlags.None,
				AccessControlType.Allow));
			rootDirectorySecurity.AddAccessRule(new FileSystemAccessRule(username,
				FileSystemRights.ListDirectory,
				InheritanceFlags.ContainerInherit |
					InheritanceFlags.ObjectInherit,
				PropagationFlags.None,
				AccessControlType.Allow));
			Directory.SetAccessControl(clientRoot, rootDirectorySecurity);

			var orders = Path.Combine(clientRoot, "Orders");
			if (Directory.Exists(orders))
			{
				var ordersDirectorySecurity = Directory.GetAccessControl(orders);
				ordersDirectorySecurity.AddAccessRule(new FileSystemAccessRule(username,
					FileSystemRights.DeleteSubdirectoriesAndFiles,
					InheritanceFlags.ContainerInherit |
						InheritanceFlags.ObjectInherit,
					PropagationFlags.None,
					AccessControlType.Allow));
				Directory.SetAccessControl(orders, ordersDirectorySecurity);
			}
#endif
		}

		public virtual void AddContactGroup()
		{
			using (var scope = new TransactionScope())
			{
				var groupOwner = Client.ContactGroupOwner;
				var group = groupOwner.AddContactGroup(ContactGroupType.General);
				group.Save();
				this.ContactGroup = group;
			}
		}

		public static void SaveContacts(uint addressId, AddressContactInfo[] contacts)
		{
			var address = Address.Find(addressId);
			var group = address.ContactGroup;
			var existsContacts = new List<Contact>();

			foreach (var contact in group.Contacts)
				existsContacts.Add(contact);

			using (var scope = new TransactionScope(OnDispose.Rollback))
			{
				foreach (var existsContact in existsContacts)
				{
					var deleted = contacts.Where(contact => (existsContact.Id == contact.Id) && (contact.Deleted));
					if ((deleted != null) && (deleted.Count() > 0))
					{
						var tempGroup = existsContact.ContactOwner;
						tempGroup.Contacts.Remove(existsContact);						
					}
				}

				foreach (var contact in contacts)
				{
					if (contact.Id < 0)
					{
						if (!String.IsNullOrEmpty(contact.ContactText))
						{
							var newContact = address.ContactGroup.AddContact(contact.Type, contact.ContactText);
							newContact.Save();
						}
					}
					else
					{
						var editContacts = existsContacts.Where(existsContact => existsContact.Id == contact.Id);
						if ((editContacts == null) || (editContacts.Count() == 0))
							continue;
						if (!String.Equals(contact.ContactText, editContacts.First().ContactText))
						{
							editContacts.First().ContactText = contact.ContactText;
							editContacts.First().Save();
						}
					}
				}
				scope.VoteCommit();
			}
		}
	}
}