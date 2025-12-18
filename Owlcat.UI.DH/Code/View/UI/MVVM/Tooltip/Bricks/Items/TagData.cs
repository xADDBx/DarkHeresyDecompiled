using Kingmaker.UnitLogic.Progression.Features;
using UnityEngine;

namespace Code.View.UI.MVVM.Tooltip.Bricks.Items;

public abstract class TagData
{
	public Sprite Icon { get; protected set; }

	public Color BgrColor { get; protected set; }

	public BlueprintFeature OwnerFeature { get; protected set; }

	public abstract void GetNameAndDescription(out string name, out string description);
}
