using System.Collections.Generic;
using Owlcat.Runtime.Core.ObjectTracking;
using Unity.Collections;
using UnityEngine;

namespace Owlcat.Runtime.Visual.VirtualTexture.Materials;

public class ShaderTracker : ObjectTracker<Shader>
{
	public ShaderTracker(ObjectDispatcherService.TypeTrackingFlags trackingFlags)
		: base(trackingFlags)
	{
	}

	public override void ProcessData(List<Object> changed, NativeArray<int> changedID, NativeArray<int> destroyedID)
	{
	}
}
