using System.Collections.Generic;
using System.Linq;
using Code.View.UI.UIUtils;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.Progression.Paths;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public abstract class BaseRankEntryFeatureVM : CharInfoFeatureVM
{
	protected readonly ReactiveProperty<RankFeatureState> m_FeatureState = new ReactiveProperty<RankFeatureState>(RankFeatureState.NotActive);

	private readonly ReactiveProperty<bool> m_FocusedState = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsCurrentRankEntryItem = new ReactiveProperty<bool>();

	public readonly CareerPathVM CareerPathVM;

	protected readonly UIFeature UIFeature;

	protected readonly ReactiveCommand<Unit> OnUpdateState = new ReactiveCommand<Unit>();

	private bool m_HasFavorites;

	private TooltipBaseTemplate m_HintTooltip;

	public ReadOnlyReactiveProperty<RankFeatureState> FeatureState => m_FeatureState;

	public ReadOnlyReactiveProperty<bool> FocusedState => m_FocusedState;

	public ReadOnlyReactiveProperty<bool> IsCurrentRankEntryItem => m_IsCurrentRankEntryItem;

	public BlueprintFeature Feature => UIFeature.Feature;

	public BaseUnitProgressionVM UnitProgressionVM => CareerPathVM.UnitProgressionVM;

	public bool IsRecommended
	{
		get
		{
			CareerPathUIMetaData careerPathUIMetaData = CareerPathVM.CareerPathUIMetaData;
			if (careerPathUIMetaData == null)
			{
				return false;
			}
			return careerPathUIMetaData.RecommendedFeatures.Contains(UIFeature.Feature);
		}
	}

	public bool IsFavorite
	{
		get
		{
			if (Game.Instance.Player.UISettings.UnitToFavoritesMap.TryGetValue(UnitProgressionVM.Unit.CurrentValue.Ref, out var value))
			{
				return value.Contains(UIFeature.Feature.ToReference<BlueprintFeature.Reference>());
			}
			return false;
		}
	}

	public bool HasFavorites => m_HasFavorites;

	public string HintText
	{
		get
		{
			if (IsKeystoneFeature)
			{
				return UIStrings.Instance.CharacterSheet.KeystoneFeaturesHeader.Text;
			}
			if (IsUltimateFeature)
			{
				return UIStrings.Instance.CharacterSheet.UltimateAbilityFeatureGroupHint.Text;
			}
			if (IsKeystoneAbility)
			{
				return UIStrings.Instance.CharacterSheet.KeystoneAbilitiesHeader.Text;
			}
			return UIStrings.Instance.CharacterSheet.HeaderImprovement.Text;
		}
	}

	public TooltipBaseTemplate HintTooltip => m_HintTooltip ?? (m_HintTooltip = CreateHintTooltip());

	private bool IsKeystoneFeature
	{
		get
		{
			CareerPathUIMetaData careerPathUIMetaData = CareerPathVM.CareerPathUIMetaData;
			if (careerPathUIMetaData == null)
			{
				return false;
			}
			return careerPathUIMetaData.KeystoneFeatures.Contains(UIFeature.Feature);
		}
	}

	private bool IsUltimateFeature
	{
		get
		{
			CareerPathUIMetaData careerPathUIMetaData = CareerPathVM.CareerPathUIMetaData;
			if (careerPathUIMetaData == null)
			{
				return false;
			}
			return careerPathUIMetaData.UltimateFeatures.Contains(UIFeature.Feature);
		}
	}

	private bool IsKeystoneAbility
	{
		get
		{
			CareerPathUIMetaData careerPathUIMetaData = CareerPathVM.CareerPathUIMetaData;
			if (careerPathUIMetaData == null)
			{
				return false;
			}
			return careerPathUIMetaData.KeystoneAbilities.Any((BlueprintAbility a) => UIUtilityUnit.GetBlueprintUnitFactFromFact<BlueprintAbility>(UIFeature.Feature)?.Contains(a) ?? false);
		}
	}

	protected BaseRankEntryFeatureVM(CareerPathVM careerPathVM, UIFeature uiFeature)
		: base(uiFeature, careerPathVM.Unit)
	{
		CareerPathVM = careerPathVM;
		UIFeature = uiFeature;
		AddDisposable(UnitProgressionVM.CurrentRankEntryItem.Subscribe(delegate(IRankEntrySelectItem item)
		{
			m_IsCurrentRankEntryItem.Value = item == this;
		}));
	}

	private TooltipBaseTemplate CreateHintTooltip()
	{
		if (IsKeystoneFeature || IsKeystoneAbility)
		{
			return new TooltipTemplateSimple(UIStrings.Instance.CharacterSheet.KeystoneFeaturesHeader.Text, UIStrings.Instance.CharacterSheet.KeystoneFeaturesChargenDescription.Text);
		}
		if (IsUltimateFeature)
		{
			return new TooltipTemplateSimple(UIStrings.Instance.CharacterSheet.UltimateAbilityFeatureGroupHint.Text, UIStrings.Instance.CharacterSheet.UltimateAbilitiesChargenDescription.Text);
		}
		return new TooltipTemplateSimple(UIStrings.Instance.CharacterSheet.HeaderImprovement, UIStrings.Instance.CharacterSheet.PredefinedAbilitiesChargenDescription.Text);
	}

	protected abstract void UpdateFeatureState();

	public void UpdateState(LevelUpManager levelUpManager)
	{
		UpdateFeatureState();
		OnUpdateState.Execute(Unit.Default);
	}

	public abstract void Select();

	public abstract bool CanSelect();

	public void SetHasFavorites(bool hasFavorites)
	{
		m_HasFavorites = hasFavorites;
	}

	public void SetFavoritesState(bool state)
	{
		Dictionary<EntityRef<MechanicEntity>, List<BlueprintFeature.Reference>> unitToFavoritesMap = Game.Instance.Player.UISettings.UnitToFavoritesMap;
		EntityRef<MechanicEntity> @ref = UnitProgressionVM.Unit.CurrentValue.Ref;
		if (!unitToFavoritesMap.TryGetValue(@ref, out var value))
		{
			value = new List<BlueprintFeature.Reference>();
			unitToFavoritesMap.Add(@ref, value);
		}
		BlueprintFeature.Reference favoriteFeatureRef = UIFeature.Feature.ToReference<BlueprintFeature.Reference>();
		if (state)
		{
			if (!value.Contains(favoriteFeatureRef))
			{
				value.Add(favoriteFeatureRef);
				unitToFavoritesMap[@ref] = value;
			}
		}
		else if (value.Contains(favoriteFeatureRef))
		{
			value.RemoveAll((BlueprintFeature.Reference f) => f.Equals(favoriteFeatureRef));
			unitToFavoritesMap[@ref] = value;
		}
	}

	public void SetFocusOn(BaseRankEntryFeatureVM featureVM)
	{
		m_FocusedState.Value = featureVM == this;
	}
}
