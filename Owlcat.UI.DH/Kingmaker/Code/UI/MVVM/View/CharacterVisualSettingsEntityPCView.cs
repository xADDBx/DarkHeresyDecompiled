using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharacterVisualSettingsEntityPCView : CharacterVisualSettingsEntityView
{
	protected override void OnBind()
	{
		base.OnBind();
		m_Button.OnLeftClickAsObservable().Subscribe(base.ViewModel.Switch).AddTo(this);
	}
}
