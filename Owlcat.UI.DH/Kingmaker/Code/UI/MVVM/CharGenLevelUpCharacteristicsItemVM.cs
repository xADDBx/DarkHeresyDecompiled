using System;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.View;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Levelup.Selections.Stats;
using Kingmaker.UnitLogic.Progression.Features.Advancements;
using Kingmaker.Utility.DotNetExtensions;
using ObservableCollections;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenLevelUpCharacteristicsItemVM : CharGenLevelUpSelectorBaseItemVM
{
	private readonly LevelUpManager m_LevelUpManager;

	protected ReactiveProperty<int> m_Points = new ReactiveProperty<int>();

	protected ReactiveProperty<int> m_AddedPoints = new ReactiveProperty<int>(5);

	private readonly BlueprintStatAdvancement m_StatAdvancement;

	private readonly SelectionStateStats m_StateStats;

	public readonly ObservableList<CharacteristicsPointStateView.CharacteristicsPointState> PointsState = new ObservableList<CharacteristicsPointStateView.CharacteristicsPointState>();

	private BaseUnitEntity m_Unit => m_LevelUpManager.PreviewUnit;

	public ReadOnlyReactiveProperty<int> Points => m_Points;

	public ReadOnlyReactiveProperty<int> AddedPoints => m_AddedPoints;

	public bool HasPointsToSpend
	{
		get
		{
			if (m_StateStats.GetPointsTotal(Stat) < m_StateStats.MaxPointsPerStat)
			{
				return m_StateStats.PointsSpentTotal < 1;
			}
			return false;
		}
	}

	public StatType Stat => m_StatAdvancement.Stat;

	public bool IsBattleStat => Stat.IsCombatSkill();

	public bool HasBackground { get; private set; }

	public bool HasAttributeMark { get; private set; }

	public AttributeBonusState BonusPenaltyState { get; private set; }

	public CharGenLevelUpCharacteristicsItemVM(BlueprintStatAdvancement statAdvancement, SelectionStateStats stateStats, LevelUpManager levelUpManager, Action onPointsChanged, Action<CharGenLevelUpSelectorBaseItemVM> onHover, CharGenLevelUpNestedListHeaderVM parentNodeVm = null)
		: base(statAdvancement, onHover, parentNodeVm)
	{
		m_StatAdvancement = statAdvancement;
		m_StateStats = stateStats;
		m_LevelUpManager = levelUpManager;
		Init();
		UpdatePointsState();
	}

	public void UpdatePointsState()
	{
		int maxPointsPerStat = m_StateStats.MaxPointsPerStat;
		int pointsInitial = m_StateStats.GetPointsInitial(Stat);
		int pointsTotal = m_StateStats.GetPointsTotal(Stat);
		PointsState.Clear();
		if (!IsSelected.CurrentValue)
		{
			m_Points.Value = m_Unit.GetStatOptional(Stat);
		}
		for (int i = 1; i <= maxPointsPerStat; i++)
		{
			PointsState.Add((i <= pointsInitial) ? CharacteristicsPointStateView.CharacteristicsPointState.NotAvailable : ((i <= pointsTotal) ? CharacteristicsPointStateView.CharacteristicsPointState.JustSpent : CharacteristicsPointStateView.CharacteristicsPointState.Available));
		}
		m_AddedPoints.Value = (int)m_Unit.GetStatOptional(Stat) - m_Points.Value;
		m_State.Value = ((PointsState.Any((CharacteristicsPointStateView.CharacteristicsPointState p) => p == CharacteristicsPointStateView.CharacteristicsPointState.JustSpent) && pointsTotal == maxPointsPerStat) ? LEVEL_UP_ITEM_STATE.AlreadyExist : LEVEL_UP_ITEM_STATE.Available);
		UpdateTooltip();
	}

	private void Init()
	{
		m_Label.Value = LocalizedTexts.Instance.Stats.GetText(Stat);
		m_Points.Value = m_Unit.GetStatOptional(Stat);
		if (Stat.IsAttribute())
		{
			m_Acronym.Value = LocalizedTexts.Instance.Stats.GetShortText(Stat);
			ModifiableValueAttributeStat attribute = m_Unit.Stats.GetAttribute(Stat);
			BonusPenaltyState = (attribute.HasBonuses ? AttributeBonusState.Bonus : (attribute.HasPenalties ? AttributeBonusState.Penalty : AttributeBonusState.Normal));
		}
		else
		{
			StatType type = m_Unit.GetSkillOptional(Stat).BaseStat.Type;
			m_Acronym.Value = LocalizedTexts.Instance.Stats.GetShortText(type);
			HasAttributeMark = type != GetPrevSkillBaseStatType();
			HasBackground = (int)type % 2 != 0;
		}
	}

	private void UpdateTooltip()
	{
		int upgradeProgression = m_StateStats.GetPointsInitial(Stat) + m_StateStats.GetPointsSpent(Stat);
		base.Template = new TooltipTemplateLevelUpStat(Stat, m_Unit, upgradeProgression);
	}

	private StatType GetPrevSkillBaseStatType()
	{
		StatType statType = (StatType)(object)StatTypeHelper.DisplayOrder[StatTypeHelper.DisplayOrder.IndexOf(Stat) - 1];
		if (!statType.IsSkill())
		{
			return StatType.Unknown;
		}
		return m_Unit.GetSkillOptional(statType).BaseStat.Type;
	}
}
