using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// Keeps track of a specified directory. Does not know information about sub-directories.
/// </summary>
public class FileSystemListViewModel : IListViewModel
{
	public string RootPath { get; private set; }

	public bool Valid { get; private set; }

	private List<IListViewItem> items = new List<IListViewItem>();

	public FileSystemListViewModel(string rootPath)
	{
		RootPath = rootPath;
		LoadFiles();
		ColumnsViewUtility.ProjectRefresh += OnProjectRefresh;
	}

	~FileSystemListViewModel()
	{
		ColumnsViewUtility.ProjectRefresh -= OnProjectRefresh;
	}

	private void OnProjectRefresh()
	{
		items.Clear();
		LoadFiles();
	}

	private void LoadFiles()
	{
		if (!Directory.Exists(RootPath))
		{
			Valid = false;
			return;
		}
		var paths = Directory.GetFileSystemEntries(RootPath);

		foreach(var p in paths)
		{
			items.Add(new FileSystemListViewItem(p));
		}
	}

	#region IEnumerable implementation

	public IEnumerator<IListViewItem> GetEnumerator()
	{
		return items.GetEnumerator();
	}

	#endregion

	#region IEnumerable implementation

	System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	#endregion
}