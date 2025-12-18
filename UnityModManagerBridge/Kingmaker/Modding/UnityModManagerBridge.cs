using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using Code.Utility.ExtendedModInfo;
using dnlib.DotNet;
using JetBrains.Annotations;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.Modding;

public class UnityModManagerBridge : IModManagerBridge
{
	private const int UnityModManagerVersion = 13;

	private const string UmmKey = "UmmVersion";

	private const string OwlcatUnityModManagerAsset = "OwlcatUnityModManager.zip";

	private const string OwlcatUnityModManagerDllFileName = "UnityModManager.dll";

	private const string OwlcatUnityModManagerPdbFileName = "UnityModManager.pdb";

	private static readonly string OwlcatUnityModManagerPersistantDirectory;

	private static readonly string OwlcatUnityModManagerDllPath;

	private static readonly string OwlcatUnityModManagerPdbPath;

	private static readonly List<string> ModMakerDllDependencies;

	private bool m_Initialized;

	private Type m_OwlcatUnityModManagerType;

	public bool? DoorstopUsed { get; private set; }

	private Type OwlcatUnityModManagerType
	{
		get
		{
			if (m_OwlcatUnityModManagerType != null)
			{
				return m_OwlcatUnityModManagerType;
			}
			try
			{
				Assembly assembly = Assembly.LoadFrom(OwlcatUnityModManagerDllPath);
				m_OwlcatUnityModManagerType = assembly.GetType("UnityModManagerNet.UnityModManager");
			}
			catch (Exception ex)
			{
				Debug.LogError($"UnityModManager exception: {ex}");
				PFLog.UnityModManager.Exception(ex);
			}
			return m_OwlcatUnityModManagerType;
		}
	}

	private bool ReadyToUse
	{
		get
		{
			bool? doorstopUsed = DoorstopUsed;
			bool valueOrDefault = doorstopUsed.GetValueOrDefault();
			if (!doorstopUsed.HasValue)
			{
				valueOrDefault = DoorstopDetected();
				bool? doorstopUsed2 = valueOrDefault;
				DoorstopUsed = doorstopUsed2;
			}
			if (m_Initialized)
			{
				return !DoorstopUsed.Value;
			}
			return false;
		}
	}

