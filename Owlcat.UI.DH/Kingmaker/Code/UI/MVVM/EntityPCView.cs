using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class EntityPCView : View<EntityVM>
{
	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private TextMeshProUGUI m_Label;

	protected override void OnBind()
	{
		SetIcon();
		SetLabel();
		SetTooltip();
	}

	private void SetIcon()
	{
		m_Icon.sprite = base.ViewModel.Icon;
		m_Icon.preserveAspect = true;
		m_Icon.gameObject.SetActive(base.ViewModel.Icon);
	}

	private void SetLabel()
	{
		m_Label.text = base.ViewModel.Name;
	}

	private void SetTooltip()
	{
		this.SetTooltip(base.ViewModel.Tooltip).AddTo(this);
	}
}
