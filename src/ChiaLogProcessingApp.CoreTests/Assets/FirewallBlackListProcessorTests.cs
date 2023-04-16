using Microsoft.VisualStudio.TestTools.UnitTesting;
using ChiaLogProcessingApp.Core.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChiaLogProcessingApp.Core.Services;
using Moq;
using System.Reflection.Metadata.Ecma335;

namespace ChiaLogProcessingApp.Core.Assets.Tests
{
	[TestClass()]
	public class FirewallBlackListProcessorTests
	{
		[TestMethod()]
		public void Run_WhenRemoteAddressIsSetAndBodyContainsBanning_ExpectAppendToBlackListFile()
		{
			string ip = "127.0.0.1";
			IEnumerable<string> contents = new string[] { };
			Mock<IFileSystemService> fileSystemServiceMock = new Mock<IFileSystemService>();
			fileSystemServiceMock.Setup(x => x.FileAppendAllLines(It.IsAny<string>(), It.IsAny<IEnumerable<string>>())).Callback<string, IEnumerable<string>>((f, c) => contents = c);
			Assets.FirewallBlackListProcessor firewallBlackListProcessor = new FirewallBlackListProcessor(fileSystemServiceMock.Object);
			Models.LogEntry logEntry = new Models.LogEntry();
			logEntry.RemoteAddress = ip;
			logEntry.Body = "It's Banning";

			firewallBlackListProcessor.Process(logEntry);

			Assert.AreEqual(ip, contents.First());
		}

		[TestMethod()]
		public void Run_WhenRemoteAddressAlreadyExists_ExpectNoAppendToBlackListFile()
		{
			string ip = "127.0.0.1";
			IEnumerable<string> contents = new string[] { };
			Mock<IFileSystemService> fileSystemServiceMock = new Mock<IFileSystemService>();
			fileSystemServiceMock.Setup(x => x.FileReadAllLines(It.IsAny<string>())).Returns(new string[] { ip });
			fileSystemServiceMock.Setup(x => x.FileExists(It.IsAny<string>())).Returns(true);
			Assets.FirewallBlackListProcessor firewallBlackListProcessor = new FirewallBlackListProcessor(fileSystemServiceMock.Object);
			Models.LogEntry logEntry = new Models.LogEntry();
			logEntry.RemoteAddress = ip;
			logEntry.Body = "It's Banning";

			firewallBlackListProcessor.Process(logEntry);

			fileSystemServiceMock.Verify(x => x.FileAppendAllLines(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()), Times.Never);
		}

		[TestMethod()]
		public void Run_WhenRemoteAddressIsSetAndNotBodyContainsBanning_ExpectNoAppendToBlackListFile()
		{
			string ip = "127.0.0.1";
			IEnumerable<string> contents = new string[] { };
			Mock<IFileSystemService> fileSystemServiceMock = new Mock<IFileSystemService>();
			Assets.FirewallBlackListProcessor firewallBlackListProcessor = new FirewallBlackListProcessor(fileSystemServiceMock.Object);
			Models.LogEntry logEntry = new Models.LogEntry();
			logEntry.RemoteAddress = ip;
			logEntry.Body = "It's not";

			firewallBlackListProcessor.Process(logEntry);

			fileSystemServiceMock.Verify(x => x.FileAppendAllLines(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()), Times.Never);
		}
	}
}