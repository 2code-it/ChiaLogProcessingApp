using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChiaLogProcessingApp.Core.Models
{
	public class ExceptionMessage
	{
		public ExceptionMessage(Exception exception)
		{
			Exception = exception;
		}

		public Exception Exception { get; private set; }
	}
}
