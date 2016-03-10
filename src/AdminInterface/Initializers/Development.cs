using System;
using System.IO;
using AddUser;
using AdminInterface.Helpers;
using AdminInterface.Models.Security;
using Castle.ActiveRecord;
using Common.Web.Ui.MonoRailExtentions;
using log4net;
using RemotePriceProcessor;

namespace AdminInterface.Initializers
{
	public class Stub : IRemotePriceProcessor
	{
		public void ResendPrice(WcfCallParameter paramDownlogId)
		{
			throw new NotImplementedException();
		}

		public void RetransPrice(WcfCallParameter downlogId)
		{
			throw new NotImplementedException();
		}

		public void RetransPriceSmart(uint priceId)
		{
			throw new NotImplementedException();
		}

		public string[] ErrorFiles()
		{
			throw new NotImplementedException();
		}

		public string[] InboundFiles()
		{
			throw new NotImplementedException();
		}

		public WcfPriceProcessItem[] GetPriceItemList()
		{
			return new WcfPriceProcessItem[0];
		}

		public bool TopInInboundList(int hashCode)
		{
			throw new NotImplementedException();
		}

		public bool DeleteItemInInboundList(int hashCode)
		{
			throw new NotImplementedException();
		}

		public string[] InboundPriceItemIds()
		{
			throw new NotImplementedException();
		}

		public Stream BaseFile(uint priceItemId)
		{
			throw new NotImplementedException();
		}

		public HistoryFile GetFileFormHistory(WcfCallParameter downlogId)
		{
			throw new NotImplementedException();
		}

		public void PutFileToInbound(FilePriceInfo filePriceInfo)
		{
			throw new NotImplementedException();
		}

		public void PutFileToBase(FilePriceInfo filePriceInfo)
		{
			throw new NotImplementedException();
		}

		public void RetransErrorPrice(WcfCallParameter priceItemId)
		{
			throw new NotImplementedException();
		}

		public string[] FindSynonyms(uint priceItemId)
		{
			throw new NotImplementedException();
		}

		public string[] FindSynonymsResult(string taskId)
		{
			throw new NotImplementedException();
		}

		public void StopFindSynonyms(string taskId)
		{
			throw new NotImplementedException();
		}

		public void AppendToIndex(string[] synonymsId)
		{
			throw new NotImplementedException();
		}
	}

	public class Development : IEnvironment
	{
		public void Run()
		{
			ADHelper.Storage = new MemoryUserStorage();
			BaseRemoteRequest.Runner = new StubRequestRunner();
			RemoteServiceHelper.Stub = new Stub();

			var config = Global.Config;
			var dataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");

			config.UserPreparedDataDirectory = Path.Combine(dataPath, config.UserPreparedDataDirectory);
			config.AptBox = Path.Combine(dataPath, config.AptBox);
			config.OptBox = Path.Combine(dataPath, config.OptBox);
			config.PromotionsPath = Path.Combine(dataPath, config.PromotionsPath);
			config.CertificatesPath = Path.Combine(dataPath, config.CertificatesPath);
			config.AttachmentsPath = Path.Combine(dataPath, config.AttachmentsPath);
			config.DocsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Docs");
			config.NewSupplierMailFilePath = Path.Combine(dataPath, config.NewSupplierMailFilePath);

			config.PrinterPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, config.PrinterPath);

			InitDirs(
				dataPath,
				config.AttachmentsPath,
				config.CertificatesPath,
				config.PromotionsPath,
				config.UserPreparedDataDirectory);

			new Seed().Run();
		}

		public void InitDirs(params string[] dirs)
		{
			foreach (var dir in dirs) {
				if (!Directory.Exists(dir))
					Directory.CreateDirectory(dir);
			}
		}
	}
}