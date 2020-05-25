using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

public class FileManager
{
	public static void SetAppFolder(string path)
	{
		if (Directory.Exists(path))
		{
			while (path.EndsWith("\\") || path.EndsWith("/") || path.EndsWith(FileManager.GetPathSeparator().ToString()))
			{
				path = path.Remove(FileManager._appfolder.Length - 2);
			}
			FileManager._appfolder = path;
		}
	}

	public static string ReplacePathSeparators(string path)
	{
		return path.Replace('/', FileManager.GetPathSeparator()).Replace('\\', FileManager.GetPathSeparator());
	}

	public static char GetPathSeparator()
	{
		return Path.DirectorySeparatorChar;
	}

	public static bool FileExists(string path)
	{
		return File.Exists(path);
	}

	public static bool DictionaryExists(string path)
	{
		return Directory.Exists(path);
	}

	public static string[] ReadAllLines(string path)
	{
		return File.ReadAllLines(path, Encoding.UTF8);
	}

	public static void WriteToFile(IEnumerable<string> data, string path, bool removeempty = false)
	{
		string[] contents;
		if (removeempty)
		{
			contents = (from line in data
						where !string.IsNullOrEmpty(line.Replace(Environment.NewLine, string.Empty).Replace("\r\n", string.Empty).Replace("\n", string.Empty).Replace(" ", string.Empty))
						select line).ToArray<string>();
		}
		else
		{
			contents = data.ToArray<string>();
		}
		File.WriteAllLines(path, contents, Encoding.UTF8);
	}

	public static void WriteStringToFile(string data, string path)
	{
		File.WriteAllText(path, data, Encoding.UTF8);
	}

	public static void AppendFile(string data, string path, bool newLine = true)
	{
		string[] array = FileManager.ReadAllLines(path);
		if (!newLine || array.Length == 0 || array[array.Length - 1].EndsWith(Environment.NewLine) || array[array.Length - 1].EndsWith("\n"))
		{
			File.AppendAllText(path, data, Encoding.UTF8);
			return;
		}
		File.AppendAllText(path, Environment.NewLine + data, Encoding.UTF8);
	}

	public static void RenameFile(string path, string newpath)
	{
		File.Move(path, newpath);
	}

	public static void DeleteFile(string path)
	{
		File.Delete(path);
	}

	public static void ReplaceLine(int line, string text, string path)
	{
		string[] array = FileManager.ReadAllLines(path);
		array[line] = text;
		FileManager.WriteToFile(array, path, false);
	}

	public static void RemoveEmptyLines(string path)
	{
		string[] array = (from s in FileManager.ReadAllLines(path)
						  where !string.IsNullOrEmpty(s.Replace(Environment.NewLine, string.Empty).Replace("\r\n", string.Empty).Replace("\n", string.Empty).Replace(" ", string.Empty))
						  select s).ToArray<string>();
		if (FileManager.ReadAllLines(path) != array)
		{
			FileManager.WriteToFile(array, path, false);
		}
	}

	private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs = true)
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(sourceDirName);
		if (!directoryInfo.Exists)
		{
			throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirName);
		}
		DirectoryInfo[] directories = directoryInfo.GetDirectories();
		if (Directory.Exists(destDirName))
		{
			Directory.Delete(destDirName, true);
		}
		Directory.CreateDirectory(destDirName);
		FileInfo[] files = directoryInfo.GetFiles();
		FileInfo[] array = files;
		foreach (FileInfo fileInfo in array)
		{
			string destFileName = Path.Combine(destDirName, fileInfo.Name);
			fileInfo.CopyTo(destFileName, true);
		}
		if (copySubDirs)
		{
			DirectoryInfo[] array3 = directories;
			foreach (DirectoryInfo directoryInfo2 in array3)
			{
				string destDirName2 = Path.Combine(destDirName, directoryInfo2.Name);
				FileManager.DirectoryCopy(directoryInfo2.FullName, destDirName2, true);
			}
		}
	}

	private static string _appfolder = string.Empty;
}