using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChiaLogProcessingApp.Core.Services
{
	public interface IFileSystemService
	{
		bool FileExists(string path);
		byte[] FileReadAllBytes(string path);
		string[] FileReadAllLines(string path);
		void FileWriteallBytes(string path, byte[] bytes);
		void FileAppendAllLines(string path, IEnumerable<string> contents);
		DateTime FileGetLastWriteTimeUtc(string path);
		string GetFolderPathUserProfile();
		string PathGetFileName(string path);
		string PathGetDirectoryName(string path);
		long GetFileSize(string path);
		byte[] FilePartialRead(string path, long start, int length = 0);
	}
}
