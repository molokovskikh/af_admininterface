using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using KadeSoft;

namespace AdminInterface.Helpers.Wav
{
	public class WaveFile
	{
		public riffChunk maindata;
		public fmtChunk format;
		public factChunk fact;
		public dataChunk data;
	}

	public class WavHelper
	{
		/// <summary>
		/// Возвращает продолжительность трека в секундах
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public static ulong GetSoundLength(string fileName)
		{
			try {
				var reader = new WaveFileReader(fileName);
				var contents = new WaveFile();
				contents.maindata = reader.ReadMainFileHeader();
				contents.maindata.FileName = fileName;
				while (reader.GetPosition() < (long)contents.maindata.dwFileLength) {
					var chunkName = reader.GetChunkName();
					if (chunkName == "fmt ") {
						contents.format = reader.ReadFormatHeader();
						if (reader.GetPosition() + contents.format.dwChunkSize == contents.maindata.dwFileLength)
							break;
					}
					else if (chunkName == "fact") {
						contents.fact = reader.ReadFactHeader();
						if (reader.GetPosition() + contents.fact.dwChunkSize == contents.maindata.dwFileLength)
							break;
					}
					else if (chunkName.Equals("data")) {
						contents.data = reader.ReadDataHeader();
						return Convert.ToUInt64(contents.data.dSecLength);
					}
					else
						reader.AdvanceToNext();
				}
				if (contents.maindata != null && contents.format != null)
					return contents.maindata.dwFileLength / contents.format.dwAvgBytesPerSec;
			}
			catch (Exception) {
			}
			return 0;
		}
	}
}