	public void TryStart(string passedGameVersion)
	{
		AppDomain.CurrentDomain.FirstChanceException += delegate(object sender, FirstChanceExceptionEventArgs fe)
		{
			try
			{
				Exception exception = fe.Exception;
				PFLog.UnityModManager.Log("[FirstChanceException] " + exception.GetType().FullName + ": " + exception.Message);
				PFLog.UnityModManager.Log(exception.ToString());
				if (exception is TypeLoadException ex23)
				{
					PFLog.UnityModManager.Log("[TypeLoadException.TypeName] " + ex23.TypeName);
				}
			}
			catch
			{
			}
		};
		AppDomain.CurrentDomain.AssemblyResolve += delegate(object sender, ResolveEventArgs args)
		{
			PFLog.UnityModManager.Log("[AssemblyResolve] Requested: " + args.Name);
			PFLog.UnityModManager.Log(Environment.StackTrace);
			return (Assembly)null;
		};
		string text = EnsureValidGameVersion(passedGameVersion);
		bool? doorstopUsed = DoorstopUsed;
		bool valueOrDefault = doorstopUsed.GetValueOrDefault();
		if (!doorstopUsed.HasValue)
		{
			valueOrDefault = DoorstopDetected();
			bool? doorstopUsed2 = valueOrDefault;
			DoorstopUsed = doorstopUsed2;
		}
		if (DoorstopUsed.Value || !PrepareOwlcatUnityModManagerDll())
		{
			return;
		}
		try
		{
			try
			{
				try
				{
					RuntimeHelpers.RunClassConstructor(OwlcatUnityModManagerType.TypeHandle);
					PFLog.UnityModManager.Log("RunClassConstructor succeeded (no .cctor failure).");
				}
				catch (Exception ex)
				{
					PFLog.UnityModManager.Error("RunClassConstructor threw: " + ex.GetType().FullName + " -- " + ex.Message);
					PFLog.UnityModManager.Error(ex.ToString());
					for (Exception innerException = ex.InnerException; innerException != null; innerException = innerException.InnerException)
					{
						PFLog.UnityModManager.Error("Inner: " + innerException.GetType().FullName + " -- " + innerException.Message);
						PFLog.UnityModManager.Error(innerException.ToString());
					}
				}
				try
				{
					Assembly assembly = OwlcatUnityModManagerType.Assembly;
					string text2 = assembly.Location;
					if (string.IsNullOrEmpty(text2))
					{
						text2 = new Uri(assembly.CodeBase).LocalPath;
					}
					PFLog.UnityModManager.Log("Inspecting with dnlib at: " + text2);
					ModuleDefMD moduleDefMD = ModuleDefMD.Load(text2);
					TypeDef typeDef = moduleDefMD.Find(OwlcatUnityModManagerType.FullName, isReflectionName: false);
					if (typeDef != null)
					{
						MethodDef methodDef = typeDef.FindMethod("Start");
						if (methodDef != null && methodDef.HasBody)
						{
							PFLog.UnityModManager.Log("Start method body exists - listing TypeRefs used by module:");
							foreach (TypeRef typeRef in moduleDefMD.GetTypeRefs())
							{
								PFLog.UnityModManager.Log("dnlib TypeRef: " + typeRef.FullName);
							}
						}
						else
						{
							PFLog.UnityModManager.Log("Start method not found or no body via dnlib.");
						}
					}
					else
					{
						PFLog.UnityModManager.Log("Type not found in module via dnlib.");
					}
				}
				catch (Exception ex2)
				{
					LogChannel unityModManager = PFLog.UnityModManager;
					string text3 = "dnlib inspect failed: ";
					unityModManager.Error(text3 + ex2);
				}
				try
				{
					Assembly assembly2 = OwlcatUnityModManagerType.Assembly;
					try
					{
						Type[] types = assembly2.GetTypes();
						PFLog.UnityModManager.Log($"Assembly.GetTypes succeeded: {types.Length} types.");
					}
					catch (ReflectionTypeLoadException ex3)
					{
						PFLog.UnityModManager.Error("Assembly.GetTypes -> ReflectionTypeLoadException");
						for (int i = 0; i < ex3.LoaderExceptions.Length; i++)
						{
							Exception ex4 = ex3.LoaderExceptions[i];
							PFLog.UnityModManager.Error($"LoaderException[{i}]: {ex4?.GetType().FullName} : {ex4?.Message}");
							PFLog.UnityModManager.Error(ex4?.ToString());
						}
					}
				}
				catch (Exception ex5)
				{
					LogChannel unityModManager2 = PFLog.UnityModManager;
					string text4 = "Assembly.GetTypes outer error: ";
					unityModManager2.Error(text4 + ex5);
				}
				MethodInfo method = OwlcatUnityModManagerType.GetMethod("Start", BindingFlags.Static | BindingFlags.Public, null, new Type[1] { typeof(string) }, null);
				PFLog.UnityModManager.Log($"Found method: {method}");
				try
				{
					try
					{
						PFLog.UnityModManager.Log("Method ReturnType: " + method.ReturnType.FullName);
					}
					catch (Exception ex6)
					{
						LogChannel unityModManager3 = PFLog.UnityModManager;
						string text5 = "ReturnType probe failed: ";
						unityModManager3.Error(text5 + ex6);
					}
					try
					{
						ParameterInfo[] parameters = method.GetParameters();
						foreach (ParameterInfo parameterInfo in parameters)
						{
							PFLog.UnityModManager.Log("Param: " + parameterInfo.Name + " : " + parameterInfo.ParameterType.FullName);
						}
					}
					catch (Exception ex7)
					{
						LogChannel unityModManager4 = PFLog.UnityModManager;
						string text6 = "GetParameters probe failed: ";
						unityModManager4.Error(text6 + ex7);
					}
					try
					{
						foreach (CustomAttributeData customAttributesDatum in method.GetCustomAttributesData())
						{
							PFLog.UnityModManager.Log("Method attribute type: " + customAttributesDatum.AttributeType.FullName);
						}
					}
					catch (Exception ex8)
					{
						LogChannel unityModManager5 = PFLog.UnityModManager;
						string text7 = "GetCustomAttributesData(method) failed: ";
						unityModManager5.Error(text7 + ex8);
					}
					try
					{
						foreach (CustomAttributeData customAttributesDatum2 in OwlcatUnityModManagerType.GetCustomAttributesData())
						{
							PFLog.UnityModManager.Log("Type attribute: " + customAttributesDatum2.AttributeType.FullName);
						}
					}
					catch (Exception ex9)
					{
						LogChannel unityModManager6 = PFLog.UnityModManager;
						string text8 = "GetCustomAttributesData(type) failed: ";
						unityModManager6.Error(text8 + ex9);
					}
					try
					{
						FieldInfo[] fields = OwlcatUnityModManagerType.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
						foreach (FieldInfo fieldInfo in fields)
						{
							try
							{
								PFLog.UnityModManager.Log("Field: " + fieldInfo.Name + " : " + fieldInfo.FieldType.FullName);
							}
							catch (Exception ex10)
							{
								LogChannel unityModManager7 = PFLog.UnityModManager;
								string text9 = "Field type probe failed for ";
								string name = fieldInfo.Name;
								string text10 = " : ";
								unityModManager7.Error(text9 + name + text10 + ex10);
							}
						}
					}
					catch (Exception ex11)
					{
						LogChannel unityModManager8 = PFLog.UnityModManager;
						string text11 = "GetFields probe failed: ";
						unityModManager8.Error(text11 + ex11);
					}
					try
					{
						method.Invoke(null, new object[1] { text });
					}
					catch (TargetInvocationException ex12)
					{
						LogChannel unityModManager9 = PFLog.UnityModManager;
						string text12 = "Invoke threw TargetInvocationException -> inner: ";
						unityModManager9.Error(text12 + (ex12.InnerException?.ToString() ?? "<null>"));
					}
					catch (Exception ex13)
					{
						LogChannel unityModManager10 = PFLog.UnityModManager;
						string text13 = "Invoke threw: ";
						unityModManager10.Error(text13 + ex13);
					}
				}
				catch (Exception ex14)
				{
					LogChannel unityModManager11 = PFLog.UnityModManager;
					string text14 = "Unexpected probe error: ";
					unityModManager11.Error(text14 + ex14);
				}
			}
			catch (TargetInvocationException ex15)
			{
				Exception innerException2 = ex15.InnerException;
				PFLog.UnityModManager.Error("TargetInvocationException -> inner: " + (innerException2?.GetType().FullName ?? "<null>"));
				PFLog.UnityModManager.Error("Inner message: " + (innerException2?.Message ?? "<null>"));
				PFLog.UnityModManager.Error("Inner stack: " + (innerException2?.StackTrace ?? "<null>"));
				if (innerException2 is TypeLoadException ex16)
				{
					PFLog.UnityModManager.Error("TypeLoadException.TypeName = " + ex16.TypeName);
					PFLog.UnityModManager.Error("TypeLoadException.Message = " + ex16.Message);
				}
				if (innerException2 is ReflectionTypeLoadException ex17)
				{
					for (int k = 0; k < ex17.LoaderExceptions.Length; k++)
					{
						LogChannel unityModManager12 = PFLog.UnityModManager;
						string format = "LoaderException[{0}]: {1}";
						object arg = k;
						unityModManager12.Error(string.Format(format, arg, ex17.LoaderExceptions[k]?.ToString() ?? "<null>"));
					}
				}
				for (Exception ex18 = innerException2; ex18 != null; ex18 = ex18.InnerException)
				{
					PFLog.UnityModManager.Error("InnerWalk: " + ex18.GetType().FullName + " -- " + ex18.Message);
				}
			}
			catch (Exception ex19)
			{
				PFLog.UnityModManager.Error("Invoke exception: " + ex19.ToString());
			}
			try
			{
				Type owlcatUnityModManagerType = OwlcatUnityModManagerType;
				PFLog.UnityModManager.Log("Owlcat type: " + (((owlcatUnityModManagerType != null) ? owlcatUnityModManagerType.FullName : null) ?? "<null>"));
				PFLog.UnityModManager.Log("Owlcat assembly: " + (((owlcatUnityModManagerType != null) ? owlcatUnityModManagerType.Assembly.FullName : null) ?? "<null>"));
				Assembly assembly3 = ((owlcatUnityModManagerType != null) ? owlcatUnityModManagerType.Assembly : null);
				if (assembly3 != null)
				{
					AssemblyName[] referencedAssemblies = assembly3.GetReferencedAssemblies();
					foreach (AssemblyName assemblyName in referencedAssemblies)
					{
						PFLog.UnityModManager.Log("Referenced: " + assemblyName.FullName);
					}
				}
				PFLog.UnityModManager.Log("Loaded assemblies:");
				Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
				foreach (Assembly assembly4 in assemblies)
				{
					LogChannel unityModManager13 = PFLog.UnityModManager;
					string[] array = new string[5]
					{
						" - ",
						assembly4.GetName().Name,
						" (",
						null,
						null
					};
					int num = 3;
					Version version = assembly4.GetName().Version;
					array[num] = ((version != null) ? version.ToString() : null);
					array[4] = ")";
					unityModManager13.Log(string.Concat(array));
				}
			}
			catch (Exception ex20)
			{
				LogChannel unityModManager14 = PFLog.UnityModManager;
				string text15 = "Error while logging assemblies: ";
				unityModManager14.Error(text15 + ex20);
			}
			AppDomain.CurrentDomain.AssemblyResolve += delegate(object sender, ResolveEventArgs args)
			{
				PFLog.UnityModManager.Error("AssemblyResolve requested: " + args.Name);
				return (Assembly)null;
			};
			m_Initialized = true;
		}
		catch (Exception ex21)
		{
			if (ex21.InnerException is TypeLoadException ex22)
			{
				PFLog.UnityModManager.Log(ex22.TypeName);
				PFLog.UnityModManager.Log(OwlcatUnityModManagerType.FullName);
				PFLog.UnityModManager.Log(OwlcatUnityModManagerType.ToString());
				PFLog.UnityModManager.Log(OwlcatUnityModManagerType.Assembly.FullName);
			}
			PFLog.UnityModManager.Error($"UnityModManager exception: {ex21}");
			PFLog.UnityModManager.Exception(ex21);
		}
	}

