using Microsoft.VisualStudio.TestTools.UnitTesting;
using ChiaLogProcessingApp.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using NSubstitute;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;

namespace ChiaLogProcessingApp.Core.Services.Tests
{
	[TestClass]
	public class FileWatcherServiceTests
	{
		public FileWatcherServiceTests() 
		{
			 _filename = _fileSystemService.PathGetFullPath("./test.txt");
		}

		private IFileSystemService _fileSystemServiceMock = Substitute.For<IFileSystemService>();
		private IMessenger _messengerMock = Substitute.For<IMessenger>();
		private IFileSystemService _fileSystemService = new FileSystemService();

		private readonly string _filename;

		[TestMethod, ExpectedException(typeof(InvalidOperationException))]
		public void Watch_WhenAlreadyWatching_ShouldThrowException()
		{
			ResetMocks();
			ResetFile();
			IFileSystemService fileSystemService = new Services.FileSystemService();
			IFileWatcherService fileWatcherService = new Services.FileWatcherService(fileSystemService, _messengerMock);

			fileWatcherService.Watch(_filename);
			fileWatcherService.Watch(_filename);
		}

		[TestMethod, ExpectedException(typeof(ArgumentException))]
		public void Watch_WhenFileNotExists_ShouldThrowException()
		{
			ResetMocks();
			_fileSystemServiceMock.FileExists(Arg.Any<string>()).Returns(false);
			IFileWatcherService fileWatcherService = new Services.FileWatcherService(_fileSystemServiceMock, _messengerMock);

			fileWatcherService.Watch(_filename);
		}

		[TestMethod]
		public void FileChanged_WhenFileChanged_ShouldMessageAdditionOnly()
		{
			ResetMocks();
			ResetFile();
			string input = "line 1";
			byte[]? data = null;
			_messengerMock.Send(Arg.Do<Models.FileAppendedMessage>(x => data = x.AppendedData), Arg.Any<int>());

			IFileSystemService fileSystemService = new Services.FileSystemService();
			IFileWatcherService fileWatcherService = new Services.FileWatcherService(fileSystemService, _messengerMock);

			fileWatcherService.Watch(_filename);
			_fileSystemService.FileAppendAllLines(_filename, new string[] { input });
			Thread.Sleep(200);
			_fileSystemService.FileAppendAllLines(_filename, new string[] { input });
			Thread.Sleep(200);

			fileWatcherService.Unwatch();

			_messengerMock.Received(2).Send(Arg.Any<Models.FileAppendedMessage>(), Arg.Any<int>());
			string? result = data is null ? null : Encoding.UTF8.GetString(data).Trim();

			Assert.AreEqual(input, result);
		}

		private void ResetMocks()
		{
			_fileSystemServiceMock = Substitute.For<IFileSystemService>();
			_messengerMock = Substitute.For<IMessenger>();
		}

		private void ResetFile()
		{
			File.WriteAllText(_filename, string.Empty);
		}
	}
}