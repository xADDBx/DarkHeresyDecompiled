using Kingmaker.Code.UI.MVVM.View;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class NetRolesPlayerCharacterConsoleView : NetRolesPlayerCharacterBaseView, IConsoleNavigationEntity, IConsoleEntity
{
	[SerializeField]
	private RectTransform m_FocusButton;

	[SerializeField]
	private HintView m_HintUp;

	[SerializeField]
	private HintView m_HintDown;

	private readonly ReactiveProperty<bool> m_IsFocused = new ReactiveProperty<bool>();

	public ReadOnlyReactiveProperty<bool> IsFocused => m_IsFocused;

	protected override void OnBind()
	{
		base.OnBind();
		base.ViewModel.PlayerRoleMe.Skip(1).Subscribe(delegate
		{
			EventBus.RaiseEvent(delegate(INetRolesConsoleHandler h)
			{
				h.HandleUpdateCharactersNavigation(base.Character);
			});
		}).AddTo(this);
	}

	public void SetFocus(bool value)
	{
		if (value)
		{
			ButtonsSounds.Instance.Default.Hover.Play();
		}
		m_IsFocused.Value = value;
		m_FocusButton.gameObject.SetActive(value);
	}

	public bool IsValid()
	{
		return m_Portrait.gameObject.activeInHierarchy;
	}

	public void AddPlayerInput()
	{
	}
}
