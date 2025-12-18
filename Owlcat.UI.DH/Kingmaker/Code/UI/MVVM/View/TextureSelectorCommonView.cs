using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class TextureSelectorCommonView : BaseCharGenAppearancePageComponentView<TextureSelectorVM>, IConsoleEntityProxy, IConsoleEntity
{
	[SerializeField]
	private TextureSelectorGroupView m_GroupView;

	[SerializeField]
	private VirtualListLayoutElementSettings m_LayoutSettings;

	public override VirtualListLayoutElementSettings LayoutSettings => m_LayoutSettings;

	public IConsoleEntity ConsoleEntityProxy => m_GroupView;

	public void Initialize()
	{
		Hide();
	}

	protected override void BindViewImplementation()
	{
		m_GroupView.Bind(base.ViewModel.SelectionGroup);
		AddDisposable(base.ViewModel.Title.Subscribe(m_GroupView.SetTitleText));
		AddDisposable(base.ViewModel.Description.Subscribe(m_GroupView.SetDescriptionText));
		AddDisposable(base.ViewModel.IsAvailable.Subscribe(OnAvailableStateChange));
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform as RectTransform);
		m_LayoutSettings.SetDirty();
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_GroupView.Unbind();
		Hide();
	}

	public override void SetFocus(bool value)
	{
		base.SetFocus(value);
		m_GroupView.SetFocus(value);
	}

	protected virtual void OnAvailableStateChange(bool state)
	{
		base.gameObject.SetActive(state);
	}
}
