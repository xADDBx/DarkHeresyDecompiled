using Kingmaker.UnitLogic.Levelup.Selections;
using Owlcat.UI;
using UnityEngine;

namespace Assets.Code.View.UI.MVVM;

public class ChargenProgressionFeatureLevelVM : ViewModel
{
	public int Index { get; private set; }

	public TooltipBaseTemplate Tooltip { get; private set; }

	public Sprite Icon { get; private set; }

	public string Label { get; private set; }

	public string Acronym { get; private set; }

	public TalentIconInfo TalentIconInfo { get; private set; }

	public ChargenProgressionFeatureLevelVM(int index, Sprite icon = null, string label = null, string acronym = null, TalentIconInfo talentIconInfo = null, TooltipBaseTemplate tooltip = null)
	{
		Index = index;
		Icon = icon;
		Label = label;
		Acronym = acronym;
		TalentIconInfo = talentIconInfo;
		Tooltip = tooltip;
	}
}
