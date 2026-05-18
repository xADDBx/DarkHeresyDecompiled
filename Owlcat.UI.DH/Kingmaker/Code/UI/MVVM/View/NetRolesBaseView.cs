using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.Bridge.Root;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM.View;

public class NetRolesBaseView : View<NetRolesVM>, IInitializable
{
	public virtual void Initialize()
	{
		base.gameObject.SetActive(value: false);
	}

	protected override void OnBind()
	{
		EventBus.Subscribe(this).AddTo(this);
		SetVisibility(state: true);
	}

	protected override void OnUnbind()
	{
		SetVisibility(state: false);
	}

	private void SetVisibility(bool state)
	{
		if (state)
		{
			ModalWindowsSounds.Instance.MessageBox.Show.Play();
		}
		else
		{
			ModalWindowsSounds.Instance.MessageBox.Hide.Play();
		}
		EventBus.RaiseEvent(delegate(IModalWindowUIHandler h)
		{
			h.HandleModalWindowUiChanged(state, ModalWindowUIType.NetRoles);
		});
		base.gameObject.SetActive(state);
		Game.Instance.RequestPauseUi(state);
	}
}
