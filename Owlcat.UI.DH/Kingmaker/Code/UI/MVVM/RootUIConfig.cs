using System;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.View.Bridge.Root;
using Kingmaker.Utility.BuildModeUtils;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class RootUIConfig : MonoBehaviour
{
	[HideInInspector]
	public MonoBehaviour View;

	public MonoBehaviour TryCreateView<TViewModel>(TViewModel viewModel) where TViewModel : ViewModel
	{
		try
		{
			return CreateAndBindView(viewModel, Game.Instance.IsControllerMouse);
		}
		catch (Exception ex)
		{
			PFLog.UI.Exception(ex);
		}
		return null;
	}

	private MonoBehaviour CreateAndBindView<TViewModel>(TViewModel viewModel, bool isMouseControl) where TViewModel : ViewModel
	{
		View<TViewModel> component = UIConfig.Instance.ViewConfigs.LoadPrefab(viewModel, isMouseControl).GetComponent<View<TViewModel>>();
		if (component == null)
		{
			PFLog.UI.Error("Failed to load prefab for view model of type " + viewModel.GetType().Name);
			return null;
		}
		View<TViewModel> view = UnityEngine.Object.Instantiate(component, base.transform);
		view.transform.parent = null;
		if (view is IInitializable initializable)
		{
			initializable.Initialize();
		}
		view.Bind(viewModel);
		return view;
	}

	public void Unload()
	{
		if ((BuildModeUtility.Data?.Loading?.DestroyUIPrefabs).GetValueOrDefault())
		{
			UIConfig.Instance.ViewConfigs.UnloadAll();
		}
	}

	public void Destroy()
	{
		if ((BuildModeUtility.Data?.Loading?.DestroyUIPrefabs).GetValueOrDefault())
		{
			UIConfig.Instance.ViewConfigs.DestroyAll();
		}
	}
}
