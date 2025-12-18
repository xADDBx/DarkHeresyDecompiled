using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.UIUtils;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.UI;
using R3;

namespace Assets.Code.View.UI.MVVM;

public class CharInfoTalentItemVM : ViewModel
{
	public readonly string Name;

	public readonly string Acronym;

	public readonly TooltipBaseTemplate Tooltip;

	public readonly TalentIconInfo TalentInfo;

	private readonly ReactiveProperty<bool> m_IsVisible = new ReactiveProperty<bool>(value: true);

	public ReadOnlyReactiveProperty<bool> IsVisible => m_IsVisible;

	public CharInfoTalentItemVM(BlueprintFeature talent, ReadOnlyReactiveProperty<string> searchedString, MechanicEntity unit = null)
	{
		Name = GetName(talent);
		Acronym = UIUtilityAbilities.GetAbilityAcronym(talent.Name);
		searchedString.Subscribe(delegate(string s)
		{
			m_IsVisible.Value = Name.ToLower().Contains(s.ToLower());
		}).AddTo(this);
		Tooltip = new TooltipTemplateTalent(talent, unit);
		TalentInfo = talent.TalentIconInfo;
	}

	private string GetName(BlueprintFeature talent)
	{
		if (!string.IsNullOrEmpty(talent.LocalizedName.Text))
		{
			return talent.LocalizedName.Text;
		}
		if (!string.IsNullOrEmpty(talent.Name))
		{
			return talent.Name;
		}
		return talent.AssetName;
	}
}
