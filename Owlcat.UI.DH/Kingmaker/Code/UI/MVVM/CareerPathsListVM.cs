using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.View;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Levelup.Selections.Prerequisites;
using Kingmaker.UnitLogic.Progression.Paths;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CareerPathsListVM : ViewModel, ICareerPathHoverHandler, ISubscriber
{
	public readonly CareerPathTier Tier;

	private readonly ReactiveProperty<bool> m_IsActive = new ReactiveProperty<bool>();

	public readonly List<CareerPathVM> CareerPathVMs;

	public readonly List<CareerPathVM> UnlockingCareersVMs = new List<CareerPathVM>();

	private readonly ReactiveProperty<CareerPathVM> m_PreviewCareer = new ReactiveProperty<CareerPathVM>();

	private readonly ReactiveProperty<CareerPathVM> m_SelectedCareer = new ReactiveProperty<CareerPathVM>();

	private readonly ReactiveProperty<bool> m_IsUnlocked = new ReactiveProperty<bool>();

	private readonly ReadOnlyReactiveProperty<CareerPathVM> m_PreselectedCareer;

	private CareerPathVM m_HoveredCareer;

	public ReadOnlyReactiveProperty<bool> IsActive => m_IsActive;

	public ReadOnlyReactiveProperty<CareerPathVM> PreviewCareer => m_PreviewCareer;

	public ReadOnlyReactiveProperty<CareerPathVM> SelectedCareer => m_SelectedCareer;

	public ReadOnlyReactiveProperty<bool> IsUnlocked => m_IsUnlocked;

	public CareerPathsListVM(CareerPathTier tier, List<CareerPathVM> careers, ReadOnlyReactiveProperty<CareerPathVM> preselectedCareer, List<BlueprintCareerPath> choosedCareers)
	{
		CareerPathsListVM careerPathsListVM = this;
		Tier = tier;
		m_PreselectedCareer = preselectedCareer;
		CareerPathVMs = careers;
		BuildCareersPrerequisites();
		BuildUnlockingCareers();
		List<CareerPathVM> list = CareerPathVMs.Where((CareerPathVM vm) => vm.IsInProgress || vm.IsFinished || vm.PrerequisiteCareerPaths.FindIndex(choosedCareers.Contains) >= 0).ToList();
		if (list.Any())
		{
			CareerPathVMs = list;
		}
		m_PreselectedCareer.Subscribe(delegate
		{
			careerPathsListVM.UpdatePreviewCareer();
		}).AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
	}

	protected override void OnDispose()
	{
		CareerPathVMs.Clear();
	}

	private void BuildCareersPrerequisites()
	{
		foreach (CareerPathVM careerPathVM in CareerPathVMs)
		{
			List<BlueprintCareerPath> list = new List<BlueprintCareerPath>();
			CalculatedPrerequisite prerequisite = careerPathVM.Prerequisite;
			GetPrerequisitesCareers(prerequisite, list);
			careerPathVM.PrerequisiteCareerPaths = list;
		}
	}

	private void BuildUnlockingCareers()
	{
		UnlockingCareersVMs.Add(CareerPathVMs.ElementAt(0));
	}

	private void GetPrerequisitesCareers(CalculatedPrerequisite prerequisite, List<BlueprintCareerPath> result)
	{
		if (prerequisite is CalculatedPrerequisiteFact calculatedPrerequisiteFact)
		{
			if (calculatedPrerequisiteFact.Fact is BlueprintCareerPath)
			{
				result.Add(calculatedPrerequisiteFact.Fact as BlueprintCareerPath);
			}
		}
		else
		{
			if (!(prerequisite is CalculatedPrerequisiteComposite { Prerequisites: var prerequisites }))
			{
				return;
			}
			foreach (CalculatedPrerequisite item in prerequisites)
			{
				if (item is CalculatedPrerequisiteFact calculatedPrerequisiteFact2 && calculatedPrerequisiteFact2.Fact is BlueprintCareerPath)
				{
					result.Add(calculatedPrerequisiteFact2.Fact as BlueprintCareerPath);
				}
				else
				{
					GetPrerequisitesCareers(item, result);
				}
			}
		}
	}

	public void UpdateState()
	{
		foreach (CareerPathVM careerPathVM in CareerPathVMs)
		{
			careerPathVM.UpdateState(updateRanks: false);
			if (careerPathVM.IsAvailableToUpgrade || careerPathVM.IsSelectedAndInProgress)
			{
				m_IsActive.Value = true;
			}
			if (careerPathVM.IsSelectedAndInProgress || careerPathVM.IsFinished)
			{
				careerPathVM.UpdateState(updateRanks: true);
				m_SelectedCareer.Value = careerPathVM;
			}
		}
		m_IsUnlocked.Value = CareerPathVMs.Any((CareerPathVM path) => path.IsUnlocked || path.CanShowToAnotherCoopPlayer());
	}

	public void HandleHoverStart(BlueprintCareerPath careerPath)
	{
		CareerPathVM hoveredCareer = CareerPathVMs.FirstOrDefault((CareerPathVM i) => i.CareerPath == careerPath);
		m_HoveredCareer = hoveredCareer;
		UpdatePreviewCareer();
	}

	public void HandleHoverStop()
	{
		m_HoveredCareer = null;
		UpdatePreviewCareer();
	}

	private void UpdatePreviewCareer()
	{
		CareerPathVM careerPathVM = CareerPathVMs.FirstOrDefault((CareerPathVM i) => i == m_PreselectedCareer.CurrentValue);
		m_PreviewCareer.Value = m_HoveredCareer ?? careerPathVM;
	}

	public int GetLevelToUnlock()
	{
		int num = 1;
		int i;
		for (i = 1; i <= (int)Tier; i++)
		{
			BlueprintCareerPath blueprintCareerPath = ProgressionRoot.Instance.CareerPaths.FirstOrDefault((BlueprintCareerPath path) => path.Tier == (CareerPathTier)(i - 1));
			if (blueprintCareerPath != null)
			{
				num += blueprintCareerPath.Ranks;
			}
		}
		return num;
	}
}
