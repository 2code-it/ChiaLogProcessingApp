using ChiaLogProcessingApp.Core.Models;
using ChiaLogProcessingApp.Core.Services;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChiaLogProcessingApp.Core.Assets
{
	public class WarningsProcessor: ILogEntryProcessor
	{
		public WarningsProcessor(IFileSystemService fileSystemService) 
		{
			_fileSystemService = fileSystemService;
		}

		private readonly IFileSystemService _fileSystemService;
		private static readonly string _warningsFile = ".\\warnings.txt";


		public void Process(LogEntry entry)
		{
			if (entry.Source == "harvester" && entry.Body.Contains("Looking up qualities"))
			{
				string line = $"{entry.Created.ToString("yyyy-MM-dd HH:mm:ss.fff")}\t{entry.Source}\t{entry.Severity}\t{entry.Body}";
				_fileSystemService.FileAppendAllLines(_warningsFile, new[] { line });
			}
		}
	}
}
