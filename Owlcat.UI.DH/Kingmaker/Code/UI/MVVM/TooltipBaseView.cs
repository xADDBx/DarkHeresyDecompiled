using DG.Tweening;
using Kingmaker.UI.Common;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public abstract class TooltipBaseView : InfoBaseView<TooltipVM>
{
	[Space]
	[Header("Tooltip")]
	[SerializeField]
	private OwlcatMultiSelectable m_BackgroundSelectable;

	[SerializeField]
	protected LayoutElement m_LayoutElement;

	[SerializeField]
	protected LayoutElement m_BodyLayoutElement;

	[SerializeField]
	protected VerticalLayoutGroup m_ContentVerticalLayoutGroup;

	[ShowIf("m_HasScroll")]
	[SerializeField]
	protected ScrollRectExtended m_BodyScroll;

	[SerializeField]
	private ContentSizeFitterExtended m_TooltipSizeFitter;

	[Header("Values")]
	[SerializeField]
	protected float m_MaxHeight = 710f;

	[SerializeField]
	private float m_DefaultWidth = 475f;

	[SerializeField]
	private bool m_HasScroll;

	protected Tweener m_ShowTween;

	protected bool m_IsShowed;

	private CanvasGroup m_CanvasGroup;

	protected CanvasGroup CanvasGroup => m_CanvasGroup ?? (m_CanvasGroup = base.gameObject.EnsureComponent<CanvasGroup>());

	protected override void OnBind()
	{
		base.OnBind();
		if (base.ViewModel != null)
		{
			base.gameObject.SetActive(value: true);
			CanvasGroup.alpha = (base.ViewModel.IsComparative ? 1 : 0);
			m_BackgroundSelectable.SetActiveLayer(base.ViewModel.Background.ToString());
			if (m_ContentVerticalLayoutGroup != null)
			{
				m_ContentVerticalLayoutGroup.spacing = base.ViewModel.ContentSpacing;
			}
			if (m_HasScroll && m_BodyScroll != null)
			{
				bool active = base.ViewModel.HasScroll && m_BodyContainer.gameObject.activeSelf;
				m_BodyScroll.enabled = active;
				m_BodyScroll.verticalScrollbar.gameObject.SetActive(active);
			}
			Show();
		}
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		CanvasGroup.alpha = 0f;
		m_LayoutElement.preferredHeight = -1f;
		if ((bool)m_BodyLayoutElement)
		{
			m_BodyLayoutElement.preferredHeight = -1f;
		}
		base.gameObject.SetActive(value: false);
		m_ShowTween?.Kill();
		m_ShowTween = null;
	}

	protected abstract void Show();

	protected void SetHeight()
	{
		float num = CalculateBodyHeight();
		if (base.ViewModel.PreferredHeight > 0)
		{
			m_LayoutElement.preferredHeight = base.ViewModel.PreferredHeight;
			m_BodyLayoutElement.preferredHeight = num;
			return;
		}
		float num2 = ((RectTransform)base.transform).rect.height + num;
		float num3 = ((base.ViewModel.MaxHeight > 0) ? ((float)base.ViewModel.MaxHeight) : m_MaxHeight);
		if (num3 > 0f && num2 > num3)
		{
			m_LayoutElement.preferredHeight = num3;
			float num4 = num2 - num3;
			float preferredHeight = num - num4;
			m_BodyLayoutElement.preferredHeight = preferredHeight;
		}
		else
		{
			m_BodyLayoutElement.preferredHeight = num;
		}
	}

	protected void SetWidth()
	{
		LayoutElement layoutElement = m_LayoutElement;
		float preferredWidth = (m_LayoutElement.minWidth = ((base.ViewModel.Width > 0) ? ((float)base.ViewModel.Width) : m_DefaultWidth));
		layoutElement.preferredWidth = preferredWidth;
	}

	protected float CalculateBodyHeight()
	{
		return CalculateHeight(m_BodyContainer);
	}

	protected float CalculateHeight(RectTransform parent)
	{
		float num = 0f;
		VerticalLayoutGroup component = parent.GetComponent<VerticalLayoutGroup>();
		float num2 = (component ? component.spacing : 0f);
		foreach (RectTransform item in parent)
		{
			if (item.gameObject.activeSelf)
			{
				LayoutElement component2 = item.GetComponent<LayoutElement>();
				if ((object)component2 == null || !component2.ignoreLayout)
				{
					num += item.rect.height + num2;
				}
			}
		}
		return num;
	}

	public void Scroll(float value)
	{
		if (m_HasScroll && !(m_BodyScroll == null))
		{
			m_BodyScroll.Scroll(value * m_BodyScroll.scrollSensitivity, smooth: true);
		}
	}

	public void SetContentSizeFitter(bool value)
	{
		if (!(m_TooltipSizeFitter == null))
		{
			m_TooltipSizeFitter.enabled = value;
		}
	}
}
