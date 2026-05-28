using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Framework.Editor.Cutscenes.Serialization;

[Serializable]
public class CutsceneEditorNode
{
	public enum NodeDirection
	{
		Both,
		In,
		Out
	}

	[SerializeField]
	public string NodeId;

	[SerializeField]
	public string ContentTypeName;

	[SerializeField]
	public string ContentId;

	[SerializeField]
	public Rect Rect;

	[SerializeField]
	public CutsceneEditorPort InPort;

	[SerializeField]
	public List<CutsceneEditorPort> OutPorts = new List<CutsceneEditorPort>();

	public ICutsceneEditorNodeContent Content { get; private set; }

	public static CutsceneEditorNode CreateNode(NodeDirection direction, ICutsceneEditorNodeContent content)
	{
		CutsceneEditorNode cutsceneEditorNode = new CutsceneEditorNode
		{
			NodeId = Guid.NewGuid().ToString(),
			ContentTypeName = content.GetType().AssemblyQualifiedName,
			ContentId = content.GetContentId()
		};
		switch (direction)
		{
		case NodeDirection.In:
			cutsceneEditorNode.InPort = CutsceneEditorPort.CreateInstance(cutsceneEditorNode.NodeId, null, -1, CutsceneEditorPort.Direction.In, CutsceneEditorPort.PortSourceType.Node);
			break;
		case NodeDirection.Out:
			cutsceneEditorNode.OutPorts.Add(CutsceneEditorPort.CreateInstance(cutsceneEditorNode.NodeId, null, -1, CutsceneEditorPort.Direction.Out, CutsceneEditorPort.PortSourceType.Node));
			break;
		case NodeDirection.Both:
			cutsceneEditorNode.InPort = CutsceneEditorPort.CreateInstance(cutsceneEditorNode.NodeId, null, -1, CutsceneEditorPort.Direction.In, CutsceneEditorPort.PortSourceType.Node);
			cutsceneEditorNode.OutPorts.Add(CutsceneEditorPort.CreateInstance(cutsceneEditorNode.NodeId, null, -1, CutsceneEditorPort.Direction.Out, CutsceneEditorPort.PortSourceType.Node));
			break;
		}
		return cutsceneEditorNode;
	}

	public void SetContent(ICutsceneEditorNodeContent content)
	{
		Content = content;
	}
}
