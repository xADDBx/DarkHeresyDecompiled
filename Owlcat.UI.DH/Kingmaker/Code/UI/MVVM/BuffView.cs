using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class BuffView : View<BuffVM>
{
	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private TMP_Text m_Rank;

	[SerializeField]
	private TMP_Text m_Damage;

	[SerializeField]
	private GameObject m_NonStackNotification;

	protected override void OnBind()
	{
		m_Icon.sprite = base.ViewModel.Icon;
		base.ViewModel.ShowNonStackNotification.Subscribe(delegate(bool value)
		{
			m_NonStackNotification.SetActive(value);
		}).AddTo(this);
		base.ViewModel.Rank.Subscribe(HandleBuffRankChanged).AddTo(this);
		this.SetTooltip(new TooltipTemplateBuff(base.ViewModel.Buff), new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.None)).AddTo(this);
	}

	private void HandleBuffRankChanged(int newRank)
	{
		m_Rank.gameObject.SetActive(newRank > 1 && !base.ViewModel.DealsDamage);
		m_Rank.text = newRank.ToString();
		m_Damage.gameObject.SetActive(base.ViewModel.DealsDamage);
		m_Damage.text = newRank.ToString();
	}
}
