using Kingmaker.Globalmap.Blueprints.Colonization;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickResourceInfoVM : TooltipBaseBrickVM
{
	public readonly string Name;

	public readonly Sprite Icon;

	public readonly int Count;

	public TooltipBrickResourceInfoVM(BlueprintResource blueprintResource, int count)
	{
		Name = blueprintResource.Name;
		Icon = blueprintResource.Icon;
		Count = count;
	}
}
