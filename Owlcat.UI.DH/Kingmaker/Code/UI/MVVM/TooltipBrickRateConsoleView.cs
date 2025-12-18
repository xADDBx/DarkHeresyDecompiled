using System.Collections.Generic;
using System.Linq;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickRateConsoleView : TooltipBrickRateView, IConsoleTooltipBrick
{
	[SerializeField]
	private List<OwlcatMultiButton> m_Buttons;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	public bool IsBinded => base.ViewModel != null;

	protected override void OnBind()
	{
		base.OnBind();
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour().AddTo(this);
	}

	private void CreateNavigation()
	{
		m_NavigationBehaviour.Clear();
		m_NavigationBehaviour.SetEntitiesHorizontal(m_Buttons.Select((OwlcatMultiButton b) => new SimpleConsoleNavigationEntity(b)).ToList());
	}

	public IConsoleEntity GetConsoleEntity()
	{
		CreateNavigation();
		return m_NavigationBehaviour;
	}
}
