﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Web;

namespace AdminInterface.Helpers
{
	public static class SoundHelper
	{
		[DllImport("winmm.dll")]
		private static extern uint mciSendString(
			string command,
			StringBuilder returnValue,
			int returnLength,
			IntPtr winHandle);

		public static int GetSoundLength(string fileName)
		{
			try
			{
				var lengthBuf = new StringBuilder(32);

				mciSendString(string.Format("open \"{0}\" type waveaudio alias wave", fileName), null, 0, IntPtr.Zero);
				mciSendString("status wave length", lengthBuf, lengthBuf.Capacity, IntPtr.Zero);
				mciSendString("close wave", null, 0, IntPtr.Zero);

				int length = 0;
				int.TryParse(lengthBuf.ToString(), out length);

				return length;
			}
			catch (Exception)
			{}
			return 0;
		}
	}
}
