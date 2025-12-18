using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.XPBD;

public struct UpdateContext
{
	public ScriptableRenderContext Context;

	public List<Camera> Cameras;

	public float StepDelta;

	public int SimulationSteps;

	public int StepIndex;

	public float SimulatedTime;

	public float SubstepTime;

	public float TimeLeft;

	public float DeltaTimeBetweenSimulations;
}
