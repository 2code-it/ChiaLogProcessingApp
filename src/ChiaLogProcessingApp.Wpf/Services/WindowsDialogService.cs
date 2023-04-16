using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChiaLogProcessingApp.Wpf.Services
{
	public class WindowsDialogService : Core.Services.IDialogService
	{
		public string? ShowOpenFileDialog(string? filter = null)
		{
			OpenFileDialog dialog = new OpenFileDialog();
			if (filter != null)
			{
				dialog.Filter = filter;
			}
			dialog.CheckFileExists = true;
			dialog.CheckPathExists = true;
			if (dialog.ShowDialog() ?? false)
			{
				return dialog.FileName;
			}

			return null;
		}
	}
}
