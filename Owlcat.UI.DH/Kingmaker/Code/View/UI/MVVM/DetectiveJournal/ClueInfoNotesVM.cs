using System.Collections.Generic;
using System.Linq;
using Kingmaker.Framework.DetectiveSystem;
using ObservableCollections;
using Owlcat.Fmw.Blueprints;
using Owlcat.UI;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class ClueInfoNotesVM : ViewModel
{
	public readonly ObservableList<BlueprintClueNote> Notes = new ObservableList<BlueprintClueNote>();

	private readonly BlueprintClue m_BlueprintClue;

	public ClueInfoNotesVM(BlueprintClue clue)
	{
		m_BlueprintClue = clue;
		UpdateData();
	}

	public void UpdateData()
	{
		IEnumerable<BlueprintClueNote> items = from n in m_BlueprintClue.Notes
			where n.Blueprint.IsVisible
			select n.Blueprint;
		Notes.Clear();
		Notes.AddRange(items);
	}
}
