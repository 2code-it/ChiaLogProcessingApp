using ChiaLogProcessingApp.Core.Models;
using ChiaLogProcessingApp.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ChiaLogProcessingApp.Core.Assets
{
	public class FirewallBlackListProcessor: ILogEntryProcessor
	{
		public FirewallBlackListProcessor(IFileSystemService fileSystemService)
		{
			_fileSystemService = fileSystemService;
		}

		private readonly Services.IFileSystemService _fileSystemService;
		private static readonly string _blackListFile = ".\\blacklist.txt";

		public void Process(LogEntry entry)
		{
			if (entry.RemoteAddress != null && entry.Body.Contains("Banning"))
			{
				string[] lines = new string[] { };
				if (_fileSystemService.FileExists(_blackListFile)) 
				{
					lines = _fileSystemService.FileReadAllLines(_blackListFile);
				}
				if (!lines.Any(x => x == entry.RemoteAddress))
				{
					_fileSystemService.FileAppendAllLines(_blackListFile, new[] { entry.RemoteAddress });
				}
			}
		}
	}
}
