using UnityEngine;
using System.IO;
using System.Text;
using System.Collections.Generic;

/// <summary>
/// Static utility class for accessing formation and writeing out templates.
/// </summary>
public static class ScriptTemplates
{
	private static string TemplatesFolderPath { get { return Application.dataPath + "/Project+/Script Templates"; } }

	public static List<string> TemplateNames
	{
		get
		{
			var files = Directory.GetFiles(TemplatesFolderPath);
			var names = new List<string>();
			foreach(var f in files)
			{
				var info = new FileInfo(f);
				if (info.Extension == ".meta")
				{
					continue;
				}
				string name = info.Name.Replace(info.Extension, "");
				names.Add(name);
			}
			return names;
		}
	}

	/// <summary>
	/// Copies the specified template to destination location. Can replace a single occurrence of '#SCRIPTNAME#' within the file.
	/// Meant to emulate Unity's default create script functionality.
	/// </summary>
	/// <param name="templateName">Template name.</param>
	/// <param name="path">Path.</param>
	/// <param name="nameReplace">If set to <c>true</c> name replace.</param>
	public static void CloneTemplateToPath(string templateName, string path, bool nameReplace = false)
	{
		var files = Directory.GetFiles(TemplatesFolderPath);
		FileInfo templateFile = null;
		foreach(var f in files)
		{
			var info = new FileInfo(f);
			if (info.Name.Replace(info.Extension, "") == templateName)
			{
				templateFile = info;
				break;
			}
		}

		if (templateFile == null)
		{
			Debug.LogError("Template file not found: " + templateName);
		}

		var inFile = File.OpenText(templateFile.FullName);
		var outFile = File.OpenWrite(path);

		var data = inFile.ReadToEnd();

		if (nameReplace)
		{
			var newFileInfo = new FileInfo(path);
			var name = newFileInfo.Name;
			name = name.Replace(newFileInfo.Extension, "");

			data = data.Replace("#SCRIPTNAME#", name);
		}

		var bytes = Encoding.ASCII.GetBytes(data);
		outFile.Write(bytes, 0, bytes.Length);

		inFile.Close();
		outFile.Close();
	}
}
