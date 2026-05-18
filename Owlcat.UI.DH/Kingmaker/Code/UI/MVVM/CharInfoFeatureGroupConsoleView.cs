using System;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoFeatureGroupConsoleView : CharInfoFeatureGroupPCView
{
	[Header("Console")]
	[SerializeField]
	private int m_ItemsInRow = 3;

	public CharInfoFeatureGroupVM.FeatureGroupType GroupType => m_GroupType;

	public void SetupChooseModeActions(Action<CharInfoFeatureConsoleView> onClick, Action<CharInfoFeatureConsoleView> onFocus)
	{
		m_WidgetList.Entries?.ForEach(delegate(IBindable e)
		{
			(e as CharInfoFeatureConsoleView)?.SetupChooseModeActions(onClick, onFocus);
		});
	}
}
