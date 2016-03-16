using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// Data for a drag event inside the List and Column Views
/// </summary>
public class ListViewDragData
{
	public List<IListViewItem> ItemList = new List<IListViewItem>();
}

// NOTE: Possible refactor needed that would replace RectItemPair and RectItemModelPair
// View classes that store same information and are used to draw the items similar to List and Column Views but for individual List Items

/// <summary>
/// Utility class for giving reference between a Rect from GUI element and the Model that was drawn there.
/// </summary>
public class RectItemPair
{
	public Rect Box;
	public IListViewItem Item;

	public RectItemPair(Rect box, IListViewItem item)
	{
		Box = box;
		Item = item;
	}
}

/// <summary>
/// Utility class for giving reference between a Rect from GUI element and the Model that was drawn there.
/// </summary>
public class RectItemModelPair
{
	public Rect Box;
	public IListViewModel Model;

	public RectItemModelPair(Rect box, IListViewModel model)
	{
		Box = box;
		Model = model;
	}
}

/// <summary>
/// Static class for storing commonly used methods and data for Columns View
/// </summary>
public static class ColumnsViewUtility
{
	public static event System.Action ProjectRefresh;

	public static void Refresh()
	{
		AssetDatabase.Refresh();
		if (ProjectRefresh != null)
		{
			ProjectRefresh();
		}
	}

	public static void OpenFile(FileSystemListViewItem item)
	{
		int assetIndex = item.Info.FullName.IndexOf("/Assets/");
		string relativePath = item.Info.FullName.Substring(assetIndex+1);
		AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath(relativePath, typeof(Object)));
	}
}