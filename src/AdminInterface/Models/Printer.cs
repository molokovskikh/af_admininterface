using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Printing;
using System.IO;
using System.Linq;

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

		public static List<string> All()
		{
			return PrinterSettings.InstalledPrinters
				.Cast<string>()
#if !DEBUG
				.Where(p => p.Contains("Бух"))
				.OrderBy(p => p)
#endif
				.ToList();
		}
	}
}