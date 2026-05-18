using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class NewGameBaseView : View<NewGameVM>
{
	[Header("Common")]
	[SerializeField]
	protected NewGameMenuSelectorBaseView m_Selector;

	protected override void OnBind()
	{
		base.gameObject.SetActive(value: true);
		m_Selector.Bind(base.ViewModel.MenuSelectionGroup);
		Show();
	}

	protected override void OnUnbind()
	{
		Hide();
		base.gameObject.SetActive(value: false);
	}

	private void Show()
	{
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: true, FullScreenUIType.NewGame);
		});
		UISounds.Instance.Sounds.ServiceWindowsSounds.PlayOpenSound(ServiceWindowsType.LocalMap);
	}

	private void Hide()
	{
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: false, FullScreenUIType.NewGame);
		});
		UISounds.Instance.Sounds.ServiceWindowsSounds.PlayCloseSound(ServiceWindowsType.LocalMap);
	}
}
