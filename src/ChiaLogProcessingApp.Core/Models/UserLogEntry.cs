using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChiaLogProcessingApp.Core.Models
{
	public class UserLogEntry
	{
		public DateTime Created { get; set; }
		public string Severity { get; set; } = default!;
		public string Message { get; set; } = default!;
	}
}
