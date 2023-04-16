using Microsoft.VisualStudio.TestTools.UnitTesting;
using ChiaLogProcessingApp.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using Moq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;

namespace ChiaLogProcessingApp.Core.Services.Tests
{
	[TestClass()]
	public class FileWatcherServiceTests
	{
		private Mock<IFileSystemService> _fileSystemServiceMock = new Mock<IFileSystemService>();
		private Mock<IMessenger> _messengerMock = new Mock<IMessenger>();

		private static readonly string _filename = ".\\test.txt";

		[TestMethod, ExpectedException(typeof(InvalidOperationException))]
		public void Watch_WhenAlreadyWatching_ShouldThrowException()
		{
			ResetMocks();
			ResetFile();
			IFileSystemService fileSystemService = new Services.FileSystemService();
			IFileWatcherService fileWatcherService = new Services.FileWatcherService(fileSystemService, _messengerMock.Object);

			fileWatcherService.Watch(_filename);
			fileWatcherService.Watch(_filename);
		}

		[TestMethod, ExpectedException(typeof(ArgumentException))]
		public void Watch_WhenFileNotExists_ShouldThrowException()
		{
			ResetMocks();
			_fileSystemServiceMock.Setup(x=>x.FileExists(It.IsAny<string>())).Returns(false);
			IFileWatcherService fileWatcherService = new Services.FileWatcherService(_fileSystemServiceMock.Object, _messengerMock.Object);

			fileWatcherService.Watch(_filename);
		}

		[TestMethod]
		public void FileChanged_WhenFileChanged_ShouldMessageAdditionOnly()
		{
			ResetMocks();
			ResetFile();
			string? result = null;
			string input = "line 1";
			byte[]? data = null;
			_messengerMock.Setup(x => x.Send(It.IsAny<Models.FileAppendedMessage>(), It.IsAny<int>()))
				.Callback<Models.FileAppendedMessage, int>((m,i) => data = m.AppendedData);

			IFileSystemService fileSystemService = new Services.FileSystemService();
			IFileWatcherService fileWatcherService = new Services.FileWatcherService(fileSystemService, _messengerMock.Object);

			fileWatcherService.Watch(_filename);
			File.AppendAllLines(_filename, new string[] { input });
			Thread.Sleep(200);
			File.AppendAllLines(_filename, new string[] { input });
			Thread.Sleep(200);

			fileWatcherService.Unwatch();

			_messengerMock.Verify(x => x.Send(It.IsAny<Models.FileAppendedMessage>(), It.IsAny<int>()), Times.Exactly(2));
			if(data != null)
			{
				result = Encoding.UTF8.GetString(data).Trim();
			}
			

			Assert.AreEqual(input, result);
		}

		private void ResetMocks()
		{
			_fileSystemServiceMock = new Mock<IFileSystemService>();
			_messengerMock = new Mock<IMessenger>();
		}

		private void ResetFile()
		{
			File.WriteAllText(_filename, string.Empty);
		}
	}
}