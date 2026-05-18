using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.GameCommands;
using Owlcat.Fmw.Blueprints;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class StudyGroup
{
	public readonly string StudyName;

	public readonly string BarkText;

	public readonly BlueprintUnit Companion;

	public readonly List<AddendumInfoVM> NewAddendums = new List<AddendumInfoVM>();

	private readonly List<BlueprintClueStudy> m_Study = new List<BlueprintClueStudy>();

	public IReadOnlyList<BlueprintClueStudy> Studies => m_Study;

	public StudyGroup(BlueprintClueStudy study)
	{
		AddStudy(study);
		StudyName = study.Name.Text;
		BarkText = study.StudyBark.Text;
		Companion = study.StudyCompanion.MaybeBlueprint;
	}

	public void AddStudy(BlueprintClueStudy study)
	{
		m_Study.Add(study);
		List<BlueprintClueAddendum> list = (from a in study.GiveItems.Dereference()
			where a is BlueprintClueAddendum blueprintClueAddendum && blueprintClueAddendum.ParentClue == study.ParentClue
			select a as BlueprintClueAddendum).ToList().RemoveLowerTier();
		list.RemoveAll((BlueprintClueAddendum a) => Game.Instance.DetectiveSystem.HasClueAddendumExcludingHidden(a));
		IEnumerable<AddendumInfoVM> collection = list.Select((BlueprintClueAddendum a) => new AddendumInfoVM(a, null));
		NewAddendums.AddRange(collection);
		if (UIUtilityDetective.HasFakeStudies(study))
		{
			NewAddendums.Add(new AddendumInfoVM(study, null));
		}
	}

	public void MakeStudy()
	{
		Game.Instance.GameCommandQueue.StudyClues(m_Study);
		Game.Instance.Controllers.VoiceOverController.PlayStudyVoiceOver(m_Study.FirstOrDefault());
	}
}
