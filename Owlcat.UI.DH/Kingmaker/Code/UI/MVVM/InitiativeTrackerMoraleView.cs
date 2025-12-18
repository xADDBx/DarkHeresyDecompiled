using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.Code.View.UI.MVVM.Tooltip.Templates;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class InitiativeTrackerMoraleView : View<UnitMoraleVM>
{
	[SerializeField]
	private UnitMoraleView m_MoraleView;

	[SerializeField]
	private Image m_MoraleLeaderIcon;

	[SerializeField]
	private MonoBehaviour m_TooltipSource;

	[SerializeField]
	private OwlcatMultiSelectable m_MultiSelectable;

	[Header("Tooltip")]
	[SerializeField]
	private TooltipConfig m_TooltipConfig;

	private IDisposable m_TooltipDisposable;

	protected override void OnBind()
	{
		UpdateVisibility();
		base.ViewModel.UpdateVisibility.Subscribe(UpdateVisibility).AddTo(this);
		base.ViewModel.IsMoraleLeader.Subscribe(m_MoraleLeaderIcon.gameObject.SetActive).AddTo(this);
		base.ViewModel.MoralePhase.Subscribe(delegate
		{
			UpdateTooltip();
		}).AddTo(this);
		base.ViewModel.MoraleValue.Subscribe(UpdateMorale).AddTo(this);
		m_MoraleLeaderIcon.SetHint(UIStrings.Instance.HUDTexts.MoraleLeader).AddTo(this);
		m_MoraleView.Bind(base.ViewModel);
	}

	protected override void OnUnbind()
	{
		m_TooltipDisposable?.Dispose();
		m_TooltipDisposable = null;
		m_MoraleView.Unbind();
	}

	private void UpdateMorale(int morale)
	{
		if (!base.ViewModel.IsMoraleLeader.CurrentValue)
		{
			m_MultiSelectable.SetActiveLayer("None");
			return;
		}
		MoralePhaseType currentValue = base.ViewModel.MoralePhase.CurrentValue;
		if (currentValue != 0)
		{
			m_MultiSelectable.SetActiveLayer(currentValue.ToString());
			return;
		}
		string activeLayer = ((morale == 0) ? "Neutral" : ((morale > 0) ? "Positive" : "Negative"));
		m_MultiSelectable.SetActiveLayer(activeLayer);
	}

	private void UpdateTooltip()
	{
		m_TooltipDisposable?.Dispose();
		m_TooltipDisposable = m_TooltipSource.SetTooltip(new TooltipTemplateMoraleUnit(base.ViewModel.MechanicEntityUIState), m_TooltipConfig);
	}

	private void UpdateVisibility()
	{
		base.gameObject.SetActive(base.ViewModel.IsVisible());
	}
}
