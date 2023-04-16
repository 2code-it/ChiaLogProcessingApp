using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChiaLogProcessingApp.Core.Services
{
	public class JsonSettingsService : IJsonSettingsService
	{
		public JsonSettingsService(IFileSystemService fileSystemService)
		{
			_fileSystemService = fileSystemService;
		}

		private readonly IFileSystemService _fileSystemService;

		public T Load<T>() where T : class
		{
			string file = GetFilePath<T>();
			if (_fileSystemService.FileExists(file))
			{
				byte[] json = _fileSystemService.FileReadAllBytes(file);
				T? settings = JsonSerializer.Deserialize<T>(json);
				if(settings != null)
				{
					return settings;
				}
			}
			return Activator.CreateInstance<T>();
		}

		public void Save<T>(T settings) where T : class
		{
			string file = GetFilePath<T>();
			using (MemoryStream ms = new MemoryStream())
			{
				JsonSerializer.Serialize(ms, settings);
				_fileSystemService.FileWriteallBytes(file, ms.ToArray());
			}
			
		}

		private string GetFilePath<T>()
		{
			return $".\\{typeof(T).Name}.json";
		}
	}
}
