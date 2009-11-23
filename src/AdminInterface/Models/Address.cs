﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
#if !DEBUG
using System.Security.AccessControl;
#endif
using Castle.ActiveRecord;
using log4net;

namespace AdminInterface.Models
{
	[ActiveRecord("Addresses", Schema = "Future")]
	public class Address : ActiveRecordBase<Address>
	{
		[PrimaryKey]
		public uint Id { get; set; }

		[Property("Address")]
		public string Value { get; set; }

		[BelongsTo("ClientId")]
		public Client Client { get; set; }

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
			var ordersDirectorySecurity = Directory.GetAccessControl(orders);
			ordersDirectorySecurity.AddAccessRule(new FileSystemAccessRule(username,
				FileSystemRights.DeleteSubdirectoriesAndFiles,
				InheritanceFlags.ContainerInherit |
					InheritanceFlags.ObjectInherit,
				PropagationFlags.None,
				AccessControlType.Allow));
			Directory.SetAccessControl(orders, ordersDirectorySecurity);
#endif
		}
	}
}