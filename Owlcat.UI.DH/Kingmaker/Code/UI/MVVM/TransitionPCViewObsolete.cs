using Kingmaker.UI.InputSystems;
using R3;
using R3.Triggers;

namespace Kingmaker.Code.UI.MVVM;

public class TransitionPCViewObsolete : TransitionBaseView_OBSOLETE
{
	protected override void OnBind()
	{
		base.OnBind();
		m_CurrentPartObsolete.Close.OnPointerClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.Close();
		}).AddTo(this);
		EscHotkeyManager.Instance.Subscribe(delegate
		{
			base.ViewModel.Close();
		}).AddTo(this);
	}
}
