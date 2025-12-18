using System.Collections;
using System.Threading.Tasks;
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
		if (!(viewModel is CommonVM viewModel2))
		{
			if (!(viewModel is RootVM viewModel3))
			{
				if (viewModel is LoadingScreenRootVM viewModel4)
				{
					return CreateAndBindView(viewModel4, Game.Instance.IsControllerMouse);
				}
				return null;
			}
			return CreateAndBindView(viewModel3, Game.Instance.IsControllerMouse);
		}
		return CreateAndBindView(viewModel2, isMouseControl: true);
	}

	private MonoBehaviour CreateAndBindView<TViewModel>(TViewModel viewModel, bool isMouseControl) where TViewModel : ViewModel
	{
		View<TViewModel> component = UIConfig.Instance.ViewConfigs.LoadPrefab(viewModel, isMouseControl).GetComponent<View<TViewModel>>();
		if (component == null)
		{
			return null;
		}
		View<TViewModel> view = Object.Instantiate(component, base.transform);
		view.transform.parent = null;
		if (view is IInitializable initializable)
		{
			initializable.Initialize();
		}
		view.Bind(viewModel);
		return view;
	}

	public IEnumerator TryCreateViewCoroutine<TViewModel>(TViewModel viewModel, MonoBehaviour view) where TViewModel : ViewModel
	{
		if (!(viewModel is CommonVM viewModel2))
		{
			if (!(viewModel is RootVM viewModel3))
			{
				if (viewModel is MainMenuVM viewModel4)
				{
					return CreateAndBindViewCoroutine(viewModel4, view);
				}
				return null;
			}
			return CreateAndBindViewCoroutine(viewModel3, view);
		}
		return CreateAndBindViewCoroutine(viewModel2, view);
	}

	private IEnumerator CreateAndBindViewCoroutine<TViewModel>(TViewModel viewModel, MonoBehaviour view) where TViewModel : ViewModel
	{
		Task<View<TViewModel>> loadPrefab = LoadPrefabAsync(viewModel);
		while (!loadPrefab.IsCompleted)
		{
			yield return null;
		}
		View<TViewModel> prefab = loadPrefab.Result;
		if (!(prefab == null))
		{
			yield return null;
			view = Object.Instantiate(prefab, base.transform);
			yield return null;
			view.transform.parent = null;
			if (view is IInitializable initializable)
			{
				initializable.Initialize();
			}
			yield return null;
			((View<TViewModel>)view).Bind(viewModel);
			View = view;
		}
	}

	private static async Task<View<TViewModel>> LoadPrefabAsync<TViewModel>(TViewModel viewModel) where TViewModel : ViewModel
	{
		return (await UIConfig.Instance.ViewConfigs.LoadPrefabAsync(viewModel, Game.Instance.IsControllerMouse)).GetComponent<View<TViewModel>>();
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
