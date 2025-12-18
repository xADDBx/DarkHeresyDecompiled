using System;
using Kingmaker.Blueprints.Attributes;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Blueprints.Camera;

[ComponentName("Root/Camera/CameraRoot")]
[TypeId("c502cb732e5f4cd0ad2ce2c24b48c82c")]
public class CameraRoot : BlueprintScriptableObject
{
	[Serializable]
	[HashRoot]
	public class Reference : BlueprintReference<CameraRoot>
	{
	}

	[SerializeField]
	public BlueprintCameraSettings.Reference GroundMapSettings;

	[SerializeField]
	public BlueprintCameraSettings.Reference GlobalMapSettings;

	[SerializeField]
	public BlueprintCameraFollowSettings.Reference CameraFollowSettings;
}
