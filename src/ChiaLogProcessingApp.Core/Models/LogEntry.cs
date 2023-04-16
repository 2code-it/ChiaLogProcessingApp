using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChiaLogProcessingApp.Core.Models
{
	public class LogEntry
	{
		public DateTime Created { get; set; }
		public string Source { get; set; } = default!;
		public string Severity { get; set; } = default!;
		public string Body { get; set; } = default!;
		public string? RemoteAddress { get; set; }
	}
}
