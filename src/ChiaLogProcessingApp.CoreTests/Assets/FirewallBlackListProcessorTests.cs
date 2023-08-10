using Microsoft.VisualStudio.TestTools.UnitTesting;
using ChiaLogProcessingApp.Core.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChiaLogProcessingApp.Core.Services;
using System.Reflection.Metadata.Ecma335;
using NSubstitute;

namespace ChiaLogProcessingApp.Core.Assets.Tests
{
	[TestClass]
	public class FirewallBlackListProcessorTests
	{
		[TestMethod]
		public void Run_WhenRemoteAddressIsSetAndBodyContainsBanning_ExpectAppendToBlackListFile()
		{
			string ip = "127.0.0.1";
			IEnumerable<string> contents = new string[] { };
			IFileSystemService fsMock = Substitute.For<IFileSystemService>();
			fsMock.FileAppendAllLines(Arg.Any<string>(), Arg.Do<IEnumerable<string>>(x=> contents=x));
			Assets.FirewallBlackListProcessor firewallBlackListProcessor = new FirewallBlackListProcessor(fsMock);
			Models.LogEntry logEntry = new Models.LogEntry();
			logEntry.RemoteAddress = ip;
			logEntry.Body = "It's Banning";

			firewallBlackListProcessor.Process(logEntry);

			Assert.AreEqual(ip, contents.First());
		}

		[TestMethod]
		public void Run_WhenRemoteAddressAlreadyExists_ExpectNoAppendToBlackListFile()
		{
			string ip = "127.0.0.1";
			IEnumerable<string> contents = new string[] { };
			IFileSystemService fsMock = Substitute.For<IFileSystemService>();
			fsMock.FileReadAllLines(Arg.Any<string>()).Returns(new string[] { ip });
			fsMock.FileExists(Arg.Any<string>()).Returns(true);
			Assets.FirewallBlackListProcessor firewallBlackListProcessor = new FirewallBlackListProcessor(fsMock);
			Models.LogEntry logEntry = new Models.LogEntry();
			logEntry.RemoteAddress = ip;
			logEntry.Body = "It's Banning";

			firewallBlackListProcessor.Process(logEntry);

			fsMock.DidNotReceive().FileAppendAllLines(Arg.Any<string>(), Arg.Any<IEnumerable<string>>());
		}

		[TestMethod]
		public void Run_WhenRemoteAddressIsSetAndNotBodyContainsBanning_ExpectNoAppendToBlackListFile()
		{
			string ip = "127.0.0.1";
			IEnumerable<string> contents = new string[] { };
			IFileSystemService fsMock = Substitute.For<IFileSystemService>();
			Assets.FirewallBlackListProcessor firewallBlackListProcessor = new FirewallBlackListProcessor(fsMock);
			Models.LogEntry logEntry = new Models.LogEntry();
			logEntry.RemoteAddress = ip;
			logEntry.Body = "It's not";

			firewallBlackListProcessor.Process(logEntry);

			fsMock.DidNotReceive().FileAppendAllLines(Arg.Any<string>(), Arg.Any<IEnumerable<string>>());
		}
	}
}