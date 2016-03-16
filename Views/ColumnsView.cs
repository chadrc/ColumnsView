using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

//NOTE: Possibly need to refactor to include what is in List View because handling inputs with nested views causes some to be ignored.

/// <summary>
/// Used for drawing the Columns GUI. Keeps track of mouse and keyboard inputs, raising events when they occurr.
/// </summary>
public class ColumnsView
{
	private IColumnsViewModel model;

	public event System.Action<IListViewItem, int> ItemSelected;
	public event System.Action<IListViewItem, int> ItemDeselected;
	public event System.Action<IListViewItem, int> ItemDoubleClicked;
	public event System.Action<RectItemPair, int> ItemSlowDoubleClicked;
	public event System.Action<IListViewModel, IListViewItem> ContextClicked;

	private int colCounter = 0;
	private Vector2 scrollPos;
	private List<ListView> listViews = new List<ListView>();
	private List<float> colSizes = new List<float>();
	private List<RectItemPair> itemPairs = new List<RectItemPair>();
	private List<RectItemModelPair> modelPairs = new List<RectItemModelPair>();
	private RectItemPair dropPair;
	private RectItemModelPair dropModelPair;
	private float minColSize = 225.0f;

	private int editSizeIndex = -1;

	private bool cancelDraw = false;

	public IColumnsViewDragDropResponder DragDropper { get; set; }
	public System.Func<IListViewItem, string, bool> RenameDelegate;

	public ColumnsView(IColumnsViewModel m)
	{
		model = m;
		model.ColumnAdded += OnColumnAdded;
		model.RevertedTo += OnRevertedTo;

		if (model.Count > 0)
		{
			foreach(var c in model)
			{
				AddNewColumnFromModel(c);
			}
		}
	}

	/// <summary>
	/// Draw thie Columns View.
	/// </summary>
	/// <returns>True if Draw() finished succesfully, flase if canceled</returns>
	public bool Draw()
	{
//		Debug.Log("Event: " + Event.current.type);
		cancelDraw = false;
		bool status = true;
		colCounter = 0;
		itemPairs.Clear();

		scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
		EditorGUILayout.BeginHorizontal();

		foreach(var c in model)
		{
			EditorGUILayout.BeginVertical(GUILayout.Width(colSizes[colCounter]));

			listViews[colCounter].Model = c;
			listViews[colCounter].Draw();
			if (cancelDraw)
			{
				status = false;
				break;
			}

			foreach(var i in listViews[colCounter].ItemPairs)
			{
				if (colCounter > 0)
				{
					i.Box.x += colSizes[colCounter-1];
				}
//				Debug.Log(i.Item.Label + ": " + i.Box);
				if (i.Box.x != 0 && i.Box.y != 0)
				{
					itemPairs.Add(i);
				}
			}

			EditorGUILayout.EndVertical();

//			var rect = GUILayoutUtility.GetLastRect();
//			Debug.Log(colCounter + ": " + rect);
			modelPairs.Add(new RectItemModelPair(GUILayoutUtility.GetLastRect(), c));

			EditorGUILayout.BeginVertical(GUILayout.Width(6.0f));
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndVertical();

			var r = GUILayoutUtility.GetLastRect();
			EditorGUIUtility.AddCursorRect(r, MouseCursor.SplitResizeLeftRight);
			float clr = 206f/255f;
			EditorGUI.DrawRect(r, new Color(clr, clr, clr));

			clr = 120f/255f;
			r.x++;
			r.width -= 2;
			EditorGUI.DrawRect(r, new Color(clr, clr, clr));

			clr = 162f/255f;
			r.x++;
			r.width -= 2;
			EditorGUI.DrawRect(r, new Color(clr, clr, clr));

			// Revert border back to original for hit testing
			r.x -= 2;
			r.width += 4;
			if (Event.current.type == EventType.MouseDown && r.Contains(Event.current.mousePosition) && editSizeIndex == -1)
			{
				editSizeIndex = colCounter;
				Event.current.Use();
			}
			else if(Event.current.type == EventType.MouseDrag && editSizeIndex != -1 && editSizeIndex == colCounter)
			{
				colSizes[editSizeIndex] += Event.current.delta.x;
				Event.current.Use();
			}
			else if (Event.current.type == EventType.MouseUp && editSizeIndex != -1)
			{
				editSizeIndex = -1;
				Event.current.Use();
			}

			colCounter++;
		}

		EditorGUILayout.EndHorizontal();
		EditorGUILayout.EndScrollView();

		var evt = Event.current;
		switch(evt.type)
		{

			case EventType.MouseDown:
				if (DragDropper == null)
				{
					break;
				}

				foreach(var p in itemPairs)
				{
					if (p.Box.Contains(Event.current.mousePosition))
					{
						DragAndDrop.PrepareStartDrag();
						ListViewDragData data = new ListViewDragData();
						data.ItemList.Add(p.Item);
						DragAndDrop.SetGenericData("ColumnViewData", data);
						DragAndDrop.objectReferences = new Object[1] { p.Item as Object };
//						Debug.Log("Mouse Down");
					}
				}

				//evt.Use();
				break;

			case EventType.MouseDrag:
				var dragData = DragAndDrop.GetGenericData("ColumnViewData") as ListViewDragData;
				if (dragData != null)
				{
					DragAndDrop.StartDrag("ColumnViewDrag");
					evt.Use();
					Debug.Log("Drag Start");
				}
				Debug.Log("Mouse Drag");
				break;

			case EventType.DragUpdated:

				if (DragDropper == null)
				{
					break;
				}

				bool validTarget = false;
				dropPair = null;
				foreach(var p in itemPairs)
				{
					if (p.Box.Contains(Event.current.mousePosition))
					{
						// Valid Check
						if (DragDropper.ValidDropTarget(p.Item))
						{
							dropPair = p;
							validTarget = true;
						}
					}
				}

				foreach(var p in modelPairs)
				{
					if (p.Box.Contains(Event.current.mousePosition))
					{
						dropModelPair = p;
						validTarget = true;
					}
				}

				Debug.Log("Drag Update");
				if (validTarget)
				{
					DragAndDrop.visualMode = DragAndDropVisualMode.Move;
				}

				evt.Use();
				break;

			case EventType.DragPerform:
				if (DragDropper == null)
				{
					break;
				}

				DragAndDrop.AcceptDrag();

				DragDropper.DragPerformed((DragAndDrop.GetGenericData("ColumnViewData") as ListViewDragData),
					dropModelPair.Model, 
					dropPair == null ? null : dropPair.Item);
				evt.Use();
				break;
		}

		return status;
	}

