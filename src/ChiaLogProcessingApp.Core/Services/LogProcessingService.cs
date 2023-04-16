using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ChiaLogProcessingApp.Core.Services
{
	public class LogProcessingService: IDisposable
	{
		public LogProcessingService(IMessenger messenger, IServiceProvider serviceProvider) 
		{
			_messenger = messenger;
			_serviceProvider = serviceProvider;
			_processors = GetProcessors();
			_messenger.Register<Models.FileAppendedMessage>(this, OnFileAppended);
		}

		private readonly IMessenger _messenger;
		private Assets.ILogEntryProcessor[] _processors;
		private IServiceProvider _serviceProvider;

		private static readonly string _dateFormat = "yyyy-MM-ddTHH:mm:ss.fff";

		private void OnFileAppended(object sender, Models.FileAppendedMessage message)
		{
			if(message.AppendedData == null) return; 

			string dataString = Encoding.UTF8.GetString(message.AppendedData);
			string[] lines = dataString.Split("\r\n");

			Regex regex = new Regex("^\\d{4}\\-\\d{2}\\-\\d{2}T");
			Models.LogEntry[] entries = lines.Where(x => regex.IsMatch(x)).Select(x => GetEntryFromRawDataLine(x)).ToArray();
			foreach( Models.LogEntry entry in entries)
			{
				ProcessLogEntry(entry);
			}
		}

		private void ProcessLogEntry(Models.LogEntry entry)
		{
			foreach(Assets.ILogEntryProcessor processor in _processors)
			{
				try
				{
					processor.Process(entry);
				}
				catch (Exception ex)
				{
					_messenger.Send(new Models.ExceptionMessage(ex));
				}
			}
			_messenger.Send(new Models.NewLogEntryMessage(entry));
		}

		private Models.LogEntry GetEntryFromRawDataLine(string line)
		{
			string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries).Where(x => x != ":").Select(x => x.Trim(':')).ToArray();

			Models.LogEntry entry = new Models.LogEntry();
			entry.Created = DateTime.ParseExact(parts[0], _dateFormat, CultureInfo.InvariantCulture);
			entry.Severity = parts[3];
			entry.Source = parts[1];
			entry.Body = string.Join(' ', parts.Skip(4));

			Regex regexIP = new Regex("\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}");
			Match match = regexIP.Match(line);
			if (match.Success)
			{
				entry.RemoteAddress = match.Value;
			}
			return entry;
		}

		private Assets.ILogEntryProcessor[] GetProcessors()
		{
			return this.GetType().Assembly.GetTypes().Where(x => x.IsClass && x.IsAssignableTo(typeof(Assets.ILogEntryProcessor)))
				.Select(x => (Assets.ILogEntryProcessor)ActivatorUtilities.CreateInstance(_serviceProvider, x))
				.ToArray();
		}

		public void Dispose()
		{
			_messenger.Unregister<Models.FileAppendedMessage>(this);
		}
	}
}