	public void TryStartUI()
	{
		if (!ReadyToUse)
		{
			return;
		}
		try
		{
			OwlcatUnityModManagerType.GetMethod("UiFirstLaunch", BindingFlags.Static | BindingFlags.Public)?.Invoke(null, null);
		}
		catch (Exception ex)
		{
			PFLog.UnityModManager.Error($"Troubles while starting UnityModManagerUi; {ex}");
			PFLog.UnityModManager.Exception(ex);
		}
	}

	public List<ExtendedModInfo> GetAllModsInfo()
	{
		if (!ReadyToUse)
		{
			return null;
		}
		List<ExtendedModInfo> result = null;
		try
		{
			object obj = OwlcatUnityModManagerType.GetMethod("GetAllModsInfo", BindingFlags.Static | BindingFlags.Public)?.Invoke(null, null);
			if (obj == null)
			{
				return null;
			}
			result = (List<ExtendedModInfo>)obj;
		}
		catch (Exception ex)
		{
			PFLog.UnityModManager.Error($"Troubles while getting all mods info from UMM; {ex}");
			PFLog.UnityModManager.Exception(ex);
		}
		return result;
	}

	public ExtendedModInfo GetModInfo(string modId)
	{
		if (!ReadyToUse)
		{
			return null;
		}
		ExtendedModInfo result = null;
		try
		{
			object obj = OwlcatUnityModManagerType.GetMethod("GetModInfo", BindingFlags.Static | BindingFlags.Public)?.Invoke(null, new object[1] { modId });
			if (obj == null)
			{
				return null;
			}
			result = (ExtendedModInfo)obj;
		}
		catch (Exception ex)
		{
			PFLog.UnityModManager.Error($"Troubles while getting ModInfo from UMM; {ex}");
			PFLog.UnityModManager.Exception(ex);
		}
		return result;
	}

