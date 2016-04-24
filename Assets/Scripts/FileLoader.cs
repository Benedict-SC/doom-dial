using UnityEngine;
using System.Collections;
using System.IO;

public class FileLoader{
	
	readonly string badpath = "DON'T WRITE TO ASSETS";
	
	string path;
	string folder;
    string fileName;
	
	public FileLoader(string folder, string fileName)
	{
        this.fileName = fileName;
		this.folder = folder;
		//this.path = Application.streamingAssetsPath + Path.DirectorySeparatorChar + folder;
		//Debug.Log ("streaming assets path is " + Application.streamingAssetsPath);
		this.path = badpath;//Application.dataPath  + Path.DirectorySeparatorChar + "Resources"  + Path.DirectorySeparatorChar + folder;
	}
	public FileLoader(string path, string folder, string fileName){
		this.fileName = fileName;
		this.folder = folder;
		this.path = path + Path.DirectorySeparatorChar + folder;
	}

	public static FileLoader GetSaveDataLoader(string folder, string filename){
		return new FileLoader(Application.persistentDataPath,folder,filename);
	}
	public static FileLoader GetGameJSONDataLoader(string folder, string filename){
		return new FileLoader("JSONData" + Path.DirectorySeparatorChar + folder,filename);
	}
	
	public string Read()
	{
		if(path.Equals(badpath)){
			string slashfolder = folder.Replace(Path.DirectorySeparatorChar,'/');
			TextAsset ta = Resources.Load<TextAsset>(slashfolder + "/" + fileName);
			//Debug.Log ("path is "+slashfolder + "/" + fileName);
			return ta.text;
		}else{
			if(File.Exists(path + Path.DirectorySeparatorChar + fileName + ".txt"))
				return File.ReadAllText(path + Path.DirectorySeparatorChar + fileName + ".txt");
			else 
				return "ERROR";
		}
		
	}
	public string CreatedPath(){
		if(path.Equals(badpath)){
			string slashfolder = folder.Replace(Path.DirectorySeparatorChar,'/');
			return slashfolder + "/" + fileName;
		}
		return path + Path.DirectorySeparatorChar + fileName + ".txt";
	}
	
	public void Write(string json)
	{
        Directory.CreateDirectory(path);
        StreamWriter sw = new StreamWriter(path + Path.DirectorySeparatorChar + fileName + ".txt");
        sw.Write(json);
		sw.Close();
	}
}

/*
okay, to get the game working in build, i had to make some major changes to the way files load

benedict_dh [5:04 PM]
i'll note what i did here, in case it's relevant to what anyone else is doing

benedict_dh [5:05 PM]
so, the problem in build was that the game wasn't loading any files from Assets- because that folder isn't actually available in an android build, it's all packaged into the .apk which can't be explored like a file system

benedict_dh [5:05 PM]
so the FileLoader class was failing

benedict_dh [5:05 PM]
what i had to do was remove the File class stuff from FileLoader and change it to work with Unity's Resources.Load<TextAsset> function

benedict_dh [5:06 PM]5:06
unfortunately, Unity's Resources.Load<TextAsset> doesn't recognize .json files as text files, even though that's pretty much exactly what they are

benedict_dh [5:06 PM]
thankfully, the FileLoader is filetype-agnostic from the script end

benedict_dh [5:06 PM]
so i didn't need to change any of the code that was feeding it paths

benedict_dh [5:07 PM]
but what i did need to do was take every single .json file and convert it to .txt

benedict_dh [5:08 PM]
MiniJSON just parses strings, so the filetype doesn't actually matter- if we were using some json-specific tools we'd have a problem, but we don't, so we're all set there

benedict_dh [5:10 PM]
i'm going to push this update without deleting the existing .json files (i did a Save As, which created duplicates as .txt), but the only thing you'll need to worry about when working with data is that ​*the .json files are junk now, if you want to edit any files you need to edit the .txts*​

benedict_dh [5:12 PM]
also, i don't think it'll be a big problem for anyone, but there's two constructors for FileLoaders- there always have been, but now they function significantly differently

benedict_dh [5:12 PM]
the first one takes two arguments- a folder (implicitly in Resources), and a filename with no extension

benedict_dh [5:13 PM]
that's for loading json .txts from Resources, and ​*can't write*​

benedict_dh [5:13 PM]
the other one takes three arguments- a path, a folder, and a filename again with no extension

benedict_dh [5:14 PM]
the path lets you specify any location, but the only path that actually ​_works_​ in Android is Application.persistentDataPath

benedict_dh [5:15 PM]
this is for saving user files, and you can create or write to any folder inside that path- note that it continues to use C# File class and not Resources.Load

benedict_dh [5:15 PM]
if you're loading static game data that you've put in Resources, use the two-argument constructor, and if you're loading saved user data, use the three-argument constructor with Application.persistentDataPath as the first argument
*/
