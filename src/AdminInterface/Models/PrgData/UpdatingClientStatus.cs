using System;
using System.Runtime.Serialization;

namespace AdminInterface.Models.PrgData
{
	[DataContractAttribute(Name = "ClientStatus", Namespace = "http://schemas.datacontract.org/2004/07/Counter")]
	[SerializableAttribute]
	public class UpdatingClientStatus : IExtensibleDataObject
	{
		[NonSerializedAttribute]
		private ExtensionDataObject extensionDataField;

		public ExtensionDataObject ExtensionData { get; set; }

		[DataMemberAttribute(Name = "_UserId")]
		public uint UserId { get; set; }

		[DataMemberAttribute(Name = "_MethodName")]
		public string MethodName { get; set;}

		[DataMemberAttribute(Name = "_StartTime")]
		public DateTime StartTime { get; set;}

		public Client Client { get; set; }

		public uint ClientCode { get; set; }

		public string ShortName
		{
			get { return Client.ShortName; }
		}

		public void FetchClient()
		{
			Client = User.Find(UserId).Client;
			ClientCode = Client.Id;
		}
	}
}
