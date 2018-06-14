using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace ProjectPreConfig
{
    /// <summary>
    /// Esta clase se encarga de mostrar la visualizacion de la configuracion de las carpetas.
    /// </summary>
    public class ConfigVisualizer : EditorWindow
    {
        public int toolbarInt = 0;
        public int PresetSelected = 0;
        public static bool HasBeenModified = false; //Esto va a ser verdadero cada vez que realizamos alguna operacion. Se vuelve falsa al guardar.

        public static List<ConfigPreset> LocalConfigPresets = new List<ConfigPreset>(); //Copia de los presets en mi archivo de Configuracion.
        public static List<FolderConfig> LocalFolderPresets = new List<FolderConfig>(); //Copia de la configuracion de las carpetas contenidas en mi archivo de Configuracion.
        public static List<string> LocalExtentions = new List<string>();//Copia de las Extensiones por defecto en mi archivo de Configuración.
        public static List<FilterType> LocalFilters = new List<FilterType>(); //Copia de los Filtros por defecto en mi archivo de Configuración.
        public static List<string> ExtentionsToClear = new List<string>();

        public static string[] MainTabs = new string[] { "Configs", "Types", "Extentions" };//Prestañas Principales.
        public static string[] FolderTypeOptions; //Array de Types, Copia Local de los nombres de Filtro dentro de mi archivo de Configuración.

        static ProjectFolderConfigData ConfigData;
        static string ConfigRoute = "Assets/Editor/Config";


        int newFolderTypeSelected = 0;

        static string DefaultFolderName = "NewFolder";
        string newFolderName = DefaultFolderName;
        static string DefaultExtentionName = "";
        string NewExtentionName = DefaultExtentionName;
        string[] AviablePresets;

        bool NewExtentionNameIsInvalid = false;
        bool EnableRename = false;
        bool WritableConfigPresetsAviable = false;
        //bool CreateFolderPerExtention = false;

        Vector2 ExtentionScrollPos = Vector2.zero;
        Vector2 ConfigScrollPos = Vector2.zero;
        static int _MainWindowHeight = 300;
        static int _MainWindowWidth = 450;

        //----------------------------------------------------------------------------------------------------------------------------------------------
        [MenuItem("CustomTools/FolderConfig")]
        public static void OpenWindow()
        {
            var MainWindow = GetWindow<ConfigVisualizer>(); //Obtengo una instancia nueva de esta ventana.
            LoadConfigInfo();
            MainWindow.minSize = new Vector2(_MainWindowWidth, _MainWindowHeight);
            MainWindow.maxSize = new Vector2(_MainWindowWidth * 3, _MainWindowHeight * 3);
            MainWindow.Show(); //Muestro la ventana.
        }
        //----------------------------------------------------------------------------------------------------------------------------------------------
        private void OnGUI()
        {
            toolbarInt = GUILayout.Toolbar(toolbarInt, MainTabs);
            GUI.backgroundColor = Color.green;
            EditorGUI.BeginDisabledGroup(!HasBeenModified);
            if (GUILayout.Button("SaveChanges"))
            {
                SaveConfigInfo();
                HasBeenModified = false;
                Repaint();
            }
            EditorGUI.EndDisabledGroup();

            GUI.backgroundColor = Color.white;
            switch (toolbarInt)
            {
                case 0:
                    ShowConfigGUI();
                    break;
                case 1:
                    ShowTypeGUI();
                    break;
                case 2:
                    ShowExtentionGUI();
                    break;
                default:
                    break;
            }
        }
        private void OnDestroy()
        {
            //Limpio todo lo que tengo cargado:
            LocalConfigPresets.Clear();
            LocalExtentions.Clear();
            LocalFilters.Clear();
            LocalFolderPresets.Clear();
            MonoBehaviour.print("Cierro la ventana");
        }
        //-----------------------------------------------TaB GUI'S--------------------------------------------------------------------------------------
        private void ShowConfigGUI()
        {
            if (LocalConfigPresets.Count > 0)
                EnableRename = true;
            else
                EnableRename = false;

            //----------------------------------------//Header\\----------------------------------------------------------------------------------------
            #region Header 
            EditorGUILayout.BeginHorizontal();//------------------------------------This is a Horizontal Group.
                                              //Chequeo cuantos presets estan disponibles y los muestro, si no hay nada aún, Muestro "Empty".
            ConfirmIfAviablePresetsExist();
            PresetSelected = EditorGUILayout.Popup(PresetSelected, AviablePresets);
            if (GUILayout.Button("Add"))
            {
                var item = new ConfigPreset();
                //string name = item.ConfigurationName;
                LocalConfigPresets.Add(item);
                if (LocalConfigPresets.Count - 1 >= 0)
                    PresetSelected = LocalConfigPresets.Count - 1;
                else
                    PresetSelected = 0;
                var editWindow = GetWindow<EditPreset>().ToEdit(item);
                HasBeenModified = true;
                editWindow.Show();
            }
            GUI.enabled = EnableRename;//El boton rename se deshabilita si configpresets no tiene configuraciones seleccionables.
            if (GUILayout.Button("Rename"))
            {
                var editWindow = GetWindow<EditPreset>();
                editWindow.ToEdit(LocalConfigPresets[PresetSelected]);
                HasBeenModified = true;
                editWindow.Show();
            }
            GUI.enabled = true;

            if (LocalConfigPresets.Count > 1) //Esto permite que no se pueda Eliminar la configuracion por defecto.
                GUI.enabled = true;
            else
                GUI.enabled = false;

            if (GUILayout.Button("Delete"))
            {
                LocalConfigPresets.Remove(LocalConfigPresets[PresetSelected]);
                if (LocalConfigPresets.Count - 1 >= 0)
                    PresetSelected = LocalConfigPresets.Count - 1;
                else
                    PresetSelected = 0;
                HasBeenModified = true;
                ConfirmIfAviablePresetsExist();
            }
            GUI.enabled = true; //Habilita todo lo demás.
            EditorGUILayout.EndHorizontal();//--------------------------------------Here ends a Horizontal Group.
            #endregion
            //-------------------------------------------//Body\\-----------------------------------------------------------------------------------------
            #region Body
            GUILayout.Space(3);
            HorizontalLine(Color.grey);
            HorizontalLine(Color.grey);
            GUILayout.Space(3);

            ConfigScrollPos = EditorGUILayout.BeginScrollView(ConfigScrollPos, false, false, new GUILayoutOption[] { GUILayout.Height(150) });
            if (LocalConfigPresets.Count > 0)
            {
                WritableConfigPresetsAviable = true;
                LocalFolderPresets = LocalConfigPresets[PresetSelected].GetFolderPresets();
                foreach (var item in LocalConfigPresets[PresetSelected].GetFolderPresets())
                    ShowFolderConfigSet(item);
            }
            else
                WritableConfigPresetsAviable = false;
            EditorGUILayout.EndScrollView();

            GUILayout.Space(3);
            HorizontalLine(Color.grey);
            HorizontalLine(Color.grey);
            GUILayout.Space(3);
            #endregion
            //-------------------------------------------//Bottom\\---------------------------------------------------------------------------------------
            #region Bottom
            EditorGUI.BeginDisabledGroup(!WritableConfigPresetsAviable);

            EditorGUILayout.PrefixLabel("Add New Folders", new GUIStyle() { fontStyle = FontStyle.Bold });
            EditorGUILayout.BeginHorizontal();//------------------------------------This is a Horizontal Group.
                                              //---------------------------------------Creo un nuevo Preset de Carpeta---------------------------
                                              //EditorGUILayout.LabelField("New Folder name: ", GUILayout.Width(75));

            if (GUILayout.Button("Create New Folder"))
            {
                if (LocalConfigPresets.Count > 0)
                {
                    List<string> NewAviableExtentionList = new List<string> { ".mat" }; //Guardamos la Lista de extensiones.
                    LocalConfigPresets[PresetSelected].AddFolderPreset(newFolderName, newFolderTypeSelected, NewAviableExtentionList);//Guardamos el preset de la carpeta.
                    LocalFolderPresets = LocalConfigPresets[PresetSelected].GetFolderPresets();
                    newFolderName = DefaultFolderName;//Reseteamos la previsualizacion del nombre.
                    newFolderTypeSelected = 0;//Reseteamos la previsualizacion del tipo.
                }
                HasBeenModified = true;
            }
            newFolderName = EditorGUILayout.TextField(newFolderName);
            newFolderTypeSelected = EditorGUILayout.Popup(newFolderTypeSelected, FolderTypeOptions);
            EditorGUILayout.EndHorizontal();//--------------------------------------Here ends a Horizontal Group.
            EditorGUI.EndDisabledGroup();
            #endregion
        }
        private void ShowFolderConfigSet(FolderConfig FolderData)
        {
            //Selected Type
            var CurrentType = LocalFilters[FolderData.TypeIndex];
            //Selected Local Type.
            var CurrentListOfExtentions = CurrentType.ExtentionsAllowed;

            EditorGUILayout.BeginHorizontal();//------------------------------------This is a Horizontal Group.
            FolderData.FolderName = EditorGUILayout.DelayedTextField(FolderData.FolderName); //Nombre de la carpeta.

            EditorGUILayout.LabelField("Type:", new GUIStyle() { fontStyle = FontStyle.Bold }, new GUILayoutOption[] { GUILayout.MaxWidth(50) });
            FolderData.TypeIndex = EditorGUILayout.Popup(FolderData.TypeIndex, FolderTypeOptions);

            //La condicion es que haya mas de una sub-opcion.
            EditorGUI.BeginDisabledGroup(CurrentListOfExtentions.Count > 1 ? true : false);//CheckPoint2 : Invertir el True/False.
            FolderData.EnableFolderPerExtention = EditorGUILayout.Toggle(FolderData.EnableFolderPerExtention, new GUILayoutOption[] { GUILayout.MaxWidth(20) });
            EditorGUILayout.LabelField("Folder Per Extention");
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();//--------------------------------------Here ends a Horizontal Group.
        } // CheckPoint1 : Chequeamos la opcion folder per ext, las extensiones se bloquean.
        private void ShowTypeGUI()
        {
            //MonoBehaviour.print(LocalFilters.Count);
            //MonoBehaviour.print("Min size --> X: " + minSize[0] + "/ Y: " + minSize[1]);
            //MonoBehaviour.print("Max size --> X: " + maxSize[0] + "/ Y: " + maxSize[1]);
            ExtentionScrollPos = EditorGUILayout.BeginScrollView(ExtentionScrollPos, false, false, new GUILayoutOption[] { GUILayout.Width(_MainWindowWidth), GUILayout.Height(_MainWindowHeight - 50), GUILayout.MinWidth(_MainWindowWidth) });
            foreach (var Type in LocalFilters)
            {
                EditorGUILayout.LabelField(Type.FilterName);
            }
            EditorGUILayout.EndScrollView();
        }
        private void ShowExtentionGUI()
        {
            ExtentionScrollPos = EditorGUILayout.BeginScrollView(ExtentionScrollPos, false, false, new GUILayoutOption[] { GUILayout.Width(_MainWindowWidth), GUILayout.Height(_MainWindowHeight - 115) });

            if (LocalExtentions.Count > 0)
            {
                foreach (var Extention in LocalExtentions)
                {
                    showExtentions(Extention);
                }
                if (ExtentionsToClear.Count > 0)
                    ClearExtentionList();
            }
            EditorGUILayout.EndScrollView();

            //---------------------------------------------------Botton-----------------------------------------------------
            EditorGUILayout.PrefixLabel("Expand current extentions Aviable");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("New Extention: ");
            NewExtentionName = EditorGUILayout.TextField(NewExtentionName);
            EditorGUILayout.EndHorizontal();

            if (NewExtentionName == "" || NewExtentionName == ".")
                NewExtentionNameIsInvalid = true;
            else
                NewExtentionNameIsInvalid = false;

            EditorGUI.BeginDisabledGroup(NewExtentionNameIsInvalid);
            if (GUILayout.Button("Add New Extention"))
            {
                LocalExtentions.Add(NewExtentionName);
                NewExtentionName = DefaultExtentionName;
                HasBeenModified = true;
            }
            EditorGUI.EndDisabledGroup();
        }

        private void ClearExtentionList()
        {
            foreach (var ext in ExtentionsToClear)
                LocalExtentions.Remove(ext);
            HasBeenModified = true;
            ExtentionsToClear.Clear();
            Repaint();
        }

        private void showExtentions(string Extention)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(Extention);
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("Remove Extention"))
                ExtentionsToClear.Add(Extention);
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
        }

        //----------------------------------------------------------------------------------------------------------------------------------------------
        #region Auxiliares
        /// <summary>
        /// Se encarga de mostrar la lista de presets disponibles.
        /// </summary>
        private void ConfirmIfAviablePresetsExist()
        {
            if (LocalConfigPresets.Count > 0)
            {
                AviablePresets = new string[LocalConfigPresets.Count];
                for (int i = 0; i < LocalConfigPresets.Count; i++)
                {
                    AviablePresets[i] = LocalConfigPresets[i].ConfigurationName;
                }
            }
            else
                AviablePresets = new string[] { "Empty" };
        }
        /// <summary>
        /// Localiza en el proyecto la instancia que contiene la Data
        /// </summary>
        public static void LoadConfigInfo()
        {
            if (!AuxiliaryMethods.ContainsItem("ProjectFolderConfigData", ConfigRoute))
            {
                AssetDatabase.CreateFolder("Assets/Editor", "Config");
                ScriptableObjectUtility.CreateAsset<ProjectFolderConfigData>(ConfigRoute, out ConfigData, false);
                MonoBehaviour.print("El archivo de Configuracion no existe!\n Se ha creado la configuracion dentro de la carpeta Config");
            }

            //Cargo la info guardada.
            ConfigData = AssetDatabase.LoadAssetAtPath<ProjectFolderConfigData>(ConfigRoute + "/ProjectFolderConfigData.Asset");

            LocalConfigPresets = ConfigData.Presets.Count > 0 ? new List<ConfigPreset>(ConfigData.Presets) : new List<ConfigPreset>();
            LocalExtentions = new List<string>(ConfigData.Extentions);
            LocalFilters = new List<FilterType>(ConfigData.FilterTypes);

            FolderTypeOptions = new string[LocalFilters.Count];
            for (int i = 0; i < LocalFilters.Count; i++)
                FolderTypeOptions[i] = LocalFilters[i].FilterName;

            MonoBehaviour.print("Encontre el archivo de configuracion. \n Configuracion cargada.");
        }
        private static void SaveConfigInfo()
        {
            //Copio la configuracion almacenada hasta el momento.
            ConfigData.Presets.Clear();
            ConfigData.Presets = new List<ConfigPreset>(LocalConfigPresets); //Guardo en mi scriptable object los Presets de configuraciones Creadas.
            foreach (var item in ConfigData.Presets)
            {
                var existingPresets = item.FolderPresets;
                foreach (var Preset in existingPresets)
                {
                    MonoBehaviour.print(Preset.FolderName);
                }
            }

            ConfigData.Extentions = new List<string>(LocalExtentions);
            ConfigData.FilterTypes = new List<FilterType>(LocalFilters);
        }
        static void HorizontalLine(Color color)
        {
            // create your style
            GUIStyle horizontalLine = new GUIStyle();
            horizontalLine.normal.background = EditorGUIUtility.whiteTexture;
            horizontalLine.margin = new RectOffset(0, 0, 4, 4);
            horizontalLine.fixedHeight = 3;

            var c = GUI.color;
            GUI.color = color;
            GUILayout.Box(GUIContent.none, horizontalLine);
            GUI.color = c;
        }
        #endregion
    }

    /// <summary>
    /// Esta ventana Permite la edicion del los presets.
    /// </summary>
    public class EditPreset : EditorWindow
    {
        //Nombre
        private string InputName = "";
        private string OriginalName = "";
        private static ConfigPreset InEdit;

        public static void OpenWindow()
        {
            var MainWindow = GetWindow<EditPreset>();
            MainWindow.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.PrefixLabel("Edit this configuration name");
            InputName = EditorGUILayout.TextField("Preset Name: ", InputName);
            
            if (GUILayout.Button("Guardar Cambios"))
            {
                InEdit.ConfigurationName = InputName;
                InputName = InEdit.ConfigurationName;
                OriginalName = InEdit.ConfigurationName;
            }

            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("Salir"))
                Close();
        }

        public EditPreset ToEdit(ConfigPreset EditablePreset)
        {
            OriginalName = EditablePreset.ConfigurationName;
            InputName = OriginalName;
            InEdit = EditablePreset;
            return this;
        }
    }
}
