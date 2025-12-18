using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class RankEntryDescriptionVM : VirtualListElementVMBase
{
	public readonly string Description;

	public RankEntryDescriptionVM(string description)
	{
		Description = description;
	}

	protected override void DisposeImplementation()
	{
	}
}
