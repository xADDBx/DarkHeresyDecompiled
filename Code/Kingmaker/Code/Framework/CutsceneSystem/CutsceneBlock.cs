using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kingmaker.Code.Framework.CutsceneSystem;

[Serializable]
public class CutsceneBlock
{
	public enum BlockActivationMode
	{
		None,
		AnySignal,
		AllSignals
	}

	[SerializeField]
	private string m_Guid;

	[SerializeField]
	private string m_Name;

	[SerializeField]
	private string m_Comment;

	[SerializeField]
	private bool m_LockControl;

	[SerializeField]
	private bool m_IsDisabled;

	[SerializeField]
	private BlockActivationMode m_ActivationMode;

	[SerializeField]
	private List<CutsceneGate> m_Gates = new List<CutsceneGate>();

	[SerializeField]
	private List<string> m_NextBlockIds = new List<string>();

	public string Guid => m_Guid;

	public string Name => m_Name;

	public string Comment => m_Comment;

	public bool LockControl => m_LockControl;

	public bool IsDisabled => m_IsDisabled;

	public List<CutsceneGate> Gates => m_Gates;

	public BlockActivationMode ActivationMode => m_ActivationMode;

	public List<string> NextBlocks => m_NextBlockIds;

	public CutsceneGate GetGateById(string id)
	{
		if (string.IsNullOrEmpty(id))
		{
			return null;
		}
		return Gates.FirstOrDefault((CutsceneGate g) => g.Guid == id);
	}
}
