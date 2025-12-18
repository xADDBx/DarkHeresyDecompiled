using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickPrerequisiteView : TooltipBaseBrickView<TooltipBrickPrerequisiteVM>, IConsoleTooltipBrick
{
	[SerializeField]
	private WidgetList m_WidgetList;

	[SerializeField]
	private PrerequisiteEntryView m_PrerequisiteEntryView;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	public bool IsBinded => base.ViewModel != null;

	protected override void OnBind()
	{
		DrawEntries();
		CreateNavigation();
	}

	private void DrawEntries()
	{
		m_WidgetList.DrawEntries(base.ViewModel.PrerequisiteEntries.ToArray(), m_PrerequisiteEntryView);
	}

	private void CreateNavigation()
	{
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour().AddTo(this);
		foreach (IBindable entry in m_WidgetList.Entries)
		{
			if (entry is PrerequisiteEntryView prerequisiteEntryView)
			{
				m_NavigationBehaviour.AddRow<FloatConsoleNavigationBehaviour>(prerequisiteEntryView.GetNavigation());
			}
		}
	}

	public IConsoleEntity GetConsoleEntity()
	{
		return m_NavigationBehaviour;
	}
}
