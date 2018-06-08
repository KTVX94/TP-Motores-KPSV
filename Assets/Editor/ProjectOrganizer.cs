using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;



public class ProjectOrganizer : EditorWindow{
    //--------------------------------------------Variables-----------------------------------------------------------------------------------
    //StoredInfo
    static ProjectFolderConfigData ConfigData;
    static string ConfigRoute = "Assets/Editor/Config";

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

    //----------------------------------------Basic Methods-------------------------------------------------------------------------------------
    [MenuItem("CustomTools/OrganizeProject")]
	public static void OpenWindow()
    {
        var MainWindow = GetWindow<ProjectOrganizer>();
        if (!AuxiliaryMethods.ContainsItem("ProjectFolderConfigData", ConfigRoute))
        {
            AssetDatabase.CreateFolder("Assets/Editor", "Config");
            ScriptableObjectUtility.CreateAsset<ProjectFolderConfigData>(ConfigRoute, out ConfigData, false);
            MonoBehaviour.print("El archivo de Configuracion no existe!, se ha creado la configuracion dentro de la carpeta Config");
            MonoBehaviour.print(ConfigData.GetInstanceID());
        }
        else
        {
            if (ConfigData == null)
                LoadConfigInfo();
        }
        MainWindow.Show();
    }

    private static void LoadConfigInfo()
    {
        ConfigData = AssetDatabase.LoadAssetAtPath<ProjectFolderConfigData>(ConfigRoute + "/ProjectFolderConfigData.Asset");
        MonoBehaviour.print("Encontre la configuracion, asi que lo cargo xd");
    }

    private void OnGUI()
    {
        DrawMainWindowOptions();
    }
    //-------------------------------------------------------------------------------------------------------------------------------------------
    //----------------------------------------Custom Methods-------------------------------------------------------------------------------------
    private void DrawMainWindowOptions()
    {
        //------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------Organization Select---------------------------------------------------------------
        GUILayoutOption[] AOptions = { GUILayout.MaxWidth(200) };
        if (GUILayout.Button("Apply Default Organization", AOptions))
        {
            CheckIFDefaultFolderExists();
        }
        //------------------------------------------------------------------------------------------------------------------------
        //Boton que muestre la lista
        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("Show FolderConfig"))
        {
            var w = GetWindow<FolderConfigVisualizer>();
            w.Show();
        }
        //Tengo que dibujar un algo que me permita cambiar el nombre (que por defecto tenga un nombre genèrico) asignar un tipo de objeto y por ultimo una extension especifica
        //El valor por defecto de extension va a ser (All) que sera especial para cada caso, ej: Models (Obj,FBX).
        //--------------------------------------Asset Search----------------------------------------------------------------------
        GUI.backgroundColor = Color.white;
        if (GUILayout.Button("Scan Project!"))
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
                    string c = string.Format("Object Name: {0}, extention: {1}, and path: {2}", b[0], b[1], b[2]);
                    MonoBehaviour.print(c);
                }
            }
        }
        //-------------------------------------Reorganize Assets!------------------------------------------------------------------
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

            //----------------------------------------Final Actions--------------------------------------------------------------------
            UpdateDatabase();
        }
    }
    public void UpdateDatabase()
    {
        //para que aparezcan los archivos nuevos creados o los cambios hechos:
        //aplica los cambios hechos a los assets en memoria
        AssetDatabase.SaveAssets();
        //recarga la database (y actualiza el panel "project" del editor)
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
    }
    //----------------------------------------Auxiliary Methods-----------------------------------------------------------------------------------
    #region Auxiliary Methods
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
    #endregion
}
// Auxiliares
public static class AuxiliaryMethods
{
    /// <summary>
    /// Chequea si la carpeta contiene un asset con el nombre especificado.
    /// </summary>
    /// <param name="AssetName">Nombre del Asset</param>
    /// <param name="FolderPath">Ruta de la carpeta contenedora</param>
    /// <returns>Verdadero si la carpeta contiene un asset con el nombre especificado.</returns>
    public static bool ContainsItem(string AssetName, string FolderPath)
    {
        string[] Path = { FolderPath };
        string[] founded = AssetDatabase.FindAssets(AssetName, Path);
        if (founded.Length > 0)
            return true;
        else
            return false;
    }
    public static bool ContainsItem(string AssetName, string FolderPath, out string[] AssetsFounded)
    {
        string[] Path = { FolderPath };
        string[] founded = AssetDatabase.FindAssets(AssetName, Path);
        AssetsFounded = founded;
        if (founded.Length > 1)
            return true;
        else
            return false;
    }
}

