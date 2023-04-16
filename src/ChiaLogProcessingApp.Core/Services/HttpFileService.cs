using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChiaLogProcessingApp.Core.Services
{
	public class HttpFileService : IHttpFileService
	{
		public HttpFileService(IFileSystemService fileSystemService) 
		{
			_fileSystemService = fileSystemService;
		}

		private readonly IFileSystemService _fileSystemService;

		private HttpListener? _httpListener;
		private static readonly object _locker = new object();

		public void Start(string httpBindingUrl)
		{
			if (_httpListener != null) throw new InvalidOperationException("Service already started");

			_httpListener = new HttpListener();
			_httpListener.Prefixes.Add(httpBindingUrl);
			_httpListener.Start();
			_httpListener.BeginGetContext(new AsyncCallback(ProcessRequest), null);
		}

		public void Stop()
		{
			if(_httpListener != null)
			{
				_httpListener.Stop();
				_httpListener = null;
			}
		}

		public bool IsRunning()
		{
			return _httpListener != null;
		}

		private void ProcessRequest(IAsyncResult asyncResult)
		{
			lock (_locker)
			{
				if (_httpListener == null)
				{
					return;
				}

				HttpListenerContext context = _httpListener.EndGetContext(asyncResult);
				context.Response.SendChunked = true;
				context.Response.ContentType = "text/plain; charset=UTF-8";
				context.Response.ContentEncoding = Encoding.UTF8;
				

				string? file = $".\\{context.Request.Url?.AbsolutePath}";
				if(_fileSystemService.FileExists(file)) 
				{
					DateTime lastWrite = _fileSystemService.FileGetLastWriteTimeUtc(file);
					context.Response.AddHeader("Last-Modified", lastWrite.ToString("ddd, dd MMM yyyy HH:mm:ss 'UTC'"));

					if (context.Request.HttpMethod == "GET")
					{
						context.Response.StatusCode = (int)HttpStatusCode.OK;
						byte[] data = _fileSystemService.FileReadAllBytes(file);
						context.Response.OutputStream.Write(data);
					}
					if (context.Request.HttpMethod == "HEAD")
					{
						context.Response.StatusCode = (int)HttpStatusCode.NoContent;
					}
				}
				else
				{
					context.Response.StatusCode = (int)HttpStatusCode.NotFound;					
				}

				context.Response.OutputStream.Flush();
				context.Response.OutputStream.Close();

				_httpListener.BeginGetContext(new AsyncCallback(ProcessRequest), null);
			}
		}

		public void Dispose()
		{
			Stop();
		}
	}
}
