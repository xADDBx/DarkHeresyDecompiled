using System;
using Code.View.UI.MVVM.Tooltip.Templates;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.UnitLogic.Progression.Paths;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenCareerSelectionItemVM : SelectionGroupEntityVM
{
	private readonly BlueprintCareerPath m_CareerPath;

	private readonly Action<CharGenCareerSelectionItemVM> m_OnHover;

	private readonly ReactiveProperty<string> m_Label = new ReactiveProperty<string>(string.Empty);

	private readonly ReactiveProperty<Sprite> m_Sprite = new ReactiveProperty<Sprite>(null);

	public ReadOnlyReactiveProperty<string> Label => m_Label;

	public ReadOnlyReactiveProperty<Sprite> Sprite => m_Sprite;

	public BlueprintCareerPath CareerPath => m_CareerPath;

	public TooltipBaseTemplate Template { get; private set; }

	public CharGenCareerSelectionItemVM(BlueprintCareerPath careerPath, Action<CharGenCareerSelectionItemVM> onHover)
		: base(allowSwitchOff: true)
	{
		m_CareerPath = careerPath;
		m_OnHover = onHover;
		m_Label.Value = careerPath.Name;
		m_Sprite.Value = careerPath.Icon.GetDefaultIfNull(DefaultImageType.Ability);
		Template = new TooltipTemplateChargenCareerPath(careerPath);
	}

	public void OnHover(bool isHovered)
	{
		m_OnHover?.Invoke(isHovered ? this : null);
	}

	protected override void DoSelectMe()
	{
	}
}
