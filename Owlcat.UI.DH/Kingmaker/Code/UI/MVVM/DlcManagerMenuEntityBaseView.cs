using Code.View.UI.Helpers;
using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class DlcManagerMenuEntityBaseView : SelectionGroupEntityView<DlcManagerMenuEntityVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Label;

	private AccessibilityTextHelper m_AccessibilityTextHelper;

	public override void DoInitialize()
	{
		base.DoInitialize();
		base.gameObject.SetActive(value: false);
		AddDisposable(m_AccessibilityTextHelper = new AccessibilityTextHelper(m_Label));
	}

	protected override void BindViewImplementation()
	{
		base.gameObject.SetActive(value: true);
		m_Label.text = base.ViewModel.Title;
		base.BindViewImplementation();
		m_AccessibilityTextHelper.UpdateTextSize();
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		base.gameObject.SetActive(value: false);
	}
}
