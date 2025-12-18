using Code.View.UI.Helpers;
using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public abstract class ConvictionBarBaseView : CharInfoComponentView<ConvictionBarVM>
{
	[SerializeField]
	protected RectTransform m_Container;

	[SerializeField]
	protected RectTransform m_Cursor;

	[Header("Buttons")]
	[SerializeField]
	protected OwlcatMultiButton m_RightButtonRadical;

	[SerializeField]
	protected OwlcatMultiButton m_LeftButtonPuritan;

	[SerializeField]
	protected OwlcatMultiButton m_CurrentCursor;

	[Header("Labels")]
	[SerializeField]
	protected TextMeshProUGUI m_RightLabel;

	[SerializeField]
	protected TextMeshProUGUI m_LeftLabel;

	private AccessibilityTextHelper m_TextHelper;

	protected override void OnBind()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_RightLabel, m_LeftLabel);
		}
		base.OnBind();
		float halfContainerLength = m_Container.sizeDelta.x / 2f;
		base.ViewModel.CurrentRelativeValue.Subscribe(delegate(float value)
		{
			m_Cursor.anchoredPosition = new Vector2(halfContainerLength * value, m_Cursor.anchoredPosition.y);
		}).AddTo(this);
		SetupLabels();
		m_TextHelper.UpdateTextSize();
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_TextHelper.Dispose();
	}

	private void SetupLabels()
	{
		m_RightLabel.text = UIStrings.Instance.Alignment.RadicalTitle;
		m_LeftLabel.text = UIStrings.Instance.Alignment.PuritanTitle;
	}
}
