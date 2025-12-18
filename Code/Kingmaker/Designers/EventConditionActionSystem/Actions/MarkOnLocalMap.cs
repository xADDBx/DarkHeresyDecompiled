using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.View.MapObjects;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Obsolete]
[AllowMultipleComponents]
[TypeId("5e2f2424c06de7c45b73c2318523bbab")]
public class MarkOnLocalMap : GameAction
{
	[AllowedEntityType(typeof(MapObjectView))]
	[ValidateNotEmpty]
	[SerializeReference]
	public MapObjectEvaluator MapObject;

	public bool Hidden;

	public override string GetDescription()
	{
		return string.Format("{0} маркер для мапобжекта {1} на локальной карте\n", Hidden ? "Выключает" : "Включает", MapObject) + "Для работы на мапобжекте должен висеть компонент LocalMapMarker";
	}

	protected override void RunAction()
	{
		MapObjectEntity value = MapObject.GetValue();
		LocalMapMarkerPart optional = value.GetOptional<LocalMapMarkerPart>();
		if (optional != null)
		{
			optional.SetHidden(Hidden);
			return;
		}
		Element.LogError("Cannot mark {0}: no LocalMapMarker component.", value);
	}

	public override string GetCaption()
	{
		return string.Format("Make ({0}) {1} on map", MapObject, Hidden ? "hidden" : "marked");
	}
}
