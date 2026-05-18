using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class TextureAndColorSelectorItemVM : ViewModel
{
	private readonly Subject<Unit> m_OnRemoveRequested = new Subject<Unit>();

	public int Index { get; private set; }

	public TextureSelectorVM TexturesSelectorVm { get; private set; }

	public TextureSelectorVM ColorsSelectorVm { get; private set; }

	public Observable<Unit> OnRemoveRequested => m_OnRemoveRequested;

	public bool CanRemove => Index > 0;

	public TextureAndColorSelectorItemVM(int index, TextureSelectorVM texturesSelectorVm, TextureSelectorVM colorsSelectorVm)
	{
		Index = index;
		TexturesSelectorVm = texturesSelectorVm;
		ColorsSelectorVm = colorsSelectorVm;
	}

	public void RequestRemove()
	{
		m_OnRemoveRequested.OnNext(Unit.Default);
	}

	protected override void OnDispose()
	{
		m_OnRemoveRequested.Dispose();
		base.OnDispose();
	}
}
