using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChiaLogProcessingApp.Core.Services
{
	public interface IHttpFileService: IDisposable
	{
		void Start(string httpBindingUrl);
		void Stop();
		bool IsRunning();
	}
}
