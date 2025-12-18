using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.View;
using Kingmaker.View.MapObjects;
using UnityEngine;

namespace Kingmaker.Code.Gameplay.Controllers.DetectiveRadar;

[RequireComponent(typeof(EntityViewBase))]
[KnowledgeDatabaseID("625a189bbfe14969982ba674d90712b1")]
public class DetectiveClueSignalComponent : EntityPartComponent<DetectiveClueSignalPart, DetectiveRadarSignalSettings>
{
	public bool IsJammer => Settings.IsJammer;
}
