using System.Collections.Generic;
using System.Linq;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoFactionsReputationConsoleView : CharInfoFactionsReputationPCView, ICharInfoComponentConsoleView, ICharInfoComponentView
{
	[SerializeField]
	private int m_RowsCount = 2;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	protected override void OnBind()
	{
		base.OnBind();
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour().AddTo(this);
		UpdateNavigation();
	}

	private void UpdateNavigation()
	{
		m_NavigationBehaviour.Clear();
		List<IConsoleNavigationEntity> list = m_WidgetList.Entries.Select((IBindable e) => e as IConsoleNavigationEntity).ToList();
		int num = Mathf.CeilToInt(1f * (float)list.Count / (float)m_RowsCount);
		for (int i = 0; i < m_RowsCount; i++)
		{
			int count = Mathf.Min(list.Count - i * num, num);
			List<IConsoleNavigationEntity> range = list.GetRange(i * num, count);
			m_NavigationBehaviour.AddRow(range);
		}
	}

	public void AddInput(ref InputLayer inputLayer, ref GridConsoleNavigationBehaviour navigationBehaviour, ConsoleHintsWidget hintsWidget)
	{
		navigationBehaviour.AddColumn<GridConsoleNavigationBehaviour>(m_NavigationBehaviour);
	}
}
