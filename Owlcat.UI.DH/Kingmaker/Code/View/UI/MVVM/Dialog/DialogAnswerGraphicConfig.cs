using System;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.Dialog;

[Serializable]
public class DialogAnswerGraphicConfig
{
	public Color32 SkillcheckInAnswerErrorColor;

	public Color32 SkillcheckInAnswerColor;

	public Color32 ExchangeInAnswerColor;

	public Color32 SkillcheckRequirementSucceededInAnswerColor;

	public Color32 ConditionFailSpriteColor;

	public Color32 ConditionSuccessSpriteColor;

	public Color32 DialogExchangeSpriteColor;

	public Color32 DialogCloseCaseSpriteColor;

	public Color32 DiceSpriteColor;

	public string ConditionSuccessSpriteID => "UI_Dialog_ConditionSuccess";

	public string ConditionFailSpriteID => "UI_Dialog_ConditionFail";

	public string DialogExchangeSpriteID => "UI_DialogExchange";

	public string DialogDetectiveRelatedItemsSpriteID => "UI_DialogDetective";

	public string DialogDetectiveCloseCaseSpriteID => "UI_Dialog_CloseCase";

	public string DiceSpriteID => "UI_Dice";
}
