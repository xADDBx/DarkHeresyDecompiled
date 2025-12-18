using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class FactionVendorInformationBaseView : View<FactionVendorInformationVM>, IConsoleNavigationEntity, IConsoleEntity
{
	[SerializeField]
	protected TextMeshProUGUI m_VendorLocation;

	[SerializeField]
	protected TextMeshProUGUI m_VendorName;

	[SerializeField]
	protected OwlcatButton m_MainButton;

	protected override void OnBind()
	{
		m_VendorLocation.text = base.ViewModel.Location;
		m_VendorName.text = base.ViewModel.Name;
		if (base.ViewModel.Vendor != null && (bool)m_MainButton)
		{
			m_MainButton.gameObject.SetActive(value: true);
			ObservableSubscribeExtensions.Subscribe(m_MainButton.OnLeftClickAsObservable(), delegate
			{
				StartTrade();
			}).AddTo(this);
		}
		else
		{
			m_MainButton.gameObject.SetActive(value: false);
		}
	}

	public void StartTrade()
	{
		base.ViewModel.StartTrade();
	}

	public void SetFocus(bool value)
	{
		m_MainButton.SetFocus(value);
	}

	public bool IsValid()
	{
		return m_MainButton.IsValid();
	}
}
