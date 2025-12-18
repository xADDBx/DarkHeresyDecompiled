using Kingmaker.UnitLogic.Progression.Paths;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenCareerSelectionItemVM : SelectionGroupEntityVM
{
	private readonly BlueprintCareerPath m_CareerPath;

	private readonly ReactiveProperty<string> m_Label = new ReactiveProperty<string>(string.Empty);

	private readonly ReactiveProperty<Sprite> m_Sprite = new ReactiveProperty<Sprite>(null);

	public ReadOnlyReactiveProperty<string> Label => m_Label;

	public ReadOnlyReactiveProperty<Sprite> Sprite => m_Sprite;

	public BlueprintCareerPath CareerPath => m_CareerPath;

	public CharGenCareerSelectionItemVM(BlueprintCareerPath careerPath)
		: base(allowSwitchOff: true)
	{
		m_CareerPath = careerPath;
		m_Label.Value = careerPath.Name;
		m_Sprite.Value = careerPath.Icon;
	}

	protected override void DoSelectMe()
	{
	}
}