	public void CheckForUpdates()
	{
		if (!ReadyToUse)
		{
			return;
		}
		try
		{
			OwlcatUnityModManagerType.GetMethod("CheckForUpdates", BindingFlags.Static | BindingFlags.Public)?.Invoke(null, null);
		}
		catch (Exception ex)
		{
			PFLog.UnityModManager.Error($"Troubles while trying to update mods in UMM; {ex}");
			PFLog.UnityModManager.Exception(ex);
		}
	}

	public void OpenModInfoWindow(string modId)
	{
		if (!ReadyToUse)
		{
			return;
		}
		try
		{
			OwlcatUnityModManagerType.GetMethod("OpenModInfoWindow", BindingFlags.Static | BindingFlags.Public)?.Invoke(null, new object[1] { modId });
		}
		catch (Exception ex)
		{
			PFLog.UnityModManager.Error($"Troubles while trying to open mod settings UMM ui window; {ex}");
			PFLog.UnityModManager.Exception(ex);
		}
	}

	public void EnableMod(string modId, bool state)
	{
		if (!ReadyToUse)
		{
			return;
		}
		try
		{
			OwlcatUnityModManagerType.GetMethod("EnableMod", BindingFlags.Static | BindingFlags.Public)?.Invoke(null, new object[2] { modId, state });
		}
		catch (Exception ex)
		{
			PFLog.UnityModManager.Error($"Troubles while trying to open mod settings UMM ui window; {ex}");
			PFLog.UnityModManager.Exception(ex);
		}
	}

