using System;
using UnityEngine;

namespace Kingmaker.Code.View.Bridge.Data;

[Serializable]
public class DialogCueColors
{
	[Header("Name")]
	public Color32 NameColorMultiplyer;

	[Header("Narrator")]
	public static string NarratorColorStringID = "[NARRATOR_COLOR]";

	public Color32 Narrator;

	[Space]
	public SoulMarkShiftColors SoulMarkShiftColors;

	public SkillCheckColors SkillCheckColors;

	public Color32 DevComment;
}
