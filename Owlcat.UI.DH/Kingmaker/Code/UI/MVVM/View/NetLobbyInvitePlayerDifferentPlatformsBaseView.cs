using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class NetLobbyInvitePlayerDifferentPlatformsBaseView : View<NetLobbyInvitePlayerDifferentPlatformsVM>
{
	public virtual void Initialize()
	{
	}

	protected override void OnBind()
	{
		base.gameObject.SetActive(value: true);
	}

	protected override void OnUnbind()
	{
		base.gameObject.SetActive(value: false);
	}
}
