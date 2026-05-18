using System.Collections.Generic;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class AbilityRestrictionGroupView : View<AbilityRestrictionGroupVM>
{
	[SerializeField]
	private RectTransform m_Container;

	[SerializeField]
	private AbilityRestrictionWidget m_RestrictionWidgetPrefab;

	[SerializeField]
	private Graphic m_ConnectorImage;

	[SerializeField]
	private Graphic m_NextConnectorImage;

	[SerializeField]
	private Color m_PassedColor;

	[SerializeField]
	private Color m_PassedORColor;

	[SerializeField]
	private Color m_NotPassedColor;

	[SerializeField]
	private TMP_Text m_ORText;

	private readonly List<AbilityRestrictionWidget> m_Widgets = new List<AbilityRestrictionWidget>();

	protected override void OnBind()
	{
		for (int i = 0; i < base.ViewModel.Entries.Count; i++)
		{
			AbilityRestrictionEntry abilityRestrictionEntry = base.ViewModel.Entries[i];
			AbilityRestrictionWidget widget = WidgetFactory.GetWidget(m_RestrictionWidgetPrefab);
			widget.transform.SetParent(m_Container, worldPositionStays: false);
			widget.Setup(abilityRestrictionEntry.Description, abilityRestrictionEntry.IsPassed, i == 0, i == base.ViewModel.Entries.Count - 1);
			widget.DescriptionText.SetLinkTooltip(null, null, new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: true), base.ViewModel.Owner).AddTo(this);
			m_Widgets.Add(widget);
		}
		m_ORText.SetText(base.ViewModel.LogicalOrText);
		SetupLogicalType();
		base.ViewModel.ShowConnector.Subscribe(SetupConnector).AddTo(this);
		base.ViewModel.NextConnectorData.Subscribe(SetupNextConnector).AddTo(this);
	}

	protected override void OnUnbind()
	{
		m_Widgets.ForEach(WidgetFactory.DisposeWidget);
		m_Widgets.Clear();
	}

	private void SetupConnector(bool showConnector)
	{
		m_ConnectorImage.color = (base.ViewModel.IsPassed ? m_PassedColor : m_NotPassedColor);
	}

	private void SetupNextConnector(NextConnectorData data)
	{
		m_NextConnectorImage.gameObject.SetActive(data.ShowConnector);
		if (data.ShowConnector)
		{
			m_NextConnectorImage.color = (data.IsPassed ? m_PassedColor : m_NotPassedColor);
		}
	}

	private void SetupLogicalType()
	{
		m_ORText.gameObject.SetActive(base.ViewModel.ShowLogicalOr);
		if (base.ViewModel.ShowLogicalOr)
		{
			m_ORText.color = (base.ViewModel.IsPassed ? m_PassedORColor : m_NotPassedColor);
		}
	}
}
