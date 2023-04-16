using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChiaLogProcessingApp.Core.Services
{
	public class FileWatcherService : IFileWatcherService, IDisposable

	{
		public FileWatcherService(IFileSystemService fileSystemService, IMessenger messenger) 
		{
			_fileSystemService = fileSystemService;
			_messenger = messenger;
		}

		private readonly IMessenger _messenger;
		private readonly IFileSystemService _fileSystemService;
		private FileSystemWatcher? _watcher;
		private long _lastFileSize;

		private static readonly object _locker = new object();
		public void Unwatch()
		{
			if(_watcher != null )
			{
				_watcher.Dispose();
				_watcher = null;
			}
		}

		public void Watch(string filename)
		{
			if(_watcher != null)
			{
				throw new InvalidOperationException("Already watching");
			}
			if (!_fileSystemService.FileExists(filename))
			{
				throw new ArgumentException("File not found", "filename");
			}

			_lastFileSize = _fileSystemService.GetFileSize(filename);
			string dir = _fileSystemService.PathGetDirectoryName(filename);
			string file = _fileSystemService.PathGetFileName(filename);

			_watcher = new FileSystemWatcher(dir, file);
			_watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size;
			_watcher.IncludeSubdirectories = false;
			_watcher.EnableRaisingEvents = true;
			_watcher.Changed += File_Changed;
		}

		private void File_Changed(object sender, FileSystemEventArgs e)
		{
			lock (_locker)
			{
				long newFileSize = _fileSystemService.GetFileSize(e.FullPath);
				if (_lastFileSize < newFileSize)
				{
					Models.FileAppendedMessage message = new Models.FileAppendedMessage();
					message.AppendedData = _fileSystemService.FilePartialRead(e.FullPath, _lastFileSize);
					_messenger.Send(message, 0);
				}

				_lastFileSize = newFileSize;
			}
		}

		public void Dispose()
		{
			Unwatch();
		}
	}
}
