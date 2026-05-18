using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Framework;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UI.UIUtils;
using Kingmaker.UIDataProvider;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Levelup.Selections;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoFeatureVM : SelectionGroupEntityVM, IHasTooltipTemplate, IUIDataProvider
{
	public Sprite Icon;

	public string Acronym;

	public string DisplayName;

	public string Description;

	public string FactDescription;

	public string TimeLeft;

	public int? Rank;

	public Ability Ability;

	public TalentIconInfo TalentIconsInfo;

	public readonly string SourceName;

	public readonly string StacksText;

	protected ReactiveProperty<TooltipBaseTemplate> m_Tooltip;

	private readonly object m_TooltipSource;

	public bool IsActive = true;

	public ReadOnlyReactiveProperty<TooltipBaseTemplate> Tooltip => m_Tooltip ?? (m_Tooltip = CreateTooltip());

	string IUIDataProvider.Name => DisplayName;

	string IUIDataProvider.Description => Description;

	Sprite IUIDataProvider.Icon => Icon;

	string IUIDataProvider.NameForAcronym => Acronym;

	private ReactiveProperty<TooltipBaseTemplate> CreateTooltip()
	{
		object tooltipSource = m_TooltipSource;
		if (!(tooltipSource is Buff buff))
		{
			if (!(tooltipSource is Feature feature))
			{
				if (!(tooltipSource is Ability ability))
				{
					if (!(tooltipSource is UIFeature uiFeature))
					{
						if (tooltipSource is ToggleAbility ability2)
						{
							return new ReactiveProperty<TooltipBaseTemplate>(new TooltipTemplateToggleAbility(ability2));
						}
						return new ReactiveProperty<TooltipBaseTemplate>();
					}
					return new ReactiveProperty<TooltipBaseTemplate>(new TooltipTemplateUIFeature(uiFeature));
				}
				return new ReactiveProperty<TooltipBaseTemplate>(new TooltipTemplateAbility(ability.Data));
			}
			return new ReactiveProperty<TooltipBaseTemplate>(new TooltipTemplateFeature(feature));
		}
		return new ReactiveProperty<TooltipBaseTemplate>(new TooltipTemplateBuff(buff));
	}

	public CharInfoFeatureVM(Buff buff, MechanicEntity unit)
		: base(allowSwitchOff: false)
	{
		Icon = buff.Icon;
		DisplayName = buff.Name;
		IsActive = buff.Active;
		using (GameLogContext.Scope)
		{
			GameLogContext.DescriptionOwner = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)unit;
			FactDescription = buff.Description;
		}
		Acronym = UIUtilityAbilities.GetAbilityAcronym(buff.Blueprint.Name);
		SourceName = buff.Caster?.Name ?? string.Empty;
		StacksText = buff.GetStacksText();
		FillTimeLeft(buff);
		FillDescription();
		m_TooltipSource = buff;
	}

	public CharInfoFeatureVM(Feature feature, MechanicEntity unit)
		: base(allowSwitchOff: false)
	{
		Icon = feature.Icon;
		DisplayName = feature.Name;
		IsActive = feature.Active;
		using (GameLogContext.Scope)
		{
			GameLogContext.DescriptionOwner = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)unit;
			FactDescription = feature.Description;
		}
		Rank = feature.Rank;
		Acronym = UIUtilityAbilities.GetAbilityAcronym(feature.Blueprint);
		SourceName = feature.Caster?.Name ?? string.Empty;
		StacksText = string.Empty;
		FillDescription();
		m_TooltipSource = feature;
		TalentIconsInfo = feature.Blueprint.TalentIconInfo;
	}

	public CharInfoFeatureVM(Ability ability, MechanicEntity unit)
		: base(allowSwitchOff: false)
	{
		Ability = ability;
		Icon = ability.Icon;
		DisplayName = ability.Name;
		IsActive = ability.Data.Fact?.Active ?? true;
		using (GameLogContext.Scope)
		{
			GameLogContext.DescriptionOwner = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)unit;
			FactDescription = ability.Description;
		}
		Acronym = UIUtilityAbilities.GetAbilityAcronym(ability.Blueprint.Name);
		SourceName = string.Empty;
		StacksText = string.Empty;
		FillDescription();
		m_TooltipSource = ability;
	}

	public CharInfoFeatureVM(UIFeature uiFeature, MechanicEntity unit)
		: base(allowSwitchOff: false)
	{
		BlueprintAbility abilityFromFeature = RankEntrySelectionFeaturesUtils.GetAbilityFromFeature(uiFeature.Feature);
		Icon = ((abilityFromFeature != null) ? abilityFromFeature.Icon : uiFeature.Icon);
		DisplayName = ((abilityFromFeature != null) ? abilityFromFeature.Name : uiFeature.Name);
		using (GameLogContext.Scope)
		{
			GameLogContext.DescriptionOwner = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)unit;
			FactDescription = uiFeature.Description;
		}
		Rank = uiFeature.Rank;
		SourceName = string.Empty;
		StacksText = string.Empty;
		Acronym = UIUtilityAbilities.GetAbilityAcronym(uiFeature.Feature);
		FillDescription();
		m_TooltipSource = uiFeature;
		TalentIconsInfo = uiFeature.TalentIconsInfo;
	}

	private void FillTimeLeft(Buff buff)
	{
		if (buff.IsPermanent)
		{
			TimeLeft = ConfigRoot.Instance.LocalizedTexts.UserInterfacesText.CharacterSheet.Permanent.Text;
			return;
		}
		string arg = ((buff.ExpirationInRounds == 1) ? ConfigRoot.Instance.LocalizedTexts.UserInterfacesText.TurnBasedTexts.Round.Text : ConfigRoot.Instance.LocalizedTexts.UserInterfacesText.TurnBasedTexts.Rounds.Text);
		TimeLeft = $"{buff.ExpirationInRounds} {arg}";
	}

	private void FillDescription()
	{
		Description = (IsActive ? string.Empty : $"<color=#B2443F>{UIStrings.Instance.CharacterSheet.DeactivatedFeature.Text}</color>");
	}

	protected override void DoSelectMe()
	{
	}

	protected override void DisposeImplementation()
	{
	}

	public void SetAvailable(bool expand)
	{
		m_IsAvailable.Value = expand;
	}

	public TooltipBaseTemplate TooltipTemplate()
	{
		return Tooltip.CurrentValue;
	}
}
