using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.DialogSystem.Blueprints;

[TypeId("eea0d7fdafd8fe047a81a3208e5ed9ab")]
public class BlueprintCueSequence : BlueprintCueBase
{
	public List<BlueprintCueBaseReference> Cues = new List<BlueprintCueBaseReference>();

	[SerializeField]
	[FormerlySerializedAs("Exit")]
	private BlueprintSequenceExitReference m_Exit;

	public BlueprintSequenceExit Exit => m_Exit?.Get();

	public override bool CanShow(bool isFromSequence = false)
	{
		if (!base.CanShow(isFromSequence))
		{
			return false;
		}
		if (!Cues.Where((BlueprintCueBaseReference cue) => !cue.IsEmpty()).Any((BlueprintCueBaseReference cue) => cue.Get().CanShow(isFromSequence: true)))
		{
			DialogDebug.Add(this, "no valid cues", Color.red);
			return false;
		}
		return true;
	}
}
