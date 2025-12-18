using System.Collections.Generic;
using Code.View.UI.UIUtils;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.View.Bridge.Data;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Localization;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Settings;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoStatVM : ViewModel, IModifiableValueChangedHandler<EntitySubscriber>, IModifiableValueChangedHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IModifiableValueChangedHandler, EntitySubscriber>
{
	private readonly ReactiveProperty<ModifiableValue> m_Stat = new ReactiveProperty<ModifiableValue>();

	private readonly ReactiveProperty<ModifiableValue> m_PreviewStat = new ReactiveProperty<ModifiableValue>();

	private readonly ReactiveProperty<string> m_Name = new ReactiveProperty<string>();

	private readonly ReactiveProperty<bool> m_HasBonuses = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_HasPenalties = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsValueEnabled = new ReactiveProperty<bool>(value: true);

	private readonly ReactiveProperty<int> m_StatValue = new ReactiveProperty<int>();

	private readonly ReactiveProperty<int> m_PreviewStatValue = new ReactiveProperty<int>();

	private readonly ReactiveProperty<string> m_StringValue = new ReactiveProperty<string>();

	private readonly ReactiveProperty<bool> m_IsRecommended = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<int> m_Bonus = new ReactiveProperty<int>();

	private readonly ReactiveProperty<bool> m_HighlightedBySource = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsBattleSkill = new ReactiveProperty<bool>();

	private readonly bool m_IsValuePermanent;

	private readonly ReactiveProperty<int?> m_RaceBonus = new ReactiveProperty<int?>();

	private readonly ReactiveProperty<int> m_Rank = new ReactiveProperty<int>();

	private readonly ReactiveProperty<TooltipBaseTemplate> m_Tooltip = new ReactiveProperty<TooltipBaseTemplate>();

	private LocalizedString m_FlatFooted;

	public readonly float FontMultiplier = FontSizeMultiplier;

	public StatType? SourceStatType { get; private set; }

	public StatType StatType { get; private set; }

	public string ShortName => UIUtilityText.GetStatShortName(StatType);

	public ReadOnlyReactiveProperty<string> Name => m_Name;

	public ReadOnlyReactiveProperty<bool> HasBonuses => m_HasBonuses;

	public ReadOnlyReactiveProperty<bool> HasPenalties => m_HasPenalties;

	public ReadOnlyReactiveProperty<bool> IsValueEnabled => m_IsValueEnabled;

	public ReadOnlyReactiveProperty<int> StatValue => m_StatValue;

	public ReadOnlyReactiveProperty<int> PreviewStatValue => m_PreviewStatValue;

	public ReadOnlyReactiveProperty<string> StringValue => m_StringValue;

	public ReadOnlyReactiveProperty<bool> IsRecommended => m_IsRecommended;

	public ReadOnlyReactiveProperty<int> Bonus => m_Bonus;

	public ReadOnlyReactiveProperty<bool> HighlightedBySource => m_HighlightedBySource;

	public ReadOnlyReactiveProperty<bool> IsBattleSkill => m_IsBattleSkill;

	public ReadOnlyReactiveProperty<TooltipBaseTemplate> Tooltip => m_Tooltip;

	private static float FontSizeMultiplier => SettingsRoot.Accessiability.FontSizeMultiplier;

	public CharInfoStatVM([NotNull] ModifiableValue stat, bool showPermanentValue)
	{
		m_Stat.Value = stat;
		m_IsValuePermanent = showPermanentValue;
		m_Stat.Subscribe(delegate
		{
			OnStatUpdated();
		}).AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
	}

	public CharInfoStatVM([NotNull] ModifiableValue stat, ModifiableValue previewStat)
		: this(stat, showPermanentValue: false)
	{
		m_PreviewStat.Value = previewStat;
		OnStatUpdated();
	}

	private void OnStatValueChanged(ModifiableValue stat)
	{
		OnStatUpdated();
	}

	private void OnStatUpdated()
	{
		if (m_Stat.Value == null)
		{
			return;
		}
		StatType = m_Stat.Value.Type;
		SourceStatType = UIUtilityUnit.GetSourceStatType(m_Stat.Value);
		m_Name.Value = LocalizedTexts.Instance.Stats.GetText(StatType);
		m_StatValue.Value = (m_IsValuePermanent ? m_Stat.Value.PermanentValue : m_Stat.Value.ModifiedValue);
		m_Rank.Value = m_Stat.Value.BaseValue;
		m_IsBattleSkill.Value = StatType.IsCombatSkill();
		if (m_PreviewStat.Value != null)
		{
			m_PreviewStatValue.Value = (m_IsValuePermanent ? m_PreviewStat.Value.PermanentValue : m_PreviewStat.Value.ModifiedValue);
		}
		else
		{
			m_PreviewStatValue.Value = StatValue.CurrentValue;
		}
		if (!TryExtractModifiableValueAttributeStat(m_Stat.Value))
		{
			TryExtractModifiableValueSkill(m_Stat.Value);
		}
		StatTooltipData statData = null;
		if (m_Stat.Value is ModifiableValueAttributeStat attribute)
		{
			statData = new StatTooltipData(attribute);
		}
		else if (m_Stat.Value is ModifiableValueSkill skill)
		{
			statData = new StatTooltipData(skill);
		}
		else
		{
			ModifiableValue value = m_Stat.Value;
			if (value != null)
			{
				statData = new StatTooltipData(value);
			}
		}
		m_Tooltip.Value = new TooltipTemplateStat(statData);
	}

	public void UpdateRecommendedMark(List<StatType> recommendedStats)
	{
		m_IsRecommended.Value = recommendedStats?.Contains(StatType) ?? false;
	}

	private bool TryExtractModifiableValueAttributeStat(ModifiableValue stat)
	{
		if (!(stat is ModifiableValueAttributeStat modifiableValueAttributeStat))
		{
			return false;
		}
		m_IsValueEnabled.Value = modifiableValueAttributeStat.Enabled;
		m_Bonus.Value = GetModifier(modifiableValueAttributeStat, m_IsValuePermanent);
		m_HasBonuses.Value = modifiableValueAttributeStat.HasBonuses;
		m_HasPenalties.Value = modifiableValueAttributeStat.HasPenalties;
		return true;
	}

	private bool TryExtractModifiableValueSkill(ModifiableValue stat)
	{
		if (!(stat is ModifiableValueSkill modifiableValueSkill))
		{
			return false;
		}
		m_IsValueEnabled.Value = true;
		m_Bonus.Value = GetModifier(modifiableValueSkill, m_IsValuePermanent);
		m_HasBonuses.Value = modifiableValueSkill.HasBonuses && m_Bonus.Value > 0;
		m_HasPenalties.Value = modifiableValueSkill.HasPenalties && m_Bonus.Value < 0;
		return true;
	}

	private int GetModifier(ModifiableValueAttributeStat stat, bool permanent)
	{
		if (!permanent)
		{
			return stat.ModifiedValue - stat.PermanentValue;
		}
		return stat.ModifiedValue - stat.BaseValue;
	}

	private int GetModifier(ModifiableValueSkill skill, bool permanent)
	{
		if (!permanent)
		{
			return skill.ModifiedValue - skill.PermanentValue;
		}
		return skill.ModifiedValue - skill.BaseValue;
	}

	public void HighlightBySourceType(StatType? sourceType)
	{
		m_HighlightedBySource.Value = sourceType.HasValue && SourceStatType == sourceType;
	}

	public void HandleModifiableValueChanged(ModifiableValue modifiableValue)
	{
		OnStatValueChanged(modifiableValue);
	}
}
