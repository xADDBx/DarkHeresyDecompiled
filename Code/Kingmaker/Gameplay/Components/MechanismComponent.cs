using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.UnitLogic;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;

namespace Kingmaker.Gameplay.Components;

[Serializable]
[ClassInfoBox("ArmorDurability механизма равен HitPoints. Урон HP и лечение HP игнорируется, урон HP всегда равен урону по броне. Механизмы имунны к некоторым абилкам и эффектам. Все механизмы получают CommonMechanismFact который задается в SystemMechanicsRoot")]
[ComponentName("Custom/MechanismComponent")]
[AllowedOn(typeof(BlueprintUnit))]
[TypeId("d9e7e15f00354f1aa3722407061d696d")]
public sealed class MechanismComponent : UnitFactComponentDelegate
{
	protected override void OnFactAttached()
	{
		BlueprintUnitFact commonMechanismFact = ConfigRoot.Instance.SystemMechanics.CommonMechanismFact;
		base.Owner.Facts.Add(new UnitFact(commonMechanismFact, base.Context));
	}

	protected override void OnFactDetached()
	{
		BlueprintUnitFact commonMechanismFact = ConfigRoot.Instance.SystemMechanics.CommonMechanismFact;
		base.Owner.Facts.Remove(commonMechanismFact);
	}
}
