﻿using System;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;

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
			get { return Client.Name; }
		}

		public void FetchClient()
		{
			Client = User.Find(UserId).Client;
			ClientCode = Client.Id;
		}
	}

	public interface IStatisticService
	{
		[OperationContract(Action = "http://tempuri.org/IStatisticService/GetUpdateInfo",
			ReplyAction = "http://tempuri.org/IStatisticService/GetUpdateInfoResponse")]
		UpdatingClientStatus[] GetUpdateInfo();

		[OperationContract(Action = "http://tempuri.org/IStatisticService/GetUpdatingClientCount")]
		int GetUpdatingClientCount();
	}

	public interface IStatisticServiceChannel : IStatisticService, IClientChannel
	{
	}

	public class StatisticServiceClient : ClientBase<IStatisticService>, IStatisticService
	{
		public StatisticServiceClient()
		{
		}

		public StatisticServiceClient(string endpointConfigurationName) :
			base(endpointConfigurationName)
		{
		}

		public StatisticServiceClient(string endpointConfigurationName, string remoteAddress) :
			base(endpointConfigurationName, remoteAddress)
		{
		}

		public StatisticServiceClient(string endpointConfigurationName, EndpointAddress remoteAddress) :
			base(endpointConfigurationName, remoteAddress)
		{
		}

		public StatisticServiceClient(Binding binding, EndpointAddress remoteAddress) :
			base(binding, remoteAddress)
		{
		}

		public UpdatingClientStatus[] GetUpdateInfo()
		{
			return Channel.GetUpdateInfo();
		}

		public int GetUpdatingClientCount()
		{
			return Channel.GetUpdatingClientCount();
		}
	}
}
