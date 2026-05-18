using System.Collections.Generic;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Framework.DetectiveSystem;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class NewAddendumsVM : ViewModel
{
	public readonly BlueprintClue Clue;

	public List<BlueprintClueAddendum> NewAddendums = new List<BlueprintClueAddendum>();

	private readonly ReactiveCommand<Unit> m_RefreshAddendumsCommand = new ReactiveCommand<Unit>();

	public Observable<Unit> RefreshAddendumsCommand => m_RefreshAddendumsCommand;

	public NewAddendumsVM(BlueprintClue clue)
	{
		Clue = clue;
		RefreshAddendums();
	}

	public void RefreshAddendums()
	{
		NewAddendums.Clear();
		NewAddendums.AddRange(UIUtilityDetective.GetNewAddendums(Clue));
		m_RefreshAddendumsCommand?.Execute(Unit.Default);
	}
}
