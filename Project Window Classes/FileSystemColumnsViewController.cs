using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// Defines the interaction between a ColumnsView and a FileSystemColumnsViewModel.
/// </summary>
public class FileSystemColumnsViewController : IColumnsViewDragDropResponder
{
	public ColumnsView View { get; private set; }
	public FileSystemColumnsViewModel Model { get; private set; }

	public FileSystemColumnsViewController(string rootPath)
	{
		Model = new FileSystemColumnsViewModel(rootPath);
		View = new ColumnsView(Model);

		View.ItemSelected += OnItemSelected;
		View.ItemDeselected += OnItemDeselected;
		View.ItemDoubleClicked += OnItemDoubleClicked;
		View.ItemSlowDoubleClicked += OnItemSlowDoubleClicked; 
		View.ContextClicked += OnContextClicked;
		View.RenameDelegate += RenameItemCheck;

		View.DragDropper = this;
	}

	bool RenameItemCheck(IListViewItem item, string name)
	{
		var fileItem = item as FileSystemListViewItem;
		string[] parts = fileItem.Info.FullName.Split('/');
		parts[parts.Length-1] = name;
		string newfile = string.Join("/", parts);
		var newInfo = new FileInfo(newfile);


		if (newInfo.Extension == "")
		{
			newfile += fileItem.Info.Extension;
		}
		else if (newInfo.Extension != fileItem.Info.Extension)
		{
			if (EditorUtility.DisplayDialog("Change Extension", 
				"Are you sure you wish to changed the extension of this file from '" + fileItem.Info.Extension + "' to '" + newInfo.Extension + "'.",
				"Use '" + fileItem.Info.Extension + "'",
				"Use '" + newInfo.Extension + "'"))
			{
				var index = newfile.LastIndexOf('.');
				var noExt = newfile.Substring(0, index);
				newfile = noExt + fileItem.Info.Extension;
			}
		}

		Directory.Move(fileItem.Info.FullName, newfile);

		int i = 0;
		foreach(var c in Model)
		{
			var f = c as FileSystemListViewModel;
			if (f.RootPath == fileItem.Info.FullName)
			{
				Model.RevertToColumn(i-1);
			}

			i++;
		}

		ColumnsViewUtility.Refresh();
		//		Debug.Log("Renamed file '" + fileItem.Info.FullName + "' to '" + newfile + "'.");
		return true;
	}

	void OnItemSlowDoubleClicked (RectItemPair arg1, int arg2)
	{

	}

	void OnItemSelected (IListViewItem item, int col)
	{
		Model.RevertToColumn(col);
		var fileItem = item as FileSystemListViewItem;
		if (Directory.Exists(fileItem.Info.FullName))
		{
			//			Debug.Log("Making new Column");
			Model.AddColumn(fileItem.Info.FullName);
		}
	}

	void OnItemDeselected(IListViewItem item, int col)
	{

	}

	void OnItemDoubleClicked (IListViewItem item, int col)
	{
		var fileItem = item as FileSystemListViewItem;
		ColumnsViewUtility.OpenFile(fileItem);
	}

	void OnContextClicked(IListViewModel model, IListViewItem item)
	{
		var menu = new ProjectContextMenu();
		menu.Show(model, item);
	}


	#region IListViewDragDropResponder implementation

	public bool ValidDropTarget(IListViewItem item)
	{
		var file = item as FileSystemListViewItem;
		if (Directory.Exists(file.Info.FullName))
		{
			return true;
		}
		return false;
	}

	public void DragPerformed(ListViewDragData data, IListViewModel dropModel, IListViewItem dropItem)
	{
		string baseDest;
		if (dropItem == null)
		{
			baseDest = (dropModel as FileSystemListViewModel).RootPath;
		}
		else
		{
			baseDest = (dropItem as FileSystemListViewItem).Info.FullName;
		}

		foreach(var i in data.ItemList)
		{
			var f = i as FileSystemListViewItem;
			if (f == dropItem)
			{
				continue;
			}

			var destPath = baseDest + "/" + f.Info.Name;
			try
			{
				Directory.Move(f.Info.FullName, destPath);
			}
			catch (System.Exception)
			{

			}

			//			Debug.Log("Moving " + f.Info.Name + " into " + baseDest);
		}
		ColumnsViewUtility.Refresh();
	}

	#endregion
}
