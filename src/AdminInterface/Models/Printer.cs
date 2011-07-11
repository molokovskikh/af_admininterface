using System;
using System.Diagnostics;
using System.IO;

namespace AdminInterface.Models
{
	public class Printer
	{
		public static void Execute(string arguments)
		{
			var printerPath = @"U:\Apps\Printer\Printer.exe";
#if DEBUG
			printerPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\Printer\bin\debug\Printer.exe");
#endif
			var info = new ProcessStartInfo(printerPath,
				arguments) {
					UseShellExecute = false,
					CreateNoWindow = true,
					RedirectStandardError = true,
					RedirectStandardInput = true,
					RedirectStandardOutput = true
				};
			var process = Process.Start(info);
			process.OutputDataReceived += (sender, args) => { };
			process.ErrorDataReceived += (sender, args) => { };
			process.BeginErrorReadLine();
			process.BeginOutputReadLine();
			process.WaitForExit(30*1000);
		}
	}
}