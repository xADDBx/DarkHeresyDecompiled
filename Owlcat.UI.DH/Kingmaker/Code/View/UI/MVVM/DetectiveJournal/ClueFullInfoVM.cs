using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using ObservableCollections;
using Owlcat.Fmw.Blueprints;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class ClueFullInfoVM : ViewModel, IClueDataChangedHandler, ISubscriber
{
	public readonly BlueprintClue BlueprintClue;

	public readonly ObservableList<AddendumInfoVM> AddendumVMs = new ObservableList<AddendumInfoVM>();

	private readonly ReactiveProperty<DetectiveStudyVM> m_StudyVM = new ReactiveProperty<DetectiveStudyVM>();

	private readonly ReactiveProperty<NewStudyVM> m_StudyToMake = new ReactiveProperty<NewStudyVM>();

	private readonly ReactiveCommand<Unit> m_AddendumsRefreshed = new ReactiveCommand<Unit>();

	private readonly Action m_CloseAction;

	private readonly Action<BlueprintClue> m_OpenClueInfo;

	public ReadOnlyReactiveProperty<DetectiveStudyVM> StudyVM => m_StudyVM;

	public ReadOnlyReactiveProperty<NewStudyVM> StudyToMake => m_StudyToMake;

	public ClueUIData UIData => BlueprintClue.GetUIData();

	public ClueFullInfoVM(BlueprintClue blueprintClue, Action<BlueprintClue> openClueInfo, Action close)
	{
		BlueprintClue = blueprintClue;
		m_OpenClueInfo = openClueInfo;
		m_CloseAction = close;
		List<BlueprintClueAddendum> list = (blueprintClue.ParentCase.Blueprint.IsUnknown() ? new List<BlueprintClueAddendum>() : UIUtilityDetective.Detective.GetOpenedAddendumsFor(blueprintClue).ToList());
		List<BlueprintClueAddendum> list2 = list.RemoveLowerTier();
		list.Except(list2).ToList().ForEach(UIUtilityDetective.ExaminedDetectiveData.ExaminedAddendums.AddExaminedEntityIfNeeded);
		list = list2;
		List<BlueprintClueStudy> fakeStudies = GetFakeStudies();
		CreateInfo(list, fakeStudies);
		UpdateCanBeExplored();
		EventBus.Subscribe(this).AddTo(this);
	}

	protected override void OnDispose()
	{
		base.OnDispose();
		AddendumVMs.ForEach(delegate(AddendumInfoVM a)
		{
			a.Dispose();
		});
		AddendumVMs.Clear();
	}

	public void Close()
	{
		AddendumVMs.ForEach(delegate(AddendumInfoVM a)
		{
			a.MarkAsViewed();
		});
		m_CloseAction?.Invoke();
	}

	private void CreateInfo(List<BlueprintClueAddendum> addendums, List<BlueprintClueStudy> studies)
	{
		AddendumVMs.AddRange(addendums.Select((BlueprintClueAddendum a) => new AddendumInfoVM(a, m_OpenClueInfo)).ToList());
		studies = studies.OrderBy(delegate(BlueprintClueStudy s)
		{
			if (!Game.Instance.Player.UISettings.DetectiveSystemData.StudyIds.TryGetValue(s, out var value))
			{
				Game.Instance.Player.UISettings.DetectiveSystemData.StudyIds.Add(s, AddendumVMs.Count);
				return AddendumVMs.Count;
			}
			return value;
		}).ToList();
		foreach (BlueprintClueStudy study in studies)
		{
			int valueOrDefault = Game.Instance.Player.UISettings.DetectiveSystemData.StudyIds.GetValueOrDefault(study, 0);
			AddendumVMs.Insert(valueOrDefault, new AddendumInfoVM(study, m_OpenClueInfo));
		}
	}

	private List<BlueprintClueStudy> GetFakeStudies()
	{
		List<BlueprintClueStudy> list = (from s in BlueprintClue.Studies.Dereference()
			where Game.Instance.DetectiveSystem.IsStudied(s)
			select s).ToList();
		list.RemoveAll((BlueprintClueStudy s) => !UIUtilityDetective.HasFakeStudies(s));
		return list;
	}

	public void RefreshDataFor(BlueprintClue clue)
	{
		if (BlueprintClue != clue)
		{
			return;
		}
		UpdateCanBeExplored();
		List<BlueprintClueAddendum> list = Game.Instance.DetectiveSystem.GetOpenedAddendumsFor(BlueprintClue).ToList().RemoveLowerTier();
		List<BlueprintClueStudy> fakeStudies = GetFakeStudies();
		list.RemoveAll((BlueprintClueAddendum a) => AddendumVMs.Any((AddendumInfoVM vm) => vm.Info is AddendumInfo addendumInfo && addendumInfo.BlueprintAddendum == a));
		fakeStudies.RemoveAll((BlueprintClueStudy s) => AddendumVMs.Any((AddendumInfoVM vm) => vm.Info is StudyInfo studyInfo && studyInfo.BlueprintStudy == s));
		CreateInfo(list, fakeStudies);
		if (list.Any())
		{
			m_AddendumsRefreshed?.Execute();
		}
	}

	private void CloseStudyPanel()
	{
		StudyToMake.CurrentValue?.Dispose();
		m_StudyToMake.Value = null;
		RefreshDataFor(BlueprintClue);
	}

	private void UpdateCanBeExplored()
	{
		if (!BlueprintClue.ParentCase.Blueprint.IsUnknown())
		{
			if (!Game.Instance.DetectiveSystem.IsAvailableForStudy(BlueprintClue) || m_StudyToMake.Value != null)
			{
				m_StudyVM.Value?.Dispose();
				m_StudyVM.Value = null;
			}
			else
			{
				Queue<StudyGroup> queue = UIUtilityDetective.CreateStudyGroups(BlueprintClue);
				m_StudyVM.Value = ((queue != null && queue.Count > 0) ? new DetectiveStudyVM(queue.First(), OpenStudy).AddTo(this) : null);
			}
		}
		void OpenStudy()
		{
			m_StudyToMake.Value = new NewStudyVM(BlueprintClue, AddendumVMs, CloseStudyPanel).AddTo(this);
			m_StudyToMake.Value.SetupNextStudy();
			m_StudyVM.Value = null;
		}
	}
}
