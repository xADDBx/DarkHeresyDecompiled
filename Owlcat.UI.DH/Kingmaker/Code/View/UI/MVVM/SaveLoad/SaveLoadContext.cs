using System;
using System.Collections;
using JetBrains.Annotations;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.EntitySystem.Persistence.Scenes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Logging;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.View.UI.MVVM.SaveLoad;

public class SaveLoadContext : ViewModel, ISaveLoadUIHandler, ISubscriber, IUILoadService
{
	private readonly ReactiveProperty<SaveLoadVM> m_SaveLoadVM;

	private bool m_EnterGameStarted;

	public ReadOnlyReactiveProperty<SaveLoadVM> SaveLoadVM => m_SaveLoadVM;

	public SaveLoadContext(ReactiveProperty<SaveLoadVM> saveLoadVM)
	{
		m_SaveLoadVM = saveLoadVM;
		EventBus.Subscribe(this).AddTo(this);
	}

	public void HandleOpenSaveLoad(SaveLoadMode mode, bool singleMode)
	{
		IUILoadService iUILoadService2;
		if (!GameUIState.Instance.IsInMainMenu)
		{
			IUILoadService iUILoadService = new CommonUILoadService(DisposeSaveLoad);
			iUILoadService2 = iUILoadService;
		}
		else
		{
			IUILoadService iUILoadService = this;
			iUILoadService2 = iUILoadService;
		}
		IUILoadService loadService = iUILoadService2;
		m_SaveLoadVM.Value = new SaveLoadVM(mode, singleMode, DisposeSaveLoad, loadService).AddTo(this);
	}

	public void Load(SaveInfo saveInfo)
	{
		Load(saveInfo, null);
	}

	public void Load(SaveInfo saveInfo, [CanBeNull] Action callback)
	{
		DisposeSaveLoad();
		EnterGame(delegate
		{
			Game.Instance.LoadGameFromMainMenu(saveInfo, callback);
		}, saveInfo);
	}

	public void DisposeSaveLoad()
	{
		SaveLoadVM.CurrentValue?.Dispose();
		m_SaveLoadVM.Value = null;
	}

	private void EnterGame(Action action, SaveInfo saveToLoad)
	{
		if (m_EnterGameStarted)
		{
			UberDebug.LogError("Double game start detected!");
		}
		else
		{
			LoadingProcess.Instance.StartLoadingProcess("EnterGameCoroutine", EnterGameCoroutine(action));
		}
	}

	private IEnumerator EnterGameCoroutine(Action action)
	{
		m_EnterGameStarted = true;
		yield return null;
		SceneLoader.LoadObligatoryScenes();
		yield return null;
		action?.Invoke();
	}
}
