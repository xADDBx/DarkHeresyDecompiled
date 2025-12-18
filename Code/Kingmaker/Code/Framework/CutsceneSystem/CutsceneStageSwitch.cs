using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Code.Framework.CutsceneSystem;

[Serializable]
public class CutsceneStageSwitch
{
	[SerializeField]
	private int m_StageId;

	[SerializeField]
	private List<string> m_NextBlockIds = new List<string>();

	[SerializeField]
	private string m_Comment;

	public string Comment => m_Comment;

	public int StageId => m_StageId;

	public List<string> NextBlocks => m_NextBlockIds;

	public CutsceneStageSwitch(int stageId)
	{
		m_StageId = stageId;
	}

	public void SetDescription(string description)
	{
		m_Comment = description;
	}
}
