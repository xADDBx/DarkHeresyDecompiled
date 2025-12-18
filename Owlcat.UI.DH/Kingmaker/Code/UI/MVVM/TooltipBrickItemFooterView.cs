using Code.View.UI.Helpers;
using Owlcat.Runtime.Core.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickItemFooterView : TooltipBaseBrickView<TooltipBrickItemFooterVM>
{
	[SerializeField]
	protected TextMeshProUGUI m_LeftLabel;

	[SerializeField]
	protected GameObject m_LeftSide;

	[SerializeField]
	protected TextMeshProUGUI m_RightLabel;

	[SerializeField]
	protected GameObject m_RightSide;

	[SerializeField]
	private GameObject m_LeftEmpty;

	[SerializeField]
	private GameObject m_LeftInfo;

	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private TMP_Text m_AdditionalLabel;

	private AccessibilityTextHelper m_TextHelper;

	protected override void OnBind()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_LeftLabel, m_RightLabel, m_AdditionalLabel);
		}
		if ((bool)m_LeftEmpty || (bool)m_LeftInfo)
		{
			m_LeftLabel.text = base.ViewModel.LeftLine;
			m_LeftInfo.SetActive(!string.IsNullOrEmpty(base.ViewModel.LeftLine));
			m_LeftInfo.EnsureComponent<HorizontalLayoutGroup>().childAlignment = base.ViewModel.LeftAlignment;
			m_LeftEmpty.SetActive(!m_LeftInfo.activeSelf);
			m_RightLabel.text = base.ViewModel.RightLine;
			m_RightSide.SetActive(!string.IsNullOrEmpty(base.ViewModel.RightLine));
			m_RightSide.EnsureComponent<HorizontalLayoutGroup>().childAlignment = base.ViewModel.RightAlignment;
		}
		else
		{
			m_LeftLabel.text = base.ViewModel.LeftLine;
			m_LeftSide.SetActive(!string.IsNullOrEmpty(base.ViewModel.LeftLine));
			m_LeftSide.EnsureComponent<HorizontalLayoutGroup>().childAlignment = base.ViewModel.LeftAlignment;
			m_RightLabel.text = base.ViewModel.RightLine;
			m_RightSide.SetActive(!string.IsNullOrEmpty(base.ViewModel.RightLine));
			m_RightSide.EnsureComponent<HorizontalLayoutGroup>().childAlignment = base.ViewModel.RightAlignment;
		}
		bool flag = !string.IsNullOrEmpty(base.ViewModel.AdditionalLine);
		if (flag)
		{
			m_AdditionalLabel.text = base.ViewModel.AdditionalLine;
		}
		m_AdditionalLabel.gameObject.SetActive(flag);
		m_TextHelper.UpdateTextSize();
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_TextHelper.Dispose();
	}
}
