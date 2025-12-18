using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Camera;
using Kingmaker.Blueprints.Root;
using Kingmaker.GameModes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Globalmap.Blueprints;

[Obsolete]
[ComponentName("Area/BlueprintSectorMapArea")]
[TypeId("87fd89dbdde949fd98f618f6c42acc32")]
public class BlueprintSectorMapArea : BlueprintArea
{
	[SerializeField]
	private BlueprintAreaEnterPointReference m_SectorMapEnterPoint;

	[SerializeField]
	public BlueprintSectorMapPointReference DefaultStarSystem;

	public override GameModeType AreaStatGameMode => GameModeType.GlobalMap;

	public BlueprintAreaEnterPointReference SectorMapEnterPoint => m_SectorMapEnterPoint;

	public override BlueprintCameraSettings CameraSettings => ConfigRoot.Instance.CameraRoot.GlobalMapSettings;
}
