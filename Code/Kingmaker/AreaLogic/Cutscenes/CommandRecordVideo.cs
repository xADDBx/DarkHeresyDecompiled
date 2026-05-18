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

	protected override CommandResult OnRun(CutscenePlayerData player, bool skipping)
	{
		m_Player = new GameObject("[Video recorder]").AddComponent<VideoRecorder>();
		m_Player.Folder = Folder;
		m_Player.FrameRate = FrameRate;
		m_Player.Width = Width;
		m_Player.Height = Height;
		m_Player.RecordTime = RecordTime;
		m_Player.StartRecording();
		return CommandResult.Success;
	}

	protected override CommandResult OnSkip(CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	protected override CommandResult OnStop(CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	public override CommandResult Interrupt(CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return !m_Player.IsRecording;
	}

	protected override CommandResult OnSetTime(double time, CutscenePlayerData player)
	{
		return CommandResult.Success;
	}
}
