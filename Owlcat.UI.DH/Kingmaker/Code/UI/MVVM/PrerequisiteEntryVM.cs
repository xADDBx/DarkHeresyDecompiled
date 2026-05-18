using System.Collections.Generic;
using System.Linq;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class PrerequisiteEntryVM : ViewModel
{
	public readonly bool IsGroup;

	public readonly bool IsOrComposition;

	public readonly List<PrerequisiteEntryVM> Prerequisites;

	public readonly TextValueElement Info;

	public readonly bool Done;

	public readonly bool Inverted;

	public PrerequisiteEntryVM(TextValueElement info, bool done, bool inverted)
	{
		IsGroup = false;
		Info = info;
		Done = done;
		Inverted = inverted;
	}

	public PrerequisiteEntryVM(List<PrerequisiteEntryVM> prerequisites, bool isOrComposition)
	{
		IsGroup = true;
		IsOrComposition = isOrComposition;
		Prerequisites = prerequisites;
		Info = new TextValueElement(string.Join("\n", prerequisites.Select((PrerequisiteEntryVM p) => p.Info.Text)));
	}
}
