using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// Recreation of Unity's Project Window context menu
/// </summary>
public class ProjectContextMenu
{
	private GenericMenu menu;

	private FileSystemListViewModel curModel;
	private FileSystemListViewItem curItem;

	public ProjectContextMenu()
	{
		menu = new GenericMenu();

		#region Create Submenu

		AddItem("Create/Folder", () => {
			StringPromptDialogue.Show("Enter Folder Name.", (str) => { 
				var path = GenerateUniquePathFromModel(str);
				Directory.CreateDirectory(path);
				ColumnsViewUtility.Refresh();
			});
		});
		menu.AddSeparator("Create/");
		AddItem("Create/C# Script", () => {
			StringPromptDialogue.Show("Enter Class Name for Script.", (str) => { 
				var path = GenerateUniquePathFromModel(str + ".cs");
				ScriptTemplates.CloneTemplateToPath("Default C-Sharp", path, true);
				ColumnsViewUtility.Refresh();
			});
		});
		AddItem("Create/Javascript", null);
		AddItem("Create/Editor Test C# Script", null);
		AddItem("Create/Shader/Standard Surface Shader", null);
		AddItem("Create/Shader/Unlit Shader", null);
		AddItem("Create/Shader/Image Effect Shader", null);
		AddItem("Create/Shader/Compute Shader", null);
		menu.AddSeparator("Create/");
		AddItem("Create/Scene", null);
		AddItem("Create/Prefab", null);
		menu.AddSeparator("Create/");
		AddItem("Create/Audio Mixer", null);
		menu.AddSeparator("Create/");
		AddItem("Create/Material", null);
		AddItem("Create/Lens Flare", null);
		AddItem("Create/Render Texture", null);
		AddItem("Create/Lightmap Parameters", null);
		menu.AddSeparator("Create/");
		AddItem("Create/Sprites/Square", null);
		AddItem("Create/Sprites/Triangle", null);
		AddItem("Create/Sprites/Diamond", null);
		AddItem("Create/Sprites/Hexagon", null);
		AddItem("Create/Sprites/Circle", null);
		AddItem("Create/Sprites/Polygon", null);
		menu.AddSeparator("Create/");
		AddItem("Create/Animation Controller", null);
		AddItem("Create/Animation", null);
		AddItem("Create/Animator Override Controller", null);
		AddItem("Create/Avatar Mask", null);
		menu.AddSeparator("Create/");
		AddItem("Create/Physics Material", null);
		AddItem("Create/Physics 2D Material", null);
		menu.AddSeparator("Create/");
		AddItem("Create/GUI Skin", null);
		AddItem("Create/Custom Font", null);
		AddItem("Create/Shader Varient Collection", null);
		menu.AddSeparator("Create/");
		AddItem("Create/Legacy/Cubemap", null);

		#endregion

		AddItem("Reveal In Finder", null);
		AddItem("Open", () => {
			ColumnsViewUtility.OpenFile(curItem);
			ColumnsViewUtility.Refresh();
		});
		AddItem("Delete", () => {
			if (!EditorUtility.DisplayDialog("Delete File", "Are you sure you want to delete \"" + curItem.Info.Name + "\" permanently?", "Delete", "Cancel"))
			{
				return;
			}
			if (Directory.Exists(curItem.Info.FullName))
			{
				Directory.Delete(curItem.Info.FullName, true);
			}
			else if (File.Exists(curItem.Info.FullName))
			{
				File.Delete(curItem.Info.FullName);
			}
			ColumnsViewUtility.Refresh();
		});
		menu.AddSeparator("");
		AddItem("Open Scene Additive", null);
		menu.AddSeparator("");
		AddItem("Import New Asset...", null);
		AddItem("Import Package/", null);
		AddItem("Export Package...", null);
		AddItem("Find References in Scene", null);
		AddItem("Select Dependencies", null);
		menu.AddSeparator("");
		AddItem("Refresh", () => {
			ColumnsViewUtility.Refresh();
		});
		AddItem("Reimport", null);
		menu.AddSeparator("");
		AddItem("Reimport All", null);
		menu.AddSeparator("");
		AddItem("Run API Updater", null);
		menu.AddSeparator("");
		AddItem("Open C# Project", null);
	}

	/// <summary>
	/// Show the context menu setting the context information of the click as well.
	/// </summary>
	/// <param name="model">Model that mouse is over when context clicked.</param>
	/// <param name="item">Item that mouse is over when context clicked.</param>
	public void Show(IListViewModel model, IListViewItem item)
	{
//		Debug.Log("Context Item: " + item);
		if (item != null)
		{
			curItem = item as FileSystemListViewItem;
		}
		else
		{
			curItem = null;
		}

		curModel = model as FileSystemListViewModel;

		menu.ShowAsContext();
	}

	// Shortcut method for adding items to menu.
	private void AddItem(string name, GenericMenu.MenuFunction func, bool active = false)
	{
		menu.AddItem(new GUIContent(name), active, func);
	}

	// Utility for making a unique path based on the contexted model.
	// NOTE: Possibly need refactoring into a Utility class or the model itself.
	private string GenerateUniquePathFromModel(string filename)
	{
		int assetIndex = curModel.RootPath.IndexOf("/Assets");
		string relativePath = curModel.RootPath.Substring(assetIndex+1);
		string systemPath = curModel.RootPath.Substring(0, assetIndex+1);
		var path = AssetDatabase.GenerateUniqueAssetPath(relativePath + "/" + filename);
		return systemPath + path;
	}
}
