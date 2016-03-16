using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class ProjectColumnsWindow : EditorWindow 
{
	private string AssetRoot { get { return Application.dataPath; } }
	private FileSystemColumnsViewController fileCtrl;

	[MenuItem("Window/Project+")]
	public static void Init()
	{
		ProjectColumnsWindow window = (ProjectColumnsWindow)EditorWindow.GetWindow (typeof (ProjectColumnsWindow));
		window.wantsMouseMove = true;

		window.Show();
	}

	void OnEnable()
	{
		fileCtrl = new FileSystemColumnsViewController(AssetRoot);

//		fileCtrl.View.ItemSelected += (obj, i) => { Debug.Log("Selected: " + obj + " | col: " + i); shouldRepaint = true; };
//		fileCtrl.View.ItemDeselected += (obj, i) => { Debug.Log("Deselected: " + obj + " | col: " + i); };
//		fileCtrl.View.ItemDoubleClicked += (obj, i) => { Debug.Log("Double Clicked: " + obj + " | col: " + i); };
	}

	void OnGUI()
	{
		if (!fileCtrl.View.Draw())
		{
		}
		Repaint();
	}
}