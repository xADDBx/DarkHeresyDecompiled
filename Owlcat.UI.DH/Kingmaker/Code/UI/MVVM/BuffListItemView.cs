using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class BuffListItemView : View<IBuffListItemVM>
{
	[SerializeField]
	private TMP_Text m_Title;

	[SerializeField]
	private TMP_Text m_Timer;

	[SerializeField]
	private TMP_Text m_Rank;

	[SerializeField]
	private TMP_Text m_Source;

	[SerializeField]
	private Image m_Icon;

	protected override void OnBind()
	{
		m_Title.text = base.ViewModel.Name;
		m_Rank.text = base.ViewModel.Stack;
		m_Source.text = base.ViewModel.SourceName;
		m_Icon.sprite = base.ViewModel.Icon;
		base.ViewModel.Duration.Subscribe(SetDuration).AddTo(this);
		this.SetTooltip(base.ViewModel.Tooltip).AddTo(this);
	}

	private void SetDuration(string duration)
	{
		m_Timer.text = duration;
	}
}