	private string EnsureValidGameVersion(string passedGameVersion)
	{
		if (!Version.TryParse(passedGameVersion, out var _))
		{
			return "1.0.0";
		}
		return passedGameVersion;
	}

	[UsedImplicitly]
	private void ModifyDependentDllsSearch()
	{
		if (m_Initialized)
		{
			return;
		}
		AppDomain.CurrentDomain.AssemblyResolve += delegate(object sender, ResolveEventArgs args)
		{
			string text = (args.Name.Contains(',') ? args.Name.Substring(0, args.Name.IndexOf(',')) : args.Name);
			text += ".dll";
			if (!ModMakerDllDependencies.Contains(text))
			{
				return (Assembly)null;
			}
			string path = Path.Combine(OwlcatUnityModManagerPersistantDirectory, text);
			try
			{
				return Assembly.LoadFile(path);
			}
			catch (Exception arg)
			{
				Debug.LogError($"{arg}");
				return (Assembly)null;
			}
		};
	}

	private bool DoorstopDetected()
	{
		string environmentVariable = Environment.GetEnvironmentVariable("DOORSTOP_INITIALIZED");
		if (environmentVariable == null || environmentVariable != "TRUE")
		{
			return false;
		}
		PFLog.UnityModManager.Error("External UnityModManager detected. Won't start OwlcatUnityModManager.");
		return true;
	}

	private bool UnityModManagerIntegrationIsUpToDate()
	{
		return PlayerPrefs.GetInt("UmmVersion", 0) == 13;
	}

	private void SaveActualUmmVersion()
	{
		PlayerPrefs.SetInt("UmmVersion", 13);
		PlayerPrefs.Save();
	}

