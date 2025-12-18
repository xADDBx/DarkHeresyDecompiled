using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.Gameplay.Parts.ViewBased;
using Kingmaker.View;
using Kingmaker.View.MapObjects;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using UnityEngine;

namespace Kingmaker.Code.View.Scene;

[RequireComponent(typeof(EntityViewBase))]
[DisallowMultipleComponent]
[KnowledgeDatabaseID("d3f8ca73113845b98ed2953c2a2c218a")]
public class DetectiveObjectComponent : EntityPartComponent<PartDetectiveObject, DetectiveObjectSettings>, IInteractionComponent
{
	public float ProximityRadius => 0f;
}