//................................................................................................................................................
#region Almacenaje de datos
public class ConfigPreset
{
    public string ConfigurationName = "NewConfig";
    public static List<FolderConfig> FolderPresets = new List<FolderConfig>();

    public List<FolderConfig> GetFolderPresets()
    {
        return FolderPresets;
    }
    public void UpdateFolderConfigs(List<FolderConfig> Folderpresets)
    {
        Folderpresets.Clear();
        FolderPresets = Folderpresets;
    }
    public void AddFolderPreset(string NewFolderName,string MainType,List<string> Extentions)
    {
        FolderConfig N = new FolderConfig().SetFolderName(NewFolderName).SetType(MainType).SetExtentions(Extentions);
        FolderPresets.Add(N);
    }
}
public class FolderConfig
{
    public string FolderName = "NewFolder";
    public string Type = "Any";
    public List<string> extentions;
    
    public FolderConfig SetFolderName(string Name)
    {
        FolderName = Name;
        return this;
    }
    public FolderConfig SetType(string AttachedType)
    {
        Type = AttachedType;
        return this;
    }
    public FolderConfig SetExtentions(List<string> AviableExtentions)
    {
        extentions = AviableExtentions;
        return this;
    }
}

public class ProjectFolderConfigData : ScriptableObject
{
    public List<ConfigPreset> Presets;
}
#endregion
//................................................................................................................................................
/// <summary>
/// Esta clase se encarga de mostrar la visualizacion de la configuracion de las carpetas.
/// </summary>
public class FolderConfigVisualizer : EditorWindow
{
    static ProjectFolderConfigData ConfigData;
    static string ConfigRoute = "Assets/Editor/Config";

    public static List<ConfigPreset> ConfigPresets = new List<ConfigPreset>(); //Acá voy a guardar los presets.
    public static List<FolderConfig> FolderPresets = new List<FolderConfig>(); // Acá voy a guardar la configuracion de las carpetas contenidas en mi clase de presets.

    string[] AviablePresets;
    public static int PresetSelected = 0;

    bool HaveMultipleOptions = false;
    bool CreateFolderPerExtention = false;
    string newFolderName = "New Folder";
    string[] options = new string[]
    {
        "Any","Material","Model","Script"
    };
    int newFolderTypeSelected = 0;

    //----------------------------------------------------------------------------------------------------------------------------------------------
    [MenuItem("CustomTools/FolderConfig")]
    public static void OpenWindow()
    {
        var MainWindow = GetWindow<FolderConfigVisualizer>(); //Obtengo una instancia nueva de esta ventana.
        MainWindow.Show(); //Muestro la ventana.
    }
    //----------------------------------------------------------------------------------------------------------------------------------------------
    private void OnGUI()
    {
        //----------------------------------------//Header\\----------------------------------------------------------------------------------------
        EditorGUILayout.BeginHorizontal();//------------------------------------This is a Horizontal Group.
        //Chequeo cuantos presets estan disponibles y los muestro, si no hay nada aún, Muestro "Empty".
        ConfirmIfAviablePresetsExist();
        PresetSelected = EditorGUILayout.Popup(PresetSelected, AviablePresets);
        if (GUILayout.Button("Add"))
        {
            var item = new ConfigPreset();
            string name = item.ConfigurationName;
            ConfigPresets.Add(item);
            if (ConfigPresets.Count - 1 >= 0)
                PresetSelected = ConfigPresets.Count - 1;
            else
                PresetSelected = 0;
            var editWindow = GetWindow<EditPreset>();
            editWindow.ToEdit(item);
            editWindow.Show();
        }
        if (GUILayout.Button("Rename") && ConfigPresets.Count > 0)
        {
            var editWindow = GetWindow<EditPreset>();
            editWindow.ToEdit(ConfigPresets[PresetSelected]);
            editWindow.Show();
        }
        if (GUILayout.Button("Delete"))
        {
            int configIndex = 0;
            for (int i = 0; i < ConfigPresets.Count; i++)
                if (ConfigPresets[i] == ConfigPresets[PresetSelected])
                    configIndex = i;
            ConfigPresets.Remove(ConfigPresets[PresetSelected]);
            if (ConfigPresets.Count - 1 >= 0)
                PresetSelected = ConfigPresets.Count - 1;
            else
                PresetSelected = 0;
            ConfirmIfAviablePresetsExist();
        }
        EditorGUILayout.EndHorizontal();//--------------------------------------Here ends a Horizontal Group.
                                        //-------------------------------------------//Body\\-----------------------------------------------------------------------------------------

        foreach (var item in FolderPresets)
        {
            ShowConfigSet(item);
        }

        //-------------------------------------------//Bottom\\---------------------------------------------------------------------------------------

        EditorGUILayout.BeginHorizontal();//------------------------------------This is a Horizontal Group.
        //---------------------------------------Creo un nuevo Preset de Carpeta---------------------------
        EditorGUILayout.LabelField("Folder name: ", GUILayout.Width(75));
        newFolderName = EditorGUILayout.DelayedTextField(newFolderName);
        if (GUILayout.Button("Create Folder"))
        {
            if (ConfigPresets.Count > 0)
            {
                List<string> ext = new List<string> { ".mat" };
                ConfigPresets[PresetSelected].AddFolderPreset(newFolderName, "Material", ext);
                FolderPresets = ConfigPresets[PresetSelected].GetFolderPresets();
            }
        }
        newFolderTypeSelected = EditorGUILayout.Popup(newFolderTypeSelected, options);
        EditorGUILayout.EndHorizontal();//--------------------------------------Here ends a Horizontal Group.
    }

