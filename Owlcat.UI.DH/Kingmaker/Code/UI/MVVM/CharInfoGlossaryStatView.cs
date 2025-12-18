using System;
using Code.View.UI.UIUtils;
using Kingmaker.Code.View.Bridge.Utils;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoGlossaryStatView : View<CharInfoGlossaryStatVM>
{
	[SerializeField]
	private TMP_Text m_StatName;

	[SerializeField]
	private TMP_Text m_StatValue;

	[SerializeField]
	private OwlcatMultiSelectable m_Tooltip;

	private IDisposable m_TooltipDisposable;

	protected override void OnBind()
	{
		base.OnBind();
		m_StatName.text = UIUtilityEncyclopedy.GetGlossaryEntry(UtilityStats.GetGlossaryName(base.ViewModel.Stat))?.Title.Text;
		base.ViewModel.StatValue.Subscribe(delegate(int value)
		{
			m_StatValue.text = value.ToString();
		}).AddTo(this);
		base.ViewModel.Tooltip.Subscribe(delegate(TooltipBaseTemplate value)
		{
			m_TooltipDisposable?.Dispose();
			m_TooltipDisposable = m_Tooltip.SetTooltip(value).AddTo(this);
		}).AddTo(this);
	}
}
