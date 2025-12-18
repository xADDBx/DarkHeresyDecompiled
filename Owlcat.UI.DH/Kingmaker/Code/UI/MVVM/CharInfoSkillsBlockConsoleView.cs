using System.Linq;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoSkillsBlockConsoleView : CharInfoSkillsBlockCommonView
{
	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	protected override void OnBind()
	{
		base.OnBind();
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour().AddTo(this);
	}

	public void CreateNavigation(int columnsCount)
	{
		m_NavigationBehaviour.Clear();
		if (columnsCount > 1)
		{
			int num = Mathf.CeilToInt(1f * (float)SkillEntries.Count / (float)columnsCount);
			for (int i = 0; i < columnsCount; i++)
			{
				m_NavigationBehaviour.AddColumn(SkillEntries.Skip(i * num).Take(num).ToArray());
			}
		}
		else
		{
			m_NavigationBehaviour.SetEntitiesVertical(SkillEntries);
		}
	}

	public IConsoleEntity GetConsoleEntity(int columnsCount = 2)
	{
		CreateNavigation(columnsCount);
		return m_NavigationBehaviour;
	}
}
