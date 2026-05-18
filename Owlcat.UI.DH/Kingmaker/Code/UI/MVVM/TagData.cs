using Kingmaker.Blueprints.Items;
using Kingmaker.UnitLogic.Progression.Features;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public abstract class TagData
{
	public Sprite Icon { get; protected set; }

	public Color BgrColor { get; protected set; }

	public BlueprintFeature OwnerFeature { get; protected set; }

	public BlueprintItem BlueprintItem { get; protected set; }

	public abstract string GetName();

	public abstract string GetDescription();
}
