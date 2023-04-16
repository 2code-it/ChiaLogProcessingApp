using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChiaLogProcessingApp.Core.Services
{
	public class FileSystemService : IFileSystemService
	{
		public bool FileExists(string? path)
		{
			return File.Exists(path);
		}

		public byte[] FileReadAllBytes(string path)
		{
			return File.ReadAllBytes(path);
		}

		public string[] FileReadAllLines(string path)
		{
			return File.ReadAllLines(path);
		}

		public void FileWriteallBytes(string path, byte[] bytes)
		{
			File.WriteAllBytes(path, bytes);
		}

		public void FileAppendAllLines(string path, IEnumerable<string> contents)
		{
			File.AppendAllLines(path, contents);
		}

		public DateTime FileGetLastWriteTimeUtc(string path)
		{
			return File.GetLastWriteTimeUtc(path);
		}

		public string GetFolderPathUserProfile()
		{
			return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
		}

		public string PathGetFileName(string path)
		{
			string? filename = Path.GetFileName(path);
			if(filename == null)
			{
				throw new ArgumentException("Can't get filename from path", "path");
			}
			return filename;
		}

		public string PathGetDirectoryName(string path)
		{
			string? directoryName = Path.GetDirectoryName(path);
			if (directoryName == null)
			{
				throw new ArgumentException("Can't get directory from path", "path");
			}
			return directoryName;
		}

		public long GetFileSize(string path)
		{
			FileInfo fileInfo = new FileInfo(path);
			return fileInfo.Length;
		}

		public byte[] FilePartialRead(string path, long start, int length = 0)
		{
			try
			{
				using (FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				{
					if(length == 0) 
					{ 
						length = (int)(fs.Length - start);
					}
					byte[] buffer = new byte[length];
					fs.Position = start;
					fs.Read(buffer, 0, length);
					return buffer;
				}
			}
			catch
			{
				throw;
			}
		}
	}
}