	private bool PrepareOwlcatUnityModManagerDll()
	{
		if (File.Exists(OwlcatUnityModManagerDllPath) && File.Exists(Path.Combine(OwlcatUnityModManagerPersistantDirectory, ModMakerDllDependencies[0])))
		{
			if (UnityModManagerIntegrationIsUpToDate())
			{
				PFLog.UnityModManager.Log("Owlcat UnityModManager.dll is already set up.");
				return true;
			}
			try
			{
				File.Delete(OwlcatUnityModManagerDllPath);
				File.Delete(OwlcatUnityModManagerPdbPath);
				File.Delete(Path.Combine(OwlcatUnityModManagerPersistantDirectory, ModMakerDllDependencies[0]));
				File.Delete(Path.Combine(OwlcatUnityModManagerPersistantDirectory, ModMakerDllDependencies[1]));
				PFLog.UnityModManager.Log("Detected old UnityModManager.dll version, reinstalling.");
			}
			catch (Exception ex)
			{
				PFLog.UnityModManager.Error("Failed to remove old UMM version because of exception.");
				PFLog.UnityModManager.Exception(ex);
				return false;
			}
		}
		PFLog.UnityModManager.Log($"Going to update UMM to version {13}");
		string text = Path.Combine(Application.streamingAssetsPath, "OwlcatUnityModManager.zip");
		if (!File.Exists(text))
		{
			PFLog.UnityModManager.Exception(new FileNotFoundException("OwlcatUnityModManager.zip not found in StreamingAssets"));
			return false;
		}
		try
		{
			bool flag = false;
			string persistentDataPath = Application.persistentDataPath;
			string text2 = Path.Combine(Application.persistentDataPath, "UnityModManager");
			string text3;
			if (Directory.Exists(text2))
			{
				flag = true;
				text3 = Path.Combine(persistentDataPath, "UnityModManager");
			}
			else
			{
				text3 = persistentDataPath;
			}
			ZipArchive source = ZipFile.Open(text, ZipArchiveMode.Read);
			string text4 = Path.Combine(text3, "UnityModManager");
			if (Directory.Exists(text4))
			{
				Directory.Delete(text4, recursive: true);
			}
			source.ExtractToDirectory(text3);
			if (flag)
			{
				PFLog.UnityModManager.Log("Removing old UnityModManager libraries.");
				foreach (string item in Directory.EnumerateFiles(text4))
				{
					string text5 = item.Substring(text4.Length + 1);
					PFLog.Mods.Log("filename : " + text5);
					PFLog.Mods.Log("source : " + item);
					PFLog.Mods.Log("dest : " + Path.Combine(text2, text5));
					string path = Path.Combine(text2, text5);
					if (File.Exists(path))
					{
						File.Delete(path);
					}
					File.Move(item, Path.Combine(text2, text5));
				}
				Directory.Delete(Path.Combine(text3, "UnityModManager"));
			}
			SaveActualUmmVersion();
			PFLog.UnityModManager.Log("Updating UnityModManager finished.");
			return true;
		}
		catch (Exception ex2)
		{
			PFLog.UnityModManager.Error("UnityModManager update failed.");
			PFLog.UnityModManager.Exception(ex2, "Exception while trying to extract owlcat UnityModManager.dll");
			return false;
		}
	}

	static UnityModManagerBridge()
	{
		OwlcatUnityModManagerPersistantDirectory = Path.Combine(Application.persistentDataPath, "UnityModManager");
		OwlcatUnityModManagerDllPath = Path.Combine(OwlcatUnityModManagerPersistantDirectory, "UnityModManager.dll");
		OwlcatUnityModManagerPdbPath = Path.Combine(OwlcatUnityModManagerPersistantDirectory, "UnityModManager.pdb");
		ModMakerDllDependencies = new List<string> { "dnlib.dll", "Ionic.Zip.dll" };
	}
}
