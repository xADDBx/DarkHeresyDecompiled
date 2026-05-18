using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.View.MapObjects;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[ComponentName("Evaluators/MapObjectPosition")]
[AllowMultipleComponents]
[TypeId("966cd65bb1951f04a8aee3e6dcabcc12")]
public class MapObjectPosition : PositionEvaluator
{
	[ValidateNotEmpty]
	[AllowedEntityType(typeof(MapObjectView))]
	public EntityReference MapObject;

	protected override Vector3 GetValueInternal()
	{
		if (!(MapObject.FindData() is MapObjectEntity mapObjectEntity))
		{
			return Vector3.zero;
		}
		return mapObjectEntity.Position;
	}

	public override string GetCaption()
	{
		return MapObject?.ToString() ?? "";
	}
}
