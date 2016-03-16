using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class StringPromptDialogue : EditorWindow
{
	private string message;
	private string input;
	private System.Action<string> callback;

	public static void Show(string message, System.Action<string> callback)
	{
		var window = ScriptableObject.CreateInstance<StringPromptDialogue>();
		window.message = message;
		window.callback = callback;
		window.maxSize = new Vector2(200.0f, 100.0f);
		window.minSize = new Vector2(200.0f, 100.0f);

		// Figure out how to position window
//		var mousePos = Event.current.mousePosition;
//		window.position = new Rect(mousePos.x, mousePos.y, window.position.width, window.position.height);

		window.ShowAuxWindow();
	}

	void OnGUI()
	{
		EditorGUILayout.BeginVertical();
		GUILayout.FlexibleSpace();

		EditorGUILayout.LabelField(message);
		GUI.SetNextControlName("StringPromptInput");
		input = EditorGUILayout.TextField(input);

		if (GUILayout.Button("Enter") || (Event.current.type == EventType.KeyUp && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter)))
		{
			if (callback != null)
			{
				callback(input);
			}
			Close();
		}
		else
		{
			EditorGUI.FocusTextInControl("StringPromptInput");
		}

		GUILayout.FlexibleSpace();
		EditorGUILayout.EndVertical();
	}
}
