using System.Linq;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using R3;
using R3.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class ChargenFooterDecorView : MonoBehaviour
{
	[SerializeField]
	private LayoutElement m_LayoutElement;

	[SerializeField]
	private RectTransform m_ContentRectTransform;

	[SerializeField]
	private RectTransform m_ViewportRectTransform;

	[SerializeField]
	private RectTransform m_ScrollRectTransform;

	[SerializeField]
	private RectTransform[] m_FooterVariants;

	[SerializeField]
	private RandomSpritesPicker[] m_RandomSpritesPicker;

	[SerializeField]
	private RandomTextPicker[] m_RandomTextPicker;

	private CanvasGroup[] m_FooterVariantsCanvasGroup;

	private void Start()
	{
		ObservableSubscribeExtensions.Subscribe(m_ContentRectTransform.OnRectTransformDimensionsChangeAsObservable(), delegate
		{
			RecalculateHeight();
		}).AddTo(this);
		m_FooterVariantsCanvasGroup = m_FooterVariants.Select((RectTransform footer) => footer.GetComponent<CanvasGroup>() ?? footer.gameObject.AddComponent<CanvasGroup>()).ToArray();
		m_LayoutElement.preferredHeight = m_FooterVariants.MinBy((RectTransform f) => f.rect.height).rect.height;
	}

	private void RecalculateHeight()
	{
		if (m_FooterVariants != null && m_FooterVariants.Length != 0)
		{
			float preferredHeight = LayoutUtility.GetPreferredHeight(m_ContentRectTransform);
			float availableHeight = m_ViewportRectTransform.rect.height + m_LayoutElement.preferredHeight - preferredHeight;
			RectTransform bestFooter = (from footer in m_FooterVariants
				where footer != null && footer.rect.height <= availableHeight
				orderby footer.rect.height descending
				select footer).FirstOrDefault().Or(m_FooterVariants.MinBy((RectTransform f) => f.rect.height));
			m_FooterVariantsCanvasGroup.ForEach(delegate(CanvasGroup c)
			{
				c.alpha = ((c.transform == bestFooter) ? 1f : 0f);
			});
			m_RandomSpritesPicker?.ForEach(delegate(RandomSpritesPicker c)
			{
				c.Randomize(availableHeight.ToString());
			});
			m_RandomTextPicker?.ForEach(delegate(RandomTextPicker c)
			{
				c.Randomize(availableHeight.ToString());
			});
		}
	}
}
