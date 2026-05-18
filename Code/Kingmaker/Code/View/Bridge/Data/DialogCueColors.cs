using System;
using Kingmaker.Code.Framework.Settings.UISettings;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Code.View.Bridge.Data;

[Serializable]
public class DialogCueColors
{
	[FormerlySerializedAs("NameColorMultiplyer")]
	[Header("Name")]
	public Color32 NameColorMultiplayer;

	[Header("Narrator")]
	public static string NarratorColorStringID = "[NARRATOR_COLOR]";

	public Color32 Narrator;

	[Space]
	public SoulMarkShiftColors SoulMarkShiftColors;

	public SkillCheckColors SkillCheckColors;

	public Color32 DevComment;

	[SerializeField]
	private UIColorList m_SpeakerColorList;

	public Color32 GetSpeakerColor(string speakerName, bool isOverrideColor, Color32 defaultColor)
	{
		int valueOrDefault = (m_SpeakerColorList?.Colors?.Length).GetValueOrDefault();
		if (isOverrideColor || valueOrDefault == 0)
		{
			return defaultColor;
		}
		int num = speakerName.GetHashCode() % valueOrDefault;
		if (num < 0)
		{
			num += valueOrDefault - 1;
		}
		return m_SpeakerColorList.Colors[num];
	}
}
