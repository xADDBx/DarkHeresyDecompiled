using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Framework.Editor.Cutscenes.Serialization;

[Serializable]
public class CutsceneEditorPort
{
	public enum Direction
	{
		In,
		Out
	}

	public enum PortSourceType
	{
		Gate,
		Track,
		Node,
		ExtraSignal,
		TimelineSignal
	}

	[SerializeField]
	public string PortId;

	[SerializeField]
	public string NodeId;

	[SerializeField]
	public string GateId;

	[SerializeField]
	public int TrackIndex;

	[SerializeField]
	public Direction PortDirection;

	[SerializeField]
	public PortSourceType PortType;

	[SerializeField]
	public List<CutsceneEditorLink> Links = new List<CutsceneEditorLink>();

	public static CutsceneEditorPort CreateInstance(string nodeId, string gateId, int trackIndex, Direction portDirection, PortSourceType portSourceType)
	{
		return new CutsceneEditorPort
		{
			PortId = Guid.NewGuid().ToString(),
			NodeId = nodeId,
			GateId = gateId,
			TrackIndex = trackIndex,
			PortDirection = portDirection,
			PortType = portSourceType
		};
	}

	public bool IsConnectedTo(string portId)
	{
		return Links.Exists((CutsceneEditorLink link) => link.PortFromId == portId || link.PortToId == portId);
	}

	public void Connect(CutsceneEditorPort other)
	{
		CutsceneEditorPort cutsceneEditorPort;
		CutsceneEditorPort cutsceneEditorPort2;
		if (PortDirection == Direction.In)
		{
			cutsceneEditorPort = this;
			cutsceneEditorPort2 = other;
		}
		else
		{
			cutsceneEditorPort = other;
			cutsceneEditorPort2 = this;
		}
		CutsceneEditorLink item = new CutsceneEditorLink
		{
			PortFromId = cutsceneEditorPort2.PortId,
			PortToId = cutsceneEditorPort.PortId
		};
		cutsceneEditorPort2.Links.Add(item);
		cutsceneEditorPort.Links.Add(item);
	}
}
