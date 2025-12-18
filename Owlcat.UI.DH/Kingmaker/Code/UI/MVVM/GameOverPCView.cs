using Kingmaker.UI.InputSystems;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

[ViewFactoryPolicy(ViewFactoryPolicyFlag.DontReparent, null)]
public class GameOverPCView : GameOverView
{
	protected override void OnBind()
	{
		base.OnBind();
		EscHotkeyManager.Instance.Subscribe(delegate
		{
		}).AddTo(this);
	}
}
