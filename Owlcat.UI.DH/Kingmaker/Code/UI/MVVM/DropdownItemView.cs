using Owlcat.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class DropdownItemView : View<DropdownItemVM>, IConsoleEntityProxy, IConsoleEntity
{
	[SerializeField]
	private OwlcatToggle m_Toggle;

	[SerializeField]
	private TextMeshProUGUI m_Text;

	[SerializeField]
	private Image m_Image;

	public MonoBehaviour MonoBehaviour => this;

	public OwlcatToggle Toggle => m_Toggle;

	public IConsoleEntity ConsoleEntityProxy => m_Toggle;

	public string TextValue => base.ViewModel.Text;

	public void SetToggleGroup(OwlcatToggleGroup toggleGroup)
	{
		m_Toggle.Group = toggleGroup;
	}

	protected override void OnBind()
	{
		m_Text.text = base.ViewModel.Text;
		if (base.ViewModel.Icon != null && m_Image != null)
		{
			m_Image.sprite = base.ViewModel.Icon;
		}
	}

	public void SetItemHeight(float height)
	{
		RectTransform rectTransform = m_Text.transform as RectTransform;
		if (rectTransform != null)
		{
			rectTransform.sizeDelta = new Vector2(rectTransform.rect.width, height);
		}
	}
}
