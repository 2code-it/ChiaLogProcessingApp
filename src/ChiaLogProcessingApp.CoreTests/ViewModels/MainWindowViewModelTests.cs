using Microsoft.VisualStudio.TestTools.UnitTesting;
using ChiaLogProcessingApp.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChiaLogProcessingApp.Core.Services;
using Moq;
using CommunityToolkit.Mvvm.Messaging;
using ChiaLogProcessingApp.Core.Models;
using System.ComponentModel;

namespace ChiaLogProcessingApp.Core.ViewModels.Tests
{
	[TestClass()]
	public class MainWindowViewModelTests
	{

		private Mock<IHttpFileService> _httpServiceMock = new Mock<IHttpFileService>();
		private Mock<IMessenger> _messengerMock = new Mock<IMessenger>();
		private Mock<IFileSystemService> _fileSystemServiceMock = new Mock<IFileSystemService>();
		private Mock<IFileWatcherService> _fileWatcherServiceMock = new Mock<IFileWatcherService>();
		private Mock<IDialogService> _dialogServiceMock = new Mock<IDialogService>();

		private void ResetMocks()
		{
			_httpServiceMock = new Mock<IHttpFileService>();
			_messengerMock = new Mock<IMessenger>();
			_fileSystemServiceMock = new Mock<IFileSystemService>();
			_fileWatcherServiceMock = new Mock<IFileWatcherService>();
			_dialogServiceMock = new Mock<IDialogService>();
		}

		[TestMethod()]
		public void ToggleHttpServiceCommand_WithIsListeningFalse_ShouldStartHttpService()
		{
			string? result = null;
			ResetMocks();
			_httpServiceMock.Setup(o => o.Start(It.IsAny<string>())).Callback<string>(x => result = x);
			UserSettings userSettings = new UserSettings { HttpBindingUrl = "http://+:8080/" };
			MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(_messengerMock.Object, _fileSystemServiceMock.Object, _dialogServiceMock.Object, _fileWatcherServiceMock.Object, _httpServiceMock.Object, userSettings);

			mainWindowViewModel.ToggleHttpServiceCommand.Execute(null);

			Assert.AreEqual(userSettings.HttpBindingUrl, result);
			Assert.IsTrue(mainWindowViewModel.IsHttpListening);
		}

		[TestMethod()]
		public void ToggleHttpServiceCommand_WithIsListeningTrue_ShouldStopHttpService()
		{
			ResetMocks();
			UserSettings userSettings = new UserSettings();
			MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(_messengerMock.Object, _fileSystemServiceMock.Object, _dialogServiceMock.Object, _fileWatcherServiceMock.Object, _httpServiceMock.Object, userSettings);
			mainWindowViewModel.IsHttpListening = true;

			mainWindowViewModel.ToggleHttpServiceCommand.Execute(null);

			_httpServiceMock.Verify(o => o.Stop(), Times.Once());
			Assert.IsFalse(mainWindowViewModel.IsHttpListening);
		}

		[TestMethod()]
		public void ToggleHttpServiceCommand_WhenHttpServiceThrowsException_ShouldOutputUserMessage()
		{
			ResetMocks();
			InvalidEnumArgumentException exception = new InvalidEnumArgumentException("Argument is invalid");
			_httpServiceMock.Setup(x => x.Start(It.IsAny<string>())).Throws(exception);
			UserSettings userSettings = new UserSettings();
			MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(_messengerMock.Object, _fileSystemServiceMock.Object, _dialogServiceMock.Object, _fileWatcherServiceMock.Object, _httpServiceMock.Object, userSettings);

			mainWindowViewModel.ToggleHttpServiceCommand.Execute(null);

			Assert.IsFalse(mainWindowViewModel.IsHttpListening);
			Assert.AreEqual(mainWindowViewModel.UserLogEntries.Count, 1);
		}

		[TestMethod()]
		public void ToggleWatchLogFileCommand_WhenIsWatchingLogFileIsTrue_ShouldInvokeUnwatch()
		{
			ResetMocks();
			UserSettings userSettings = new UserSettings();
			MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(_messengerMock.Object, _fileSystemServiceMock.Object, _dialogServiceMock.Object, _fileWatcherServiceMock.Object, _httpServiceMock.Object, userSettings);
			mainWindowViewModel.IsWatchingLogFile = true;

			mainWindowViewModel.ToggleWatchLogFileCommand.Execute(null);

			_fileWatcherServiceMock.Verify(x => x.Unwatch(), Times.Once());
			Assert.IsFalse(mainWindowViewModel.IsWatchingLogFile);
		}

