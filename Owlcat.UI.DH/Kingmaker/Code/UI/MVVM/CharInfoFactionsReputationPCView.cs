using System.Collections.Generic;
using System.Linq;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoFactionsReputationPCView : CharInfoComponentView<CharInfoFactionsReputationVM>
{
	[SerializeField]
	protected WidgetList m_WidgetList;

	[SerializeField]
	private CharInfoFactionReputationItemPCView m_FactionReputationItemPCView;

	protected override void OnBind()
	{
		base.OnBind();
		DrawEntities();
	}

	protected override void RefreshView()
	{
	}

	private void DrawEntities()
	{
		m_WidgetList.Clear();
		m_WidgetList.DrawMultiEntries(base.ViewModel.ScreenItems.ToArray(), new List<MonoBehaviour> { m_FactionReputationItemPCView });
	}
}
