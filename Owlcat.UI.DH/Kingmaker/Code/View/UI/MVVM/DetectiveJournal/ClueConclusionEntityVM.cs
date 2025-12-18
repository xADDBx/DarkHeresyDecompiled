using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.Localization;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class ClueConclusionEntityVM : ViewModel
{
	public readonly BlueprintCaseItem BlueprintCaseItem;

	public readonly LocalizedString Name;

	public readonly LocalizedString Description;

	public readonly Sprite Icon;

	public ClueConclusionEntityVM(BlueprintClue clue)
	{
		BlueprintCaseItem = clue;
		Name = clue.Name;
		Icon = clue.Icon;
		Description = clue.Description;
	}

	public ClueConclusionEntityVM(BlueprintClueAddendum addendum)
	{
		BlueprintCaseItem = addendum;
		Name = addendum.ParentClue.Blueprint.Name;
		Icon = addendum.ParentClue.Blueprint.Icon;
		Description = addendum.Description;
	}
}