		[TestMethod()]
		public void ToggleWatchLogFileCommand_WhenIsWatchingLogFileIsFalseAndFileExists_ShouldInvokeWatch()
		{
			ResetMocks();
			UserSettings userSettings = new UserSettings();
			_fileSystemServiceMock.Setup(x => x.FileExists(It.IsAny<string>())).Returns(true);
			MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(_messengerMock.Object, _fileSystemServiceMock.Object, _dialogServiceMock.Object, _fileWatcherServiceMock.Object, _httpServiceMock.Object, userSettings);
			mainWindowViewModel.IsWatchingLogFile = false;

			mainWindowViewModel.ToggleWatchLogFileCommand.Execute(null);

			_fileWatcherServiceMock.Verify(x => x.Watch(It.IsAny<string>()), Times.Once());
			Assert.IsTrue(mainWindowViewModel.IsWatchingLogFile);
		}

		[TestMethod()]
		public void ToggleWatchLogFileCommand_WhenIsWatchingLogFileIsFalseAndNotFileExists_ShouldNotInvokeWatchAndAddUserLogEntry()
		{
			ResetMocks();
			UserSettings userSettings = new UserSettings();
			_fileSystemServiceMock.Setup(x => x.FileExists(It.IsAny<string>())).Returns(false);
			MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(_messengerMock.Object, _fileSystemServiceMock.Object, _dialogServiceMock.Object, _fileWatcherServiceMock.Object, _httpServiceMock.Object, userSettings);
			mainWindowViewModel.IsWatchingLogFile = false;

			mainWindowViewModel.ToggleWatchLogFileCommand.Execute(null);

			_fileWatcherServiceMock.Verify(x => x.Watch(It.IsAny<string>()), Times.Never());
			Assert.IsFalse(mainWindowViewModel.IsWatchingLogFile);
			Assert.AreEqual(1, mainWindowViewModel.UserLogEntries.Count);
		}

		[TestMethod()]
		public void BrowseLogFileCommand_WhenShowOpenFileDialogReturnsNull_ShouldNotSetLogFile()
		{
			ResetMocks();
			string? logFile = ".\\debug.log";
			UserSettings userSettings = new UserSettings();
			_dialogServiceMock.Setup(x => x.ShowOpenFileDialog(null)).Returns<string?>(x => null);
			MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(_messengerMock.Object, _fileSystemServiceMock.Object, _dialogServiceMock.Object, _fileWatcherServiceMock.Object, _httpServiceMock.Object, userSettings);
			mainWindowViewModel.ChiaLogFilePath = logFile;

			mainWindowViewModel.BrowseLogFileCommand.Execute(null);

			Assert.AreEqual(logFile, mainWindowViewModel.ChiaLogFilePath);
		}

		[TestMethod()]
		public void BrowseLogFileCommand_WhenShowOpenFileDialogReturnsNull_ShouldSetLogFile()
		{
			ResetMocks();
			string? logFile = ".\\debug.log";
			UserSettings userSettings = new UserSettings();
			_dialogServiceMock.Setup(x => x.ShowOpenFileDialog(null)).Returns<string?>(x => logFile);
			MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(_messengerMock.Object, _fileSystemServiceMock.Object, _dialogServiceMock.Object, _fileWatcherServiceMock.Object, _httpServiceMock.Object, userSettings);

			mainWindowViewModel.BrowseLogFileCommand.Execute(null);

			Assert.AreEqual(logFile, mainWindowViewModel.ChiaLogFilePath);
		}

		[TestMethod()]
		public void UserWrite_WhenUserLogEntryAdded_ShouldTrimCollectionTo100()
		{
			ResetMocks();
			UserSettings userSettings = new UserSettings();
			IMessenger messenger = new StrongReferenceMessenger();
			MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(messenger, _fileSystemServiceMock.Object, _dialogServiceMock.Object, _fileWatcherServiceMock.Object, _httpServiceMock.Object, userSettings);

			for(int i=0; i<120; i++)
			{
				messenger.Send(new Models.ExceptionMessage(new InvalidOperationException("invalid")));
			}

			Assert.AreEqual(100, mainWindowViewModel.UserLogEntries.Count);
		}

		[TestMethod()]
		public void UserWrite_WhenLogEntryAdded_ShouldTrimCollectionTo500()
		{
			ResetMocks();
			UserSettings userSettings = new UserSettings();
			IMessenger messenger = new StrongReferenceMessenger();
			MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(messenger, _fileSystemServiceMock.Object, _dialogServiceMock.Object, _fileWatcherServiceMock.Object, _httpServiceMock.Object, userSettings);

			for (int i = 0; i < 520; i++)
			{
				messenger.Send(new Models.NewLogEntryMessage(new LogEntry()));
			}

			Assert.AreEqual(500, mainWindowViewModel.LogEntries.Count);
		}
	}
}