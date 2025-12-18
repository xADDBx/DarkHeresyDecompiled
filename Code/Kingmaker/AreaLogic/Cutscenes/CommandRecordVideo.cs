using System;
using Kingmaker.Utility.UnityExtensions;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes;

[Obsolete]
[TypeId("60b1ed0ef15afff4ea25897c6de8c689")]
public class CommandRecordVideo : CommandBase
{
	public string Folder = "ScreenshotFolder";

	public int FrameRate = 60;

	public int Width = 4096;

	public int Height = 2160;

	public float RecordTime = 2f;

	private VideoRecorder m_Player;

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		m_Player = new GameObject("[Video recorder]").AddComponent<VideoRecorder>();
		m_Player.Folder = Folder;
		m_Player.FrameRate = FrameRate;
		m_Player.Width = Width;
		m_Player.Height = Height;
		m_Player.RecordTime = RecordTime;
		m_Player.StartRecording();
	}

	protected override void OnSkip(CutscenePlayerData player)
	{
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return !m_Player.IsRecording;
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
	}
}
