using System;

namespace Kingmaker.Blueprints.Root;

[Serializable]
[Flags]
public enum AnswerDebugValues
{
	None = 0,
	All = -1,
	ChangeAppearance = 1,
	ShowLug = 2,
	ShowAnswersAtCard = 4,
	ShowAnswersLines = 8,
	ChangeDotColor = 0x10,
	AnimateAnswersLines = 0x20,
	CommonLinesAreGreen = 0x80,
	ShowTierToastOnUpgrade = 0x100,
	ShowTierToastOnDowngrade = 0x200,
	ShowAnswersListAtCard = 0x400,
	ShowNotSelectedAnswersAsCommon = 0x800,
	ShowHighlightOnSelectedAnswer = 0x1000
}
