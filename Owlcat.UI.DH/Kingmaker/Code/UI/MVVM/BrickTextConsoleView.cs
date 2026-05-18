using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickTextConsoleView : BrickTextView, IConsoleTooltipBrick
{
	[SerializeField]
	private RectTransform m_TextContainer;

	[SerializeField]
	private OwlcatMultiButton m_FrameButtonPrefab;

	[SerializeField]
	private OwlcatMultiButton m_MultiButtonFirstFocus;

	[SerializeField]
	private OwlcatMultiButton m_MultiButtonSecondFocus;

	private IConsoleEntity m_CurrentNavigation;

	private RectTransform m_TextTransform;

	protected override void OnBind()
	{
		if ((object)m_TextTransform == null)
		{
			m_TextTransform = m_Text.GetComponent<RectTransform>();
		}
		m_TextContainer.gameObject.SetActive(value: true);
		base.OnBind();
	}

	public IConsoleEntity GetConsoleEntity()
	{
		return null;
	}
}
