using Code.View.UI.MVVM;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickSeparatorView : BrickBaseView<BrickSeparatorVM>
{
	[SerializeField]
	private EnumToObjectSelector<TooltipBrickElementType, GameObject> m_SeparatorTypeSelector = new EnumToObjectSelector<TooltipBrickElementType, GameObject>();

	protected override void OnBind()
	{
		foreach (EnumToObjectSelector<TooltipBrickElementType, GameObject>.Entity entitiesWithType in m_SeparatorTypeSelector.EntitiesWithTypes)
		{
			entitiesWithType.Value.SetActive(value: false);
		}
		m_SeparatorTypeSelector.GetEntity(base.ViewModel.Type).SetActive(value: true);
	}
}
