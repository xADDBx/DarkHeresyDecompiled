using System;
using JetBrains.Annotations;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Tutorial;
using Owlcat.Runtime.Core.Logging;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class TutorialVM : ViewModel, IGameModeHandler, ISubscriber, INewTutorialUIHandler
{
	private readonly ReactiveProperty<TutorialModalWindowVM> m_BigWindowVM = new ReactiveProperty<TutorialModalWindowVM>();

	private readonly ReactiveProperty<TutorialHintWindowVM> m_SmallWindowVM = new ReactiveProperty<TutorialHintWindowVM>();

	private TutorialData m_DataToRestore;

	public ReadOnlyReactiveProperty<TutorialModalWindowVM> BigWindowVM => m_BigWindowVM;

	public ReadOnlyReactiveProperty<TutorialHintWindowVM> SmallWindowVM => m_SmallWindowVM;

	private bool IsShowingBigWindow => BigWindowVM.CurrentValue?.Data != null;

	private bool IsShowingSmallWindow
	{
		get
		{
			if (!IsShowingBigWindow)
			{
				return SmallWindowVM.CurrentValue?.Data != null;
			}
			return false;
		}
	}

	public TutorialVM()
	{
		EventBus.Subscribe(this).AddTo(this);
	}

	protected override void OnDispose()
	{
		base.OnDispose();
		DisposeReactiveVM(m_BigWindowVM);
		DisposeReactiveVM(m_SmallWindowVM);
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		if ((IsShowingBigWindow && !CanShowBigWindow(gameMode)) || (IsShowingSmallWindow && !CanShowSmallWindow(gameMode)))
		{
			m_DataToRestore = BigWindowVM.CurrentValue?.Data ?? SmallWindowVM.CurrentValue?.Data;
			BigWindowVM.CurrentValue?.Hide();
			SmallWindowVM.CurrentValue?.Hide();
		}
		else if (m_DataToRestore != null && Game.Instance.TutorialSystem.ShowingData == null && (!IsBigWindowTutorial(m_DataToRestore) || CanShowBigWindow(gameMode)) && (IsBigWindowTutorial(m_DataToRestore) || CanShowSmallWindow(gameMode)))
		{
			ShowTutorialInternal(m_DataToRestore);
			m_DataToRestore = null;
		}
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
	}

	private static bool CanShowBigWindow(GameModeType gameMode)
	{
		if (!Game.Instance.IsModeActive(GameModeType.Cutscene) && !Game.Instance.IsModeActive(GameModeType.CutsceneGlobalMap))
		{
			return !Game.Instance.IsModeActive(GameModeType.Dialog);
		}
		return false;
	}

	private static bool CanShowSmallWindow(GameModeType gameMode)
	{
		if (!Game.Instance.IsModeActive(GameModeType.Cutscene))
		{
			return !Game.Instance.IsModeActive(GameModeType.CutsceneGlobalMap);
		}
		return false;
	}

	private static bool IsBigWindowTutorial([NotNull] TutorialData data)
	{
		return data.Blueprint.Windowed;
	}

	private bool IsBan(TutorialData data)
	{
		if (data.Trigger != null && Game.Instance.TutorialSystem.Ensure(data.Blueprint).Banned)
		{
			return true;
		}
		if (Game.Instance.TutorialSystem.IsTagBanned(data.Blueprint.Tag))
		{
			return true;
		}
		return false;
	}

	public void ShowTutorial(TutorialData data)
	{
		GameModeType currentModeType = Game.Instance.CurrentModeType;
		if ((IsBigWindowTutorial(data) && !CanShowBigWindow(currentModeType)) || !CanShowSmallWindow(currentModeType))
		{
			m_DataToRestore = data;
		}
		else
		{
			ShowTutorialInternal(data);
		}
	}

	private void ShowTutorialInternal(TutorialData data)
	{
		if (IsBan(data))
		{
			UberDebug.LogError("Tutorial is Banned");
			HideTutorial(data);
			return;
		}
		Game.Instance.TutorialSystem.ShowingData = data;
		if (IsBigWindowTutorial(data))
		{
			m_BigWindowVM.Value = new TutorialModalWindowVM(data, delegate
			{
				DisposeReactiveVM(m_BigWindowVM);
			}).AddTo(this);
		}
		else
		{
			m_SmallWindowVM.Value = new TutorialHintWindowVM(data, delegate
			{
				DisposeReactiveVM(m_SmallWindowVM);
			}).AddTo(this);
		}
	}

	public void HideTutorial(TutorialData data)
	{
		if (IsBigWindowTutorial(data))
		{
			DisposeReactiveVM(m_BigWindowVM);
		}
		else
		{
			DisposeReactiveVM(m_SmallWindowVM);
		}
		Game.Instance.TutorialSystem.ShowingData = null;
	}

	private void DisposeReactiveVM<T>(ReactiveProperty<T> reactiveVm) where T : class, IDisposable
	{
		reactiveVm.Value?.Dispose();
		reactiveVm.Value = null;
	}
}
