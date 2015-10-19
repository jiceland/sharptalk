using System;
using System.Collections.Generic;
using System.IO;

namespace sharptalk
{
	public static class FileCache
	{
		static List<string> fileNameList = new List<string>();
		static List<long> fileSizeList = new List<long>();
		static int cacheSize = 500; // Mb

		public static string CacheDir {
			get
			{
				return Path.Combine(UserSettings.SettingsDir, "cache");
			}
		}

		public static bool Exists (string filename)
		{
			if (fileNameList.Count == 0)
				GetFiles();

			if(fileNameList.Contains(filename))
				return true;
			else
				return false;
		}

		public static void Add (string filename)
		{
			String cacheSizeString = UserSettings.getValue("CacheSize");
			if(cacheSizeString != string.Empty)
				cacheSize = Convert.ToInt32(cacheSizeString);

			long filesize = new FileInfo(Path.Combine(CacheDir, filename)).Length;

			if (fileNameList.Count == 0)
				GetFiles ();

			while(TotalSize() + filesize > cacheSize * 1024 * 1024)
			{
				File.Delete(fileNameList[0]);
				fileNameList.RemoveAt(0);
				fileSizeList.RemoveAt(0);
			}
			fileNameList.Add(filename);
			fileSizeList.Add(filesize);
		}

		static public void Clear ()
		{
			for (int i=0; i < fileSizeList.Count; i++)
			{
				File.Delete(fileNameList[i]);
			}
			fileNameList.Clear();
			fileSizeList.Clear();
		}

		static long TotalSize()
	    {
			long size = 0;
			for (int i=0; i < fileSizeList.Count; i++)
			{
	    		size += fileSizeList[i];
			}
			return size;
    	}

		static void GetFiles ()
		{
			string[] fileNames = Directory.GetFiles(CacheDir);
			DateTime[] creationTimes = new DateTime[fileNames.Length];
			long[] fileSizes = new long[fileNames.Length];
			for (int i=0; i < fileNames.Length; i++)
			{
				creationTimes[i] = new FileInfo (fileNames [i]).CreationTime;
				fileSizes[i] = new FileInfo(fileNames[i]).Length;
			}
			Array.Sort(creationTimes, fileNames);
			Array.Sort(creationTimes, fileSizes);
			fileNameList = new List<string>(fileNames);
			fileSizeList = new List<long>(fileSizes);
		}
	}
}

