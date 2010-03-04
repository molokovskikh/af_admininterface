using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using AdminInterface.Helpers;

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
				var strLength = info.Length.ToString();
				var result = String.Empty;
				var part = String.Empty;
				for (var i = strLength.Length; i > 0; i -= 3)
				{
					for (var j = 0; (j < 3) && ((i - j) > 0); j++)
						part = strLength[i - j - 1] + part;
					result = (i <= 3) ? part + result : "," + part + result;
					part = String.Empty;
				}
				return result + " байт";
			}
		}

		public string Duration
		{
			get
			{
				var duration = SoundHelper.GetSoundLength(_filename);
				if (duration <= 0)
					return String.Empty;
				duration /= 1000;
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
