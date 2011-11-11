using System;
using System.IO;
using AdminInterface.Helpers.Wav;
using Common.Web.Ui.Helpers;

namespace AdminInterface.Models.Telephony
{
	public class CallRecordFile
	{
		private string _filename;

		public CallRecordFile(string filename)
		{
			_filename = filename;
		}

		public string Name { get { return _filename; } }

		public CallRecord Call { get; set; }

		public string Size
		{
			get
			{
				var info = new FileInfo(_filename);
				return ViewHelper.ConvertToUserFriendlySize(Convert.ToUInt64(info.Length));
			}
		}

		public string Duration
		{
			get
			{
				var duration = WavHelper.GetSoundLength(_filename);
				if (duration <= 0)
					return String.Empty;
				var hours = duration / 3600;
				var minutes = duration / 60 - hours * 60;
				var seconds = duration - hours*3600 - minutes*60;
				var result = hours > 0 ? String.Format("{0}ч. ", hours) : String.Empty;
				result += minutes > 0 ? String.Format("{0}м. ", minutes) : String.Empty;
				result += seconds > 0 ? String.Format("{0}сек. ", seconds) : String.Empty;
				return result;
			}
		}
	}
}
