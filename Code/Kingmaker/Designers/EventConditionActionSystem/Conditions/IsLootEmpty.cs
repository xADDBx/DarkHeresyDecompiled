using System;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.MapObjects;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[Obsolete]
[PlayerUpgraderAllowed(true)]
[TypeId("8b79da878f4b8af45b4c69c4ad94122f")]
public class IsLootEmpty : Condition
{
	[HideIf("EvaluateMapObject")]
	[AllowedEntityType(typeof(MapObjectView))]
	public EntityReference LootObject;

	[ShowIf("EvaluateMapObject")]
	[SerializeReference]
	public MapObjectEvaluator MapObject;

	public bool EvaluateMapObject;

	protected override string GetConditionCaption()
	{
		return "Is loot " + ((!EvaluateMapObject) ? LootObject.ToString() : MapObject?.ToString()) + " empty?";
	}

	protected override bool CheckCondition()
	{
		if (EvaluateMapObject)
		{
			return MapObject.GetValue().GetOptional<InteractionLootPart>()?.Loot.Empty() ?? false;
		}
		IEntity entity = LootObject.FindData();
		if (entity == null)
		{
			return false;
		}
		return ((Entity)entity).GetOptional<InteractionLootPart>()?.Loot.Empty() ?? false;
	}
}
