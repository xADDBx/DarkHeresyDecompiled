using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class ContextMenuEntityPCView : ContextMenuEntityView
{
	protected override void OnBind()
	{
		base.OnBind();
		if (m_ButtonFx != null)
		{
			m_Button.OnHoverAsObservable().Subscribe(m_ButtonFx.DoHovered).AddTo(this);
		}
	}
}
