using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class JournalOrderProfitFactorBaseView : View<JournalOrderProfitFactorVM>
{
	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private TextMeshProUGUI m_Count;

	[SerializeField]
	private Image m_Arrow;

	[SerializeField]
	private Sprite m_GreenArrow;

	[SerializeField]
	private Sprite m_RedArrow;

	[SerializeField]
	private OwlcatSelectable m_OwlcatSelectable;

	[SerializeField]
	private Image m_BackgroundImage;

	protected override void OnBind()
	{
		base.ViewModel.Icon.Subscribe(delegate(Sprite i)
		{
			m_Icon.sprite = i;
		}).AddTo(this);
		base.ViewModel.Count.Subscribe(delegate(float c)
		{
			m_Count.text = c.ToString();
		}).AddTo(this);
		base.ViewModel.ArrowDirection.Subscribe(delegate(int value)
		{
			m_OwlcatSelectable.SetFocus(value != 0);
			m_Arrow.gameObject.SetActive(value != 0);
			if (value != 0)
			{
				m_Arrow.sprite = ((value == 1) ? m_GreenArrow : m_RedArrow);
			}
		}).AddTo(this);
	}
}
