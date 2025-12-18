using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoHitPointsPCView : View<CharInfoHitPointsVM>
{
	[Header("Elements")]
	[SerializeField]
	private TextMeshProUGUI m_MainHPField;

	[SerializeField]
	private TextMeshProUGUI m_MainArmorField;

	[SerializeField]
	private RectTransform m_WoundsTransform;

	[Header("Tooltip")]
	[SerializeField]
	private Image m_WoundsTooltip;

	[SerializeField]
	private Image m_ArmorTooltip;

	[SerializeField]
	private Image m_DefenceTooltip;

	protected override void OnBind()
	{
		ObservableSubscribeExtensions.Subscribe(base.ViewModel.Refresh, delegate
		{
			UpdateVisual();
		}).AddTo(this);
		SetTooltips();
		UpdateVisual();
	}

	private void UpdateVisual()
	{
		m_MainHPField.text = $"{base.ViewModel.CurrentHp}/{base.ViewModel.MaxHP}";
		m_MainArmorField.text = $"{base.ViewModel.CurrentArmor}/{base.ViewModel.MaxArmor}";
		float x = (float)base.ViewModel.CurrentHp.CurrentValue / (float)(base.ViewModel.MaxHP.CurrentValue + base.ViewModel.MaxArmor.CurrentValue);
		m_WoundsTransform.localScale = new Vector3(x, 1f, 1f);
	}

	private void SetTooltips()
	{
		m_WoundsTooltip.SetTooltip(base.ViewModel.WoundsTooltip).AddTo(this);
		m_ArmorTooltip.SetTooltip(base.ViewModel.ArmorTooltip).AddTo(this);
		m_DefenceTooltip.Or(null)?.SetTooltip(base.ViewModel.DefenceTooltip).AddTo(this);
	}
}
