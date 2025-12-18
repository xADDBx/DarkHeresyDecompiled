using Kingmaker.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class CreditsHeaderElement : CreditElement, ICreditsElement
{
	[SerializeField]
	protected TextMeshProUGUI m_Label;

	public void Initialize(string header, ICreditsView view)
	{
		base.gameObject.SetActive(value: true);
		m_Label.text = header;
		base.transform.SetParent(view.Content);
		base.transform.ResetAll();
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform as RectTransform);
	}

	public void Ping()
	{
	}
}
