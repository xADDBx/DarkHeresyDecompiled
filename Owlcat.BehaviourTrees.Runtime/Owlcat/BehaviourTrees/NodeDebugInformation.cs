using UnityEngine;

namespace Owlcat.BehaviourTrees;

public class NodeDebugInformation
{
	public NodeResult? Result { get; set; }

	public GameObject GameObjectOwner { get; set; }

	public NodeBreakpointSetting BreakpointSetting { get; set; }

	public void ResetForNewPass()
	{
		Result = null;
	}
}
