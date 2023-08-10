using Microsoft.VisualStudio.TestTools.UnitTesting;
using ChiaLogProcessingApp.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChiaLogProcessingApp.Core.Services;
using NSubstitute;
using CommunityToolkit.Mvvm.Messaging;
using ChiaLogProcessingApp.Core.Models;
using System.ComponentModel;

namespace ChiaLogProcessingApp.Core.ViewModels.Tests
{
	[TestClass]
	public class MainWindowViewModelTests
	{

		private IHttpFileService _httpServiceMock = Substitute.For<IHttpFileService>();
		private IMessenger _messengerMock = Substitute.For<IMessenger>();
		private IFileSystemService _fileSystemServiceMock = Substitute.For<IFileSystemService>();
		private IFileWatcherService _fileWatcherServiceMock = Substitute.For<IFileWatcherService>();
		private IDialogService _dialogServiceMock = Substitute.For<IDialogService>();

		private void ResetMocks()
		{
			_httpServiceMock = Substitute.For<IHttpFileService>();
			_messengerMock = Substitute.For<IMessenger>();
			_fileSystemServiceMock = Substitute.For<IFileSystemService>();
			_fileWatcherServiceMock = Substitute.For<IFileWatcherService>();
			_dialogServiceMock = Substitute.For<IDialogService>();
		}

		[TestMethod]
		public void ToggleHttpServiceCommand_WithIsListeningFalse_ShouldStartHttpService()
		{
			string? result = null;
			ResetMocks();
			_httpServiceMock.Start(Arg.Do<string>(x => result =x));
			UserSettings userSettings = new UserSettings { HttpBindingUrl = "http://+:8080/" };
			MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(_messengerMock, _fileSystemServiceMock, _dialogServiceMock, _fileWatcherServiceMock, _httpServiceMock, userSettings);

			mainWindowViewModel.ToggleHttpServiceCommand.Execute(null);

			Assert.AreEqual(userSettings.HttpBindingUrl, result);
			Assert.IsTrue(mainWindowViewModel.IsHttpListening);
		}

		[TestMethod]
		public void ToggleHttpServiceCommand_WithIsListeningTrue_ShouldStopHttpService()
		{
			ResetMocks();
			UserSettings userSettings = new UserSettings();
			MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(_messengerMock, _fileSystemServiceMock, _dialogServiceMock, _fileWatcherServiceMock, _httpServiceMock, userSettings);
			mainWindowViewModel.IsHttpListening = true;

			mainWindowViewModel.ToggleHttpServiceCommand.Execute(null);

			_httpServiceMock.Received(1).Stop();
			Assert.IsFalse(mainWindowViewModel.IsHttpListening);
		}

		[TestMethod]
		public void ToggleHttpServiceCommand_WhenHttpServiceThrowsException_ShouldOutputUserMessage()
		{
			ResetMocks();
			InvalidEnumArgumentException exception = new InvalidEnumArgumentException("Argument is invalid");
			_httpServiceMock.When(x => x.Start(Arg.Any<string>())).Throw(exception);
			UserSettings userSettings = new UserSettings();
			MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(_messengerMock, _fileSystemServiceMock, _dialogServiceMock, _fileWatcherServiceMock, _httpServiceMock, userSettings);

			mainWindowViewModel.ToggleHttpServiceCommand.Execute(null);

			Assert.IsFalse(mainWindowViewModel.IsHttpListening);
			Assert.AreEqual(mainWindowViewModel.UserLogEntries.Count, 1);
		}

		[TestMethod]
		public void ToggleWatchLogFileCommand_WhenIsWatchingLogFileIsTrue_ShouldInvokeUnwatch()
		{
			ResetMocks();
			UserSettings userSettings = new UserSettings();
			MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(_messengerMock, _fileSystemServiceMock, _dialogServiceMock, _fileWatcherServiceMock, _httpServiceMock, userSettings);
			mainWindowViewModel.IsWatchingLogFile = true;

			mainWindowViewModel.ToggleWatchLogFileCommand.Execute(null);

			_fileWatcherServiceMock.Received(1).Unwatch();
			Assert.IsFalse(mainWindowViewModel.IsWatchingLogFile);
		}

