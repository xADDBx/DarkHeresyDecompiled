using System.Collections.Generic;
using System.Linq;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickTitleConsoleView : BrickTitleView, IConsoleTooltipBrick
{
	[SerializeField]
	private List<OwlcatMultiButton> m_Buttons;

	public IConsoleEntity GetConsoleEntity()
	{
		return m_Buttons.ElementAtOrDefault((int)base.ViewModel.Type);
	}
}
