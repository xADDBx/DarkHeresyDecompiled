using Kingmaker.Code.UI.MVVM.View;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TextureAndColorSelectorItemView : View<TextureAndColorSelectorItemVM>
{
	[SerializeField]
	private TextureSelectorCommonView m_TextureSelectorCommonView;

	[SerializeField]
	private TextureSelectorCommonView m_TextureSelectorColorView;

	[SerializeField]
	private OwlcatMultiButton m_RemoveButton;

	protected override void OnBind()
	{
		base.OnBind();
		if (base.ViewModel.TexturesSelectorVm == null)
		{
			m_TextureSelectorCommonView.gameObject.SetActive(value: false);
		}
		else
		{
			m_TextureSelectorCommonView.Bind(base.ViewModel.TexturesSelectorVm);
		}
		if (base.ViewModel.ColorsSelectorVm == null)
		{
			m_TextureSelectorColorView.gameObject.SetActive(value: false);
		}
		else
		{
			m_TextureSelectorColorView.Bind(base.ViewModel.ColorsSelectorVm);
		}
		m_RemoveButton.gameObject.SetActive(base.ViewModel.CanRemove);
		if (base.ViewModel.CanRemove)
		{
			ObservableSubscribeExtensions.Subscribe(m_RemoveButton.OnLeftClickAsObservable(), delegate
			{
				base.ViewModel.RequestRemove();
			}).AddTo(this);
		}
	}
}
