using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Kingmaker.ResourceLinks;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using UnityEngine;
using UnityEngine.Scripting;

namespace Kingmaker.Blueprints.Root;

[TypeId("065ee7d03fdf6924f802585067b91373")]
public class UIViewConfigs : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<UIViewConfigs>
	{
	}

	[Serializable]
	public class ViewPrefabPair
	{
		[RequireSeparateBundle]
		public PrefabLink PCView;

		[RequireSeparateBundle]
		public PrefabLink ConsoleView;

		private GameObject m_LastObject;

		public GameObject Load(bool isPCInterface, bool hold = false)
		{
			PrefabLink prefabLink = (isPCInterface ? PCView : ConsoleView);
			prefabLink.ForceUnload();
			m_LastObject = prefabLink.Load(ignorePreloadWarning: false, hold);
			return m_LastObject;
		}

		public async Task<GameObject> LoadAsync(bool isPCInterface)
		{
			PrefabLink obj = (isPCInterface ? PCView : ConsoleView);
			obj.ForceUnload();
			m_LastObject = await obj.LoadAsync(ignorePreloadWarning: false, hold: true);
			return m_LastObject;
		}

		public void Unload()
		{
			PCView.ForceUnload();
			ConsoleView.ForceUnload();
		}

		public void Destroy()
		{
			PCView.DestroyButDontUnload();
			ConsoleView.DestroyButDontUnload();
		}
	}

	[SerializeField]
	[UsedImplicitly]
	[Preserve]
	private ScriptableObject[] m_LookupTables;

	public ViewPrefabPair Root;

	public ViewPrefabPair LoadingScreen;

	public GameObject LoadPrefab(ViewModel vm, bool isPCInterface)
	{
		string text = vm.GetType().Name;
		if (!(text == "RootVM"))
		{
			if (text == "LoadingScreenRootVM")
			{
				return LoadingScreen.Load(isPCInterface, hold: true);
			}
			return null;
		}
		return Root.Load(isPCInterface, hold: true);
	}

	public async Task<GameObject> LoadPrefabAsync(ViewModel vm, bool isPCInterface)
	{
		string text = vm.GetType().Name;
		if (!(text == "RootVM"))
		{
			if (text == "LoadingScreenRootVM")
			{
				return await LoadingScreen.LoadAsync(isPCInterface);
			}
			return null;
		}
		return await Root.LoadAsync(isPCInterface);
	}

	public void UnloadAll()
	{
		Root.Unload();
		LoadingScreen.Unload();
	}

	public void DestroyAll()
	{
		Root.Destroy();
		LoadingScreen.Destroy();
	}
}
