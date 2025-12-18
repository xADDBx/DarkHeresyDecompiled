using Kingmaker.UI.Common.Animations;
using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CombatLogToggleWithCustomHint : OwlcatToggle
{
	[SerializeField]
	private FadeAnimator m_VotesHoverFadeAnimator;

	[SerializeField]
	private TextMeshProUGUI m_VotesHoverText;

	public void SetCustomHint(string hintText)
	{
		if (!(m_VotesHoverText == null))
		{
			m_VotesHoverText.text = hintText;
		}
	}
}
