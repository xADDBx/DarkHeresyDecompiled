using System;
using System.Threading.Tasks;
using Kingmaker.ResourceLinks;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using UnityEngine;

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

	public ViewPrefabPair Common;

	public ViewPrefabPair Root;

	public ViewPrefabPair LoadingScreen;

	public ScriptableObject RootPCLookupTable;

	public ScriptableObject RootConsoleLookupTable;

	public GameObject LoadPrefab(ViewModel vm, bool isPCInterface)
	{
		return vm.GetType().Name switch
		{
			"CommonVM" => Common.Load(isPCInterface, hold: true), 
			"RootVM" => Root.Load(isPCInterface, hold: true), 
			"LoadingScreenRootVM" => LoadingScreen.Load(isPCInterface, hold: true), 
			_ => null, 
		};
	}

	public async Task<GameObject> LoadPrefabAsync(ViewModel vm, bool isPCInterface)
	{
		return vm.GetType().Name switch
		{
			"CommonVM" => await Common.LoadAsync(isPCInterface), 
			"RootVM" => await Root.LoadAsync(isPCInterface), 
			"LoadingScreenRootVM" => await LoadingScreen.LoadAsync(isPCInterface), 
			_ => null, 
		};
	}

	public void UnloadAll()
	{
		Common.Unload();
		Root.Unload();
		LoadingScreen.Unload();
	}

	public void DestroyAll()
	{
		Common.Destroy();
		Root.Destroy();
		LoadingScreen.Destroy();
	}
}
