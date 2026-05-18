using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharacterVisualSettingsEntityConsoleView : CharacterVisualSettingsEntityView, IConsoleNavigationEntity, IConsoleEntity
{
	[Header("Console")]
	[SerializeField]
	private OwlcatMultiButton m_FocusButton;

	[SerializeField]
	private HintView m_LeftHint;

	[SerializeField]
	private HintView m_RightHint;

	private readonly ReactiveProperty<bool> m_Focused = new ReactiveProperty<bool>();

	protected override void OnBind()
	{
		base.OnBind();
		m_Focused.Value = false;
	}

	public void AddInput()
	{
		_ = base.ViewModel;
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
