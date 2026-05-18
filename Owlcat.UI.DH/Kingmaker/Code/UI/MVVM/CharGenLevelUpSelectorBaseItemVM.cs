using System;
using Kingmaker.Blueprints;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.UI.UIUtils;
using Kingmaker.UIDataProvider;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenLevelUpSelectorBaseItemVM : SelectionGroupEntityVM
{
	protected readonly ReactiveProperty<string> m_Acronym = new ReactiveProperty<string>(string.Empty);

	protected BlueprintScriptableObject m_Blueprint;

	protected CharGenLevelUpNestedListHeaderVM m_ParentNodeVm;

	protected readonly ReactiveProperty<bool> m_IsFavorite = new ReactiveProperty<bool>(value: false);

	protected readonly ReactiveProperty<bool> m_CanShowFavorite = new ReactiveProperty<bool>(value: false);

	protected readonly ReactiveProperty<bool> m_IsRecommended = new ReactiveProperty<bool>(value: false);

	protected readonly ReactiveProperty<bool> m_IsShowed = new ReactiveProperty<bool>(value: true);

	protected readonly ReactiveProperty<string> m_Label = new ReactiveProperty<string>(string.Empty);

	protected readonly ReactiveProperty<string> m_SubLabel = new ReactiveProperty<string>(string.Empty);

	private readonly Action<CharGenLevelUpSelectorBaseItemVM> m_OnHover;

	protected readonly ReactiveProperty<Sprite> m_Sprite = new ReactiveProperty<Sprite>(null);

	protected readonly ReactiveProperty<Color> m_SpriteColor = new ReactiveProperty<Color>(Color.white);

	protected readonly ReactiveProperty<LEVEL_UP_ITEM_STATE> m_State = new ReactiveProperty<LEVEL_UP_ITEM_STATE>(LEVEL_UP_ITEM_STATE.Available);

	protected readonly ReactiveProperty<TalentIconInfo> m_TalentIconInfo = new ReactiveProperty<TalentIconInfo>(null);

	public ReadOnlyReactiveProperty<bool> IsShowed => m_IsShowed;

	public ReadOnlyReactiveProperty<bool> IsFavorite => m_IsFavorite;

	public ReadOnlyReactiveProperty<bool> CanShowFavorite => m_CanShowFavorite;

	public ReadOnlyReactiveProperty<bool> IsRecommended => m_IsRecommended;

	public ReadOnlyReactiveProperty<string> Label => m_Label;

	public ReadOnlyReactiveProperty<string> SubLabel => m_SubLabel;

	public ReadOnlyReactiveProperty<string> Acronym => m_Acronym;

	public ReadOnlyReactiveProperty<Sprite> Sprite => m_Sprite;

	public ReadOnlyReactiveProperty<Color> SpriteColor => m_SpriteColor;

	public ReadOnlyReactiveProperty<LEVEL_UP_ITEM_STATE> State => m_State;

	public ReadOnlyReactiveProperty<TalentIconInfo> TalentIconInfo => m_TalentIconInfo;

	public TooltipBaseTemplate Template { get; protected set; }

	public int NestingLevel { get; protected set; }

	public BlueprintScriptableObject Blueprint => m_Blueprint;

	public CharGenLevelUpSelectorBaseItemVM(IUIDataProvider uiData, Action<CharGenLevelUpSelectorBaseItemVM> onHover, CharGenLevelUpNestedListHeaderVM parentNodeVm = null, bool allowSwitchOff = true, TooltipBaseTemplate customTooltip = null)
		: base(allowSwitchOff)
	{
		m_OnHover = onHover;
		SetParentNode(parentNodeVm);
		Template = customTooltip;
		m_Label.Value = uiData?.Name;
		m_Sprite.Value = uiData?.Icon.GetDefaultIfNull(DefaultImageType.Ability);
		m_Acronym.Value = ((m_Sprite.Value == null) ? UIUtilityAbilities.GetAbilityAcronym(uiData?.Name) : string.Empty);
		if (uiData is BlueprintFeature blueprintFeature)
		{
			m_TalentIconInfo.Value = blueprintFeature?.TalentIconInfo;
		}
	}

	public virtual void OnHover(bool isHovered)
	{
		m_OnHover?.Invoke(isHovered ? this : null);
	}

	public void UpdateSelectionInParent(bool value)
	{
		m_ParentNodeVm?.UpdateSelectionFromChild(value);
	}

	public void SetParentNode(CharGenLevelUpNestedListHeaderVM parentNodeVm = null)
	{
		m_ParentNodeVm = parentNodeVm;
		NestingLevel = parentNodeVm?.NestingLevel ?? 0;
		parentNodeVm?.IsExpanded.CombineLatest(parentNodeVm?.IsShowed, (bool e, bool s) => (e: e, s: s)).Subscribe(delegate((bool e, bool s) v)
		{
			m_IsShowed.Value = v.e && v.s;
		}).AddTo(this);
	}

	public virtual void UpdateAccessibility(LEVEL_UP_ITEM_STATE value)
	{
		m_State.Value = value;
	}

	public void SetFavorite(bool canShowFavorite, bool isFavorite)
	{
		m_CanShowFavorite.Value = canShowFavorite;
		m_IsFavorite.Value = isFavorite;
	}

	public void ToggleFavorite()
	{
		m_IsFavorite.Value = !m_IsFavorite.Value;
	}

	protected override void DoSelectMe()
	{
	}
}
