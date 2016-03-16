using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// Keeps track of a collection of List View Models each representing a column.
/// </summary>
public class FileSystemColumnsViewModel : IColumnsViewModel
{
	private List<IListViewModel> lists = new List<IListViewModel>();

	public event System.Action<IListViewModel> ColumnAdded;
	public event System.Action<int> RevertedTo;
	public int Count { get { return lists.Count; } }

	public FileSystemColumnsViewModel(string rootPath)
	{
		AddColumn(rootPath);
	}

	public FileSystemListViewModel GetColumn(int index)
	{
		if (index < lists.Count && lists.Count > 0)
		{
			return lists[index] as FileSystemListViewModel;
		}
		return null;
	}

	public int GetColumnOfItem(FileSystemListViewItem item)
	{
		int col = 0;
		foreach(var l in lists)
		{
			foreach(var i in l)
			{
				if (i.Equals(item))
				{
					return col;
				}
			}
			col++;
		}
		return col;
	}

	public void AddColumn(string path)
	{
		var lvm = new FileSystemListViewModel(path);
		lists.Add(lvm);
		if (ColumnAdded != null)
		{
			ColumnAdded(lvm);
		}
	}

	public void RevertToColumn(int index)
	{
		if (index >= lists.Count-1)
		{
			return;
		}

		var newList = new List<IListViewModel>();
		for(int i=0; i<=index; i++)
		{
			newList.Add(lists[i]);
		}
		lists = newList;

		if (RevertedTo != null)
		{
			RevertedTo(index);
		}
	}

	public IListViewModel GetLastColumn()
	{
		return lists[lists.Count-1];
	}

	#region IEnumerable implementation

	public IEnumerator<IListViewModel> GetEnumerator()
	{
		return lists.GetEnumerator();
	}

	#endregion

	#region IEnumerable implementation

	System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	#endregion
}
