using Microsoft.VisualStudio.TestTools.UnitTesting;
using ChiaLogProcessingApp.Core.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChiaLogProcessingApp.Core.Services;
using Moq;

namespace ChiaLogProcessingApp.Core.Assets.Tests
{
	[TestClass()]
	public class WarningsProcessorTests
	{
		[TestMethod()]
		public void Run_WhenSourceIsSetAndBodyContains_ExpectAppendToWarningsFile()
		{
			IEnumerable<string> contents = new string[] { };
			Mock<IFileSystemService> fileSystemServiceMock = new Mock<IFileSystemService>();
			fileSystemServiceMock.Setup(x => x.FileAppendAllLines(It.IsAny<string>(), It.IsAny<IEnumerable<string>>())).Callback<string, IEnumerable<string>>((f, c) => contents = c);
			Assets.WarningsProcessor warningsProcessor = new WarningsProcessor(fileSystemServiceMock.Object);
			Models.LogEntry logEntry = new Models.LogEntry();
			logEntry.Source = "harvester";
			logEntry.Body = "Looking up qualities took too long";

			warningsProcessor.Process(logEntry);

			Assert.AreEqual(1, contents.Count());
		}

		[TestMethod()]
		public void Run_WhenSourceIsNotSetAndBodyContains_ExpectAppendToWarningsFile()
		{
			IEnumerable<string> contents = new string[] { };
			Mock<IFileSystemService> fileSystemServiceMock = new Mock<IFileSystemService>();
			fileSystemServiceMock.Setup(x => x.FileAppendAllLines(It.IsAny<string>(), It.IsAny<IEnumerable<string>>())).Callback<string, IEnumerable<string>>((f, c) => contents = c);
			Assets.WarningsProcessor warningsProcessor = new WarningsProcessor(fileSystemServiceMock.Object);
			Models.LogEntry logEntry = new Models.LogEntry();
			
			logEntry.Body = "Looking up qualities took too long";

			warningsProcessor.Process(logEntry);

			Assert.AreEqual(0, contents.Count());
		}
	}
}