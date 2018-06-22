using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ProjectPreConfig
{
	public class ProjectOrganizer : EditorWindow
	{
		//--------------------------------------------Variables-----------------------------------------------------------------------------------
		//StoredInfo
		static int SelectedPreset = 0;
		string ConfigRoute;
		string[] _AviablePresets;

		private List<string> _selectedFolders = new List<string>();//Carpetas que estare filtrando.
		private List<string[]> _findedElements = new List<string[]>();//donde: [0] = nombre del asset, [1] = extension, [2] ruta original y [3] Ruta de destino deseado.

		//----------------------------------------Basic Methods-------------------------------------------------------------------------------------
		[MenuItem("CustomTools/OrganizeProject")]
		public static void OpenWindow()
		{
			var MainWindow = GetWindow<ProjectOrganizer>();
			MainWindow.ConfigRoute = Application.dataPath + "/Editor/Config";
			MainWindow.LoadConfigInfo();
			MainWindow.Show();
		}
		private void LoadConfigInfo()
		{
			if (!Directory.Exists(ConfigRoute))
			{
				ConfigVisualizer.OpenWindow();
			}
			else
			{
				MonoBehaviour.print("La carpeta de configuracion existe.\nBuscando archivo de configuracion...");
				if (File.Exists(ConfigRoute + "/ProjectConfigData.json"))
					ConfigVisualizer.LoadConfigInfo();
				else
					ConfigVisualizer.OpenWindow();
			}
		}

		private void OnGUI()
		{
			var Configs = ProjectFolderConfig.Configurations.Presets;
			//------------------------------------------------------------------------------------------------------------------------
			//--------------------------------------Organization Select---------------------------------------------------------------
			EditorGUILayout.BeginHorizontal();
			if (Configs.Count > 0)
			{
				List<string> ConfigNames = new List<string>();
				foreach (var item in ProjectFolderConfig.Configurations.Presets)
				{
					ConfigNames.Add(item.ConfigurationName);

					_selectedFolders.Clear();
					foreach (var folder in item.FolderPresets)
					{
						_selectedFolders.Add(folder.FolderName);
					}
				}
				_AviablePresets = ConfigNames.ToArray();
			}
			else _AviablePresets = new string[] { "Empty" };

			SelectedPreset = EditorGUILayout.Popup(SelectedPreset, _AviablePresets);

			//------------------------------------------------------------------------------------------------------------------------
			GUI.backgroundColor = Color.cyan;
			//El valor por defecto de extension va a ser (All), significando que la carpeta acepta todo tipo de archivos.
			if (GUILayout.Button("Show FolderConfig")) //Este boton nos abre las configuraciones.
			{
				if (_AviablePresets.Length > 1)
					ConfigVisualizer.OpenWindow(SelectedPreset);
				else
					ConfigVisualizer.OpenWindow();
			}
			EditorGUILayout.EndHorizontal();
			//--------------------------------------Asset Search----------------------------------------------------------------------
			GUI.backgroundColor = Color.white;
			if (GUILayout.Button("Scan Project!"))
			{
				CheckIFDefaultFolderExists();
				ScanProject();
			}
			//-------------------------------------Reorganize Assets!------------------------------------------------------------------
			GUI.backgroundColor = Color.green;
			EditorGUI.BeginDisabledGroup(_findedElements.Count > 0 ? false : true);
			if (GUILayout.Button("Reorganize!"))
			{
				MonoBehaviour.print("Elementos a ordenar: " + _findedElements.Count + ".");
				foreach (var item in _findedElements) //Funcion para mover los objetos encontrados.
				{
					MonoBehaviour.print(string.Format("El objeto {0} sera reubicado.",item[0]));
					AssetDatabase.MoveAsset(item[2],item[3]);
				}
				_findedElements.Clear();
			}
			EditorGUI.EndDisabledGroup();
		}

		/// <summary>
		/// Recorre los directorios del proyecto y encuentra archivos que estan fuera de lugar, segun la configuracion seleccionada.
		/// </summary>
		private void ScanProject()
		{
			//Limpio la busqueda anterior.
			_findedElements.Clear();

			//Chequeo todos los tipos cubiertos por las carpetas.
			var A = ProjectFolderConfig.Configurations.Presets[SelectedPreset];
			MonoBehaviour.print("Preset Seleccionado: " + A.ConfigurationName);


			List<string> _coveredTypes = new List<string>();//Tipos de assets que seran buscados.
			List<string[]> _folderDirs = new List<string[]>();//Informacion de la carpeta de destino.


			foreach (var folder in A.FolderPresets)
			{
				var ti = folder.TypeIndex;
				var n = ProjectFolderConfig.Configurations.FilterTypes[ti].FilterName;
				if (n != "Any")
				{
					string Fname = folder.FolderName;
					string FDir = "Assets/" + folder.FolderName;
					string Ftype = n;
					string[] Finfo = { Fname, FDir, Ftype};
					_folderDirs.Add(Finfo);
					_coveredTypes.Add(n);
				}
			}

			//For Debug.
			//foreach (var coveredType in CoveredTypes)
			//{
			//    MonoBehaviour.print("El tipo: " + coveredType + " esta en una de las configuraciones.");
			//}

			//Busco todos los archivos del tipo determinado en Assets. ---> Se obtiene una lista de todos los objetos que concuerden con el tipo dado dentro del proyecto.
			//Si la ruta del objeto, no concuerda con la ruta deseada del objeto, y las extensiones, son las correctas; Añadimos el objeto a la cola de reordenamiento.
			foreach (var type in _coveredTypes)
			{
				MonoBehaviour.print("Buscando: " + type);
				List<string> Extentions = new List<string>();
				foreach (var filter in ProjectFolderConfig.Configurations.FilterTypes)
					if (filter.FilterName == type)
					{
						Extentions = filter.ExtentionsAllowed;
						MonoBehaviour.print("Buscando extensiones:" + filter.GetExtentionAllowedList());
					}

				//Selecciono la carpeta de destino original.
				string route = "";
				foreach (var folder in _folderDirs)
					if (folder[2] == type)
						route = folder[1];

				MonoBehaviour.print("Ruta de carpeta deseada: " + route);

				String[] D = AssetDatabase.FindAssets("t:" + type);
				List<string[]> FirstFinded = new List<string[]>();
				if (D.Length > 0)
				{
					foreach (var item in D)
					{
						string a = AssetDatabase.GUIDToAssetPath(item); //Ruta relativa del asset.
						var b = GetNameAndExtention(a);

						if (b[2] != route + "/" + b[0] + b[1] && Extentions.Contains(b[1]))
						{
							string ObjectivePath = route + "/" + b[0] + b[1];
							string[] F = {b[0],b[1],b[2],ObjectivePath };
							if (!_findedElements.Contains(F))
								FirstFinded.Add(b);
							MonoBehaviour.print("El objeto: " + b[0] + " -----> No esta ordenado.\n Ruta objetivo deseada: " + ObjectivePath);

							//Debugear el objeto obtenido.
							string c = string.Format("Object Name: {0}, extention: {1}, and path: {2}", b[0], b[1], b[2]);
							MonoBehaviour.print(c);
						}
					}
				}
				if (FirstFinded.Count > 0)
					_findedElements.AddRange(FirstFinded);
				else
					MonoBehaviour.print(string.Format("No se han hallado objetos del tipo {0} que esten desordenados",type));
			}
		}

		//----------------------------------------Auxiliary Methods-----------------------------------------------------------------------------------
		#region Auxiliary Methods
		/// <summary>
		/// Recorre una lista por Defecto y genera carpetas dentro de Assets.
		/// </summary>
		private void CheckIFDefaultFolderExists()
		{
			MonoBehaviour.print("Chequeo si existen las carpetas por defecto");
			var AssetPath = Application.dataPath;
			foreach (var folder in _selectedFolders)
			{
				if (!Directory.Exists(AssetPath + "/" + folder))
				{
					MonoBehaviour.print("La carpeta: " + folder + " no existe.\n Se ha creado una nueva carpeta...");
					AssetDatabase.CreateFolder("Assets", folder);
				}
				else
					MonoBehaviour.print("La carpeta: " + folder + " existe.");
			}
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

	//................................................................................................................................................
	#region Almacenaje de datos
	public static class ProjectFolderConfig
	{
		public static ConfigSet Configurations = new ConfigSet();
	}

	[System.Serializable]
	public class ConfigSet
	{
		public List<ConfigPreset> Presets = new List<ConfigPreset>();
		public List<string> Extentions = new List<string>();
		public List<FilterType> FilterTypes = new List<FilterType>();

		//Probablemente esto ya no sea necesario.
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


	[System.Serializable]
	public class ConfigPreset
	{
		public bool isDefault = false;
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
			FolderConfig N = new FolderConfig(NewFolderName, MainType, Extentions);
			FolderPresets.Add(N);
		}
		public ConfigPreset Clone()
		{
			ConfigPreset Clone = new ConfigPreset();
			Clone.isDefault = false;
			Clone.ConfigurationName = ConfigurationName + "(Copy)";
			Clone.FolderPresets = new List<FolderConfig>(FolderPresets);
			return Clone;
		}
	}

	[System.Serializable]
	public class FolderConfig
	{
		public bool IsDefault;
		public string FolderName = "NewFolder";
		public int TypeIndex;
		public List<string> extentions;
		public bool FolderPerExtention = false;

		public FolderConfig(string Name, int AttachedType, List<string> AviableExtentions,bool IsDefault = false)
		{
			this.IsDefault = IsDefault;
			FolderName = Name;
			TypeIndex = AttachedType;
			extentions = AviableExtentions;
		}
	}

	[System.Serializable]
	public class FilterType
	{
		public bool IsDefault;
		public bool HasMultipleExtentions = false;
		public string FilterName;
		public List<string> ExtentionsAllowed;

		public FilterType(string Name, List<string> Extentions, bool Editable = false)
		{
			FilterName = Name;
			ExtentionsAllowed = Extentions;
			if (Extentions.Count > 1)
				HasMultipleExtentions = true;
			IsDefault = Editable;
		}

		public string GetExtentionAllowedList()
		{
			string ExtentionList = "";
			if (ExtentionsAllowed != null && ExtentionsAllowed.Count > 0)
			{
				for (int i = 0; i < ExtentionsAllowed.Count; i++)
				{
					if (i == ExtentionsAllowed.Count - 1)
						ExtentionList += ExtentionsAllowed[i] + ";";
					else
						ExtentionList += ExtentionsAllowed[i] + ", ";
				}
				return ExtentionList;
			}
			else return ExtentionList;
			
		}
	}
	#endregion
}
