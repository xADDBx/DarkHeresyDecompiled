using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;

namespace Kingmaker.Gameplay.Components;

[Serializable]
[ClassInfoBox("Выставляет значение стата в 1 и помечает его как выключенный")]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintMechanicEntityFact))]
[ComponentName("Stats/DisableAttributeStat")]
[TypeId("fc237b933c294463a508470a913fe27e")]
public sealed class DisableAttributeStat : MechanicEntityFactComponentDelegate
{
	public AttributeType Attribute;

	protected override void OnActivateOrPostLoad()
	{
		base.Owner.GetStatOptional<ModifiableValueAttributeStat>(Attribute.ToStatType())?.Disable(base.Runtime);
	}

	protected override void OnDeactivate()
	{
		base.Owner.GetStatOptional<ModifiableValueAttributeStat>(Attribute.ToStatType())?.Enable(base.Runtime);
	}
}
