using Kingmaker.Code.Gameplay.Controllers.DetectiveRadar;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM.SignalDevice;

public class SignalsDeviceVM : ViewModel, IDetectiveRadarHandler, ISubscriber
{
	private readonly ReactiveProperty<DetectiveRadarState> m_State = new ReactiveProperty<DetectiveRadarState>();

	private readonly ReactiveProperty<float> m_SignalPowerClamped = new ReactiveProperty<float>();

	private readonly ReactiveProperty<string> m_SignalSourceName = new ReactiveProperty<string>();

	private readonly ReactiveProperty<bool> m_ForceHideDevice = new ReactiveProperty<bool>();

	public ReadOnlyReactiveProperty<DetectiveRadarState> State => m_State;

	public ReadOnlyReactiveProperty<float> SignalPowerClamped => m_SignalPowerClamped;

	public ReadOnlyReactiveProperty<string> SignalSourceName => m_SignalSourceName;

	public ReadOnlyReactiveProperty<bool> ForceHideDevice => m_ForceHideDevice;

	public ReadOnlyReactiveProperty<bool> ActionBarIsActive => RootVM.Instance.HUDContext.ActionBarVM.CurrentValue?.IsVisible;

	public SignalsDeviceVM()
	{
		DetectiveRadarController controller = Game.Instance.Controllers.DetectiveRadarController;
		if (controller != null)
		{
			m_State.Value = controller.SignalState;
			ObservableSubscribeExtensions.Subscribe(Observable.EveryUpdate(), delegate
			{
				m_SignalPowerClamped.Value = controller.SignalPowerClamped01;
				m_SignalSourceName.Value = controller.CurrentTrackedSignal?.Settings.SourceName?.String.Text;
			}).AddTo(this);
			GameUIState.Instance.ActiveDialogController.Subscribe(delegate
			{
				UpdateForceHideDevice();
			}).AddTo(this);
			Game.Instance.Controllers.SelectionCharacter.IsMainCharacterSelected.Subscribe(delegate
			{
				UpdateForceHideDevice();
			}).AddTo(this);
			GameUIState.Instance.GameMode.Subscribe(delegate(GameModeType value)
			{
				UpdateForceHideDevice(value == GameModeType.Cutscene);
			}).AddTo(this);
			GameUIState.Instance.IsInCombat.Subscribe(delegate
			{
				UpdateForceHideDevice();
			}).AddTo(this);
			EventBus.Subscribe(this).AddTo(this);
		}
	}

	private void UpdateForceHideDevice(bool forceHide = false)
	{
		bool flag = GameUIState.Instance.ActiveDialogController.Value != null;
		bool value = GameUIState.Instance.IsInCombat.Value;
		bool value2 = Game.Instance.Controllers.SelectionCharacter.IsMainCharacterSelected.Value;
		m_ForceHideDevice.Value = forceHide || flag || !value2 || value;
	}

	public void HandleRadarModeChange(DetectiveRadarState state)
	{
		m_State.Value = state;
	}

	public void HandleNearestSignalTurnedOn()
	{
	}
}
