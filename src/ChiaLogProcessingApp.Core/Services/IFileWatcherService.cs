using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChiaLogProcessingApp.Core.Services
{
	public interface IFileWatcherService
	{
		void Watch(string filename);
		void Unwatch();
	}
}
