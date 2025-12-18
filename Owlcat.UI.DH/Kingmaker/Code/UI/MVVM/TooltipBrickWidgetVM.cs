using System.Collections.Generic;
using ObservableCollections;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickWidgetVM : TooltipBaseBrickVM
{
	public TooltipBrickTextVM TooltipBrickTextVM;

	public readonly ObservableList<ITooltipBrick> Bricks;

	private readonly List<TooltipBaseBrickVM> m_BricksVM = new List<TooltipBaseBrickVM>();

	public TooltipBrickWidgetVM(ObservableList<ITooltipBrick> bricks, TooltipBrickText tooltipBrickText = null)
	{
		Bricks = bricks;
		if (tooltipBrickText != null)
		{
			AddDisposable(TooltipBrickTextVM = tooltipBrickText.GetVM() as TooltipBrickTextVM);
		}
	}

	public List<TooltipBaseBrickVM> CollectBricksVM()
	{
		DisposeBricks();
		foreach (ITooltipBrick brick in Bricks)
		{
			if (brick != null)
			{
				TooltipBaseBrickVM vM = brick.GetVM();
				m_BricksVM.Add(vM);
			}
		}
		return m_BricksVM;
	}

	protected override void DisposeImplementation()
	{
		base.DisposeImplementation();
		TooltipBrickTextVM?.Dispose();
		DisposeBricks();
	}

	private void DisposeBricks()
	{
		for (int i = 0; i < m_BricksVM.Count; i++)
		{
			m_BricksVM[i].Dispose();
		}
		m_BricksVM.Clear();
	}
}
