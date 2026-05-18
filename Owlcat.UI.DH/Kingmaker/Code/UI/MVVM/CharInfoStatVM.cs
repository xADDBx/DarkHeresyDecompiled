using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.View.Bridge.Data;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.Localization;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoStatVM : ViewModel
{
	private MechanicEntity m_Entity;

	private MechanicEntity m_PreviewEntity;

	private LocalizedString m_FlatFooted;

	private readonly ReactiveProperty<string> m_Name = new ReactiveProperty<string>();

	private readonly ReactiveProperty<bool> m_HasBonuses = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_HasPenalties = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsValueEnabled = new ReactiveProperty<bool>(value: true);

	private readonly ReactiveProperty<int> m_StatValue = new ReactiveProperty<int>();

	private readonly ReactiveProperty<int> m_PreviewStatValue = new ReactiveProperty<int>();

	private readonly ReactiveProperty<string> m_StringValue = new ReactiveProperty<string>();

	private readonly ReactiveProperty<int> m_Bonus = new ReactiveProperty<int>();

	private readonly ReactiveProperty<bool> m_IsBattleSkill = new ReactiveProperty<bool>();

	private readonly bool m_IsValuePermanent;

	private readonly ReactiveProperty<int> m_Rank = new ReactiveProperty<int>();

	private readonly ReactiveProperty<TooltipBaseTemplate> m_Tooltip = new ReactiveProperty<TooltipBaseTemplate>();

	private SerialDisposable m_TooltipDisposable = new SerialDisposable();

	public string ShortName => UIUtilityText.GetStatShortName(StatType);

	public ReadOnlyReactiveProperty<string> Name => m_Name;

	public ReadOnlyReactiveProperty<bool> HasBonuses => m_HasBonuses;

	public ReadOnlyReactiveProperty<bool> HasPenalties => m_HasPenalties;

	public ReadOnlyReactiveProperty<bool> IsValueEnabled => m_IsValueEnabled;

	public ReadOnlyReactiveProperty<int> StatValue => m_StatValue;

	public ReadOnlyReactiveProperty<int> PreviewStatValue => m_PreviewStatValue;

	public ReadOnlyReactiveProperty<string> StringValue => m_StringValue;

	public ReadOnlyReactiveProperty<int> Bonus => m_Bonus;

	public ReadOnlyReactiveProperty<bool> IsBattleSkill => m_IsBattleSkill;

	public ReadOnlyReactiveProperty<TooltipBaseTemplate> Tooltip => m_Tooltip;

	public StatType? SourceStatType { get; private set; }

	public StatType StatType { get; }

	public CharInfoStatVM([NotNull] MechanicEntity entity, StatType statType, bool showPermanentValue)
	{
		m_Entity = entity;
		StatType = statType;
		m_IsValuePermanent = showPermanentValue;
		OnStatUpdated();
	}

	public CharInfoStatVM([NotNull] MechanicEntity entity, StatType statType, MechanicEntity previewEntity)
		: this(entity, statType, showPermanentValue: false)
	{
		m_PreviewEntity = previewEntity;
		OnStatUpdated();
	}

	public void UpdateEntity([NotNull] MechanicEntity entity, MechanicEntity previewEntity)
	{
		m_Entity = entity;
		m_PreviewEntity = previewEntity;
		OnStatUpdated();
	}

	private void OnStatUpdated()
	{
		if (m_Entity != null)
		{
			MechanicActorStat stat = m_Entity.Actor.Stats.GetStat(StatType);
			SourceStatType = MechanicActor.GetStatBaseStat(StatType);
			m_Name.Value = LocalizedTexts.Instance.Stats.GetText(StatType);
			m_StatValue.Value = (m_IsValuePermanent ? stat.PermanentValue : stat.ModifiedValue);
			m_Rank.Value = stat.BaseValue;
			m_IsBattleSkill.Value = StatType.IsCombatSkill();
			if (m_PreviewEntity != null)
			{
				MechanicActorStat stat2 = m_PreviewEntity.Actor.Stats.GetStat(StatType);
				m_PreviewStatValue.Value = (m_IsValuePermanent ? stat2.PermanentValue : stat2.ModifiedValue);
			}
			else
			{
				m_PreviewStatValue.Value = StatValue.CurrentValue;
			}
			if (StatType.IsAttribute())
			{
				ExtractAttributeStat(stat);
			}
			else if (StatType.IsSkill())
			{
				ExtractSkillStat(stat);
			}
			m_TooltipDisposable.Disposable = ObservableSubscribeExtensions.Subscribe(Observable.NextFrame(), delegate
			{
				m_Tooltip.Value = new TooltipTemplateStat(StatTooltipData.FromActor(m_Entity, StatType));
			}).AddTo(this);
		}
	}

	private void ExtractAttributeStat(MechanicActorStat stat)
	{
		m_IsValueEnabled.Value = stat.Enabled;
		m_Bonus.Value = GetModifier(stat, m_IsValuePermanent);
		m_HasBonuses.Value = stat.HasBonuses;
		m_HasPenalties.Value = stat.HasPenalties;
	}

	private void ExtractSkillStat(MechanicActorStat stat)
	{
		m_IsValueEnabled.Value = true;
		int modifier = GetModifier(stat, m_IsValuePermanent);
		m_Bonus.Value = modifier;
		m_HasBonuses.Value = stat.HasBonuses && modifier > 0;
		m_HasPenalties.Value = stat.HasPenalties && modifier < 0;
	}

	private static int GetModifier(MechanicActorStat stat, bool permanent)
	{
		if (!permanent)
		{
			return stat.ModifiedValue - stat.PermanentValue;
		}
		return stat.ModifiedValue - stat.BaseValue;
	}
}