		[TestMethod]
		public void ToggleWatchLogFileCommand_WhenIsWatchingLogFileIsFalseAndFileExists_ShouldInvokeWatch()
		{
			ResetMocks();
			UserSettings userSettings = new UserSettings();
			_fileSystemServiceMock.FileExists(Arg.Any<string>()).Returns(true);
			MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(_messengerMock, _fileSystemServiceMock, _dialogServiceMock, _fileWatcherServiceMock, _httpServiceMock, userSettings);
			mainWindowViewModel.IsWatchingLogFile = false;

			mainWindowViewModel.ToggleWatchLogFileCommand.Execute(null);

			_fileWatcherServiceMock.Received(1).Watch(Arg.Any<string>());
			Assert.IsTrue(mainWindowViewModel.IsWatchingLogFile);
		}

		[TestMethod]
		public void ToggleWatchLogFileCommand_WhenIsWatchingLogFileIsFalseAndNotFileExists_ShouldNotInvokeWatchAndAddUserLogEntry()
		{
			ResetMocks();
			UserSettings userSettings = new UserSettings();
			_fileSystemServiceMock.FileExists(Arg.Any<string>()).Returns(false);
			MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(_messengerMock, _fileSystemServiceMock, _dialogServiceMock, _fileWatcherServiceMock, _httpServiceMock, userSettings);
			mainWindowViewModel.IsWatchingLogFile = false;

			mainWindowViewModel.ToggleWatchLogFileCommand.Execute(null);

			_fileWatcherServiceMock.Received(0).Watch(Arg.Any<string>());
			Assert.IsFalse(mainWindowViewModel.IsWatchingLogFile);
			Assert.AreEqual(1, mainWindowViewModel.UserLogEntries.Count);
		}

		[TestMethod]
		public void BrowseLogFileCommand_WhenShowOpenFileDialogReturnsNull_ShouldNotSetLogFile()
		{
			ResetMocks();
			string? logFile = ".\\debug.log";
			UserSettings userSettings = new UserSettings();
			_dialogServiceMock.ShowOpenFileDialog(null).Returns(x => null);
			MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(_messengerMock, _fileSystemServiceMock, _dialogServiceMock, _fileWatcherServiceMock, _httpServiceMock, userSettings);
			mainWindowViewModel.ChiaLogFilePath = logFile;

			mainWindowViewModel.BrowseLogFileCommand.Execute(null);

			Assert.AreEqual(logFile, mainWindowViewModel.ChiaLogFilePath);
		}

		[TestMethod]
		public void BrowseLogFileCommand_WhenShowOpenFileDialogReturnsNull_ShouldSetLogFile()
		{
			ResetMocks();
			string? logFile = ".\\debug.log";
			UserSettings userSettings = new UserSettings();
			_dialogServiceMock.ShowOpenFileDialog(null).Returns(x => logFile);
			MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(_messengerMock, _fileSystemServiceMock, _dialogServiceMock, _fileWatcherServiceMock, _httpServiceMock, userSettings);

			mainWindowViewModel.BrowseLogFileCommand.Execute(null);

			Assert.AreEqual(logFile, mainWindowViewModel.ChiaLogFilePath);
		}

		[TestMethod]
		public void UserWrite_WhenUserLogEntryAdded_ShouldTrimCollectionTo100()
		{
			ResetMocks();
			UserSettings userSettings = new UserSettings();
			IMessenger messenger = new StrongReferenceMessenger();
			MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(messenger, _fileSystemServiceMock, _dialogServiceMock, _fileWatcherServiceMock, _httpServiceMock, userSettings);

			for (int i=0; i<120; i++)
			{
				messenger.Send(new Models.ExceptionMessage(new InvalidOperationException("invalid")));
			}

			Assert.AreEqual(100, mainWindowViewModel.UserLogEntries.Count);
		}

		[TestMethod]
		public void UserWrite_WhenLogEntryAdded_ShouldTrimCollectionTo500()
		{
			ResetMocks();
			UserSettings userSettings = new UserSettings();
			IMessenger messenger = new StrongReferenceMessenger();
			MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(messenger, _fileSystemServiceMock, _dialogServiceMock, _fileWatcherServiceMock, _httpServiceMock, userSettings);

			for (int i = 0; i < 520; i++)
			{
				messenger.Send(new Models.NewLogEntryMessage(new LogEntry()));
			}

			Assert.AreEqual(500, mainWindowViewModel.LogEntries.Count);
		}
	}
}