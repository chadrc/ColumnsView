using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Defines nessesary parts for a Columns View to handle a Drag and Drop action.
/// </summary>
public interface IColumnsViewDragDropResponder
{
	bool ValidDropTarget(IListViewItem item);
	void DragPerformed(ListViewDragData data, IListViewModel dropModel, IListViewItem dropItem);
}

/// <summary>
/// Defines what a Column View requires from a Model to draw the elements.
/// </summary>
public interface IColumnsViewModel : IEnumerable<IListViewModel>
{
	int Count { get; }
	event System.Action<IListViewModel> ColumnAdded;
	event System.Action<int> RevertedTo;

	IListViewModel GetLastColumn();
}

/// <summary>
/// Defines what a Item needs to be used by a List View.
/// </summary>
public interface IListViewItem
{
	string Label { get; }
	bool Visible { get; }
	bool Equals(IListViewItem other);
}

/// <summary>
/// Defines nessesary parts for a List View to handle a Drag and Drop action.
/// </summary>
public interface IListViewDragDropResponder
{
	bool ValidDropTarget(IListViewItem item);
	void DragPerformed(ListViewDragData data, IListViewItem dropItem);
}

/// <summary>
/// Defines what a List View requires from a Model to draw the elements.
/// </summary>
public interface IListViewModel : IEnumerable<IListViewItem>
{
	bool Valid { get; }
}