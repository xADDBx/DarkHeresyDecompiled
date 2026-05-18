using System.Collections.Generic;
using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickChargenSelectionRestrictionsView : BrickBaseView<BrickChargenSelectionRestrictionsVM>
{
	[SerializeField]
	private TMP_Text m_HeaderText;

	[SerializeField]
	private RectTransform m_GroupsContainer;

	[SerializeField]
	private AbilityRestrictionGroupView m_GroupPrefab;

	[SerializeField]
	private OwlcatMultiSelectable m_Selectable;

	private readonly List<AbilityRestrictionGroupView> m_GroupWidgets = new List<AbilityRestrictionGroupView>();

	protected override void OnBind()
	{
		base.OnBind();
		m_Selectable.SetActiveLayer(base.ViewModel.IsPassed ? "Passed" : "NotPassed");
		m_HeaderText.SetText(base.ViewModel.HeaderText);
		foreach (AbilityRestrictionGroupVM group in base.ViewModel.Groups)
		{
			AbilityRestrictionGroupView widget = WidgetFactory.GetWidget(m_GroupPrefab);
			widget.transform.SetParent(m_GroupsContainer, worldPositionStays: false);
			widget.Bind(group);
			m_GroupWidgets.Add(widget);
		}
	}

	protected override void OnUnbind()
	{
		foreach (AbilityRestrictionGroupView groupWidget in m_GroupWidgets)
		{
			groupWidget.Unbind();
			WidgetFactory.DisposeWidget(groupWidget);
		}
		m_GroupWidgets.Clear();
	}
}
