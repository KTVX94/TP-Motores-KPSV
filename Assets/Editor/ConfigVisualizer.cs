using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

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

		private static List<string> ExtentionsToClear = new List<string>();
		private static List<FilterType> FiltersToClear = new List<FilterType>();
		private static List<FolderConfig> FoldersToClear = new List<FolderConfig>();

		public static string[] MainTabs = new string[] { "Configs", "Types", "Extentions" };//Prestañas Principales.
		public static string[] FolderTypeOptions; //Array de Types, Copia Local de los nombres de Filtro dentro de mi archivo de Configuración.

		public static string ConfigRoute;


		int newFolderTypeSelected = 0;

		const string DefaultFolderName = "NewFolder";
		const string DefaultExtentionName = "";
		const string DefaultTypeName = "New Type";

		string newFolderName = DefaultFolderName;
		string NewExtentionName = DefaultExtentionName;
		string NewTypeName = DefaultTypeName;

		string[] AviablePresets;

		bool NewExtentionNameIsInvalid = false;
		bool EnableRename = false;
		bool WritableConfigPresetsAviable = false;

		Vector2 ExtentionScrollPos = Vector2.zero;
		Vector2 ConfigScrollPos = Vector2.zero;
		static int _MainWindowHeight = 300;
		static int _MainWindowWidth = 450;

		//----------------------------------------------------------------------------------------------------------------------------------------------
		[MenuItem("CustomTools/FolderConfig")]
		public static void OpenWindow()
		{
			var MainWindow = GetWindow<ConfigVisualizer>(); //Obtengo una instancia nueva de esta ventana.
			ConfigRoute = Application.dataPath + "/Editor/Config";
			LoadConfigInfo();
			MainWindow.minSize = new Vector2(_MainWindowWidth, _MainWindowHeight);
			MainWindow.maxSize = new Vector2(_MainWindowWidth * 3, _MainWindowHeight * 3);
			MainWindow.Show(); //Muestro la ventana.
		}
		public static void OpenWindow(int index = 0)
		{
			var MainWindow = GetWindow<ConfigVisualizer>(); //Obtengo una instancia nueva de esta ventana.
			MainWindow.PresetSelected = index;
			ConfigRoute = Application.dataPath + "/Editor/Config";
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

			//For Debugg.
			if (GUILayout.Button("Check Saved Folders"))
			{
				//var configs = AssetDatabase.LoadAssetAtPath<ProjectFolderConfigData>(ConfigRoute + "/ProjectFolderConfigData.Asset");
				if (ProjectFolderConfig.Configurations.Presets.Count < 0)
				{
					MonoBehaviour.print("No hay presets Guardados dentro de la configuracion.");
				}else
				foreach (var Preset in ProjectFolderConfig.Configurations.Presets)
				{
					if (Preset.FolderPresets.Count > 0)
					{
					   foreach (var folder in Preset.FolderPresets)
					   {
						  MonoBehaviour.print("Configuracion guardada --->" + " Preset: " + Preset.ConfigurationName + " Carpeta: " + folder.FolderName);
					   }
					}
					else
						MonoBehaviour.print("No hay configuraciones guardadas");
				}
			}


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
                item.isDefault = false;
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
			if (GUILayout.Button("Duplicate"))
			{
				LocalConfigPresets.Add(LocalConfigPresets[PresetSelected].Clone());
			}

			EditorGUI.BeginDisabledGroup(LocalConfigPresets[PresetSelected].isDefault);
			if (GUILayout.Button("Edit"))
			{
				var editWindow = GetWindow<EditPreset>();
				editWindow.ToEdit(LocalConfigPresets[PresetSelected]);
				HasBeenModified = true;
				editWindow.Show();
			}
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
			EditorGUI.EndDisabledGroup();

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
				if (FoldersToClear.Count > 0)
					ClearFolderList();
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

					foreach (var preset in LocalConfigPresets)
					{
						foreach (var folder in preset.FolderPresets)
						{
							MonoBehaviour.print("Presets Creados -->" + " Preset: " + preset.ConfigurationName + "Carpeta: " + folder.FolderName);
						}
					}
				}
				HasBeenModified = true;
			}
			newFolderName = EditorGUILayout.TextField(newFolderName);
			if (FolderTypeOptions == null)
				FolderTypeOptions = new string[] { "Empty" };
			if (FolderTypeOptions != null)
				newFolderTypeSelected = EditorGUILayout.Popup(newFolderTypeSelected, FolderTypeOptions);
			
			EditorGUILayout.EndHorizontal();//--------------------------------------Here ends a Horizontal Group.
			EditorGUI.EndDisabledGroup();
			#endregion
		}
		private void ShowFolderConfigSet(FolderConfig FolderData)
		{
			//Selected Type
			var CurrentType = LocalFilters[FolderData.TypeIndex];

			EditorGUILayout.BeginHorizontal();//------------------------------------This is a Horizontal Group.
			FolderData.FolderName = EditorGUILayout.DelayedTextField(FolderData.FolderName,new GUILayoutOption[] {GUILayout.MinWidth(100)}); //Nombre de la carpeta.

			EditorGUILayout.LabelField("Type:", new GUIStyle() { fontStyle = FontStyle.Bold }, new GUILayoutOption[] { GUILayout.MaxWidth(50) });
			FolderData.TypeIndex = EditorGUILayout.Popup(FolderData.TypeIndex, FolderTypeOptions);

			//La condicion es que haya mas de una sub-opcion.
			bool MultipleExt = CurrentType.HasMultipleExtentions;
			
			EditorGUI.BeginDisabledGroup(!MultipleExt);
			bool value = FolderData.FolderPerExtention;
			FolderData.FolderPerExtention = EditorGUILayout.Toggle(FolderData.FolderPerExtention, new GUILayoutOption[] { GUILayout.MaxWidth(20) });
			if (value != FolderData.FolderPerExtention)
				HasBeenModified = true;

			EditorGUILayout.LabelField("Folder Per Extention");
			GUI.backgroundColor = Color.red;
			EditorGUI.EndDisabledGroup();
            EditorGUI.BeginDisabledGroup(FolderData.IsDefault);
			if (GUILayout.Button("Remove Folder"))
				FoldersToClear.Add(FolderData);
            EditorGUI.EndDisabledGroup();
			GUI.backgroundColor = Color.white;

			EditorGUILayout.EndHorizontal();//--------------------------------------Here ends a Horizontal Group.
		}

		private void ClearFolderList()
		{
			foreach (var FolderToClear in FoldersToClear)
				LocalFolderPresets.Remove(FolderToClear);
			HasBeenModified = true;
			FoldersToClear.Clear();
			Repaint();
		}
		//Types------------------------------------------------
		private void ShowTypeGUI()
		{
			ExtentionScrollPos = EditorGUILayout.BeginScrollView(ExtentionScrollPos, false, false, new GUILayoutOption[] { GUILayout.Width(_MainWindowWidth), GUILayout.Height(_MainWindowHeight - 50), GUILayout.MinWidth(_MainWindowWidth) });

			List<FilterType> toEdit = new List<FilterType>();
			foreach (var Type in LocalFilters)
			{
				
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(Type.FilterName,new GUILayoutOption[] { GUILayout.MaxWidth(100)});
				EditorGUILayout.LabelField(Type.GetExtentionAllowedList());
				if (GUILayout.Button("Edit"))
				{
					MonoBehaviour.print("Extensiones Disponibles: " + LocalExtentions.Count);
					EditType.OpenWindow(Type, Repaint,LocalExtentions);
				}
				GUI.backgroundColor = Color.red;
				if (GUILayout.Button("Remove Type"))
				{
					FiltersToClear.Add(Type);
				}
				GUI.backgroundColor = Color.white;
				EditorGUILayout.EndHorizontal();
			}
			if (FiltersToClear.Count > 0)
				ClearFilterList();

			EditorGUILayout.EndScrollView();


			//Añadir types.
			EditorGUILayout.BeginHorizontal();

			NewTypeName = EditorGUILayout.TextArea(NewTypeName);
			//Mostrar todos los extentions y permitir seleccionarlos.
			if (GUILayout.Button("Add Type"))
			{
				LocalFilters.Add(new FilterType(NewTypeName,new List<string>()));
				NewTypeName = DefaultTypeName;
				HasBeenModified = true;
			}

			EditorGUILayout.EndHorizontal();
		}

		private void ClearFilterList()
		{
			foreach (var Filtertype in FiltersToClear)
				LocalFilters.Remove(Filtertype);
			HasBeenModified = true;
			FiltersToClear.Clear();
			Repaint();
		}
		//Extentions---------------------------------------
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
			{
				AviablePresets = new string[] { "Empty" };
				PresetSelected = 0;
			}
		}
		/// <summary>
		/// Localiza en el proyecto la instancia que contiene la Data
		/// </summary>
		public static void LoadConfigInfo()
		{
			ConfigRoute = Application.dataPath + "/Editor/Config";
			//Si el directorio no existe...
			if (!Directory.Exists(ConfigRoute))
			{
				AssetDatabase.CreateFolder("Assets/Editor", "Config");
				MonoBehaviour.print("La carpeta de configuración no existe.\nSe ha creado una nueva carpeta.");
			}

			//Si el archivo no existe...
			if (File.Exists(ConfigRoute + "/ProjectConfigData.json"))
			{
				//Cargo la info guardada.
				MonoBehaviour.print("Encontre el archivo de configuracion. \n Cargando configuracion...");
				string Data = File.ReadAllText(ConfigRoute + "/ProjectConfigData.json");
				ConfigSet LoadedData = JsonUtility.FromJson<ConfigSet>(Data);

				if (LoadedData != null)
				{
					foreach (var Preset in LoadedData.Presets)
					{
						MonoBehaviour.print("Se ha cargado el preset ---> " + Preset.ConfigurationName);
						foreach (var Folder in Preset.FolderPresets)
						{
							MonoBehaviour.print("--> Se ha cargado la carpeta: " + Folder.FolderName);
						}
					}
					ProjectFolderConfig.Configurations = LoadedData;
					MonoBehaviour.print("Configuracion cargada Correctamente.\n Hurra!");
				}
				else
					MonoBehaviour.print("El archivo de configuracion cargada estaba vacía.");

				LocalConfigPresets = LoadedData != null && LoadedData.Presets.Count > 0 ? new List<ConfigPreset>(LoadedData.Presets) : new List<ConfigPreset>();
				LocalExtentions = LoadedData != null && LoadedData.Extentions.Count > 0 ? new List<string>(LoadedData.Extentions) : new List<string>();
				LocalFilters = LoadedData != null && LoadedData.FilterTypes.Count > 0 ? new List<FilterType>(LoadedData.FilterTypes) : new List<FilterType>()
		{
			new FilterType("Any", new List<string>()),
			new FilterType("AnimationClip",new List<string>()),
			new FilterType("Material",new List<string>()),
			new FilterType("AudioClip",new List<string>()),
			new FilterType("AudioMixer",new List<string>()),
			new FilterType("Font", new List<string>()),
			new FilterType("GUISkin",new List<string>()),
			new FilterType("Mesh",new List<string>()),
			new FilterType("Model",new List<string>(){".FBX",".OBJ",".DAE" }),
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

				if (LocalFilters.Count > 0)
				{
					FolderTypeOptions = new string[LocalFilters.Count];
					for (int i = 0; i < LocalFilters.Count; i++)
						FolderTypeOptions[i] = LocalFilters[i].FilterName;
				}
				else
					FolderTypeOptions = new string[] { "Empty" };
			}
			else
			{
				//Si no existe el archivo, creo uno nuevo, pero vacío.
				string JsonFilePath = ConfigRoute + "/ProjectConfigData.json";
				MonoBehaviour.print("Path creado: " + JsonFilePath);
				StreamWriter A = File.CreateText(JsonFilePath);
				A.Close();
				LoadConfigInfo();
			}
		}
		private static void SaveConfigInfo()
		{
			//Primero copio la configuracion local a mi clase estática.
			ProjectFolderConfig.Configurations.Presets = new List<ConfigPreset>(LocalConfigPresets);
			ProjectFolderConfig.Configurations.Extentions = new List<string>(LocalExtentions);
			ProjectFolderConfig.Configurations.FilterTypes = new List<FilterType>(LocalFilters);

			//Chequeo si existe la carpeta, sino, la creo.
			if (!Directory.Exists(ConfigRoute))
			{
				MonoBehaviour.print("La carpeta de configuracion no existe.\nSe ha creado una nueva carpeta de Configuracion en la ruta indicada.");
				AssetDatabase.CreateFolder("Assets/Editor", "Config");
			}
			else
				MonoBehaviour.print("La carpeta de configuracion existe.\nBuscando archivo de configuracion...");

			//For Debug.
			foreach (var Preset in ProjectFolderConfig.Configurations.Presets)
				foreach (var Folder in Preset.FolderPresets)
					MonoBehaviour.print("Folder Preset Saved -->" + "Preset: " + Preset.ConfigurationName + " Folder: " + Folder.FolderName);

			//Si no existe el archivo, creo uno nuevo.Si ya existe, lo sobreescribo. //WriteAllText autodetecta si el texto ya existe o no.
			string JsonObject = JsonUtility.ToJson(ProjectFolderConfig.Configurations);//Convierto mi objeto en un string json.
			//MonoBehaviour.print("String creado: " + JsonObject);

			string JsonFilePath = ConfigRoute + "/ProjectConfigData.json";//Creo la ruta de guardado del objeto.
			//MonoBehaviour.print("Path creado: " + JsonFilePath);
			//StreamWriter A = File.CreateText(JsonFilePath);//UsoStreamWriter para crear un texto. Nota: WriteAllText hace lo mismo y ademas se fija si el objeto de destino ya existe.
			//A.Close();

			File.WriteAllText(JsonFilePath, JsonObject);
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
	public class EditType : EditorWindow
	{
		public List<string> ObjectExtentionsAviableClone = new List<string>();
		public bool[] ExtentionsSelectedClone;

		public delegate void ReDraw();
		public static ReDraw RedrawMethod;

		//Nombre
		private string _inputName = "";
		private string _originalName = "";
		private List<string> _aviableExtentions = new List<string>();
		private FilterType _originalEditable;
		private Rect buttonRect;

		public static void OpenWindow(FilterType ToEdit, ReDraw A,List<string> ListOfExtentions)
		{
			RedrawMethod = A;
			var MainWindow = GetWindow<EditType>();
			MainWindow._originalEditable = ToEdit; //ObjetoOriginal.
			//Nombres
			MainWindow._originalName = MainWindow._originalEditable.FilterName;
			MainWindow._inputName = MainWindow._originalName;
			//Extensiones.
			MainWindow._aviableExtentions = ListOfExtentions;
			MainWindow.ObjectExtentionsAviableClone = new List<string>(MainWindow._originalEditable.ExtentionsAllowed);

			MainWindow.ExtentionsSelectedClone = new bool[ListOfExtentions.Count];

			if (MainWindow.ObjectExtentionsAviableClone.Count > 0)
			{
				for (int i = 0; i < MainWindow._aviableExtentions.Count; i++)
				{
					if (MainWindow.ObjectExtentionsAviableClone.Contains(MainWindow._aviableExtentions[i]))
						MainWindow.ExtentionsSelectedClone[i] = true;
					else
						MainWindow.ExtentionsSelectedClone[i] = false;
				}
			}

			MainWindow.Show();
		}

		private void OnDestroy()
		{
			RedrawMethod();
		}

		private void OnGUI()
		{
			EditorGUILayout.PrefixLabel("Edit this Search Type");
			_inputName = EditorGUILayout.TextField("Type Name: ", _inputName);
			EditorGUILayout.LabelField("Extentions Allowed:", GetExtentionAllowedList());


			GUILayout.Label("Add or Substract Extentions for this Filter Type", EditorStyles.boldLabel);
			if (GUILayout.Button("Select Extentions"))
			{
				PopupWindow.Show(buttonRect, new ExtentionSelector(this,_aviableExtentions,ExtentionsSelectedClone));
			}
			if (Event.current.type == EventType.Repaint) buttonRect = GUILayoutUtility.GetLastRect();

			GUILayout.Space(50);

			GUI.backgroundColor = Color.green;
			if (GUILayout.Button("Guardar Cambios"))
			{
				//Cambios al objeto Original.
				if (ObjectExtentionsAviableClone.Count > 1)_originalEditable.HasMultipleExtentions = true;

				_originalEditable.FilterName = _inputName;
				_originalEditable.ExtentionsAllowed = ObjectExtentionsAviableClone;

				//Refresh del editor.
				_originalName = _originalEditable.FilterName;
				_inputName = _originalName;
				ConfigVisualizer.HasBeenModified = true;
			}
			GUI.backgroundColor = Color.red;
			if (GUILayout.Button("Salir"))
				Close();
		}

		private string GetExtentionAllowedList()
		{
			string ExtentionList = "";
			if (ObjectExtentionsAviableClone != null && ObjectExtentionsAviableClone.Count > 0)
			{
				for (int i = 0; i < ObjectExtentionsAviableClone.Count; i++)
				{
					if (i == ObjectExtentionsAviableClone.Count - 1)
						ExtentionList += ObjectExtentionsAviableClone[i] + ";";
					else
						ExtentionList += ObjectExtentionsAviableClone[i] + ", ";
				}
				return ExtentionList;
			}
			else return ExtentionList;

		}
	}
	public class ExtentionSelector : PopupWindowContent
	{
		List<string> _AviableExtentions = new List<string>();
		bool[] _ExtentionsSelected;
		List<string> TypeExtentionsAllowed = new List<string>();
		EditType _OnEdit;
		Vector2 ScrollPos = new Vector2();

		public ExtentionSelector(EditType InEdit, List<string> Extentions,bool[] Selections)
		{
			_OnEdit = InEdit;
			_AviableExtentions = Extentions;
			_ExtentionsSelected = Selections;
		}

		public override Vector2 GetWindowSize()
		{
			return new Vector2(300, 150);
		}

		public override void OnGUI(Rect rect)
		{
			GUILayout.Label("Extentions Aviable", EditorStyles.boldLabel);
			ScrollPos = EditorGUILayout.BeginScrollView(ScrollPos,false,false);
			for (int i = 0; i < _AviableExtentions.Count; i++)
			{
				//_AviableExtentions[i] = String Extension.
				//_ExtentionsSelected[i] = bool Correspondiente.
				_ExtentionsSelected[i] = GUILayout.Toggle(_ExtentionsSelected[i], _AviableExtentions[i]);
			}
			EditorGUILayout.EndScrollView();
		}

		//public override void OnOpen()
		//{
		//    Debug.Log("Popup opened: " + this);
		//}

		public override void OnClose()
		{
			for (int i = 0; i < _ExtentionsSelected.Length; i++)
				if (_ExtentionsSelected[i])
					TypeExtentionsAllowed.Add(_AviableExtentions[i]);
			_OnEdit.ObjectExtentionsAviableClone = TypeExtentionsAllowed;
			_OnEdit.ExtentionsSelectedClone = _ExtentionsSelected;
		}
	}
}
