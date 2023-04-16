using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChiaLogProcessingApp.Core.Assets
{
	public interface ILogEntryProcessor
	{
		void Process(Models.LogEntry entry);
	}
}
