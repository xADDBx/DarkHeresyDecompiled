using Kingmaker.Blueprints.Area;
using Kingmaker.Code.Middleware.Metrics;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Networking;
using Kingmaker.Networking.NetGameFsm;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Settings;
using Kingmaker.Utility.Random;
using Kingmaker.Utility.StatefulRandom;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class LoadingScreenVM : ViewModel, IStartAwaitingUserInput, ISubscriber, IContinueLoadingHandler, INetEvents
{
	private readonly ReactiveProperty<BlueprintArea> m_AreaProperty = new ReactiveProperty<BlueprintArea>();

	public LoadingScreenState State;

	private int m_RandomScreenPercent;

	private readonly ReactiveProperty<bool> m_NeedUserInput = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<int> m_UserInputProgress = new ReactiveProperty<int>(0);

	private readonly ReactiveProperty<int> m_UserInputTarget = new ReactiveProperty<int>(1);

	private readonly ReactiveProperty<bool> m_UserInputMeIsPressed = new ReactiveProperty<bool>(value: false);

	private bool m_UpdateProgressInput;

	private readonly ReactiveProperty<bool> m_IsSaveTransfer = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<int> m_SaveTransferProgress = new ReactiveProperty<int>(0);

	private readonly ReactiveProperty<int> m_SaveTransferTarget = new ReactiveProperty<int>(1);

	private BlueprintArea m_LastArea;

	public ReadOnlyReactiveProperty<BlueprintArea> AreaProperty => m_AreaProperty;

	public ReadOnlyReactiveProperty<bool> NeedUserInput => m_NeedUserInput;

	public ReadOnlyReactiveProperty<int> UserInputProgress => m_UserInputProgress;

	public ReadOnlyReactiveProperty<int> UserInputTarget => m_UserInputTarget;

	public ReadOnlyReactiveProperty<bool> UserInputMeIsPressed => m_UserInputMeIsPressed;

	public ReadOnlyReactiveProperty<bool> IsSaveTransfer => m_IsSaveTransfer;

	public ReadOnlyReactiveProperty<int> SaveTransferProgress => m_SaveTransferProgress;

	public ReadOnlyReactiveProperty<int> SaveTransferTarget => m_SaveTransferTarget;

	public StatefulRandom Random
	{
		get
		{
			if (LoadingProcess.Instance.CurrentProcessTag == LoadingProcessTag.ExceptionReporter || LoadingProcess.Instance.CurrentProcessTag == LoadingProcessTag.ResetUI)
			{
				return PFStatefulRandom.NonDeterministic;
			}
			return PFStatefulRandom.LoadingScreen;
		}
	}

	public float FontMultiplier => SettingsRoot.Accessiability.FontSizeMultiplier;

	public LoadingScreenVM(BlueprintArea area)
	{
		EventBus.Subscribe(this).AddTo(this);
		SetLoadingArea(area);
		m_IsSaveTransfer.Value = PhotonManager.Save.InProcess;
		ObservableSubscribeExtensions.Subscribe(Observable.EveryUpdate(), delegate
		{
			SaveTransferUpdate();
			UpdateLockProgress();
			m_NeedUserInput.Value = LoadingProcess.Instance.IsAwaitingUserInput;
		}).AddTo(this);
		Metrics.Interface.Type(InterfaceMetricsEvent.InterfaceTypes.LoadingScreen).State(InterfaceMetricsEvent.InterfaceStates.Open).Send();
	}

	protected override void OnDispose()
	{
		base.OnDispose();
		Metrics.Interface.Type(InterfaceMetricsEvent.InterfaceTypes.LoadingScreen).State(InterfaceMetricsEvent.InterfaceStates.Close).Send();
	}

	public void SetLoadingArea(BlueprintArea area)
	{
		m_LastArea = Game.Instance.CurrentlyLoadedArea;
		m_AreaProperty.Value = area;
	}

	public int RandomLoadingScreen(int items, int[] itemsPercent)
	{
		if (items != itemsPercent.Length - 1)
		{
			return 0;
		}
		m_RandomScreenPercent = Random.Range(1, 100);
		int num = itemsPercent[0];
		for (int i = 0; i <= items; i++)
		{
			bool num2 = m_RandomScreenPercent >= num + 1 && m_RandomScreenPercent <= num + itemsPercent[i + 1];
			num += itemsPercent[i + 1];
			if (num2)
			{
				return i;
			}
		}
		return 0;
	}

	public void OnStartAwaitingUserInput()
	{
		LoadingProcess.Instance.IsAwaitingUserInput.Retain();
		if (AreaProperty.CurrentValue == null || AreaProperty.CurrentValue.NotPause || AreaProperty.CurrentValue == m_LastArea || !SettingsRoot.Game.Autopause.PauseOnLoadingScreen)
		{
			LoadingProcess.Instance.IsAwaitingUserInput.Release();
		}
	}

	void IContinueLoadingHandler.HandleContinueLoading()
	{
		LoadingProcess.Instance.IsAwaitingUserInput.Release();
		m_UpdateProgressInput = true;
	}

	public void HandleTransferProgressChanged(bool value)
	{
		m_IsSaveTransfer.Value = value;
		SaveTransferUpdate();
	}

	public void HandleNetGameStateChanged(NetGame.State state)
	{
	}

	public void HandleNLoadingScreenClosed()
	{
	}

	private void SaveTransferUpdate()
	{
		if (IsSaveTransfer.CurrentValue && PhotonManager.Save.GetSentProgress(out var progress, out var target))
		{
			m_SaveTransferProgress.Value = progress;
			m_SaveTransferTarget.Value = target;
		}
	}

	private void UpdateLockProgress()
	{
		if (m_UpdateProgressInput && PhotonManager.Lock.GetProgress(NetLockPointId.LoadingProcess, out var current, out var target, out var me))
		{
			m_UserInputProgress.Value = current;
			m_UserInputTarget.Value = target;
			m_UserInputMeIsPressed.Value = me;
		}
	}
}
