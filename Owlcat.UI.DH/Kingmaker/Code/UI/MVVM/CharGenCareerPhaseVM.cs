using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.GameCommands;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Levelup.Components;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.UnitLogic.Progression.Paths;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenCareerPhaseVM : CharGenPhaseBaseVM, ICareerPathHoverHandler, ISubscriber, ICharGenCareerPathHandler
{
	private readonly Dictionary<BlueprintPath, FeatureSelectionItem> m_CareerPathToSelectionItem = new Dictionary<BlueprintPath, FeatureSelectionItem>();

	private readonly ReactiveProperty<LevelUpManager> m_LevelUpManager = new ReactiveProperty<LevelUpManager>();

	private readonly ReactiveProperty<TooltipBaseTemplate> m_ReactiveTooltipTemplate = new ReactiveProperty<TooltipBaseTemplate>();

	private SelectionStateFeature m_SelectionStateFeature;

	private bool m_Subscribed;

	public UnitProgressionVM UnitProgressionVM;

	public CharGenCareerPhaseVM(CharGenContext charGenContext)
		: base(charGenContext, CharGenPhaseType.Career)
	{
		base.HasPantograph = false;
		SetShowVisualSettings(show: false);
		SetPhaseHint(UIStrings.Instance.CharGen.SelectDoctrineHint);
		CreateTooltipSystem();
		AddDisposable(EventBus.Subscribe(this));
	}

	public void HandleHoverStart(BlueprintCareerPath careerPath)
	{
		if (UnitProgressionVM == null)
		{
			return;
		}
		bool flag = UnitProgressionVM.AllCareerPaths.FirstOrDefault((CareerPathVM c) => c.CareerPath == careerPath)?.IsUnlocked ?? false;
		foreach (CareerPathVM allCareerPath in UnitProgressionVM.AllCareerPaths)
		{
			if (allCareerPath.PrerequisiteCareerPaths.Contains(careerPath))
			{
				allCareerPath.SetItemState(CareerItemState.Highlighted);
			}
			else if (flag)
			{
				if (allCareerPath.CareerPath.Tier == careerPath.Tier && allCareerPath.CareerPath != careerPath)
				{
					allCareerPath.SetItemState(CareerItemState.Darkened);
				}
				else
				{
					allCareerPath.SetItemState((allCareerPath.IsUnlocked || allCareerPath.CanShowToAnotherCoopPlayer()) ? CareerItemState.Unlocked : CareerItemState.Locked);
				}
			}
		}
	}

	public void HandleHoverStop()
	{
		SetupDefaultItemsState();
	}

	void ICharGenCareerPathHandler.HandleCareerPath(BlueprintCareerPath careerPath)
	{
		CareerPathVM careerPathVM = UnitProgressionVM.AllCareerPaths.FirstOrDefault((CareerPathVM vm) => vm.CareerPath == careerPath);
		if (careerPathVM == null)
		{
			PFLog.UI.Error("CareerPathVM not found " + careerPath.AssetGuid);
			return;
		}
		UnitProgressionVM.SetCareerPath(careerPathVM);
		if (m_CareerPathToSelectionItem.TryGetValue(careerPathVM.CareerPath, out var value))
		{
			m_SelectionStateFeature.ClearSelection();
			m_SelectionStateFeature.Select(value);
		}
		UpdateIsCompleted();
		SetPhaseHint(string.Empty);
		UpdateTooltipTemplate();
	}

	protected override bool CheckIsCompleted()
	{
		SelectionStateFeature selectionStateFeature = m_SelectionStateFeature;
		if (selectionStateFeature != null && selectionStateFeature.IsMade)
		{
			return selectionStateFeature.IsValid;
		}
		return false;
	}

	protected override void OnBeginDetailedView()
	{
		if (!m_Subscribed)
		{
			UnitProgressionVM = AddDisposableAndReturn(new UnitProgressionVM(CharGenContext.CurrentUnit, m_LevelUpManager, UnitProgressionMode.CharGen));
			AddDisposable(CharGenContext.LevelUpManager.Subscribe(HandleLevelUpManager));
			AddDisposable(UnitProgressionVM.PreselectedCareer.Subscribe(HandleSelectCareer));
			m_Subscribed = true;
		}
		UnitProgressionVM.UpdateSelectionsFromUnit(CharGenContext.LevelUpManager.CurrentValue.PreviewUnit);
		SetupDefaultItemsState();
	}

	protected override void DisposeImplementation()
	{
		base.DisposeImplementation();
		m_CareerPathToSelectionItem.Clear();
	}

	private void CreateTooltipSystem()
	{
		AddDisposable(InfoVM = new InfoSectionVM());
		AddDisposable(SecondaryInfoVM = new InfoSectionVM());
		AddDisposable(m_ReactiveTooltipTemplate.Subscribe(InfoVM.SetTemplate));
	}

	public void UpdateTooltipTemplate(BlueprintCareerPath careerPath = null)
	{
		if (careerPath == null)
		{
			m_ReactiveTooltipTemplate.Value = GetTooltipTemplate();
			return;
		}
		CareerPathVM careerPathVM = UnitProgressionVM.AllCareerPaths.FirstOrDefault((CareerPathVM vm) => vm.CareerPath == careerPath);
		m_ReactiveTooltipTemplate.Value = ((careerPathVM != null) ? ((TooltipBaseTemplate)new TooltipTemplateCareer(careerPathVM)) : ((TooltipBaseTemplate)new TooltipTemplateCharGenDoctrinesDesc()));
	}

	private TooltipBaseTemplate GetTooltipTemplate()
	{
		CareerPathVM currentValue = UnitProgressionVM.PreselectedCareer.CurrentValue;
		if (currentValue == null)
		{
			return new TooltipTemplateCharGenDoctrinesDesc();
		}
		return new TooltipTemplateCareer(currentValue);
	}

	private void HandleSelectCareer(CareerPathVM careerPathVM)
	{
		if (careerPathVM != null && m_SelectionStateFeature != null)
		{
			Game.Instance.GameCommandQueue.CharGenSelectCareerPath(careerPathVM.CareerPath);
		}
	}

	private void HandleLevelUpManager(LevelUpManager manager)
	{
		if (manager == null)
		{
			return;
		}
		IEnumerable<BlueprintSelectionFeature> featureSelectionsByGroup = UtilityChargen.GetFeatureSelectionsByGroup(manager.Path, FeatureGroup.ChargenCareerPath, manager.PreviewUnit);
		if (!featureSelectionsByGroup.Any())
		{
			return;
		}
		BlueprintSelectionFeature blueprintSelectionFeature = featureSelectionsByGroup.First();
		foreach (FeatureSelectionItem selectionItem in blueprintSelectionFeature.GetSelectionItems(manager.PreviewUnit, manager.Path))
		{
			ApplyCareerPath component = selectionItem.Feature.GetComponent<ApplyCareerPath>();
			if (component != null)
			{
				m_CareerPathToSelectionItem[component.CareerPath] = selectionItem;
			}
		}
		m_SelectionStateFeature = manager.GetSelectionState(manager.Path, blueprintSelectionFeature, 0) as SelectionStateFeature;
	}

	private void SetupDefaultItemsState()
	{
		if (UnitProgressionVM == null)
		{
			return;
		}
		CareerPathVM careerPathVM = UnitProgressionVM.AllCareerPaths.FirstOrDefault((CareerPathVM c) => c.IsSelected.Value);
		foreach (CareerPathVM allCareerPath in UnitProgressionVM.AllCareerPaths)
		{
			if (careerPathVM != null && allCareerPath.CareerPath == careerPathVM.CareerPath)
			{
				allCareerPath.SetItemState(CareerItemState.Selected);
			}
			else if (careerPathVM == null && allCareerPath.CareerPath.Tier == CareerPathTier.One)
			{
				allCareerPath.SetItemState(CareerItemState.Ready);
			}
			else
			{
				allCareerPath.SetItemState((allCareerPath.IsUnlocked || allCareerPath.CanShowToAnotherCoopPlayer()) ? CareerItemState.Unlocked : CareerItemState.Locked);
			}
		}
	}
}
