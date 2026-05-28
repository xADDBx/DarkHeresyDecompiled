using System;
using UnityEngine;

namespace Code.Framework.Editor.Cutscenes.Serialization;

[Serializable]
public class CutsceneEditorLink
{
	[SerializeField]
	public string PortFromId;

	[SerializeField]
	public string PortToId;

	[SerializeField]
	public float BendPoint;

	[SerializeField]
	public float BacklinkSecondBendPoint;

	[SerializeField]
	public float BacklinkHeight;

	[SerializeField]
	public Color Color = Color.grey;

	[SerializeField]
	public float Thickness = 3f;
}
