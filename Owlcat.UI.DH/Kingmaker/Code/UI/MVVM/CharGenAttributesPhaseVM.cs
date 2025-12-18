using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.GameCommands;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.UnitLogic.Progression.Features.Advancements;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenAttributesPhaseVM : CharGenBackgroundBasePhaseVM<CharGenAttributesItemVM>, ILevelUpManagerUIHandler, ISubscriber, ICharGenAttributesPhaseHandler
{
	public const int MaxRanksPerStat = 2;

	private readonly ReactiveProperty<int> m_AvailablePointsLeft = new ReactiveProperty<int>();

	public CharInfoSkillsBlockVM CharInfoSkillsBlock;

	private readonly ReactiveProperty<BaseUnitEntity> m_PreviewUnit = new ReactiveProperty<BaseUnitEntity>();

	private readonly List<SelectionStateFeature> m_SelectionStates = new List<SelectionStateFeature>();

	private readonly Dictionary<StatType, int> m_StatRanks = new Dictionary<StatType, int>();

	private int m_ValuePerRank;

	private readonly ReactiveCommand<Unit> m_OnUpdateState = new ReactiveCommand<Unit>();

	public ReadOnlyReactiveProperty<int> AvailablePointsLeft => m_AvailablePointsLeft;

	public Observable<Unit> OnUpdateState => m_OnUpdateState;

	private IEnumerable<SelectionStateFeature> AvailableSelectionStates => m_SelectionStates.Where((SelectionStateFeature s) => !s.IsMade);

	public CharGenAttributesPhaseVM(CharGenContext charGenContext, ReactiveProperty<CharGenPhaseBaseVM> currentPhase)
		: base(charGenContext, FeatureGroup.ChargenAttribute, CharGenPhaseType.Attributes, currentPhase)
	{
		CanShowVisualSettings = false;
	}

	void ICharGenAttributesPhaseHandler.HandleTryAdvanceStat(StatType statType, bool advance)
	{
		int num;
		if (advance)
		{
			SelectionStateFeature selectionStateFeature = AvailableSelectionStates.FirstOrDefault();
			if (selectionStateFeature == null)
			{
				return;
			}
			FeatureSelectionItem selectionItem = selectionStateFeature.Items.FirstOrDefault((FeatureSelectionItem i) => i.Feature is BlueprintStatAdvancement blueprintStatAdvancement2 && blueprintStatAdvancement2.Stat == statType);
			selectionStateFeature.Select(selectionItem);
			num = 1;
		}
		else
		{
			m_SelectionStates.FirstOrDefault(delegate(SelectionStateFeature state)
			{
				if (state.IsMade)
				{
					FeatureSelectionItem? selectionItem2 = state.SelectionItem;
					if (selectionItem2.HasValue && selectionItem2.GetValueOrDefault().Feature is BlueprintStatAdvancement blueprintStatAdvancement)
					{
						return blueprintStatAdvancement.Stat == statType;
					}
				}
				return false;
			})?.ClearSelection();
			num = -1;
		}
		m_StatRanks[statType] += num;
		EventBus.RaiseEvent(delegate(ILevelUpManagerUIHandler h)
		{
			h.HandleUISelectionChanged();
		});
		UpdateState();
		UpdateHint();
	}

	public void HandleCreateLevelUpManager(LevelUpManager manager)
	{
	}

	public void HandleDestroyLevelUpManager()
	{
	}

	public void HandleUISelectCareerPath()
	{
		UpdatePreviewUnit();
	}

	public void HandleUICommitChanges()
	{
	}

	public void HandleUISelectionChanged()
	{
		UpdateState();
	}

	protected override void Clear()
	{
		base.Clear();
		m_SelectionStates.Clear();
		m_StatRanks.Clear();
	}

	protected override void OnBeginDetailedView()
	{
		if (!Subscribed)
		{
			UpdatePreviewUnit();
			CharInfoSkillsBlock = AddDisposableAndReturn(new CharInfoSkillsBlockVM(m_PreviewUnit, null));
			AddDisposable(ObservableSubscribeExtensions.Subscribe(CharInfoSkillsBlock.OnStatsUpdated, delegate
			{
				UpdateSkillsHighlight();
			}));
			AddDisposable(base.SelectedItem.Subscribe(delegate
			{
				UpdateSkillsHighlight();
			}));
			AddDisposable(EventBus.Subscribe(this));
		}
		if (!Subscribed)
		{
			AddDisposable(CharGenContext.LevelUpManager.Subscribe(HandleLevelUpManager));
			Subscribed = true;
		}
		TrySelectItem();
		UpdateHint();
		UpdateRecommendedStats();
	}

	private void UpdateHint()
	{
		if (AvailablePointsLeft.CurrentValue == 0)
		{
			SetPhaseHint(string.Empty);
		}
		else
		{
			SetPhaseHint(UIStrings.Instance.CharGen.SpreadOutPointsHint.Text ?? "");
		}
	}

	private void UpdateRecommendedStats()
	{
		BaseUnitEntity unit = CharGenContext.LevelUpManager?.CurrentValue.PreviewUnit;
		List<StatType> selectedCareerRecommendedStats = UtilityChargen.GetSelectedCareerRecommendedStats<BlueprintAttributeAdvancement>(unit);
		List<StatType> selectedCareerRecommendedStats2 = UtilityChargen.GetSelectedCareerRecommendedStats<BlueprintSkillAdvancement>(unit);
		foreach (CharGenAttributesItemVM item in SelectionGroup.EntitiesCollection)
		{
			item.UpdateRecommendedMark(selectedCareerRecommendedStats);
		}
		CharInfoSkillsBlock.SetRecommendedMarks(selectedCareerRecommendedStats2);
	}

	protected override bool CheckIsCompleted()
	{
		if (m_SelectionStates.Any())
		{
			return m_SelectionStates.All((SelectionStateFeature s) => s.IsMade && s.IsValid);
		}
		return false;
	}

	protected override void HandleLevelUpManager(LevelUpManager manager)
	{
		Clear();
		if (manager == null)
		{
			return;
		}
		List<BlueprintSelectionFeature> list = UtilityChargen.GetFeatureSelectionsByGroup(manager.Path, FeatureGroup, manager.PreviewUnit).ToList();
		if (!list.Any())
		{
			return;
		}
		foreach (BlueprintSelectionFeature item2 in list)
		{
			if (manager.GetSelectionState(manager.Path, item2, 0) is SelectionStateFeature item)
			{
				m_SelectionStates.Add(item);
			}
		}
		IOrderedEnumerable<CharGenAttributesItemVM> orderedEnumerable = from i in list.First().GetSelectionItems(manager.PreviewUnit, manager.Path).Select(CreateItem)
			orderby CharInfoAbilityScoresBlockVM.AbilitiesOrdered.IndexOf(i.StatType)
			select i;
		foreach (CharGenAttributesItemVM item3 in orderedEnumerable)
		{
			Items.Add(item3);
			m_StatRanks[item3.StatType] = 0;
		}
		m_ValuePerRank = orderedEnumerable.First().ValuePerRank;
		SelectionGroup.TrySelectFirstValidEntity();
		UpdateState();
	}

	protected override CharGenAttributesItemVM CreateItem(FeatureSelectionItem selectionItem, SelectionStateFeature selectionStateFeature, CharGenPhaseType phaseType)
	{
		return null;
	}

	private CharGenAttributesItemVM CreateItem(FeatureSelectionItem selectionItem)
	{
		return new CharGenAttributesItemVM(selectionItem, TryAdvanceStat, ShowTooltipForItem, PhaseType, m_CurrentPhase);
	}

	private void TryAdvanceStat(StatType statType, bool advance)
	{
		Game.Instance.GameCommandQueue.CharGenTryAdvanceStat(statType, advance);
	}

	private void UpdateState()
	{
		int num = AvailableSelectionStates.Count();
		m_AvailablePointsLeft.Value = num * m_ValuePerRank;
		LevelUpManager currentValue = CharGenContext.LevelUpManager.CurrentValue;
		if (currentValue == null)
		{
			return;
		}
		foreach (CharGenAttributesItemVM item in Items)
		{
			ModifiableValue stat = currentValue.TargetUnit.Stats.GetStat(item.StatType);
			ModifiableValue stat2 = currentValue.PreviewUnit.Stats.GetStat(item.StatType);
			item.SetStatValue(stat2.ModifiedValue);
			item.SetDiffValue(stat2.ModifiedValue - stat.ModifiedValue);
			int num2 = m_StatRanks[item.StatType];
			item.SetStatRanks(num2);
			item.SetCanAdvance(num > 0 && num2 < 2);
			item.SetCanRetreat(m_SelectionStates.Any(delegate(SelectionStateFeature state)
			{
				if (state.IsMade)
				{
					FeatureSelectionItem? selectionItem = state.SelectionItem;
					if (selectionItem.HasValue && selectionItem.GetValueOrDefault().Feature is BlueprintStatAdvancement blueprintStatAdvancement)
					{
						return blueprintStatAdvancement.Stat == item.StatType;
					}
				}
				return false;
			}));
			item.UpdateTooltip(stat2);
		}
		UpdatePreviewUnit();
		UpdateIsCompleted();
		m_OnUpdateState.Execute();
		SetupTooltipTemplate();
	}

	protected override TooltipBaseTemplate TooltipTemplate()
	{
		return base.SelectedItem.CurrentValue?.Tooltip.CurrentValue;
	}

	public void ShowTooltipForItem(CharGenAttributesItemVM itemVM)
	{
		ReactiveTooltipTemplate.Value = ((itemVM == null) ? TooltipTemplate() : itemVM.Tooltip.CurrentValue);
	}

	private void UpdateSkillsHighlight()
	{
		StatType? statType = base.SelectedItem.CurrentValue?.StatType;
		CharInfoSkillsBlock.Stats.ForEach(delegate(CharInfoStatVM i)
		{
			i.HighlightBySourceType(statType);
		});
	}

	private void UpdatePreviewUnit()
	{
		m_PreviewUnit.Value = CharGenContext.LevelUpManager.CurrentValue?.PreviewUnit;
	}
}
