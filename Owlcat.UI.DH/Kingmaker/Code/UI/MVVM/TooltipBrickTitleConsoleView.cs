using System.Collections.Generic;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickTitleConsoleView : TooltipBrickTitleView, IConsoleTooltipBrick
{
	[SerializeField]
	private List<OwlcatMultiButton> m_Buttons;

	private readonly List<SimpleConsoleNavigationEntity> m_ConsoleNavigationEntities = new List<SimpleConsoleNavigationEntity>();

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	public bool IsBinded => base.ViewModel != null;

	protected override void OnBind()
	{
		base.OnBind();
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour().AddTo(this);
		CreateNavigation();
	}

	private void CreateNavigation()
	{
		m_NavigationBehaviour.Clear();
		m_ConsoleNavigationEntities.Clear();
		for (int i = 0; i < m_TitleObjects.Count; i++)
		{
			if (m_TitleObjects[i].activeSelf && (bool)m_Buttons[i])
			{
				m_ConsoleNavigationEntities.Add(new SimpleConsoleNavigationEntity(m_Buttons[i]));
				m_Buttons[i].SetInteractable(state: false);
			}
		}
		m_NavigationBehaviour.SetEntitiesVertical(m_ConsoleNavigationEntities);
	}

	public IConsoleEntity GetConsoleEntity()
	{
		CreateNavigation();
		return m_NavigationBehaviour;
	}
}
