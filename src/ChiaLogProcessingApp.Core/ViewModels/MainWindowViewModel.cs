using ChiaLogProcessingApp.Core.Models;
using ChiaLogProcessingApp.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChiaLogProcessingApp.Core.ViewModels
{
	public partial class MainWindowViewModel: ObservableObject, IDisposable
	{
		public MainWindowViewModel(IMessenger messenger, IFileSystemService fileSystemService, IDialogService dialogService, IFileWatcherService fileWatcherService, IHttpFileService httpService, Models.UserSettings userSettings)
		{
			_messenger = messenger;
			_fileSystemService = fileSystemService;
			_dialogService = dialogService;
			_fileWatcherService = fileWatcherService;
			_httpService = httpService;
			_userSettings = userSettings;
			_messenger.Register<Core.Models.NewLogEntryMessage>(this, OnNewLogEntry);
			_messenger.Register<Core.Models.ExceptionMessage>(this, OnException);
		}

		private readonly IMessenger _messenger;
		private readonly IFileSystemService _fileSystemService;
		private readonly IDialogService _dialogService;
		private readonly IFileWatcherService _fileWatcherService;
		private readonly IHttpFileService _httpService;

		private readonly Models.UserSettings _userSettings;

		private static readonly object _locker = new object();

		public ObservableCollection<Core.Models.LogEntry> LogEntries { get; private set; } = new ObservableCollection<Models.LogEntry>();
		public ObservableCollection<Core.Models.UserLogEntry> UserLogEntries { get; private set; } = new ObservableCollection<Models.UserLogEntry>();

		public string ChiaLogFilePath
		{
			get { return _userSettings.ChiaLogFilePath; }
			set { _userSettings.ChiaLogFilePath=value; OnPropertyChanged(nameof(ChiaLogFilePath)); }
		}

		public string HttpBindingUrl
		{
			get { return _userSettings.HttpBindingUrl; }
			set { _userSettings.HttpBindingUrl = value; OnPropertyChanged(nameof(HttpBindingUrl)); }
		}

		[ObservableProperty]
		private bool _isWatchingLogFile;
		[ObservableProperty]
		private bool _isHttpListening;

		private int _maxLogEntries = 500;
		private int _maxUserLogEntries = 100;

		[RelayCommand]
		private void ToggleHttpService(object parameter)
		{
			if (IsHttpListening)
			{
				_httpService.Stop();
				IsHttpListening = false;
			}
			else
			{
				try
				{
					_httpService.Start(HttpBindingUrl);
					IsHttpListening = true;
				}
				catch (Exception ex)
				{
					UserWriteError(ex);
				}
			}
		}

		[RelayCommand]
		private void ToggleWatchLogFile(object parameter)
		{
			if (IsWatchingLogFile)
			{
				_fileWatcherService.Unwatch();
				IsWatchingLogFile = false;
			}
			else
			{
				if (!_fileSystemService.FileExists(ChiaLogFilePath))
				{
					UserWrite("Chia log file does not exists", "error");
					return;
				}
				_fileWatcherService.Watch(ChiaLogFilePath);
				IsWatchingLogFile = true;
			}
		}

		[RelayCommand]
		private void BrowseLogFile(object parameter)
		{
			string? file = _dialogService.ShowOpenFileDialog();
			if(file != null)
			{
				ChiaLogFilePath = file;
			}
		}

		private void OnNewLogEntry(object sender, Core.Models.NewLogEntryMessage message)
		{
			lock (_locker)
			{
				LogEntries.Insert(0, message.LogEntry);
				TrimCollection(LogEntries, _maxLogEntries);
			}
		}

		private void OnException(object sender, Core.Models.ExceptionMessage message)
		{
			UserWriteError(message.Exception);
		}

		private void UserWriteError(Exception exception)
		{
			UserWrite(exception.Message, "error");
		}

		private void UserWrite(string message, string severity = "info")
		{
			Core.Models.UserLogEntry userLogEntry = new Models.UserLogEntry();
			userLogEntry.Severity = severity;
			userLogEntry.Message = message;
			userLogEntry.Created = DateTime.Now;

			UserLogEntries.Add(userLogEntry);
			TrimCollection(UserLogEntries, _maxUserLogEntries);
		}

		private void TrimCollection<T>(IList<T> collection, int maxEntries)
		{
			while (collection.Count > maxEntries)
			{
				collection.RemoveAt(collection.Count - 1);
			}
		}

		public void Dispose()
		{
			_messenger.UnregisterAll(this);
		}
	}
}
