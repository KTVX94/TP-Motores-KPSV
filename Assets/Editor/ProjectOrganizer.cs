using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ProjectPreConfig;


    public class ProjectOrganizer : EditorWindow
    {
        //--------------------------------------------Variables-----------------------------------------------------------------------------------
        //StoredInfo
        static ProjectFolderConfigData ConfigData;
        static string ConfigRoute = "Assets/Editor/Config";

        static int SelectedPreset = 0;
        string[] _AviablePresets;
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
    }; //Esto tiene que ser reemplazado por ConfigData.GetSelectedPresetFolderSettings(SelectedPreset);
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
            LoadConfigInfo();
            MainWindow.Show();
        }
        private static void LoadConfigInfo()
        {
            if (!AuxiliaryMethods.ContainsItem("ProjectFolderConfigData", ConfigRoute))
            {
                var ConfigurationWindow = GetWindow<ConfigVisualizer>();
                ConfigurationWindow.Show();
                MonoBehaviour.print("El archivo de Configuracion no fue encontrado: Se ha abierto la ventana de configuracion!");
            }
            else
            {
                ConfigData = AssetDatabase.LoadAssetAtPath<ProjectFolderConfigData>(ConfigRoute + "/ProjectFolderConfigData.Asset");
                MonoBehaviour.print("Archivo de configuracion encontrado.");
            }
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
            EditorGUILayout.BeginHorizontal();
            if (ConfigData.Presets.Count > 0)
            {
                List<string> ConfigNames = new List<string>();
                foreach (var item in ConfigData.Presets)
                {
                    ConfigNames.Add(item.ConfigurationName);
                }
                _AviablePresets = ConfigNames.ToArray();
            }
            else _AviablePresets = new string[] { "Empty" };

            SelectedPreset = EditorGUILayout.Popup(SelectedPreset, _AviablePresets);

            //------------------------------------------------------------------------------------------------------------------------
            GUI.backgroundColor = Color.cyan;
            if (GUILayout.Button("Show FolderConfig")) //Este boton nos abre las configuraciones.
            {
                var w = GetWindow<ConfigVisualizer>();
                w.Show();
            }
            //Tengo que dibujar un algo que me permita cambiar el nombre (que por defecto tenga un nombre genèrico) asignar un tipo de objeto y por ultimo una extension especifica
            //El valor por defecto de extension va a ser (All) que sera especial para cada caso, ej: Models (Obj,FBX).
            EditorGUILayout.EndHorizontal();
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


                foreach (var item in FindedElements) //Funcion para mover los objetos encontrados.
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

            return new string[] { Name, extention, OriginalPath };//Complete path no esta incluido.
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
    public struct FilterType
    {
        public string FilterName;
        public List<string> ExtentionsAllowed;

        public FilterType(string Name, List<string> Extentions)
        {
            FilterName = Name;
            ExtentionsAllowed = Extentions;
        }
    }

    public class ConfigPreset
    {
        public string ConfigurationName = "NewConfig";
        public List<FolderConfig> FolderPresets = new List<FolderConfig>();

        public List<FolderConfig> GetFolderPresets()
        {
            if (FolderPresets.Count > 0)
                return FolderPresets;
            else
                return new List<FolderConfig>();
        }
        public void AddFolderPreset(string NewFolderName, int MainType, List<string> Extentions)
        {
            FolderConfig N = new FolderConfig().SetFolderName(NewFolderName).SetType(MainType).SetExtentions(Extentions);
            FolderPresets.Add(N);
        }
    }
    public class FolderConfig
    {
        public string FolderName = "NewFolder";
        public int TypeIndex;
        public List<string> extentions;
        private bool FolderPerExtention = false;
        public bool EnableFolderPerExtention
        {
            get
            {
                return FolderPerExtention;
            }
            set
            {
                FolderPerExtention = value;
            }
        }

        public FolderConfig SetFolderName(string Name)
        {
            FolderName = Name;
            return this;
        }
        public FolderConfig SetType(int AttachedType)
        {
            TypeIndex = AttachedType;
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
        public List<ConfigPreset> Presets = new List<ConfigPreset>();
        public List<string> Extentions = new List<string>();
        public List<FilterType> FilterTypes = new List<FilterType>()
    {
        new FilterType("Any", new List<string>()),
        new FilterType("AnimationClip",new List<string>()),
        new FilterType("Material",new List<string>()),
        new FilterType("AudioClip",new List<string>()),
        new FilterType("AudioMixer",new List<string>()),
        new FilterType("Font", new List<string>()),
        new FilterType("GUISkin",new List<string>()),
        new FilterType("Mesh",new List<string>()),
        new FilterType("Model",new List<string>()),
        new FilterType("PhysicMaterial",new List<string>()),
        new FilterType("Prefab",new List<string>()),
        new FilterType("Scene",new List<string>()),
        new FilterType("Script",new List<string>()),
        new FilterType("Shader",new List<string>()),
        new FilterType("Sprite",new List<string>()),
        new FilterType("Texture",new List<string>()),
        new FilterType("VideoClip",new List<string>()),
        new FilterType("Non Specified",new List<string>())
    };

        public List<FolderConfig> GetSelectedPresetFolderSettings(int SelectedPresetSet)
        {
            List<FolderConfig> FolderData = new List<FolderConfig>();
            if (Presets.Count > 0)
            {
                foreach (var item in Presets)
                {
                    FolderData = item.GetFolderPresets();
                }
                return FolderData;
            }
            else
                return new List<FolderConfig>();
        }
    }
    #endregion
