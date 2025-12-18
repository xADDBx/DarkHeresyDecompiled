using System.Collections.Generic;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickSeparatorView : TooltipBaseBrickView<TooltipBrickSeparatorVM>
{
	[SerializeField]
	private List<GameObject> m_SeparatorTypes = new List<GameObject>();

	protected override void OnBind()
	{
		for (int i = 0; i < m_SeparatorTypes.Count; i++)
		{
			m_SeparatorTypes[i].SetActive(base.ViewModel.Type == (TooltipBrickElementType)i);
		}
	}
}
