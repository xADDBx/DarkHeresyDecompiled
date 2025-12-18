using Kingmaker.AreaLogic.Cutscenes.Commands;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class InterchapterContext : ViewModel
{
	private readonly ReactiveProperty<InterchapterVM> m_InterchapterVM = new ReactiveProperty<InterchapterVM>();

	public ReadOnlyReactiveProperty<InterchapterVM> InterchapterVM => m_InterchapterVM;

	public InterchapterContext(ReactiveProperty<InterchapterVM> interchapterVM)
	{
		m_InterchapterVM = interchapterVM;
		GameUIState.Instance.ActiveInterchapter.Subscribe(StartInterchapter).AddTo(this);
	}

	protected override void OnDispose()
	{
		DisposeInterchapter();
	}

	public void StartInterchapter(InterchapterData data)
	{
		if (data == null)
		{
			DisposeInterchapter();
		}
		else
		{
			m_InterchapterVM.Value = new InterchapterVM(data);
		}
	}

	private void DisposeInterchapter()
	{
		InterchapterVM.CurrentValue?.Dispose();
		m_InterchapterVM.Value = null;
	}
}
