using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;



public class ProjectOrganizer : EditorWindow{
    //----------------------------------------------------------------------------------------------------
    string newFolderName = "";
    private List<String> UserDefinedProjectFolders = new List<String>();
    private List<string[]> FindedElements = new List<string[]>();
    public List<string> DefaultProjectFolders = new List<string>{
        "Editor",
        "Scripts",
        "Prefabs",
        "Materials",
        "Scenes",
        "Textures",
        "Sprites",
        "Animations",
    };
    public List<string> FilterTypes = new List<string>
    {
        "t:AnimationClip",
        "t:Material",
        "t:AudioClip",
        "t:AudioMixer",
        "t:Font",
        "t:GUISkin",
        "t:Mesh",
        "t:Model",
        "t:PhysicMaterial",
        "t:Prefab",
        "t:Scene",
        "t:Script",
        "t:Shader",
        "t:Sprite",
        "t:Texture",
        "t:VideoClip"
    };


    //----------------------------------------------------------------------------------------------------
    [MenuItem("CustomTools/OrganizeProject")]
	public static void OpenWindow()
    {
        var w = GetWindow<ProjectOrganizer>();
        w.Show();
    }

    private void OnGUI()
    {
        GUILayoutOption[] AOptions = { GUILayout.MaxWidth(200) };
        if (GUILayout.Button("Apply Default Organization", AOptions))
        {
            CheckIFDefaultFolderExists();
        }

        if (GUILayout.Button("Search Assets"))
        {
            String[] D = AssetDatabase.FindAssets("t:Material");
            if (D.Length > 0)
            {
                foreach (var item in D)
                {
                    string a = AssetDatabase.GUIDToAssetPath(item); //Ruta relativa del asset.
                    var b = GetNameAndExtention(a);
                    FindedElements.Add(b);

                    //Debugear el objeto obtenido.
                    string c = string.Format("Object Name: {0}, extention: {1}, and path: {2}",b[0],b[1],b[2]);
                    MonoBehaviour.print(c);
                }
            }
        }


        EditorGUILayout.BeginHorizontal();//------------------------------------
        EditorGUILayout.LabelField("Folder name: ", GUILayout.Width(75));
        newFolderName = EditorGUILayout.TextField(newFolderName);
        if (GUILayout.Button("Create Folder"))
        {
            //con esto podemos revisar si un folder existe o no.
            //SOLO VALIDO PARA CARPETAS DENTRO DE ASSETS
            //Y solo valido, obviamente, para cuando estamos trabajando dentro del editor de Unity
            if (!AssetDatabase.IsValidFolder(newFolderName))
            {
                AssetDatabase.CreateFolder("Assets", newFolderName);
            }
        }
        EditorGUILayout.EndHorizontal();//------------------------------------

        GUI.backgroundColor = Color.green;
        
        if (GUILayout.Button("Reorganize!"))
        {
            string NewPathWithoutName = "";
            //Aca hay que seleccionar la carpeta de destino.
            if (ContainsDirectory(DefaultProjectFolders[3]))
            {
                NewPathWithoutName = "Assets/" + DefaultProjectFolders[3] + "/";
            }
            else
            {
                //Recreamos el directorio.
            }


            foreach (var item in FindedElements)
            {
                if (item[1] == ".mat")
                {

                    MonoBehaviour.print(NewPathWithoutName + " extension: " + item[1] + ".");
                    AssetDatabase.MoveAsset(item[2], NewPathWithoutName + item[0] + item[1]);
                }
            }
            UpdateDatabase();
        }
    }

    public void UpdateDatabase()
    {
        //para que aparezcan los archivos nuevos creados o los cambios hechos:

        //aplica los cambios hechos a los assets en memoria
        AssetDatabase.SaveAssets();

        //recarga la database (y actualiza el panel "project" del editor)
        AssetDatabase.Refresh();
    }
    //--------------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Recorre una lista por Defecto y genera carpetas dentro de Assets.
    /// </summary>
    private void CheckIFDefaultFolderExists()
    {
        foreach (var item in DefaultProjectFolders)
        {
            if (!ContainsDirectory(item))
            {
                MonoBehaviour.print("Se ha creado la carpeta " + item + "!!");
                AssetDatabase.CreateFolder("Assets", item);
            }
        }
    }

    /// <summary>
    /// Chequea si la carpeta especificada existe dentro del proyecto.
    /// </summary>
    /// <param name="FolderName">Nombre de la carpeta a chequear.</param>
    /// <returns>Verdadero si la carpeta existe dentro Assets.</returns>
    public static bool ContainsDirectory(string FolderName)
    {
        string Route = Application.dataPath; //Application.dataPath == Origen de la carpeta Assets(Path Global).
        if (Directory.Exists(Route + "/" + FolderName))
            return true;
        else
            return false;
    }

    /// <summary>
    /// Dado un Path relativo, devuelve un array de string que contiene el nombre, la extension y su path.
    /// </summary>
    /// <returns>Un array de string donde [0] = Nombre, [1] = Extension, [2] = Ruta de acceso relativa.</returns>
    public static string[] GetNameAndExtention(string RelativePath)
    {
        string OriginalPath = RelativePath;//Path original que recivimos, es la relativa.
        //string CompleteOriginalPath = "";
        string b = OriginalPath;

        List<char> extentionChars = new List<char>();
        string extention = "";
        List<char> NameChars = new List<char>();
        string Name = "";

        //Calculo el path completo.
        //string AssetsPath = Application.dataPath;
        //CompleteOriginalPath = RelativePath;
        //MonoBehaviour.print("El path completo es: " + CompleteOriginalPath);

        //Recorro el string de atras para adelante y obtengo su extension.
        for (int i = b.Length - 1; i >= 0; i--)
        {
            if (b[i] != '.')
            {
                extentionChars.Insert(0, b[i]);
                //MonoBehaviour.print(String.Format("Index: {0}, Character: {1}, Largo de b: {2}, extention lenght: {3}",i, b[i], b.Length, extention.Length));
            }
            else
            {
                extentionChars.Insert(0, b[i]);
                foreach (var chara in extentionChars)
                    extention += chara;
                //MonoBehaviour.print(String.Format("Index: {0}, Character: {1}, Largo de b: {2}, extention lenght: {3}", i, b[i], b.Length, extention.Length));
                break;
            }
        }

        int startingPos = (b.Length - 1) - extention.Length;
        for (int i = startingPos; i >= 0; i--)//Obtengo el nombre;
        {
            if (b[i] == '/')
            {
                foreach (var chara in NameChars)
                    Name += chara;
                break;
            }
            else
                NameChars.Insert(0, b[i]);
        }
        //MonoBehaviour.print("Nombre Final es: " + Name);

        return new string[]{ Name, extention, OriginalPath};//Complete path no esta incluido.
    }
}