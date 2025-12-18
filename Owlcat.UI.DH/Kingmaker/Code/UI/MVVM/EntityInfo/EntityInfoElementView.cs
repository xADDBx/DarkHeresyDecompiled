using Code.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.EntityInfo;

public class EntityInfoElementView : MonoBehaviour
{
	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private TMP_Text m_Description;

	[SerializeField]
	private GameObject m_Underline;

	[SerializeField]
	private DOTEffectIconWidget m_DotEffect;

	public void SetIcon(Sprite icon)
	{
		bool flag = icon != null;
		if (flag)
		{
			m_Icon.sprite = icon;
		}
		m_Icon.gameObject.SetActive(flag);
	}

	public void SetText(string text)
	{
		m_Description.SetText(text);
	}

	public void ShowUnderline(bool show)
	{
		m_Underline.SetActive(show);
	}

	public void SetDOT(DOT? dotType)
	{
		if (!dotType.HasValue)
		{
			m_DotEffect.SetActive(isActive: false);
		}
		else
		{
			m_DotEffect.SetDOTType(dotType.Value);
		}
	}
}
