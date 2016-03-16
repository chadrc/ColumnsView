using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ListView
{
	public Vector2 ScrollPos { get; private set; }
	public GUIStyle SelectedStyle { get; set; }

	public event System.Action<IListViewItem> Selected;
	public event System.Action<IListViewItem> Deselected;
	public event System.Action<IListViewItem> DoubleClicked;
	public event System.Action<RectItemPair> SlowDoubleClicked;
	public event System.Action<IListViewModel, IListViewItem> ContextClicked;

	private IListViewModel model;
	public IListViewModel Model 
	{
		get
		{
			return model;
		}

		set
		{
			if (model != value)
			{
				model = value;
				selectedItem = null;
				selectedRect = default(Rect);
			}
		}
	}

	private Rect selectedRect;
	private Rect selectionRect;
	private IListViewItem selectedItem;

	private double clickTime;
	private const double doubleClickTime = .2f;
	private const double timeoutForSlowDC = 1.0f;

	private bool showRenameText = false;
	private string renameText = "";
	public System.Func<IListViewItem, string, bool> RenameDelegate;

//	private IListViewItem dragItem;
	private RectItemPair dropPair;
	public IListViewDragDropResponder DragDropper { get; set; }
	public List<RectItemPair> ItemPairs { get; private set; }

	public ListView(IListViewModel model)
	{
		this.Model = model;
		SelectedStyle = new GUIStyle();
		SelectedStyle.normal.textColor = Color.white;
	}

	/// <summary>
	/// Draws List View with EditorGUILayout.
	/// </summary>
	/// <param name="items">Items to draw.</param>
	public void Draw()
	{
		ScrollPos = EditorGUILayout.BeginScrollView(ScrollPos, GUIStyle.none, new GUIStyle(GUI.skin.verticalScrollbar));
		EditorGUI.DrawRect(selectedRect, new Color(62f/255f, 125f/255f, 231f/255f));
		ItemPairs = new List<RectItemPair>();

		foreach(var i in Model)
		{
			if (!i.Visible)
			{
				continue;
			}


			if (i.Equals(selectedItem))
			{
				if (showRenameText)
				{
					GUI.SetNextControlName("RenameTextField");
					renameText = EditorGUILayout.TextField(renameText);
					if ((Event.current.type == EventType.KeyUp && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter)))
					{
						if (RenameDelegate(selectedItem, renameText))
						{
							//selectedItem.Label = renameText;
						}
						else
						{
							Debug.LogError("Could not rename file '" + selectedItem.Label + "' to '" + renameText + "'.");
						}
						showRenameText = false;
					}
					else
					{
						EditorGUI.FocusTextInControl("RenameTextField");
					}
				}
				else
				{
					EditorGUILayout.LabelField(i.Label, SelectedStyle);
					var temp = GUILayoutUtility.GetLastRect();
					if (temp.x > 0 && temp.y > 0)
					{
						selectedRect = temp;
					}
				}
			}
			else
			{
				EditorGUILayout.LabelField(i.Label);
			}

			if (Event.current.isMouse && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
			{
				CheckMouseEventsForLastItemRect(i);
			}
			ItemPairs.Add(new RectItemPair(GUILayoutUtility.GetLastRect(), i));
		}

		EditorGUILayout.EndScrollView();

		var baseRect = GUILayoutUtility.GetLastRect();
		foreach(var i in ItemPairs)
		{
			i.Box.x += baseRect.x;
		}

		if (Event.current.type == EventType.MouseUp && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
		{
			if (Deselected != null)
			{
				Deselected(selectedItem);
			}
			selectedItem = null;
			selectedRect = default(Rect);
			Event.current.Use();
		}
		else if (Event.current.type == EventType.ContextClick && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
		{
			IListViewItem item = null;
			foreach(var i in ItemPairs)
			{
				if (i.Box.Contains(Event.current.mousePosition))
				{
					item = i.Item;
					break;
				}
			}
			if (ContextClicked != null)
			{
				ContextClicked(model, item);
			}
//			Event.current.Use();
//			Debug.Log("Context Click");
		}
		else if (Event.current.type == EventType.DragUpdated)
		{
//			Debug.Log("Scroll View Drag Update");
			bool validTarget = false;
			foreach(var p in ItemPairs)
			{
				if (p.Box.Contains(Event.current.mousePosition))
				{
					// Valid Check
					if (DragDropper != null && DragDropper.ValidDropTarget(p.Item))
					{
						dropPair = p;
						validTarget = true;
					}
				}
			}

			if (validTarget)
			{
				DragAndDrop.visualMode = DragAndDropVisualMode.Move;
			}
		}
		else if (Event.current.type == EventType.DragPerform)
		{
//			Debug.Log("Scroll View Drag Perform");
			if (DragDropper != null)
			{
				DragAndDrop.AcceptDrag();
				var data = DragAndDrop.GetGenericData("ListViewData") as ListViewDragData;
				DragDropper.DragPerformed(data, dropPair.Item);
			}
//			Debug.Log("Dropping " + data.ItemList[0].Label + " on to " + dropPair.Item.Label);
		}
	}

	private void CheckMouseEventsForLastItemRect(IListViewItem item)
	{
		var evt = Event.current;
		var eType = evt.type;
//		Debug.Log("Checking Mouse Events: " + eType);
		switch (eType)
		{
			case EventType.MouseDown:
//				selectedRect = GUILayoutUtility.GetLastRect();
//				dragItem = item;
//
				if (DragDropper != null)
				{
					DragAndDrop.PrepareStartDrag();
					ListViewDragData data = new ListViewDragData();
					data.ItemList.Add(item);
					DragAndDrop.SetGenericData("ListViewData", data);
					DragAndDrop.objectReferences = new Object[1] { item as Object };
					Event.current.Use();
				}
//				Debug.Log("Mouse Down");
				break;

			case EventType.MouseUp:
				HandleClick(item);
				clickTime = EditorApplication.timeSinceStartup;
				Event.current.Use();
//				Debug.Log("Mouse Up");
				break;

			case EventType.MouseDrag:
				if (DragDropper == null)
				{
					break;
				}
				var dragData = DragAndDrop.GetGenericData("ListViewData") as ListViewDragData;
				if (dragData != null)
				{
					DragAndDrop.StartDrag("ListViewDrag");
//					Debug.Log("Drag Start");
					evt.Use();
				}
//				Debug.Log("Mouse Drag");
				break;
		}
	}

	private void HandleClick(IListViewItem i)
	{
		selectedRect = GUILayoutUtility.GetLastRect();

//		Debug.Log("Handle Click");
		if (selectedItem != null)
		{
			if (selectedItem.Equals(i))
			{
				double timeDif = EditorApplication.timeSinceStartup - clickTime;
				if (timeDif <= doubleClickTime)
				{
					if (DoubleClicked != null)
					{
						DoubleClicked(i);
					}
				}
				else if (timeDif <= timeoutForSlowDC)
				{
					if (SlowDoubleClicked != null)
					{
						SlowDoubleClicked(new RectItemPair(selectedRect, selectedItem));
					}
					showRenameText = true;
					renameText = selectedItem.Label;
				}
				return;
			}

			if (Deselected != null)
			{
				Deselected(i);
			}
		}

		selectedItem = i;
		if (Selected != null)
		{
			Selected(i);
		}
	}
}