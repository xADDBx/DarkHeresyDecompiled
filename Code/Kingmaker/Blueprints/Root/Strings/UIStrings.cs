using System;
using Code.Enums;
using Kingmaker.Gameplay.Features.AreaEffects;
using Kingmaker.Localization;

namespace Kingmaker.Blueprints.Root.Strings;

public class UIStrings : StringsContainer
{
	[Serializable]
	public struct UIAreaEffectInfoTexts
	{
		public LocalizedString InfoTitle;

		public LocalizedString BleedingDOT;

		public LocalizedString BurningDOT;

		public LocalizedString ToxicDOT;

		public LocalizedString EnterAreaEvent;

		public LocalizedString ExitAreaEvent;

		public LocalizedString MoveEvent;

		public LocalizedString EndRoundEvent;

		public LocalizedString StartTurnEvent;

		public LocalizedString EndTurnEvent;

		public LocalizedString DamageDescription;

		public LocalizedString DOTDescription;

		public LocalizedString BuffDescription;

		public LocalizedString GetEventLocalizedString(AreaEffectEventType eventType)
		{
			return eventType switch
			{
				AreaEffectEventType.Enter => EnterAreaEvent, 
				AreaEffectEventType.Exit => ExitAreaEvent, 
				AreaEffectEventType.Move => MoveEvent, 
				AreaEffectEventType.EndRound => EndRoundEvent, 
				AreaEffectEventType.StartTurn => StartTurnEvent, 
				AreaEffectEventType.EndTurn => EndTurnEvent, 
				_ => null, 
			};
		}

		public LocalizedString GetDOTLocalizedString(DOT dotType)
		{
			return dotType switch
			{
				DOT.Bleeding => BleedingDOT, 
				DOT.Burning => BurningDOT, 
				DOT.Toxic => ToxicDOT, 
				_ => null, 
			};
		}
	}

	[Serializable]
	public struct UIDestructibleCoverInfoTexts
	{
		public LocalizedString InfoTitle;

		public LocalizedString Durability;

		public LocalizedString Description;
	}

	[Serializable]
	public struct UICoverInfoTexts
	{
		public LocalizedString CoverInfoTitle;

		public LocalizedString CoverDescription;

		public LocalizedString LosBlockerInfoTitle;

		public LocalizedString LosBlockerDescription;
	}

	public UIAreaEffectInfoTexts AreaEffectInfoTexts;

	public UIDestructibleCoverInfoTexts DestructibleCoverInfoTexts;

	public UICoverInfoTexts CoverInfoTexts;

	public UIMeinMenuTexts MainMenu;

	public UITextCharSheet CharacterSheet;

	public UITextInventory InventoryScreen;

	public UITextBookEvent BookEvent;

	public UITextAlignment Alignment;

	public UITextActionBar ActionBar;

	public UICommonTexts CommonTexts;

	public UIPartySelectorTexts PartyTexts;

	public UINetLobbyTexts NetLobbyTexts;

	public UINetRolesTexts NetRolesTexts;

	public UINetLobbyErrorsTexts NetLobbyErrorsTexts;

	public UIActionText ActionTexts;

	public UITooltips Tooltips;

	public UIMoralePressureTooltip MoralePressureTooltip;

	public UITooltipElementLabels TooltipsElementLabels;

	public UIUnitInfo UnitInfo;

	public UISkillcheckTooltip SkillcheckTooltips;

	public UITutorial Tutorial;

	public UITextSettingsUI SettingsUI;

	public UICombatTexts CombatTexts;

	public UITurnBasedTexts TurnBasedTexts;

	public UITermsOfUseTexts TermsOfUseTexts;

	public UIDialog Dialog;

	public UISaveLoadTexts SaveLoadTexts;

	public UIVendor Vendor;

	public UICharGen CharGen;

	public UINewGame NewGameWin;

	public UIGameOverScreen GameOverScreen;

	public UICombatEndWindowTexts CombatEndWindow;

	public UINotificationTexts NotificationTexts;

	public UIQuestNotificationTexts QuestNotificationTexts;

	public UICaseNotificationTexts CaseNotificationTexts;

	public UICombatObjectivesTexts CombatObjectivesTexts;

	public UIKeyboardTexts KeyboardTexts;

	public UILoot LootWindow;

	public UIQuesJournalTexts QuesJournalTexts;

	public UITransitionTexts Transition;

	public GroupChangerTexts GroupChangerTexts;

	public UIContextMenu ContextMenu;

	public EncyclopediaTexts EncyclopediaTexts;

	public UIControllerModeTexts ControllerModeTexts;

	public UIBugReport UIBugReport;

	public UIFeedbackPopupTexts FeedbackPopupTexts;

	public UILootTypeTexts lootTypeTexts;

	public UIPropertyNames UIPropertyNames;

	public HUDTexts HUDTexts;

	public UIxBoxTexts XBoxTexts;

	public UICombatLogTexts CombatLog;

	public UIInspect Inspect;

	public UIInteractableSettingsReasons InteractableSettingsReasons;

	public UIOvertips Overtips;

	public UIFormationTexts FormationTexts;

	public UILocalMapTexts LocalMapTexts;

	public UICredits Credits;

	public UIEscapeMenu EscapeMenu;

	public UIEpilogues Epilogues;

	public UIDlcManager DlcManager;

	public UIPopupWindows PopUps;

	public UIWeaponCategories WeaponCategories;

	public UIDetectiveJournal DetectiveJournal;

	public DetectiveJournalDecor DetectiveDecor;

	public UIServiceWindows ServiceWindows;

	public UIAbilityModifications AbilityModifications;

	public UIPreciseAttack PreciseAttack;

	public static UIStrings Instance => ConfigRoot.Instance.LocalizedTexts.UserInterfacesText;
}
