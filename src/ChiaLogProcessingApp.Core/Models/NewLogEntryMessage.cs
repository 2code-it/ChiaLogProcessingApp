using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChiaLogProcessingApp.Core.Models
{
	public class NewLogEntryMessage
	{
		public NewLogEntryMessage(LogEntry logEntry)
		{
			LogEntry = logEntry;
		}

		public Models.LogEntry LogEntry { get; private set; }
	}
}
