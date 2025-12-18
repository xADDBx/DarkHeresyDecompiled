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
	private ConsoleHint m_HintUp;

	[SerializeField]
	private ConsoleHint m_HintDown;

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
			UISounds.Instance.Sounds.Buttons.ButtonHover.Play();
		}
		m_IsFocused.Value = value;
		m_FocusButton.gameObject.SetActive(value);
	}

	public bool IsValid()
	{
		return m_Portrait.gameObject.activeInHierarchy;
	}

	public void AddPlayerInput(InputLayer inputLayer)
	{
		if (base.ViewModel != null)
		{
			m_HintUp.BindCustomAction(6, inputLayer, base.ViewModel.CanUp.And(IsFocused).ToReadOnlyReactiveProperty(initialValue: false)).AddTo(this);
			inputLayer.AddButton(delegate
			{
				base.ViewModel.MoveRoleCharacterUp();
			}, 6, base.ViewModel.CanUp.And(IsFocused).ToReadOnlyReactiveProperty(initialValue: false)).AddTo(this);
			m_HintDown.BindCustomAction(7, inputLayer, base.ViewModel.CanDown.And(IsFocused).ToReadOnlyReactiveProperty(initialValue: false)).AddTo(this);
			inputLayer.AddButton(delegate
			{
				base.ViewModel.MoveRoleCharacterDown();
			}, 7, base.ViewModel.CanDown.And(IsFocused).ToReadOnlyReactiveProperty(initialValue: false)).AddTo(this);
		}
	}
}
