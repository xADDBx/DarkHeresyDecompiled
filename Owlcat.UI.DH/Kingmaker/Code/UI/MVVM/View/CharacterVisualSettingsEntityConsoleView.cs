using Owlcat.UI;
using R3;
using Rewired;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharacterVisualSettingsEntityConsoleView : CharacterVisualSettingsEntityView, IConsoleNavigationEntity, IConsoleEntity
{
	[Header("Console")]
	[SerializeField]
	private OwlcatMultiButton m_FocusButton;

	[SerializeField]
	private ConsoleHint m_LeftHint;

	[SerializeField]
	private ConsoleHint m_RightHint;

	private readonly ReactiveProperty<bool> m_Focused = new ReactiveProperty<bool>();

	protected override void OnBind()
	{
		base.OnBind();
		m_Focused.Value = false;
	}

	public void AddInput(InputLayer inputLayer)
	{
		if (base.ViewModel != null)
		{
			ReadOnlyReactiveProperty<bool> readOnlyReactiveProperty = m_Focused.And(base.ViewModel.Locked.Not()).ToReadOnlyReactiveProperty(initialValue: false);
			m_LeftHint.Bind(inputLayer.AddButton(delegate
			{
				base.ViewModel.Switch();
			}, 4, readOnlyReactiveProperty)).AddTo(this);
			m_RightHint.Bind(inputLayer.AddButton(delegate
			{
				base.ViewModel.Switch();
			}, 5, readOnlyReactiveProperty)).AddTo(this);
			inputLayer.AddButton(delegate
			{
				base.ViewModel.Switch();
			}, 0, readOnlyReactiveProperty).AddTo(this);
			inputLayer.AddButton(delegate
			{
				base.ViewModel.Switch();
			}, 0, readOnlyReactiveProperty, InputActionEventType.NegativeButtonJustPressed).AddTo(this);
		}
	}

	public void SetFocus(bool value)
	{
		m_FocusButton.SetFocus(value);
		m_Focused.Value = value;
	}

	public bool IsValid()
	{
		return m_FocusButton.IsValid();
	}
}
