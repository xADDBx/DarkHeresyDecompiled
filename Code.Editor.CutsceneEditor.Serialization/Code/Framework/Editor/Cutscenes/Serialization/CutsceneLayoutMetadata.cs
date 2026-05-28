using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Code.Framework.Editor.Cutscenes.Serialization;

[Serializable]
public class CutsceneLayoutMetadata
{
	[Serializable]
	public class GateLayout
	{
		[SerializeField]
		public string GateGuid;

		[SerializeField]
		public Rect Rect;

		[SerializeField]
		public CutsceneEditorPort GateInPort;

		[SerializeField]
		public List<CutsceneEditorPort> TrackOutPorts = new List<CutsceneEditorPort>();

		[SerializeField]
		public Dictionary<string, CutsceneEditorPort> CommandsExtraPorts = new Dictionary<string, CutsceneEditorPort>();
	}

	public List<CutsceneEditorNode> Nodes = new List<CutsceneEditorNode>();

	public List<GateLayout> GateLayouts = new List<GateLayout>();

	public GateLayout GetGateLayout(string gateGuid)
	{
		return GateLayouts.FirstOrDefault((GateLayout g) => g.GateGuid == gateGuid);
	}
}
