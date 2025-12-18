using System;
using System.Collections.Generic;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class InventoryDollAdditionalStatsConsoleView : InventoryDollAdditionalStatsPCView, ICharInfoComponentConsoleView, ICharInfoComponentView
{
	private enum NavigationLayout
	{
		Vertical,
		Horizontal
	}

	[SerializeField]
	private NavigationLayout m_NavigationLayout;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private ICharInfoComponentConsoleView m_CharInfoComponentConsoleViewImplementation;

	protected override void OnBind()
	{
		base.OnBind();
		CreateNavigation();
	}

	private void CreateNavigation()
	{
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour().AddTo(this);
		TooltipConfig tooltipConfig = new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: true);
		SimpleConsoleNavigationEntity item = new SimpleConsoleNavigationEntity(m_DeflectionTooltip, new TooltipTemplateGlossary("Deflection", tooltipConfig.IsGlossary));
		SimpleConsoleNavigationEntity item2 = new SimpleConsoleNavigationEntity(m_AbsorptionTooltip, new TooltipTemplateGlossary("Absorption", tooltipConfig.IsGlossary));
		SimpleConsoleNavigationEntity item3 = new SimpleConsoleNavigationEntity(m_DodgeTooltip, new TooltipTemplateGlossary("Dodge", tooltipConfig.IsGlossary));
		SimpleConsoleNavigationEntity item4 = new SimpleConsoleNavigationEntity(m_DodgePenetrationTooltip, new TooltipTemplateGlossary("DodgeReduction", tooltipConfig.IsGlossary));
		SimpleConsoleNavigationEntity item5 = new SimpleConsoleNavigationEntity(m_ResolveTooltip, new TooltipTemplateGlossary("Resolve", tooltipConfig.IsGlossary));
		SimpleConsoleNavigationEntity item6 = new SimpleConsoleNavigationEntity(m_ParryTooltip, new TooltipTemplateGlossary("Parry", tooltipConfig.IsGlossary));
		List<SimpleConsoleNavigationEntity> entities = new List<SimpleConsoleNavigationEntity> { item, item2, item3, item4, item6, item5 };
		switch (m_NavigationLayout)
		{
		case NavigationLayout.Vertical:
			m_NavigationBehaviour.AddColumn(entities);
			break;
		case NavigationLayout.Horizontal:
			m_NavigationBehaviour.AddRow(entities);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public void AddInput(ref InputLayer inputLayer, ref GridConsoleNavigationBehaviour navigationBehaviour, ConsoleHintsWidget hintsWidget)
	{
		navigationBehaviour.AddColumn<GridConsoleNavigationBehaviour>(m_NavigationBehaviour);
	}

	public GridConsoleNavigationBehaviour GetNavigation()
	{
		return m_NavigationBehaviour;
	}
}