    #region Auxiliares
    /// <summary>
    /// Se encarga de mostrar la lista de presets disponibles.
    /// </summary>
    private void ConfirmIfAviablePresetsExist()
    {
        if (ConfigPresets.Count > 0)
        {
            AviablePresets = new string[ConfigPresets.Count];
            for (int i = 0; i < ConfigPresets.Count; i++)
            {
                AviablePresets[i] = ConfigPresets[i].ConfigurationName;
            }
        }
        else
            AviablePresets = new string[] { "Empty" };
    }
    /// <summary>
    /// Localiza en el proyecto la instancia que contiene la Data
    /// </summary>
    private static void LoadConfigInfo()
    {
        ConfigData = AssetDatabase.LoadAssetAtPath<ProjectFolderConfigData>(ConfigRoute + "/ProjectFolderConfigData.Asset");
        MonoBehaviour.print("Encontre la configuracion, asi que lo cargo xd");
    }

    private void ShowConfigSet(FolderConfig ConfigData)
    {
        EditorGUILayout.BeginHorizontal();//------------------------------------This is a Horizontal Group.
        EditorGUILayout.TextArea(ConfigData.FolderName); //Nombre de la carpeta.
        int disp = 0;
        disp = EditorGUILayout.Popup("Item Type:",disp, options);

        //GUIContent TypesToShow = new GUIContent("Type", "Type");
        //EditorGUILayout.DropdownButton(TypesToShow, FocusType.Passive);

        //La condicion es que haya mas de una sub-opcion.
        HaveMultipleOptions = EditorGUILayout.BeginToggleGroup("Multiple Extentions", HaveMultipleOptions);//-----------Toogle Group START--------------

        EditorGUILayout.BeginHorizontal();//------------------------------------This is a Horizontal Group.
        CreateFolderPerExtention = EditorGUILayout.Toggle(CreateFolderPerExtention);
        EditorGUILayout.LabelField("Folder Per Extention");
        EditorGUILayout.EndHorizontal();//--------------------------------------Here ends a Horizontal Group.
        EditorGUILayout.EndToggleGroup();//-----------------------------------------------------------------------------Toogle Group END----------------
        EditorGUILayout.EndHorizontal();//--------------------------------------Here ends a Horizontal Group.

    }
    #endregion
}
public class EditPreset : EditorWindow
{
    //Nombre
    private string InputName = "";
    public string OriginalName = "";
    //Delegados--Eliminados.
    
    //ConfigPreset
    public static ConfigPreset InEdit;
    private bool Changes = false;

    public static void OpenWindow()
    {
        var MainWindow = GetWindow<EditPreset>();
        MainWindow.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        InputName = EditorGUILayout.TextField("Preset Name: ", InputName);
        if (GUILayout.Button("Guardar Cambios"))
        {
            OriginalName = InEdit.ConfigurationName;
            InEdit.ConfigurationName = InputName;
            Changes = true;
        }
        if (GUILayout.Button("Cancelar"))
            InputName = OriginalName;//******************** Sera?
        EditorGUILayout.EndHorizontal();

        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("Salir"))
            Close();
    }

    public EditPreset ToEdit(ConfigPreset EditablePreset)
    {
        InputName = EditablePreset.ConfigurationName;
        OriginalName = EditablePreset.ConfigurationName;
        InEdit = EditablePreset;
        return this;
    }
}
