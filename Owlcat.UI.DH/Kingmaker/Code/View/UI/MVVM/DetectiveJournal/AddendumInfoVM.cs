using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.Localization;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class AddendumInfoVM : ViewModel
{
	public readonly InfoWrapper Info;

	public readonly CaseEntitySourceVM SourceVM;

	private readonly ReactiveProperty<LocalizedString> m_Description = new ReactiveProperty<LocalizedString>();

	private readonly ReactiveProperty<AddendumState> m_AddendumState = new ReactiveProperty<AddendumState>();

	private readonly ReactiveProperty<BlueprintClue> m_LinkedClue = new ReactiveProperty<BlueprintClue>();

	private readonly Action m_GoToLinkedClue;

	public ReadOnlyReactiveProperty<BlueprintClue> LinkedClue => m_LinkedClue;

	public ReadOnlyReactiveProperty<LocalizedString> Description => m_Description;

	public ReadOnlyReactiveProperty<AddendumState> AddendumState => m_AddendumState;

	private AddendumInfoVM(Action<BlueprintClue> goToLinkedClue)
	{
		AddendumInfoVM addendumInfoVM = this;
		m_GoToLinkedClue = delegate
		{
			goToLinkedClue?.Invoke(addendumInfoVM.LinkedClue.CurrentValue);
		};
	}

	public AddendumInfoVM(BlueprintClueAddendum blueprintAddendum, Action<BlueprintClue> goToLinkedClue)
		: this(goToLinkedClue)
	{
		Info = new AddendumInfo(blueprintAddendum);
		BlueprintScriptableObject source = Game.Instance.DetectiveSystem.GetSource(blueprintAddendum);
		SourceVM = new CaseEntitySourceVM(blueprintAddendum.ParentClue, source);
		m_LinkedClue.Value = ((source is BlueprintClueStudy blueprintClueStudy && blueprintClueStudy.ParentClue != blueprintAddendum.ParentClue) ? blueprintClueStudy.ParentClue : null);
		m_Description.Value = UIUtilityDetective.GetAddendumDescriptionWithOverride(blueprintAddendum);
		UpdateAddendumState();
		AddendumState.AddTo(this);
	}

	public AddendumInfoVM(BlueprintClueStudy study, Action<BlueprintClue> goToLinkedClue)
		: this(goToLinkedClue)
	{
		Info = new StudyInfo(study);
		SourceVM = new CaseEntitySourceVM(study.ParentClue, study);
		m_LinkedClue.Value = UIUtilityDetective.GetStudyLink(study, out var studyType);
		ReactiveProperty<LocalizedString> description = m_Description;
		description.Value = studyType switch
		{
			FakeStudyType.Unknown => UIConfig.Instance.EmptyString, 
			FakeStudyType.Addendum => UIStrings.Instance.DetectiveJournal.AddendumsAddedToOtherClue, 
			FakeStudyType.Clue => UIStrings.Instance.DetectiveJournal.ClueAddedFromStudy, 
			FakeStudyType.Conclusion => UIConfig.Instance.EmptyString, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		UpdateAddendumState();
		AddendumState.AddTo(this);
	}

	protected override void OnDispose()
	{
		Info.MarkAsViewed();
	}

	private void UpdateAddendumState()
	{
		m_AddendumState.Value = Info.GetAddendumState();
	}

	public void MarkAsViewed()
	{
		Info.MarkAsViewed();
		UpdateAddendumState();
	}

	public void GoToLinkedClue()
	{
		m_GoToLinkedClue?.Invoke();
	}
}
