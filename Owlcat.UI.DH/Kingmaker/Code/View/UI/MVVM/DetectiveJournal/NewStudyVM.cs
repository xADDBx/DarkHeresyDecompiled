using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Events;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.DotNetExtensions;
using ObservableCollections;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class NewStudyVM : ViewModel, INewInfoHandler, ISubscriber, IClueStudyHandler
{
	private readonly ReactiveProperty<StudyGroup> m_CurrentGroup = new ReactiveProperty<StudyGroup>();

	private readonly ReactiveProperty<DetectiveStudyVM> m_NextStudy = new ReactiveProperty<DetectiveStudyVM>();

	private readonly Queue<StudyGroup> m_AvailableGroups;

	private readonly ObservableList<AddendumInfoVM> m_Addendums;

	private readonly Action m_Close;

	public readonly ObservableList<AddendumTitleVM> NewAddendums = new ObservableList<AddendumTitleVM>();

	public ReadOnlyReactiveProperty<StudyGroup> CurrentGroup => m_CurrentGroup;

	public ReadOnlyReactiveProperty<DetectiveStudyVM> NextStudy => m_NextStudy;

	public bool HasNextGroup => m_AvailableGroups.Any();

	public NewStudyVM(BlueprintClue blueprintClue, ObservableList<AddendumInfoVM> addendums, Action close)
	{
		m_Addendums = addendums;
		m_Close = close;
		m_Close = (Action)Delegate.Combine(m_Close, (Action)delegate
		{
			Game.Instance.Controllers.VoiceOverController.StopStudyVoiceOver();
		});
		m_AvailableGroups = UIUtilityDetective.CreateStudyGroups(blueprintClue);
		EventBus.Subscribe(this).AddTo(this);
	}

	public void SetupNextStudy()
	{
		if (m_AvailableGroups.TryDequeue(out var result))
		{
			m_CurrentGroup.Value = result;
			result.MakeStudy();
		}
	}

	private void TrySetNextStudy()
	{
		if (m_AvailableGroups.Any())
		{
			m_NextStudy.Value = new DetectiveStudyVM(m_AvailableGroups.First(), SetupNextStudy);
		}
	}

	public void Close()
	{
		if (m_Close != null)
		{
			m_Close();
			UISounds.Instance.Play(ServiceWindowsSounds.Instance.DetectiveJournal.ClueResearchComplete, isButton: true);
		}
	}

	void INewInfoHandler.HandleMarkAsViewed(InfoWrapper info)
	{
		NewAddendums.FirstOrDefault((AddendumTitleVM a) => a.Info.Equals(info))?.MarkAsViewed();
	}

	void IClueStudyHandler.HandleClueStudied()
	{
		StudyGroup value = m_CurrentGroup.Value;
		value.NewAddendums.ForEach(delegate(AddendumInfoVM a)
		{
			a.Info.RefreshData();
		});
		IEnumerable<AddendumInfoVM> enumerable = value.NewAddendums.Where((AddendumInfoVM a) => a.Info is StudyInfo);
		Dictionary<BlueprintClueStudy, int> studyIds = Game.Instance.Player.UISettings.DetectiveSystemData.StudyIds;
		foreach (AddendumInfoVM study in enumerable)
		{
			int value2 = m_Addendums.FindIndex((AddendumInfoVM add) => add.Info.Equals(study.Info));
			studyIds.TryAdd((study.Info as StudyInfo)?.BlueprintStudy, value2);
		}
		NewAddendums.Clear();
		IOrderedEnumerable<AddendumTitleVM> items = from a in CurrentGroup.CurrentValue.NewAddendums
			select new AddendumTitleVM(a.Info, GetId(a.Info)) into a
			orderby a.InfoId
			select a;
		NewAddendums.AddRange(items);
		TrySetNextStudy();
	}

	private int GetId(InfoWrapper info)
	{
		return m_Addendums.FindIndex((AddendumInfoVM add) => add.Info.Equals(info)) + 1;
	}
}
