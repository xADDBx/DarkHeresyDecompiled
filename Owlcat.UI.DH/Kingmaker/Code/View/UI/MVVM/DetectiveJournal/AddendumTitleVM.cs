using Owlcat.UI;
using R3;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class AddendumTitleVM : ViewModel
{
	public readonly InfoWrapper Info;

	public readonly int InfoId;

	private readonly ReactiveProperty<bool> m_IsViewed = new ReactiveProperty<bool>();

	public ReadOnlyReactiveProperty<bool> IsViewed => m_IsViewed;

	public AddendumTitleVM(InfoWrapper info, int infoId)
	{
		Info = info;
		InfoId = infoId;
	}

	public void MarkAsViewed()
	{
		m_IsViewed.Value = true;
	}
}
