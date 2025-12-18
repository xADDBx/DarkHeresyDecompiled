using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.View.Bridge.Data;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.UnitLogic.Progression.Features.Advancements;
using Owlcat.UI;
using Photon.Realtime;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenAttributesItemVM : CharGenBackgroundBaseItemVM, INetLobbyPlayersHandler, ISubscriber
{
	private readonly ReactiveProperty<bool> m_CanAdvance = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_CanRetreat = new ReactiveProperty<bool>();

	private readonly ReactiveCommand<bool> m_CheckCoopControls = new ReactiveCommand<bool>();

	private readonly ReactiveProperty<int> m_DiffValue = new ReactiveProperty<int>();

	private readonly ReactiveProperty<bool> m_IsMainCharacter = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsRecommended = new ReactiveProperty<bool>();

	private readonly Action<StatType, bool> m_AdvanceAction;

	private readonly Action<CharGenAttributesItemVM> m_OnHovered;

	private readonly ReactiveProperty<int> m_StatRanks = new ReactiveProperty<int>();

	public readonly StatType StatType;

	private readonly ReactiveProperty<int> m_StatValue = new ReactiveProperty<int>();

	private readonly ReactiveProperty<TooltipBaseTemplate> m_Tooltip = new ReactiveProperty<TooltipBaseTemplate>();

	public readonly int ValuePerRank;

	public ReadOnlyReactiveProperty<bool> CanAdvance => m_CanAdvance;

	public ReadOnlyReactiveProperty<bool> CanRetreat => m_CanRetreat;

	public Observable<bool> CheckCoopControls => m_CheckCoopControls;

	public ReadOnlyReactiveProperty<int> DiffValue => m_DiffValue;

	public ReadOnlyReactiveProperty<bool> IsMainCharacter => m_IsMainCharacter;

	public ReadOnlyReactiveProperty<bool> IsRecommended => m_IsRecommended;

	public ReadOnlyReactiveProperty<int> StatRanks => m_StatRanks;

	public ReadOnlyReactiveProperty<int> StatValue => m_StatValue;

	public ReadOnlyReactiveProperty<TooltipBaseTemplate> Tooltip => m_Tooltip;

	public CharGenAttributesItemVM(FeatureSelectionItem selectionItem, Action<StatType, bool> advanceAction, Action<CharGenAttributesItemVM> onHovered, CharGenPhaseType phaseType, ReactiveProperty<CharGenPhaseBaseVM> currentPhase)
		: base(selectionItem, null, phaseType, currentPhase)
	{
		if (selectionItem.Feature is BlueprintStatAdvancement blueprintStatAdvancement)
		{
			StatType = blueprintStatAdvancement.Stat;
			ValuePerRank = blueprintStatAdvancement.ValuePerRank;
			base.DisplayName = LocalizedTexts.Instance.Stats.GetText(StatType);
			m_AdvanceAction = advanceAction;
			m_OnHovered = onHovered;
			m_IsMainCharacter.Value = UtilityNet.IsControlMainCharacter();
			AddDisposable(EventBus.Subscribe(this));
		}
	}

	public void HandlePlayerEnteredRoom(Photon.Realtime.Player player)
	{
	}

	public void HandlePlayerLeftRoom(Photon.Realtime.Player player)
	{
		m_IsMainCharacter.Value = UtilityNet.IsControlMainCharacter();
		m_CheckCoopControls.Execute(UtilityNet.IsControlMainCharacter());
	}

	public void HandlePlayerChanged()
	{
	}

	public void HandleLastPlayerLeftLobby()
	{
	}

	public void HandleRoomOwnerChanged()
	{
	}

	protected override void DoSelectMe()
	{
	}

	public void AdvanceStat()
	{
		if (CanAdvance.CurrentValue)
		{
			m_AdvanceAction?.Invoke(StatType, arg2: true);
		}
	}

	public void RetreatStat()
	{
		if (CanRetreat.CurrentValue)
		{
			m_AdvanceAction?.Invoke(StatType, arg2: false);
		}
	}

	public void UpdateTooltip(ModifiableValue unitStat)
	{
		StatTooltipData statTooltipData = default(StatTooltipData);
		if (!(unitStat is ModifiableValueAttributeStat attribute))
		{
			if (!(unitStat is ModifiableValueSkill skill))
			{
				if (unitStat != null)
				{
					statTooltipData = new StatTooltipData(unitStat);
				}
				else
				{
					global::_003CPrivateImplementationDetails_003E.ThrowSwitchExpressionException(unitStat);
				}
			}
			else
			{
				statTooltipData = new StatTooltipData(skill);
			}
		}
		else
		{
			statTooltipData = new StatTooltipData(attribute);
		}
		StatTooltipData statData = statTooltipData;
		m_Tooltip.Value = new TooltipTemplateStat(statData);
	}

	public void OnHovered(bool state)
	{
		m_OnHovered?.Invoke(state ? this : null);
	}

	public void UpdateRecommendedMark(List<StatType> recommendedStats)
	{
		m_IsRecommended.Value = recommendedStats.Contains(StatType);
	}

	public void SetStatValue(int value)
	{
		m_StatValue.Value = value;
	}

	public void SetDiffValue(int value)
	{
		m_DiffValue.Value = value;
	}

	public void SetStatRanks(int value)
	{
		m_StatRanks.Value = value;
	}

	public void SetCanAdvance(bool value)
	{
		m_CanAdvance.Value = value;
	}

	public void SetCanRetreat(bool value)
	{
		m_CanRetreat.Value = value;
	}
}
