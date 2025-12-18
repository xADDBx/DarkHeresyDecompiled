using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public abstract class SettingsEntityView<TSettingsEntityVM> : VirtualListElementViewBase<TSettingsEntityVM> where TSettingsEntityVM : SettingsEntityVM
{
	[SerializeField]
	private GameObject m_SetConnector;

	[SerializeField]
	private GameObject m_SetConnectorIAmSet;

	[SerializeField]
	private TextMeshProUGUI m_Title;

	protected override void BindViewImplementation()
	{
		UpdateLocalization();
		if (m_SetConnector != null)
		{
			m_SetConnector.SetActive(base.ViewModel.IsConnector);
		}
		if (m_SetConnectorIAmSet != null)
		{
			m_SetConnectorIAmSet.SetActive(base.ViewModel.IsSet);
		}
		AddDisposable(ObservableSubscribeExtensions.Subscribe(base.ViewModel.LanguageChanged, delegate
		{
			UpdateLocalization();
		}));
	}

	protected override void DestroyViewImplementation()
	{
	}

	protected virtual void UpdateLocalization()
	{
		if (m_Title != null)
		{
			m_Title.text = base.ViewModel.Title;
		}
	}
}
