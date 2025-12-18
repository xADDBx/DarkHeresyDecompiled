using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoClassEntryPCView : View<CharInfoClassEntryVM>
{
	[SerializeField]
	private TextMeshProUGUI m_ClassName;

	[SerializeField]
	private TextMeshProUGUI m_Level;

	protected override void OnBind()
	{
		Clear();
		base.gameObject.SetActive(value: true);
		m_Level.text = base.ViewModel.Level.ToString();
		m_ClassName.text = base.ViewModel.ClassName;
		m_ClassName.SetTooltip(base.ViewModel.Tooltip).AddTo(this);
	}

	public void Hide()
	{
		Clear();
	}

	private void Clear()
	{
		m_Level.text = string.Empty;
		m_ClassName.text = string.Empty;
		base.gameObject.SetActive(value: false);
	}
}
