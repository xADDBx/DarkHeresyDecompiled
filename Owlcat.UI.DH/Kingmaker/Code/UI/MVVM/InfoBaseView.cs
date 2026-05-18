using System.Collections.Generic;
using System.Linq;
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
	private TooltipBricksRegistry m_BricksRegistry;

	private readonly List<IBrickView> m_Bricks = new List<IBrickView>();

	private readonly List<IBrickView> m_BricksGroups = new List<IBrickView>();

	protected override void OnBind()
	{
		base.OnBind();
		SetPart(base.ViewModel.HeaderBricks, m_HeaderContainer);
		SetPart(base.ViewModel.BodyBricks, m_BodyContainer);
		SetPart(base.ViewModel.FooterBricks, m_FooterContainer);
		SetPart(base.ViewModel.HintBricks, m_HintContainer);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_Bricks.ForEach(delegate(IBrickView v)
		{
			WidgetFactory.DisposeWidget((MonoBehaviour)v);
		});
		m_Bricks.Clear();
		m_BricksGroups.ForEach(delegate(IBrickView v)
		{
			WidgetFactory.DisposeWidget((MonoBehaviour)v);
		});
		m_BricksGroups.Clear();
	}

	private void SetPart(IEnumerable<TooltipBrickVM> bricks, RectTransform container)
	{
		container.Or(null)?.gameObject.SetActive(bricks.Any());
		foreach (TooltipBrickVM brick in bricks)
		{
			PlaceBrick(brick, container, null);
		}
	}

	private void PlaceBrick(TooltipBrickVM brickVM, RectTransform container, BrickGroupView parentGroup)
	{
		IBrickView brickView = m_BricksRegistry?.GetView(brickVM?.GetType());
		if (brickView == null)
		{
			PFLog.UI.Error($"Can't find brick view for {brickVM?.GetType()} in registry {m_BricksRegistry?.name}", this);
			return;
		}
		brickView.BindVM(brickVM);
		Attach(brickView, container, parentGroup);
		if (!(brickVM is BricksGroupBaseVM bricksGroupBaseVM))
		{
			m_Bricks.Add(brickView);
			return;
		}
		if (!(brickView is BrickGroupView brickGroupView))
		{
			PFLog.UI.Error($"Expected BrickGroupView for {brickVM.GetType()}, got {brickView.GetType()}");
			return;
		}
		m_BricksGroups.Add(brickGroupView);
		foreach (TooltipBrickVM child in bricksGroupBaseVM.Children)
		{
			PlaceBrick(child, container, brickGroupView);
		}
	}

	private static void Attach(IBrickView view, RectTransform container, BrickGroupView parentGroup)
	{
		if (parentGroup != null)
		{
			parentGroup.AddChild(view.Transform as RectTransform);
		}
		else
		{
			view.Transform.SetParent(container, worldPositionStays: false);
		}
	}
}
