using System;
using System.Collections.Generic;
using Code.View.UI.MVVM.Dialog;
using Code.View.UI.MVVM.Titles;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Framework.Settings.UISettings;
using Kingmaker.Code.UI.MVVM.CombatNotifications;
using Kingmaker.Code.UI.MVVM.DetectiveJournal;
using Kingmaker.Code.UI.MVVM.EntityInfo;
using Kingmaker.Code.UI.MVVM.SignalDevice;
using Kingmaker.Code.UI.MVVM.SkipCutscene;
using Kingmaker.Code.UI.MVVM.UnitInfo;
using Kingmaker.Code.UI.MVVM.Vendor;
using Kingmaker.Code.View.UI.MVVM.HUD.PreciseAttack;
using Kingmaker.Code.View.UI.MVVM.HUDNotification.New;
using Kingmaker.Code.View.UI.MVVM.SaveLoad;
using Kingmaker.Code.View.UI.MVVM.ServiceWindows;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Pointer;
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

	[UsedImplicitly]
	public readonly LootContext LootContext;

	[UsedImplicitly]
	public readonly CharGenContextVM CharGenContext;

	[UsedImplicitly]
	private readonly BarksContext m_BarksContext;

	public readonly ReactiveProperty<CursorVM> CursorVM = new ReactiveProperty<CursorVM>();

	public readonly ReactiveProperty<MainMenuVM> MainMenuVM = new ReactiveProperty<MainMenuVM>();

	public readonly ReactiveProperty<TermsOfUseVM> TermsOfUseVM = new ReactiveProperty<TermsOfUseVM>();

	public readonly ReactiveProperty<CombatStartWindowVM> PreparationTurnWindowVM = new ReactiveProperty<CombatStartWindowVM>();

	public readonly ReactiveProperty<CombatStartNotificationVM> CombatStartNotificationVM = new ReactiveProperty<CombatStartNotificationVM>();

	public readonly ReactiveProperty<CombatHUDNotificationsVM> CombatHUDNotificationsVM = new ReactiveProperty<CombatHUDNotificationsVM>();

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

	public readonly ReactiveProperty<VendorBaseScreenVM> VendorVM = new ReactiveProperty<VendorBaseScreenVM>();

	public readonly ReactiveProperty<ContextMenuVM> ContextMenuVM = new ReactiveProperty<ContextMenuVM>();

	public readonly ReactiveProperty<CounterWindowVM> CounterVM = new ReactiveProperty<CounterWindowVM>();

	public readonly ReactiveProperty<InterchapterVM> InterchapterVM = new ReactiveProperty<InterchapterVM>();

	public readonly ReactiveProperty<AlignmentMarkRewardVM> SoulMarkRewardVM = new ReactiveProperty<AlignmentMarkRewardVM>();

	public readonly ReactiveProperty<TransitionMapVM> TransitionMapVM = new ReactiveProperty<TransitionMapVM>();

	public readonly ReactiveProperty<NewGameVM> NewGameVM = new ReactiveProperty<NewGameVM>();

	public readonly ReactiveProperty<CharGenVM> CharGenVM = new ReactiveProperty<CharGenVM>();

	public readonly ReactiveProperty<RespecVM> RespecVM = new ReactiveProperty<RespecVM>();

	public readonly ReactiveProperty<NotificatorVM> NotificatorVM = new ReactiveProperty<NotificatorVM>();

	public readonly ReactiveProperty<PauseNotificationVM> PauseNotificationVM = new ReactiveProperty<PauseNotificationVM>();

	public readonly ReactiveProperty<SystemNotificatorVM> SystemNotificatorVM = new ReactiveProperty<SystemNotificatorVM>();

	public readonly ReactiveProperty<CursorNotificationVM> CursorNotificationVM = new ReactiveProperty<CursorNotificationVM>();

	public readonly ReactiveProperty<ServiceWindowsPanelVM> WindowsPanelVM = new ReactiveProperty<ServiceWindowsPanelVM>();

	public readonly ReactiveProperty<GamepadConnectDisconnectVM> GamepadConnectDisconnectVM = new ReactiveProperty<GamepadConnectDisconnectVM>();

	public readonly ReactiveProperty<TitlesVM> TitlesVM = new ReactiveProperty<TitlesVM>();

	public readonly ReactiveProperty<CreditsVM> CreditsVM = new ReactiveProperty<CreditsVM>();

	public readonly ReactiveProperty<FadeVM> FadeVM = new ReactiveProperty<FadeVM>();

	public readonly UIVisibilityVM VisibilityVM;

	public static RootVM Instance { get; private set; }

	public RootVM(Action loadingHandler)
	{
		Instance = this;
		MainMenuContext = new MainMenuContext(MainMenuVM, NewGameVM, TermsOfUseVM, loadingHandler).AddTo(this);
		HUDContext = new HUDContext(PreparationTurnWindowVM, CombatStartNotificationVM, CombatHUDNotificationsVM, InitiativeTrackerVM, ActionBarVM, InspectVM, CombatLogVM, IngameMenuVM, IngameMenuSettingsButtonVM, PartyVM, PreciseAttackVM, UnitInfoVM, RadarTempVM, SkipCutsceneVM, CombatEndWindowVM).AddTo(this);
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
		TransitionContext = new TransitionContext(TransitionMapVM).AddTo(this);
		SettingsContext = new SettingsContext(SettingsVM).AddTo(this);
		SaveLoadContext = new SaveLoadContext(SaveLoadVM).AddTo(this);
		m_SignalFxContext = new SignalFxContext().AddTo(this);
		CharGenContext = new CharGenContextVM(CharGenVM, RespecVM).AddTo(this);
		VisibilityVM = new UIVisibilityVM().AddTo(this);
		UISettingsEntityKeyBinding switchUIVisibility = UISettingsRoot.Instance.UIKeybindGeneralSettings.SwitchUIVisibility;
		Game.Instance.Keyboard.Bind(switchUIVisibility.name, UIVisibilityState.SwitchVisibility).AddTo(this);
		new VendorContext(VendorVM).AddTo(this);
		new InventoryContext(CounterVM, ContextMenuVM).AddTo(this);
		new ChooseControllerModeContext(GamepadConnectDisconnectVM, RootUIContext.CanChangeInput).AddTo(this);
		new TitlesContext(TitlesVM, CreditsVM, delegate
		{
			Game.Instance.ResetToMainMenu();
		}).AddTo(this);
		new GroupChangerContext(GroupChangerVM).AddTo(this);
		CursorVM.Value = new CursorVM(Game.Instance.CursorController);
		NotificatorVM.Value = new NotificatorVM().AddTo(this);
		PauseNotificationVM.Value = new PauseNotificationVM().AddTo(this);
		SystemNotificatorVM.Value = new SystemNotificatorVM().AddTo(this);
		CursorNotificationVM.Value = new CursorNotificationVM(() => Game.Instance.CursorController.Position, () => Game.Instance.CursorController.CursorHasText).AddTo(this);
		EtudeCounterVM.Value = new EtudeCounterVM().AddTo(this);
		LineOfSightControllerVM.Value = new LineOfSightControllerVM().AddTo(this);
		ChannelingLinesControllerVM.Value = new ChannelingLinesControllerVM().AddTo(this);
		FadeVM.Value = new FadeVM().AddTo(this);
		EntityInfoVM.Value = new EntityInfoVM(() => (Game.Instance?.Controllers?.TurnController?.TurnBasedModeActive).GetValueOrDefault(), () => Game.Instance?.Controllers?.ClickEventsController?.WorldPosition ?? Vector3.zero, UIStrings.Instance).AddTo(this);
		m_BarksContext = new BarksContext(new Dictionary<BarkType, List<IBarkHandler>>
		{
			{
				BarkType.Default,
				new List<IBarkHandler>
				{
					BarksContext.WrapReactive(SurfaceOvertipsVM),
					BarksContext.WrapReactive(PartyVM)
				}
			},
			{
				BarkType.ServoSkull,
				new List<IBarkHandler> { BarksContext.WrapReactive(CombatHUDNotificationsVM) }
			}
		}).AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
	}

	public void Test()
	{
	}
}
