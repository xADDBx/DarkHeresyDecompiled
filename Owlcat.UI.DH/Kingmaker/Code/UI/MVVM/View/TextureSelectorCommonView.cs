using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class TextureSelectorCommonView : BaseCharGenAppearancePageComponentView<TextureSelectorVM>
{
	[SerializeField]
	private TextureSelectorGroupView m_GroupView;

	[SerializeField]
	private VirtualListLayoutElementSettings m_LayoutSettings;

	public override VirtualListLayoutElementSettings LayoutSettings => m_LayoutSettings;

	public void Initialize()
	{
		Hide();
	}

	protected override void BindViewImplementation()
	{
		if (base.ViewModel == null)
		{
			OnAvailableStateChange(state: false);
			return;
		}
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

	protected virtual void OnAvailableStateChange(bool state)
	{
		base.gameObject.SetActive(state);
	}
}
