using Code.View.UI.Helpers;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.UI.Common.Animations;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class BarkBlockView<T> : View<T> where T : BaseBarkVM
{
	[Header("Animations")]
	[SerializeField]
	private TextMeshProUGUI m_Text;

	[SerializeField]
	private RectTransform m_BarkContainer;

	[SerializeField]
	private Vector2 m_ContainerPaddings;

	[SerializeField]
	protected FadeAnimator FadeAnimator;

	[Header("Values")]
	[SerializeField]
	private float m_YPositionInCombat = 80f;

	[SerializeField]
	private float m_YPositionInExploration = 34f;

	private AccessibilityTextHelper m_AccessibilityTextHelper;

	protected override void OnBind()
	{
		m_AccessibilityTextHelper = new AccessibilityTextHelper(m_Text).AddTo(this);
		base.ViewModel.Text.Subscribe(delegate(string text)
		{
			float num = UtilityBark.CalculateBarkWidth(text, m_Text.fontSize) + m_ContainerPaddings.x;
			int num2 = (int)Mathf.Ceil((float)text.Length * m_Text.fontSize * 0.58f / num);
			float a = (float)(int)(m_Text.fontSize * 1.1f * ((float)text.Length * m_Text.fontSize * 0.58f / num + 1f)) + m_ContainerPaddings.y;
			num = Mathf.Max(num, 0f);
			a = Mathf.Max(a, 0f);
			m_BarkContainer.sizeDelta = new Vector2(num, a);
			m_BarkContainer.localPosition = new Vector3(m_BarkContainer.localPosition.x, base.ViewModel.IsInCombat ? m_YPositionInCombat : m_YPositionInExploration);
			if (text.Length <= 25 || num2 <= 3)
			{
				text = "<align=\"center\">" + text + "</align>";
			}
			m_Text.text = text;
		}).AddTo(this);
	}
}
