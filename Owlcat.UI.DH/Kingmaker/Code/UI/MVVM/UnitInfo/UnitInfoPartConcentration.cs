using System;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.UnitInfo;

public class UnitInfoPartConcentration : UnitInfoPart
{
	[SerializeField]
	private Image m_ConcentrationIcon;

	[SerializeField]
	private TMP_Text m_ConcentrationName;

	[SerializeField]
	private TMP_Text m_ConcentrationTitle;

	private IDisposable m_TooltipIDisposable;

	protected override void OnBind()
	{
		base.OnBind();
		base.ViewModel.Data.ConcentrationIcon.Subscribe(delegate(Sprite icon)
		{
			m_ConcentrationIcon.sprite = icon;
		}).AddTo(this);
		base.ViewModel.Data.ConcentrationName.Subscribe(delegate(string n)
		{
			m_ConcentrationName.text = n;
		}).AddTo(this);
		base.ViewModel.Data.ConcentrationAbilityTooltip.Subscribe(UpdateTooltip).AddTo(this);
		m_ConcentrationTitle.SetText(base.ViewModel.ConcentrationTitle);
	}

	protected override void ShowImpl(UnitInfoPartState state)
	{
		base.gameObject.SetActive(!state.HasHit && !state.PreciseAttackHasNoTarget && state.HasConcentration);
	}

	private void UpdateTooltip(TooltipBaseTemplate template)
	{
		if (template != null)
		{
			m_TooltipIDisposable = this.SetTooltip(template).AddTo(this);
			return;
		}
		m_TooltipIDisposable?.Dispose();
		m_TooltipIDisposable = null;
	}
}