	public ListView GetColumn(int index)
	{
		if (index >=0 && index < listViews.Count)
		{
			return listViews[index];
		}
		return null;
	}

	private void AddNewColumnFromModel(IListViewModel col)
	{
		var lv = new ListView(col);
		lv.Selected += OnItemSelected;
		lv.Deselected += OnItemDeselected;
		lv.DoubleClicked += OnItemDoubleClicked;
		lv.SlowDoubleClicked += OnSlowDoubleClicked;
		lv.ContextClicked += OnContextClicked;
		lv.RenameDelegate += ItemRenamed;
		listViews.Add(lv);
		colSizes.Add(minColSize);
	}

	bool ItemRenamed(IListViewItem item, string name)
	{
		return RenameDelegate(item, name);
	}

	void OnSlowDoubleClicked (RectItemPair obj)
	{
		if (ItemSlowDoubleClicked != null)
		{
			ItemSlowDoubleClicked(obj, colCounter);
		}
	}

	private void OnColumnAdded(IListViewModel col)
	{
//		Debug.Log("Adding new column");
		if (listViews.Count < model.Count)
		{
			AddNewColumnFromModel(col);
		}
		cancelDraw = true;
	}

	private void OnRevertedTo(int index)
	{

	}

	private void OnItemSelected(IListViewItem item)
	{
		if (ItemSelected != null)
		{
			ItemSelected(item, colCounter);
		}
	}

	private void OnItemDeselected(IListViewItem item)
	{
		if (ItemDeselected != null)
		{
			ItemDeselected(item, colCounter);
		}
	}

	private void OnItemDoubleClicked(IListViewItem item)
	{
		if (ItemDoubleClicked != null)
		{
			ItemDoubleClicked(item, colCounter);
		}
	}

	private void OnContextClicked(IListViewModel model, IListViewItem item)
	{
		if (ContextClicked != null)
		{
			ContextClicked(model, item);
		}
	}
}