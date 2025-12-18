using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.View;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public abstract class InfoBaseView<TInfoBaseVM> : View<TInfoBaseVM> where TInfoBaseVM : InfoBaseVM
{
	[Header("Containers")]
	[SerializeField]
	protected RectTransform m_HeaderContainer;

	[SerializeField]
	protected RectTransform m_BodyContainer;

	[SerializeField]
	protected RectTransform m_FooterContainer;

	[SerializeField]
	protected RectTransform m_HintContainer;

	[Header("Bricks Config")]
	[SerializeField]
	private TooltipBricksView m_BricksConfig;

	protected readonly List<MonoBehaviour> m_Bricks = new List<MonoBehaviour>();

	private readonly List<MonoBehaviour> m_BricksGroups = new List<MonoBehaviour>();

	private readonly List<IConsoleTooltipBrick> m_NavigationBricks = new List<IConsoleTooltipBrick>();

	protected override void OnBind()
	{
		base.OnBind();
		SetPart(base.ViewModel.HeaderBricks, m_HeaderContainer);
		SetPart(base.ViewModel.BodyBricks, m_BodyContainer);
		SetPart(base.ViewModel.FooterBricks, m_FooterContainer);
		SetPart(base.ViewModel.HintBricks, m_HintContainer);
	}

	private void SetPart(IEnumerable<TooltipBaseBrickVM> bricks, RectTransform container)
	{
		container.Or(null)?.gameObject.SetActive(bricks.Any());
		TooltipBricksGroupView tooltipBricksGroupView = null;
		foreach (TooltipBaseBrickVM brick in bricks)
		{
			MonoBehaviour brickView = TooltipEngine.GetBrickView(m_BricksConfig, brick);
			IConsoleTooltipBrick consoleTooltipBrick = brickView as IConsoleTooltipBrick;
			if (brick is TooltipBricksGroupVM)
			{
				if (brickView != null)
				{
					tooltipBricksGroupView = (TooltipBricksGroupView)brickView;
					tooltipBricksGroupView.transform.SetParent(container, worldPositionStays: false);
					m_BricksGroups.Add(tooltipBricksGroupView);
					m_NavigationBricks.Add(tooltipBricksGroupView);
				}
				else
				{
					tooltipBricksGroupView = null;
				}
				continue;
			}
			if (tooltipBricksGroupView != null)
			{
				tooltipBricksGroupView.AddChild(brickView.transform as RectTransform);
				if (consoleTooltipBrick != null)
				{
					tooltipBricksGroupView.AddNavChild(consoleTooltipBrick.GetConsoleEntity());
				}
			}
			else
			{
				brickView.transform.SetParent(container, worldPositionStays: false);
				if (consoleTooltipBrick != null)
				{
					m_NavigationBricks.Add(consoleTooltipBrick);
				}
			}
			m_Bricks.Add(brickView);
		}
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_Bricks.ForEach(TooltipEngine.DestroyBrickView);
		m_Bricks.Clear();
		m_BricksGroups.ForEach(TooltipEngine.DestroyBrickView);
		m_BricksGroups.Clear();
		m_NavigationBricks.Clear();
	}

	public GridConsoleNavigationBehaviour GetNavigationBehaviour(IConsoleNavigationOwner owner = null)
	{
		GridConsoleNavigationBehaviour gridConsoleNavigationBehaviour = new GridConsoleNavigationBehaviour(owner);
		gridConsoleNavigationBehaviour.AddColumn(m_NavigationBricks.Select((IConsoleTooltipBrick i) => i.GetConsoleEntity()).ToList());
		return gridConsoleNavigationBehaviour;
	}
}
