using System.Collections.Generic;
using Code.View.UI.Helpers;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class BrickTitleWithIconView : BrickBaseView<BrickTitleWithIconVM>
{
	[SerializeField]
	private TMP_Text m_Label;

	[SerializeField]
	private Image m_Icon;

	protected override void OnBind()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_Label);
		}
		base.OnBind();
		m_Label.text = base.ViewModel.Name;
		m_Icon.sprite = base.ViewModel.Icon;
		this.SetTooltip(base.ViewModel.Tooltip, new TooltipConfig
		{
			PriorityPivots = new List<Vector2>
			{
				new Vector2(0f, 0.7f),
				new Vector2(0f, 0.5f)
			}
		}).AddTo(this);
		m_TextHelper.UpdateTextSize();
	}
}
