using Microsoft.VisualStudio.TestTools.UnitTesting;
using ChiaLogProcessingApp.Core.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChiaLogProcessingApp.Core.Services;
using NSubstitute;

namespace ChiaLogProcessingApp.Core.Assets.Tests
{
	[TestClass]
	public class WarningsProcessorTests
	{
		[TestMethod()]
		public void Run_WhenSourceIsSetAndBodyContains_ExpectAppendToWarningsFile()
		{
			IEnumerable<string> contents = new string[] { };
			IFileSystemService fsMock = Substitute.For<IFileSystemService>();
			fsMock.FileAppendAllLines(Arg.Any<string>(), Arg.Do<IEnumerable<string>>(x => contents = x));
			Assets.WarningsProcessor warningsProcessor = new WarningsProcessor(fsMock);
			Models.LogEntry logEntry = new Models.LogEntry();
			logEntry.Source = "harvester";
			logEntry.Body = "Looking up qualities took too long";

			warningsProcessor.Process(logEntry);

			Assert.AreEqual(1, contents.Count());
		}

		[TestMethod]
		public void Run_WhenSourceIsNotSetAndBodyContains_ExpectAppendToWarningsFile()
		{
			IEnumerable<string> contents = new string[] { };
			IFileSystemService fsMock = Substitute.For<IFileSystemService>();
			fsMock.FileAppendAllLines(Arg.Any<string>(), Arg.Do<IEnumerable<string>>(x => contents = x));
			Assets.WarningsProcessor warningsProcessor = new WarningsProcessor(fsMock);
			Models.LogEntry logEntry = new Models.LogEntry();
			
			logEntry.Body = "Looking up qualities took too long";

			warningsProcessor.Process(logEntry);

			Assert.AreEqual(0, contents.Count());
		}
	}
}