using Code.View.UI.MVVM.Dialog;
using Code.View.UI.MVVM.Titles;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.DetectiveJournal;
using Kingmaker.Code.UI.MVVM.EntityInfo;
using Kingmaker.Code.UI.MVVM.SignalDevice;
using Kingmaker.Code.UI.MVVM.SkipCutscene;
using Kingmaker.Code.UI.MVVM.UnitInfo;
using Kingmaker.Code.View.UI.MVVM.HUD.PreciseAttack;
using Kingmaker.Code.View.UI.MVVM.HUDNotification.New;
using Kingmaker.Code.View.UI.MVVM.SaveLoad;
using Kingmaker.Code.View.UI.MVVM.ServiceWindows;
using Kingmaker.PubSubSystem.Core;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class RootVM : ViewModel
{
	[UsedImplicitly]
	private readonly SignalFxContext m_SignalFxContext;

	[UsedImplicitly]
	public readonly OvertipsContext OvertipsContext;

	[UsedImplicitly]
	public readonly MainMenuContext MainMenuContext;

	[UsedImplicitly]
	public readonly HUDContext HUDContext;

	[UsedImplicitly]
	public readonly DialogContext DialogContext;

	[UsedImplicitly]
	public readonly CommonContext CommonContext;

	[UsedImplicitly]
	public readonly TooltipContext TooltipContext;

	[UsedImplicitly]
	public readonly SettingsContext SettingsContext;

	[UsedImplicitly]
	public readonly SaveLoadContext SaveLoadContext;

	[UsedImplicitly]
	public readonly EscMenuContext EscMenuContext;

	[UsedImplicitly]
	public readonly FormationContext FormationContext;

	[UsedImplicitly]
	public readonly GameOverContext GameOverContext;

	[UsedImplicitly]
	public readonly InterchapterContext InterchapterContext;

	[UsedImplicitly]
	public readonly RewardContext RewardContext;

	[UsedImplicitly]
	public readonly TransitionContext TransitionContext;

	[UsedImplicitly]
	public readonly ServiceWindowsContext ServiceWindowsContext;

	public readonly ReactiveProperty<MainMenuVM> MainMenuVM = new ReactiveProperty<MainMenuVM>();

	public readonly ReactiveProperty<TermsOfUseVM> TermsOfUseVM = new ReactiveProperty<TermsOfUseVM>();

	public readonly ReactiveProperty<CombatStartWindowVM> CombatStartWindowVM = new ReactiveProperty<CombatStartWindowVM>();

	public readonly ReactiveProperty<InitiativeTrackerVM> InitiativeTrackerVM = new ReactiveProperty<InitiativeTrackerVM>();

	public readonly ReactiveProperty<ActionBarVM> ActionBarVM = new ReactiveProperty<ActionBarVM>();

	public readonly ReactiveProperty<InspectVM> InspectVM = new ReactiveProperty<InspectVM>();

	public readonly ReactiveProperty<CombatLogVM> CombatLogVM = new ReactiveProperty<CombatLogVM>();

	public readonly ReactiveProperty<IngameMenuVM> IngameMenuVM = new ReactiveProperty<IngameMenuVM>();

	public readonly ReactiveProperty<IngameMenuSettingsButtonVM> IngameMenuSettingsButtonVM = new ReactiveProperty<IngameMenuSettingsButtonVM>();

	public readonly ReactiveProperty<PartyVM> PartyVM = new ReactiveProperty<PartyVM>();

	public readonly ReactiveProperty<PreciseAttackVM> PreciseAttackVM = new ReactiveProperty<PreciseAttackVM>();

	public readonly ReactiveProperty<UnitInfoVM> UnitInfoVM = new ReactiveProperty<UnitInfoVM>();

	public readonly ReactiveProperty<EntityInfoVM> EntityInfoVM = new ReactiveProperty<EntityInfoVM>();

	public readonly ReactiveProperty<SignalsDeviceVM> RadarTempVM = new ReactiveProperty<SignalsDeviceVM>();

	public readonly ReactiveProperty<SkipCutsceneVM> SkipCutsceneVM = new ReactiveProperty<SkipCutsceneVM>();

	public readonly ReactiveProperty<CombatEndWindowVM> CombatEndWindowVM = new ReactiveProperty<CombatEndWindowVM>();

	public readonly ReactiveProperty<VariativeInteractionVM> VariativeInteractionVM = new ReactiveProperty<VariativeInteractionVM>();

	public readonly ReactiveProperty<SurfaceOvertipsVM> SurfaceOvertipsVM = new ReactiveProperty<SurfaceOvertipsVM>();

	public readonly ReactiveProperty<PointMarkersVM> PointMarkersVM = new ReactiveProperty<PointMarkersVM>();

	public readonly ReactiveProperty<EtudeCounterVM> EtudeCounterVM = new ReactiveProperty<EtudeCounterVM>();

	public readonly ReactiveProperty<LineOfSightControllerVM> LineOfSightControllerVM = new ReactiveProperty<LineOfSightControllerVM>();

	public readonly ReactiveProperty<ChannelingLinesControllerVM> ChannelingLinesControllerVM = new ReactiveProperty<ChannelingLinesControllerVM>();

	public readonly ReactiveProperty<BookEventVM> BookEventVM = new ReactiveProperty<BookEventVM>();

	public readonly ReactiveProperty<EpilogVM> EpilogVM = new ReactiveProperty<EpilogVM>();

	public readonly ReactiveProperty<DialogVM> DialogVM = new ReactiveProperty<DialogVM>();

	public readonly ReactiveProperty<DetectiveEpilogVM> DetectiveEpilogVM = new ReactiveProperty<DetectiveEpilogVM>();

	public readonly ReactiveProperty<MessageBoxVM> MessageBoxVM = new ReactiveProperty<MessageBoxVM>();

	public readonly ReactiveProperty<BugReportVM> BugReportVM = new ReactiveProperty<BugReportVM>();

	public readonly ReactiveProperty<SubtitleVM> SubtitleVM = new ReactiveProperty<SubtitleVM>();

	public readonly ReactiveProperty<TutorialVM> TutorialVM = new ReactiveProperty<TutorialVM>();

	public readonly ReactiveProperty<TooltipVM> TooltipVM = new ReactiveProperty<TooltipVM>();

	public readonly ReactiveProperty<HintVM> HintVM = new ReactiveProperty<HintVM>();

	public readonly ReactiveProperty<InfoWindowVM> InfoWindowVM = new ReactiveProperty<InfoWindowVM>();

	public readonly ReactiveProperty<InfoWindowVM> GlossaryInfoWindowVM = new ReactiveProperty<InfoWindowVM>();

	public readonly ReactiveProperty<ComparativeTooltipVM> ComparativeTooltipVM = new ReactiveProperty<ComparativeTooltipVM>();

	public readonly ReactiveProperty<SettingsVM> SettingsVM = new ReactiveProperty<SettingsVM>();

	public readonly ReactiveProperty<SaveLoadVM> SaveLoadVM = new ReactiveProperty<SaveLoadVM>();

	public readonly ReactiveProperty<EscMenuVM> EscMenuVM = new ReactiveProperty<EscMenuVM>();

	public readonly ReactiveProperty<FormationVM> FormationVM = new ReactiveProperty<FormationVM>();

	public readonly ReactiveProperty<GroupChangerVM> GroupChangerVM = new ReactiveProperty<GroupChangerVM>();

	public readonly ReactiveProperty<GameOverVM> GameOverVM = new ReactiveProperty<GameOverVM>(null);

	public readonly ReactiveProperty<LootVM> LootVM = new ReactiveProperty<LootVM>();

	public readonly LootContext LootContext;

	public readonly ReactiveProperty<InterchapterVM> InterchapterVM = new ReactiveProperty<InterchapterVM>();

	public readonly ReactiveProperty<AlignmentMarkRewardVM> SoulMarkRewardVM = new ReactiveProperty<AlignmentMarkRewardVM>();

	public readonly ReactiveProperty<TransitionVM> TransitionVM = new ReactiveProperty<TransitionVM>();

	public readonly ReactiveProperty<NewGameVM> NewGameVM = new ReactiveProperty<NewGameVM>();

	public readonly CharGenContextVM CharGenContextVM;

	public readonly ReactiveProperty<CharGenVM> CharGenVM = new ReactiveProperty<CharGenVM>();

	public readonly ReactiveProperty<RespecVM> RespecVM = new ReactiveProperty<RespecVM>();

	public readonly ReactiveProperty<NotificatorVM> NotificatorVM = new ReactiveProperty<NotificatorVM>();

	public readonly ReactiveProperty<PauseNotificationVM> PauseNotificationVM = new ReactiveProperty<PauseNotificationVM>();

	public readonly ReactiveProperty<SystemNotificatorVM> SystemNotificatorVM = new ReactiveProperty<SystemNotificatorVM>();

	public readonly ReactiveProperty<CursorNotificationVM> CursorNotificationVM = new ReactiveProperty<CursorNotificationVM>();

	public readonly ReactiveProperty<ServiceWindowsPanelVM> WindowsPanelVM = new ReactiveProperty<ServiceWindowsPanelVM>();

	public readonly ReactiveProperty<GamepadConnectDisconnectVM> GamepadConnectDisconnectVM = new ReactiveProperty<GamepadConnectDisconnectVM>();

	public readonly ReactiveProperty<TitlesVM> TitlesVM = new ReactiveProperty<TitlesVM>();

	private readonly ReactiveProperty<CombatMechanicEntityVM> m_CurrentUnit = new ReactiveProperty<CombatMechanicEntityVM>();

	public static RootVM Instance { get; private set; }

	public bool IsCursorActive => Game.Instance.CursorController?.IsCursorActive ?? false;

	public RootVM()
	{
		Instance = this;
		MainMenuContext = new MainMenuContext(MainMenuVM, NewGameVM, TermsOfUseVM).AddTo(this);
		HUDContext = new HUDContext(CombatStartWindowVM, InitiativeTrackerVM, m_CurrentUnit, ActionBarVM, InspectVM, CombatLogVM, IngameMenuVM, IngameMenuSettingsButtonVM, PartyVM, PreciseAttackVM, UnitInfoVM, RadarTempVM, SkipCutsceneVM, CombatEndWindowVM).AddTo(this);
		OvertipsContext = new OvertipsContext(VariativeInteractionVM, SurfaceOvertipsVM, PointMarkersVM).AddTo(this);
		CommonContext = new CommonContext(MessageBoxVM, BugReportVM, SubtitleVM, TutorialVM).AddTo(this);
		TooltipContext = new TooltipContext(TooltipVM, HintVM, InfoWindowVM, GlossaryInfoWindowVM, ComparativeTooltipVM).AddTo(this);
		LootContext = new LootContext(LootVM).AddTo(this);
		DialogContext = new DialogContext(BookEventVM, EpilogVM, DialogVM, DetectiveEpilogVM).AddTo(this);
		InterchapterContext = new InterchapterContext(InterchapterVM).AddTo(this);
		RewardContext = new RewardContext(SoulMarkRewardVM).AddTo(this);
		ServiceWindowsContext = new ServiceWindowsContext(WindowsPanelVM).AddTo(this);
		EscMenuContext = new EscMenuContext(EscMenuVM).AddTo(this);
		GameOverContext = new GameOverContext(GameOverVM).AddTo(this);
		FormationContext = new FormationContext(FormationVM).AddTo(this);
		TransitionContext = new TransitionContext(TransitionVM).AddTo(this);
		SettingsContext = new SettingsContext(SettingsVM).AddTo(this);
		SaveLoadContext = new SaveLoadContext(SaveLoadVM).AddTo(this);
		m_SignalFxContext = new SignalFxContext().AddTo(this);
		CharGenContextVM = new CharGenContextVM(CharGenVM, RespecVM).AddTo(this);
		new ChooseControllerModeContext(GamepadConnectDisconnectVM, RootUIContext.CanChangeInput).AddTo(this);
		new TitlesContext(TitlesVM, delegate
		{
			Game.Instance.ResetToMainMenu();
		}).AddTo(this);
		new GroupChangerContext(GroupChangerVM).AddTo(this);
		NotificatorVM.Value = new NotificatorVM().AddTo(this);
		PauseNotificationVM.Value = new PauseNotificationVM().AddTo(this);
		SystemNotificatorVM.Value = new SystemNotificatorVM().AddTo(this);
		CursorNotificationVM.Value = new CursorNotificationVM(() => Input.mousePosition, () => Game.Instance.CursorController.CursorHasText).AddTo(this);
		EtudeCounterVM.Value = new EtudeCounterVM().AddTo(this);
		LineOfSightControllerVM.Value = new LineOfSightControllerVM().AddTo(this);
		ChannelingLinesControllerVM.Value = new ChannelingLinesControllerVM().AddTo(this);
		EntityInfoVM.Value = new EntityInfoVM(IsTurnBasedMode, GetPointerPosition, UIStrings.Instance, UIConfig.Instance.UIIcons).AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
	}

	private bool IsTurnBasedMode()
	{
		return (Game.Instance?.Controllers?.TurnController?.TurnBasedModeActive).GetValueOrDefault();
	}

	private Vector3 GetPointerPosition()
	{
		return Game.Instance?.Controllers?.ClickEventsController?.WorldPosition ?? Vector3.zero;
	}
}
