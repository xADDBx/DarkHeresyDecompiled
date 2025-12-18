using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Gameplay.Features.VariableInteractions;
using Kingmaker.Code.View.Bridge.Data;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Controllers.Clicks.Handlers;
using Kingmaker.Controllers.Combat;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Interaction;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.MapObjects;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class InteractionVariantVM : SelectionGroupEntityVM
{
	public readonly InteractionActorWithConditions InteractionActor;

	private readonly ReactiveProperty<string> m_InteractionName = new ReactiveProperty<string>();

	private readonly ReactiveProperty<string> m_InteractionChance = new ReactiveProperty<string>();

	private readonly ReactiveProperty<string> m_SuitableUnitName = new ReactiveProperty<string>();

	private readonly ReactiveProperty<bool> m_CanInteract = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_HaveNotEnoughAPinTBM = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<CombatHintEntityVM> m_CombatHintEntityVM = new ReactiveProperty<CombatHintEntityVM>();

	private readonly ReactiveCommand<Unit> m_UpdateHint = new ReactiveCommand<Unit>();

	public int? RequiredResourceCount;

	public int? ResourceCount;

	public int? UnitCount;

	public readonly string ShowReason;

	public readonly bool OnlyOnceCheck;

	public readonly string ResourceName;

	public readonly bool Disabled;

	private readonly Action m_OnInteract;

	public ReadOnlyReactiveProperty<string> InteractionName => m_InteractionName;

	public ReadOnlyReactiveProperty<string> InteractionChance => m_InteractionChance;

	public ReadOnlyReactiveProperty<string> SuitableUnitName => m_SuitableUnitName;

	public ReadOnlyReactiveProperty<bool> CanInteract => m_CanInteract;

	public ReadOnlyReactiveProperty<bool> HaveNotEnoughAPinTBM => m_HaveNotEnoughAPinTBM;

	public ReadOnlyReactiveProperty<CombatHintEntityVM> CombatHintEntityVM => m_CombatHintEntityVM;

	public Observable<Unit> UpdateHint => m_UpdateHint;

	public bool IsAdditionalCombatObj
	{
		get
		{
			if (GameUIState.Instance.IsInCombat.CurrentValue)
			{
				return InteractionActor.VariantActor.CombatObjective != null;
			}
			return false;
		}
	}

	public bool LimitedUnitsCheck => UnitCount.HasValue;

	public InteractionVariantVM(InteractionActorWithConditions interactionActor, string resourceName, int? resourceCount, int? requiredResourceCount, int? unitCount, string showReason, Action onInteract)
		: base(allowSwitchOff: false)
	{
		InteractionActor = interactionActor;
		m_OnInteract = onInteract;
		ShowReason = showReason;
		IInteractionVariantActor variantActor = InteractionActor.VariantActor;
		InteractionSkillCheckPart interactionSkillCheckPart = variantActor.InteractionPart as InteractionSkillCheckPart;
		m_InteractionName.Value = (requiredResourceCount.HasValue ? resourceName : variantActor.GetInteractionName());
		m_SuitableUnitName.Value = GetSuitableUnitName(variantActor);
		RequiredResourceCount = requiredResourceCount;
		ResourceCount = resourceCount;
		UnitCount = unitCount;
		OnlyOnceCheck = variantActor.CheckOnlyOnce;
		ResourceName = resourceName;
		Disabled = (interactionSkillCheckPart != null && interactionSkillCheckPart.IsFailed && interactionSkillCheckPart.Settings.InteractOnlyWithToolAfterFail && !RequiredResourceCount.HasValue) || resourceCount < requiredResourceCount || !variantActor.CanUse || (LimitedUnitsCheck && UnitCount <= 0);
		RootUIContext.Instance.GameUIState.IsInCombat.Subscribe(delegate
		{
			UpdateInteractable();
		}).AddTo(this);
		UpdateInteractionChance();
		Game.Instance.Controllers.SelectionCharacter.ActualGroupUpdated.Subscribe(UpdateInteractionChance).AddTo(this);
		if (variantActor.CombatObjective != null)
		{
			m_CombatHintEntityVM.Value = new CombatHintEntityVM(variantActor.CombatObjective, new ReactiveProperty<OvertipState>());
		}
	}

	public virtual void Interact()
	{
		using (ContextData<InteractionVariantData>.Request().Setup(InteractionActor.VariantActor))
		{
			ClickMapObjectHandler.TryInteract(InteractionActor.VariantActor.InteractionPart, Game.Instance.Controllers.SelectionCharacter.SelectedUnits.ToList(), muteEvents: false, InteractionActor.VariantActor);
		}
		m_OnInteract?.Invoke();
	}

	protected override void DoSelectMe()
	{
	}

	private void UpdateInteractionChance()
	{
		List<BaseUnitEntity> units = Game.Instance.Controllers.SelectionCharacter.SelectedUnits.ToList();
		m_InteractionChance.Value = (HideChance() ? string.Empty : UtilitySkillcheck.GetInteractionChance(InteractionActor.VariantActor, units));
		m_InteractionChance.ForceNotify();
		m_UpdateHint.Execute();
	}

	private bool HideChance()
	{
		if (InteractionActor.VariantActor.Skill != 0 && CanInteract.CurrentValue)
		{
			if (OnlyOnceCheck)
			{
				return InteractionActor.VariantActor.AlreadyUsed;
			}
			return false;
		}
		return true;
	}

	private void UpdateInteractable()
	{
		ControllersAccess controllers = Game.Instance.Controllers;
		bool turnBasedModeActive = controllers.TurnController.TurnBasedModeActive;
		ReactiveProperty<bool> haveNotEnoughAPinTBM = m_HaveNotEnoughAPinTBM;
		int value;
		if (turnBasedModeActive)
		{
			PartUnitCombatState combatStateOptional = controllers.SelectionCharacter.SelectedUnit.Value.GetCombatStateOptional();
			if (combatStateOptional != null && combatStateOptional.IsInCombat)
			{
				value = ((combatStateOptional.ActionPoints < InteractionActor.VariantActor.InteractionPart.ActionPointsCost) ? 1 : 0);
				goto IL_0061;
			}
		}
		value = 0;
		goto IL_0061;
		IL_0061:
		haveNotEnoughAPinTBM.Value = (byte)value != 0;
		m_CanInteract.Value = (turnBasedModeActive ? InteractionActor.VariantActor.CheckRestriction(controllers.SelectionCharacter.SelectedUnit.Value) : controllers.SelectionCharacter.SelectedUnits.Any((BaseUnitEntity c) => InteractionActor.VariantActor.CheckRestriction(c)));
		m_CanInteract.Value &= InteractionActor.SelectConditions == null || InteractionActor.SelectConditions.Check();
		m_UpdateHint.Execute();
	}

	private static string GetSuitableUnitName(IInteractionVariantActor interactionActor)
	{
		List<BaseUnitEntity> list = Game.Instance.Controllers.SelectionCharacter.SelectedUnits.ToList();
		InteractionRestrictionPart skillUseRestriction = interactionActor as InteractionRestrictionPart;
		if (skillUseRestriction != null)
		{
			(BaseUnitEntity, int) tuple = list.Select((BaseUnitEntity u) => (u: u, skillUseRestriction.GetUserPriority(u))).MaxBy(((BaseUnitEntity u, int) p) => p.Item2);
			if (tuple.Item2 < 0)
			{
				return string.Empty;
			}
			return tuple.Item1.Name;
		}
		return interactionActor.InteractionPart?.SelectUnit(list, muteEvents: false, interactionActor)?.Name ?? string.Empty;
	}

	public virtual string GetHint()
	{
		if (InteractionActor.SelectConditions != null && !InteractionActor.SelectConditions.Check())
		{
			return HandleNarratorText(InteractionActor.CannotSelectReason.String.Text);
		}
		if (!string.IsNullOrEmpty(ShowReason))
		{
			return HandleNarratorText(ShowReason);
		}
		UIOvertips overtips = UIStrings.Instance.Overtips;
		StringBuilder stringBuilder = new StringBuilder();
		if (OnlyOnceCheck && InteractionActor.VariantActor.AlreadyUsed)
		{
			return stringBuilder.ToString();
		}
		int? requiredResourceCount = RequiredResourceCount;
		if (requiredResourceCount.HasValue && requiredResourceCount.GetValueOrDefault() > 0)
		{
			stringBuilder.Append(ResourceName + "\n");
			stringBuilder.Append($"{overtips.HasResourceCount.Text}: {ResourceCount}\n");
			stringBuilder.Append($"{overtips.RequiredResourceCount.Text}: {RequiredResourceCount}\n");
			return stringBuilder.ToString();
		}
		if (!string.IsNullOrEmpty(InteractionChance.CurrentValue) && !string.IsNullOrEmpty(SuitableUnitName.CurrentValue))
		{
			bool flag = InteractionName.CurrentValue != LocalizedTexts.Instance.Stats.GetText(InteractionActor.VariantActor.Skill);
			if (flag)
			{
				stringBuilder.Append(LocalizedTexts.Instance.Stats.GetText(InteractionActor.VariantActor.Skill) + "\n");
			}
			if (HaveNotEnoughAPinTBM.CurrentValue)
			{
				stringBuilder.Append(LocalizedTexts.Instance.Reasons.NotEnoughActionPoints.Text + "\n\n");
			}
			else if (flag)
			{
				stringBuilder.Append("\n");
			}
			stringBuilder.Append(overtips.SuitableUnit.Text + "\n");
			stringBuilder.Append(SuitableUnitName.CurrentValue ?? "");
		}
		return stringBuilder.ToString();
	}

	protected string HandleNarratorText(string text)
	{
		return UIUtilityText.StringIDToColor(text, DialogCueColors.NarratorColorStringID, UIConfig.Instance.DigitalColors.DigitalNarratorColor);
	}
}
