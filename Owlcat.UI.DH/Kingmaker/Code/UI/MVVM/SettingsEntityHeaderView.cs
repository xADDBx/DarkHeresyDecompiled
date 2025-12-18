using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class SettingsEntityHeaderView : VirtualListElementViewBase<SettingsEntityHeaderVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Tittle;

	[SerializeField]
	private VirtualListLayoutElementSettings m_LayoutElementSettings;

	public override VirtualListLayoutElementSettings LayoutSettings => m_LayoutElementSettings;

	protected override void BindViewImplementation()
	{
		m_Tittle.text = base.ViewModel.Tittle;
		AddDisposable(ObservableSubscribeExtensions.Subscribe(base.ViewModel.LanguageChanged, delegate
		{
			m_Tittle.text = base.ViewModel.Tittle;
		}));
	}

	protected override void DestroyViewImplementation()
	{
	}
}
