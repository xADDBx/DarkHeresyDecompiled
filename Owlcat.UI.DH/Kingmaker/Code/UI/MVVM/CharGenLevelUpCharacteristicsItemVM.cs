using System;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.View;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework.Mechanics.Actor;
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

	private readonly Action<CharGenLevelUpCharacteristicsItemVM, bool> m_OnPointsChanged;

	protected ReactiveProperty<int> m_Points = new ReactiveProperty<int>();

	protected ReactiveProperty<int> m_AddedPoints = new ReactiveProperty<int>(5);

	protected ReactiveProperty<bool> m_HasPointsToSpend = new ReactiveProperty<bool>();

	private readonly BlueprintStatAdvancement m_StatAdvancement;

	private readonly SelectionStateStats m_StateStats;

	public readonly ObservableList<CharacteristicsPointStateView.CharacteristicsPointState> PointsState = new ObservableList<CharacteristicsPointStateView.CharacteristicsPointState>();

	private BaseUnitEntity m_Unit => m_LevelUpManager.PreviewUnit;

	public ReadOnlyReactiveProperty<int> Points => m_Points;

	public ReadOnlyReactiveProperty<int> AddedPoints => m_AddedPoints;

	public ReadOnlyReactiveProperty<bool> HasPointsToSpend => m_HasPointsToSpend;

	public StatType Stat => m_StatAdvancement.Stat;

	public bool IsBattleStat => Stat.IsCombatSkill();

	public bool HasBackground { get; private set; }

	public bool HasAttributeMark { get; private set; }

	public bool HasSeveralPoints { get; private set; }

	public bool HasJustSpentPoints => PointsState.Any((CharacteristicsPointStateView.CharacteristicsPointState p) => p == CharacteristicsPointStateView.CharacteristicsPointState.JustSpent);

	public AttributeBonusState BonusPenaltyState { get; private set; }

	public CharGenLevelUpCharacteristicsItemVM(BlueprintStatAdvancement statAdvancement, SelectionStateStats stateStats, LevelUpManager levelUpManager, Action<CharGenLevelUpCharacteristicsItemVM, bool> onPointsChanged, Action<CharGenLevelUpSelectorBaseItemVM> onHover, CharGenLevelUpNestedListHeaderVM parentNodeVm = null)
		: base(statAdvancement, onHover, parentNodeVm)
	{
		m_StatAdvancement = statAdvancement;
		m_StateStats = stateStats;
		m_LevelUpManager = levelUpManager;
		m_OnPointsChanged = onPointsChanged;
		Init();
		UpdatePointsState();
	}

	public void UpdatePointsState()
	{
		int maxPointsPerStat = m_StateStats.MaxPointsPerStat;
		int pointsInitial = m_StateStats.GetPointsInitial(Stat);
		int pointsTotal = m_StateStats.GetPointsTotal(Stat);
		PointsState.Clear();
		int statIgnoringOverride = m_Unit.Actor.GetStatIgnoringOverride(Stat);
		if (!IsSelected.CurrentValue || m_StateStats.PointsTotal > 1)
		{
			m_Points.Value = statIgnoringOverride;
		}
		for (int i = 1; i <= maxPointsPerStat; i++)
		{
			PointsState.Add((i <= pointsInitial) ? CharacteristicsPointStateView.CharacteristicsPointState.NotAvailable : ((i <= pointsTotal) ? CharacteristicsPointStateView.CharacteristicsPointState.JustSpent : CharacteristicsPointStateView.CharacteristicsPointState.Available));
		}
		m_AddedPoints.Value = statIgnoringOverride - m_Points.Value;
		m_State.Value = ((pointsTotal == maxPointsPerStat) ? LEVEL_UP_ITEM_STATE.AlreadyExist : LEVEL_UP_ITEM_STATE.Available);
		m_HasPointsToSpend.Value = m_StateStats.GetPointsTotal(Stat) < m_StateStats.MaxPointsPerStat && m_StateStats.PointsSpentTotal < m_StateStats.PointsTotal;
		UpdateTooltip();
	}

	public void ChangePoints(bool isAdding)
	{
		m_OnPointsChanged?.Invoke(this, isAdding);
		m_HasPointsToSpend.Value = m_StateStats.GetPointsTotal(Stat) < m_StateStats.MaxPointsPerStat && m_StateStats.PointsSpentTotal < m_StateStats.PointsTotal;
	}

	private void Init()
	{
		m_Label.Value = LocalizedTexts.Instance.Stats.GetText(Stat);
		m_Points.Value = m_Unit.Actor.GetStat(Stat, null, default(StatContext), "Init");
		HasSeveralPoints = m_StateStats.PointsTotal > 1;
		m_HasPointsToSpend.Value = m_StateStats.GetPointsTotal(Stat) < m_StateStats.MaxPointsPerStat && m_StateStats.PointsSpentTotal < m_StateStats.PointsTotal;
		if (Stat.IsAttribute())
		{
			m_Acronym.Value = LocalizedTexts.Instance.Stats.GetShortText(Stat);
			StatQueryOutput statQueryOutput = new StatQueryOutput();
			m_Unit.Actor.GetStat(Stat, statQueryOutput, default(StatContext), "Init");
			BonusPenaltyState = (statQueryOutput.HasNonPermanentBonuses ? AttributeBonusState.Bonus : (statQueryOutput.HasNonPermanentPenalties ? AttributeBonusState.Penalty : AttributeBonusState.Normal));
		}
		else
		{
			StatType valueOrDefault = MechanicActor.GetStatBaseStat(Stat).GetValueOrDefault();
			m_Acronym.Value = LocalizedTexts.Instance.Stats.GetShortText(valueOrDefault);
			HasAttributeMark = valueOrDefault != GetPrevSkillBaseStatType();
			HasBackground = (int)valueOrDefault % 2 != 0;
		}
	}

	private void UpdateTooltip()
	{
		int upgradeProgression = m_StateStats.GetPointsInitial(Stat) + m_StateStats.GetPointsSpent(Stat);
		base.Template = new TooltipTemplateLevelUpStat(Stat, m_Unit, upgradeProgression, m_StateStats.MaxPointsPerStat);
	}

	private StatType GetPrevSkillBaseStatType()
	{
		StatType stat = (StatType)(object)StatTypeHelper.DisplayOrder[StatTypeHelper.DisplayOrder.IndexOf(Stat) - 1];
		if (!stat.IsSkill())
		{
			return StatType.Unknown;
		}
		return MechanicActor.GetStatBaseStat(stat).GetValueOrDefault();
	}
}
