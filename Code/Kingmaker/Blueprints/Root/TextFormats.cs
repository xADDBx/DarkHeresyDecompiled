using System;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class TextFormats
{
	[Tooltip("{0} - Cargo Volume, {1} - Label ({10%} {of Melee Weaponry Cargo})")]
	public string TooltipItemFooterFormat = "<cspace=-1><b>{0}</b></cspace> <size=95%>{1}</size>";

	[Tooltip("{0} - Current Value, {1} - Max Value ({10}| max{12})")]
	public string WeaponSetTextFormat = "<b>{0}<voffset=0.1em><size=74%>|</size></voffset></b><size=50%><color=#{color}> max</size> <size=66%><b>{1}</b></size></color>";

	[Tooltip("{0} - max range label, {1} range value")]
	public string UITooltipMaxRangeFormat = "({0} <b>{1}</b>)";

	[Tooltip("{0} - question")]
	public string QuestionFormat = "<b><size=125%>•</size></b> {0}";

	[Tooltip("{0} - number before percent")]
	public string PercentFormat = "{0}%";

	[Tooltip("{0} - label, {1} - color")]
	public string DetectiveEntityWithNumber = "{0} <color=#{1}>#</color>";

	[Tooltip("{0} - plain text")]
	public string PlainTextTrueAnswerFormat = "<line-height=75%><color=#000000><size=105%><i>{0}</i></size></color></line-height>";

	[Tooltip("{0} - other clues list")]
	public string ToOtherCluesStudyFormat = "<i><color=#ACDBC3>{0}</color></i>";
}
