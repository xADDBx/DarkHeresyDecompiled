using Kingmaker.Blueprints.Root;
using Kingmaker.Utility.Attributes;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class ClassifiedRect : MonoBehaviour
{
	[SerializeField]
	private bool m_OverrideColors;

	[ShowIf("m_OverrideColors")]
	[SerializeField]
	private Color m_DefaultColor;

	[ShowIf("m_OverrideColors")]
	[SerializeField]
	private Color m_HighlightColor;

	[field: SerializeField]
	public RectTransform RectTransform { get; private set; }

	[field: SerializeField]
	public Image Image { get; private set; }

	public void SetHasClueState(bool isShown)
	{
		if (isShown)
		{
			Image.color = (m_OverrideColors ? m_HighlightColor : UIConfig.Instance.DetectiveConfig.ClassifiedHighlightColor);
		}
		else
		{
			Image.color = (m_OverrideColors ? m_DefaultColor : UIConfig.Instance.DetectiveConfig.ClassifiedDefaultColor);
		}
	}
}
