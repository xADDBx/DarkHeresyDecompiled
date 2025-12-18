using Kingmaker.AreaLogic.Cutscenes.Commands;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Controllers.Dialog;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class GameUIState : BaseDisposable, IGameModeHandler, ISubscriber, IAreaHandler, IAdditiveAreaSwitchHandler, IPartyCombatHandler, IFullScreenUIHandler, IDialogInteractionHandler, IInterchapterHandler
{
	public readonly ReactiveProperty<GameModeType> GameMode = new ReactiveProperty<GameModeType>();

	public readonly ReactiveProperty<FullScreenUIType> CurrentFullScreenUIType = new ReactiveProperty<FullScreenUIType>(FullScreenUIType.Unknown);

	public readonly ReactiveProperty<bool> IsLoadingProcess = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<DialogController> ActiveDialogController = new ReactiveProperty<DialogController>(null);

	public readonly ReactiveProperty<InterchapterData> ActiveInterchapter = new ReactiveProperty<InterchapterData>(null);

	public readonly ReactiveProperty<bool> IsInCombat = new ReactiveProperty<bool>(value: false);

	public readonly ReactiveProperty<bool> IsInMainMenuObservable = new ReactiveProperty<bool>(value: false);

	public bool IsInventoryDollRotating;

	public static GameUIState Instance { get; private set; }

	public bool IsInMainMenu => IsInMainMenuObservable.Value;

	public GameUIState()
	{
		Instance = this;
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
		Instance = null;
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		GameMode.Value = gameMode;
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
	}

	public void HandlePartyCombatStateChanged(bool inCombat)
	{
		IsInCombat.Value = inCombat;
	}

	public void HandleFullScreenUiChanged(bool state, FullScreenUIType type)
	{
		CurrentFullScreenUIType.Value = (state ? type : FullScreenUIType.Unknown);
	}

	public void OnAreaBeginUnloading()
	{
		IsLoadingProcess.Value = true;
	}

	public void OnAreaDidLoad()
	{
		IsLoadingProcess.Value = false;
	}

	public void OnAdditiveAreaBeginDeactivated()
	{
		IsLoadingProcess.Value = false;
	}

	public void OnAdditiveAreaDidActivated()
	{
		IsLoadingProcess.Value = true;
	}

	public void StartDialogInteraction(BlueprintDialog dialog)
	{
		ActiveDialogController.Value = Game.Instance.Controllers.DialogController;
	}

	public void StopDialogInteraction(BlueprintDialog dialog)
	{
		ActiveDialogController.Value = null;
	}

	public void StartInterchapter(InterchapterData data)
	{
		ActiveInterchapter.Value = data;
	}

	public void StopInterchapter(InterchapterData data)
	{
		ActiveInterchapter.Value = null;
	}

	public void SetMainMenuActive(bool state)
	{
		IsInMainMenuObservable.Value = state;
	}
}
