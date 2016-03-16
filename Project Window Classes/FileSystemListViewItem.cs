using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// Keeps track of information of a single file.
/// </summary>
public class FileSystemListViewItem: IListViewItem
{
	public FileInfo Info { get; private set; }

	public FileSystemListViewItem(string filepath)
	{
		Info = new FileInfo(filepath);
	}

	#region IListViewItem implementation

	public string Label { get { return Info.Name; } }

	public bool Visible
	{
		get
		{
			return Info.Extension != ".meta";
		}
	}

	public bool Equals(IListViewItem other)
	{
		if (other == null)
		{
			return false;
		}

		if (other is FileSystemListViewItem)
		{
			var otherFile = other as FileSystemListViewItem;
			return Info.FullName == otherFile.Info.FullName;
		}
		else
		{
			return Label == other.Label;
		}
	}

	#endregion
}