using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class UnitPortraitPartPCView : View<UnitPortraitPartVM>
{
	[SerializeField]
	private Image m_DeadPortrait;

	[SerializeField]
	private Image m_LifePortrait;

	[SerializeField]
	private Image m_Cripple;

	protected override void OnBind()
	{
		base.ViewModel.Portrait.Subscribe(delegate(Sprite value)
		{
			m_DeadPortrait.sprite = value;
			m_LifePortrait.sprite = value;
		}).AddTo(this);
		base.ViewModel.IsDead.Subscribe(delegate(bool value)
		{
			m_DeadPortrait.gameObject.SetActive(value);
			m_LifePortrait.gameObject.SetActive(!value);
		}).AddTo(this);
		base.ViewModel.IsCrippled.Subscribe(delegate(bool value)
		{
			m_Cripple.Or(null)?.gameObject.SetActive(value);
		}).AddTo(this);
	}
}
