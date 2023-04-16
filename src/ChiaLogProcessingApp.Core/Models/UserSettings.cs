using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChiaLogProcessingApp.Core.Models
{
	public class UserSettings
	{
		public string HttpBindingUrl { get; set; } = default!;
		public string ChiaLogFilePath { get; set; } = default!;

	}
}
