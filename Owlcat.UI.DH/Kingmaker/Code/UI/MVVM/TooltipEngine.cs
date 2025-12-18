using System.Collections.Generic;
using Code.View.UI.MVVM.Tooltip;
using Code.View.UI.MVVM.Tooltip.Bricks;
using Code.View.UI.MVVM.Tooltip.Bricks.Items;
using Kingmaker.Code.UI.MVVM.View;
using Kingmaker.Code.View.UI.MVVM.Tooltip.Bricks;
using Kingmaker.UI.MVVM.View.Tooltip.PC.Bricks;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public static class TooltipEngine
{
	private static readonly List<Vector2> DefaultPivots = new List<Vector2>
	{
		new Vector2(0.5f, 1f),
		new Vector2(0.5f, 0f),
		new Vector2(0f, 0.5f),
		new Vector2(1f, 0.5f),
		new Vector2(0f, 0f),
		new Vector2(1f, 0f),
		new Vector2(0f, 1f),
		new Vector2(1f, 1f)
	};

	public static MonoBehaviour GetBrickView(TooltipBricksView config, TooltipBaseBrickVM vm)
	{
		if (!(vm is TooltipBrickTimerVM source))
		{
			if (!(vm is TooltipBrickTextVM source2))
			{
				if (!(vm is TooltipBrickSeparatorVM source3))
				{
					if (!(vm is TooltipBrickSpaceVM source4))
					{
						if (!(vm is TooltipBrickTitleVM source5))
						{
							if (!(vm is TooltipBrickPictureVM source6))
							{
								if (!(vm is TooltipBrickPortraitAndNameVM source7))
								{
									if (!(vm is TooltipBrickItemIconAndNameVM source8))
									{
										if (!(vm is TooltipBrickPFIconAndNameVM source9))
										{
											if (!(vm is TooltipBrickIconAndNameVM source10))
											{
												if (!(vm is TooltipBrickFactionStatusVM source11))
												{
													if (!(vm is TooltipBrickItemFooterVM source12))
													{
														if (!(vm is TooltipBrickTripleTextVM source13))
														{
															if (!(vm is TooltipBrickDoubleTextVM source14))
															{
																if (!(vm is TooltipBrickIconValueStatVM source15))
																{
																	if (!(vm is TooltipBrickIconStatValueVM source16))
																	{
																		if (!(vm is TooltipBrickTwoColumnsStatVM source17))
																		{
																			if (!(vm is TooltipBrickValueStatFormulaVM source18))
																			{
																				if (!(vm is TooltipBrickBuffVM source19))
																				{
																					if (!(vm is TooltipBrickBuffDOTVM source20))
																					{
																						if (!(vm is TooltipBrickWeaponDOTInitialDamageVM source21))
																						{
																							if (!(vm is TooltipBrickFeatureVM tooltipBrickFeatureVM))
																							{
																								if (!(vm is TooltipBrickAbilityScoresBlockVM source22))
																								{
																									if (!(vm is TooltipBrickAbilityScoresVM source23))
																									{
																										if (!(vm is TooltipBrickButtonVM source24))
																										{
																											if (!(vm is TooltipBrickHistoryManagementVM source25))
																											{
																												if (!(vm is TooltipBrickNonStackVm source26))
																												{
																													if (!(vm is TooltipBrickPrerequisiteVM source27))
																													{
																														if (!(vm is TooltipBrickRateVM source28))
																														{
																															if (!(vm is TooltipBrickFeatureDescriptionVM source29))
																															{
																																if (!(vm is TooltipBrickAbilityTargetVM source30))
																																{
																																	if (!(vm is TooltipBrickHintVM source31))
																																	{
																																		if (!(vm is TooltipBrickUnifiedStatusVM source32))
																																		{
																																			if (!(vm is TooltipBrickIconPatternVM source33))
																																			{
																																				if (!(vm is TooltipBricksGroupVM tooltipBricksGroupVM))
																																				{
																																					if (vm is TooltipBrickPortraitFeaturesVM source34)
																																					{
																																						TooltipBrickPortraitFeaturesView widget = WidgetFactory.GetWidget(config.PortraitFeaturesView, activate: true, strictMatching: true);
																																						widget.Bind(source34);
																																						return widget;
																																					}
																																					if (vm is TooltipBrickSliderVM source35)
																																					{
																																						TooltipBrickSliderView widget2 = WidgetFactory.GetWidget(config.SliderView, activate: true, strictMatching: true);
																																						widget2.Bind(source35);
																																						return widget2;
																																					}
																																					if (vm is TooltipBrickWeaponSetVM source36)
																																					{
																																						TooltipBrickWeaponSetView widget3 = WidgetFactory.GetWidget(config.WeaponSetView, activate: true, strictMatching: true);
																																						widget3.Bind(source36);
																																						return widget3;
																																					}
																																					if (vm is TooltipBrickEventVM source37)
																																					{
																																						TooltipBrickEventsView widget4 = WidgetFactory.GetWidget(config.EventsView, activate: true, strictMatching: true);
																																						widget4.Bind(source37);
																																						return widget4;
																																					}
																																					if (vm is TooltipBrickIconAndTextWithCustomColorsVM source38)
																																					{
																																						TooltipBrickIconAndTextWithCustomColorsView widget5 = WidgetFactory.GetWidget(config.IconAndTextWithCustomColorsView, activate: true, strictMatching: true);
																																						widget5.Bind(source38);
																																						return widget5;
																																					}
																																					if (vm is TooltipBrickShotDirectionVM source39)
																																					{
																																						TooltipBrickShotDeviationView widget6 = WidgetFactory.GetWidget(config.BrickShotDeviationView, activate: true, strictMatching: true);
																																						widget6.Bind(source39);
																																						return widget6;
																																					}
																																					if (vm is TooltipBrickShotDirectionWithNameVM source40)
																																					{
																																						TooltipBrickShotDeviationWithNameView widget7 = WidgetFactory.GetWidget(config.BrickShotDeviationWithNameView, activate: true, strictMatching: true);
																																						widget7.Bind(source40);
																																						return widget7;
																																					}
																																					if (vm is TooltipBrickWidgetVM source41)
																																					{
																																						TooltipBrickWidgetView widget8 = WidgetFactory.GetWidget(config.BrickWidgetView, activate: true, strictMatching: true);
																																						widget8.Bind(source41);
																																						return widget8;
																																					}
																																					if (vm is TooltipBrickCalculatedFormulaVM source42)
																																					{
																																						TooltipBrickCalculatedFormulaView widget9 = WidgetFactory.GetWidget(config.CalculatedFormulaView, activate: true, strictMatching: true);
																																						widget9.Bind(source42);
																																						return widget9;
																																					}
																																					if (vm is TooltipBrickTitleWithIconVM source43)
																																					{
																																						TooltipBrickTitleWithIconView widget10 = WidgetFactory.GetWidget(config.BrickTitleWithIconView, activate: true, strictMatching: true);
																																						widget10.Bind(source43);
																																						return widget10;
																																					}
																																					if (vm is TooltipBrickRankEntrySelectionVM source44)
																																					{
																																						TooltipBrickRankEntrySelectionView widget11 = WidgetFactory.GetWidget(config.BrickRankEntrySelectionView, activate: true, strictMatching: true);
																																						widget11.Bind(source44);
																																						return widget11;
																																					}
																																					if (vm is TooltipBrickTextBackgroundVM source45)
																																					{
																																						TooltipBrickTextBackgroundView widget12 = WidgetFactory.GetWidget(config.BrickTextBackgroundView, activate: true, strictMatching: true);
																																						widget12.Bind(source45);
																																						return widget12;
																																					}
																																					if (vm is TooltipBrickAttributeVM source46)
																																					{
																																						TooltipBrickAttributeView widget13 = WidgetFactory.GetWidget(config.BrickAttributeView, activate: true, strictMatching: true);
																																						widget13.Bind(source46);
																																						return widget13;
																																					}
																																					if (vm is TooltipBrickExchangeVM source47)
																																					{
																																						TooltipBrickExchangeView widget14 = WidgetFactory.GetWidget(config.BrickExchangeView, activate: true, strictMatching: true);
																																						widget14.Bind(source47);
																																						return widget14;
																																					}
																																					if (vm is TooltipBrickCantUseVM source48)
																																					{
																																						TooltipBrickCantUseView widget15 = WidgetFactory.GetWidget(config.BrickCantUseView, activate: true, strictMatching: true);
																																						widget15.Bind(source48);
																																						return widget15;
																																					}
																																					if (vm is TooltipBrickCaseItemVM source49)
																																					{
																																						TooltipBrickCaseItemView widget16 = WidgetFactory.GetWidget(config.BrickCaseItemView, activate: true, strictMatching: true);
																																						widget16.Bind(source49);
																																						return widget16;
																																					}
																																					if (vm is TooltipBrickSettingsTextVM source50)
																																					{
																																						TooltipBrickSettingsTextView widget17 = WidgetFactory.GetWidget(config.BrickSettingsTextView, activate: true, strictMatching: true);
																																						widget17.Bind(source50);
																																						return widget17;
																																					}
																																					if (vm is TooltipBrickChanceVM source51)
																																					{
																																						TooltipBrickChanceView widget18 = WidgetFactory.GetWidget(config.BrickChanceView, activate: true, strictMatching: true);
																																						widget18.Bind(source51);
																																						return widget18;
																																					}
																																					if (vm is TooltipBrickIconTextVM source52)
																																					{
																																						TooltipBrickIconTextView widget19 = WidgetFactory.GetWidget(config.BrickIconTextView, activate: true, strictMatching: true);
																																						widget19.Bind(source52);
																																						return widget19;
																																					}
																																					if (vm is TooltipBrickIconTextValueVM source53)
																																					{
																																						TooltipBrickIconTextValueView widget20 = WidgetFactory.GetWidget(config.BrickIconTextValueView, activate: true, strictMatching: true);
																																						widget20.Bind(source53);
																																						return widget20;
																																					}
																																					if (vm is TooltipBrickTextValueVM source54)
																																					{
																																						TooltipBrickTextValueView widget21 = WidgetFactory.GetWidget(config.BrickTextValueView, activate: true, strictMatching: true);
																																						widget21.Bind(source54);
																																						return widget21;
																																					}
																																					if (vm is TooltipBrickTextSignatureValueVM source55)
																																					{
																																						TooltipBrickTextSignatureValueView widget22 = WidgetFactory.GetWidget(config.BrickTextSignatureValueView, activate: true, strictMatching: true);
																																						widget22.Bind(source55);
																																						return widget22;
																																					}
																																					if (vm is TooltipBrickDamageRangeVM source56)
																																					{
																																						TooltipBrickDamageRangeView widget23 = WidgetFactory.GetWidget(config.BrickDamageRangeView, activate: true, strictMatching: true);
																																						widget23.Bind(source56);
																																						return widget23;
																																					}
																																					if (vm is TooltipBrickMinimalAdmissibleDamageVM source57)
																																					{
																																						TooltipBrickMinimalAdmissibleDamageView widget24 = WidgetFactory.GetWidget(config.BrickMinimalAdmissibleDamageView, activate: true, strictMatching: true);
																																						widget24.Bind(source57);
																																						return widget24;
																																					}
																																					if (vm is TooltipBrickTriggeredAutoVM source58)
																																					{
																																						TooltipBrickTriggeredAutoView widget25 = WidgetFactory.GetWidget(config.BrickTriggeredAutoView, activate: true, strictMatching: true);
																																						widget25.Bind(source58);
																																						return widget25;
																																					}
																																					if (vm is TooltipBrickDamageNullifierVM source59)
																																					{
																																						TooltipBrickDamageNullifierView widget26 = WidgetFactory.GetWidget(config.BrickDamageNullifierView, activate: true, strictMatching: true);
																																						widget26.Bind(source59);
																																						return widget26;
																																					}
																																					if (vm is TooltipBrickNestedMessageVM source60)
																																					{
																																						TooltipBrickNestedMessageView widget27 = WidgetFactory.GetWidget(config.BrickNestedMessageView, activate: true, strictMatching: true);
																																						widget27.Bind(source60);
																																						return widget27;
																																					}
																																					if (vm is TooltipBrickItemHeaderVM source61)
																																					{
																																						TooltipBrickItemHeaderView widget28 = WidgetFactory.GetWidget(config.BrickItemHeaderView, activate: true, strictMatching: true);
																																						widget28.Bind(source61);
																																						return widget28;
																																					}
																																					if (vm is TooltipBrickEntityHeaderVM source62)
																																					{
																																						TooltipBrickEntityHeaderView widget29 = WidgetFactory.GetWidget(config.BrickEntityHeaderView, activate: true, strictMatching: true);
																																						widget29.Bind(source62);
																																						return widget29;
																																					}
																																					if (vm is TooltipBrickWeaponHeaderVM source63)
																																					{
																																						TooltipBrickWeaponHeaderView widget30 = WidgetFactory.GetWidget(config.BrickWeaponHeaderView, activate: true, strictMatching: true);
																																						widget30.Bind(source63);
																																						return widget30;
																																					}
																																					if (vm is TooltipBrickArmourHeaderVM source64)
																																					{
																																						TooltipBrickArmourHeaderView widget31 = WidgetFactory.GetWidget(config.BrickArmourHeaderView, activate: true, strictMatching: true);
																																						widget31.Bind(source64);
																																						return widget31;
																																					}
																																					if (vm is TooltipBrickItemRestrictionVM source65)
																																					{
																																						TooltipBrickItemRestrictionView widget32 = WidgetFactory.GetWidget(config.BrickItemRestrictionView, activate: true, strictMatching: true);
																																						widget32.Bind(source65);
																																						return widget32;
																																					}
																																					if (vm is TooltipBrickTagDescriptionVM source66)
																																					{
																																						TooltipBrickTagDescriptionView widget33 = WidgetFactory.GetWidget(config.BrickTagDescriptionView, activate: true, strictMatching: true);
																																						widget33.Bind(source66);
																																						return widget33;
																																					}
																																					if (vm is TooltipBrickLevelUpHeaderVM source67)
																																					{
																																						TooltipBrickLevelUpHeaderView widget34 = WidgetFactory.GetWidget(config.BrickLevelUpHeaderView, activate: true, strictMatching: true);
																																						widget34.Bind(source67);
																																						return widget34;
																																					}
																																					if (vm is TooltipBrickLevelUpTitledValueStatGroupVM source68)
																																					{
																																						TooltipBrickLevelUpTitledValueStatGroupView widget35 = WidgetFactory.GetWidget(config.BrickLevelUpTitledValueStatGroupView, activate: true, strictMatching: true);
																																						widget35.Bind(source68);
																																						return widget35;
																																					}
																																					if (vm is TooltipBrickLevelUpStatProgressionVM source69)
																																					{
																																						TooltipBrickLevelUpStatProgressionView widget36 = WidgetFactory.GetWidget(config.BrickLevelUpStatProgressionView, activate: true, strictMatching: true);
																																						widget36.Bind(source69);
																																						return widget36;
																																					}
																																					if (vm is TooltipBrickLevelUpSkillcheckBonusVM source70)
																																					{
																																						TooltipBrickLevelUpSkillcheckBonusView widget37 = WidgetFactory.GetWidget(config.BrickLevelUpSkillcheckBonusView, activate: true, strictMatching: true);
																																						widget37.Bind(source70);
																																						return widget37;
																																					}
																																					if (vm is TooltipBrickLevelUpFittingAbilitiesVM source71)
																																					{
																																						TooltipBrickLevelUpFittingAbilitiesView widget38 = WidgetFactory.GetWidget(config.BrickLevelpFittingAbilitiesView, activate: true, strictMatching: true);
																																						widget38.Bind(source71);
																																						return widget38;
																																					}
																																					if (vm is TooltipBrickLevelUpAbilityUpgradeDescriptionVM source72)
																																					{
																																						TooltipBrickLevelUpAbilityUpgradeDescriptionView widget39 = WidgetFactory.GetWidget(config.BrickLevelUpAbilityUpgradeDescriptionView, activate: true, strictMatching: true);
																																						widget39.Bind(source72);
																																						return widget39;
																																					}
																																					if (vm is TooltipBrickLevelUpFeatureVM source73)
																																					{
																																						TooltipBrickLevelUpFeatureView widget40 = WidgetFactory.GetWidget(config.BrickLevelUpFeatureView, activate: true, strictMatching: true);
																																						widget40.Bind(source73);
																																						return widget40;
																																					}
																																					if (vm is TooltipBrickImageVM source74)
																																					{
																																						TooltipBrickImageView widget41 = WidgetFactory.GetWidget(config.BrickImageView, activate: true, strictMatching: true);
																																						widget41.Bind(source74);
																																						return widget41;
																																					}
																																				}
																																				else if (tooltipBricksGroupVM.Type == TooltipBricksGroupType.Start)
																																				{
																																					TooltipBricksGroupView widget42 = WidgetFactory.GetWidget(config.BricksGroupView, activate: true, strictMatching: true);
																																					widget42.Bind(tooltipBricksGroupVM);
																																					return widget42;
																																				}
																																				return null;
																																			}
																																			TooltipBrickIconPatternView widget43 = WidgetFactory.GetWidget(config.IconPatternView, activate: true, strictMatching: true);
																																			widget43.Bind(source33);
																																			return widget43;
																																		}
																																		TooltipBrickUnifiedStatusView widget44 = WidgetFactory.GetWidget(config.UnifiedStatusView, activate: true, strictMatching: true);
																																		widget44.Bind(source32);
																																		return widget44;
																																	}
																																	TooltipBrickHintView widget45 = WidgetFactory.GetWidget(config.BrickHintView, activate: true, strictMatching: true);
																																	widget45.Bind(source31);
																																	return widget45;
																																}
																																TooltipBrickAbilityTargetView widget46 = WidgetFactory.GetWidget(config.BrickAbilityTargetView, activate: true, strictMatching: true);
																																widget46.Bind(source30);
																																return widget46;
																															}
																															TooltipBrickFeatureShortDescriptionView widget47 = WidgetFactory.GetWidget(config.BrickFeatureShortDescriptionView, activate: true, strictMatching: true);
																															widget47.Bind(source29);
																															return widget47;
																														}
																														TooltipBrickRateView widget48 = WidgetFactory.GetWidget(config.BrickRateView, activate: true, strictMatching: true);
																														widget48.Bind(source28);
																														return widget48;
																													}
																													TooltipBrickPrerequisiteView widget49 = WidgetFactory.GetWidget(config.BrickPrerequisiteView, activate: true, strictMatching: true);
																													widget49.Bind(source27);
																													return widget49;
																												}
																												TooltipBrickNonStackView widget50 = WidgetFactory.GetWidget(config.BrickNonStackView, activate: true, strictMatching: true);
																												widget50.Bind(source26);
																												return widget50;
																											}
																											TooltipBrickHistoryManagementView widget51 = WidgetFactory.GetWidget(config.BrickHistoryManagementView, activate: true, strictMatching: true);
																											widget51.Bind(source25);
																											return widget51;
																										}
																										TooltipBrickButtonView widget52 = WidgetFactory.GetWidget(config.BrickButtonView, activate: true, strictMatching: true);
																										widget52.Bind(source24);
																										return widget52;
																									}
																									TooltipBrickAbilityScoresView widget53 = WidgetFactory.GetWidget(config.BrickAbilityScoresView, activate: true, strictMatching: true);
																									widget53.Bind(source23);
																									return widget53;
																								}
																								TooltipBrickAbilityScoresBlockView widget54 = WidgetFactory.GetWidget(config.BrickAbilityScoresBlockView, activate: true, strictMatching: true);
																								widget54.Bind(source22);
																								return widget54;
																							}
																							if (tooltipBrickFeatureVM.IsHeader)
																							{
																								TooltipBrickFeatureHeaderView widget55 = WidgetFactory.GetWidget(config.BrickFeatureHeaderView, activate: true, strictMatching: true);
																								widget55.Bind(tooltipBrickFeatureVM);
																								return widget55;
																							}
																							TooltipBrickFeatureView widget56 = WidgetFactory.GetWidget(config.BrickFeatureView, activate: true, strictMatching: true);
																							widget56.Bind(tooltipBrickFeatureVM);
																							return widget56;
																						}
																						TooltipBrickWeaponDOTInitialDamageView widget57 = WidgetFactory.GetWidget(config.BrickWeaponDOTInitialDamageView, activate: true, strictMatching: true);
																						widget57.Bind(source21);
																						return widget57;
																					}
																					TooltipBrickBuffDOTView widget58 = WidgetFactory.GetWidget(config.BrickBuffDOTView, activate: true, strictMatching: true);
																					widget58.Bind(source20);
																					return widget58;
																				}
																				TooltipBrickBuffView widget59 = WidgetFactory.GetWidget(config.BrickBuffView, activate: true, strictMatching: true);
																				widget59.Bind(source19);
																				return widget59;
																			}
																			TooltipBrickValueStatFormulaView widget60 = WidgetFactory.GetWidget(config.BrickValueStatFormulaView, activate: true, strictMatching: true);
																			widget60.Bind(source18);
																			return widget60;
																		}
																		TooltipBrickTwoColumnsStatView widget61 = WidgetFactory.GetWidget(config.BrickTwoColumnsStatView, activate: true, strictMatching: true);
																		widget61.Bind(source17);
																		return widget61;
																	}
																	TooltipBrickIconStatValueView widget62 = WidgetFactory.GetWidget(config.BrickIconStatValueView, activate: true, strictMatching: true);
																	widget62.Bind(source16);
																	return widget62;
																}
																TooltipBrickIconValueStatView widget63 = WidgetFactory.GetWidget(config.BrickIconValueStatView, activate: true, strictMatching: true);
																widget63.Bind(source15);
																return widget63;
															}
															TooltipBrickDoubleTextView widget64 = WidgetFactory.GetWidget(config.BrickDoubleTextView, activate: true, strictMatching: true);
															widget64.Bind(source14);
															return widget64;
														}
														TooltipBrickTripleTextView widget65 = WidgetFactory.GetWidget(config.BrickTripleTextView, activate: true, strictMatching: true);
														widget65.Bind(source13);
														return widget65;
													}
													TooltipBrickItemFooterView widget66 = WidgetFactory.GetWidget(config.BrickItemFooterView, activate: true, strictMatching: true);
													widget66.Bind(source12);
													return widget66;
												}
												TooltipBrickFactionStatusView widget67 = WidgetFactory.GetWidget(config.FactionStatusView, activate: true, strictMatching: true);
												widget67.Bind(source11);
												return widget67;
											}
											TooltipBrickIconAndNameView widget68 = WidgetFactory.GetWidget(config.BrickIconAndNameView, activate: true, strictMatching: true);
											widget68.Bind(source10);
											return widget68;
										}
										TooltipBrickPFIconAndNameView widget69 = WidgetFactory.GetWidget(config.BrickPFIconAndNameView, activate: true, strictMatching: true);
										widget69.Bind(source9);
										return widget69;
									}
									TooltipBrickItemIconAndNameView widget70 = WidgetFactory.GetWidget(config.BrickItemIconAndNameView, activate: true, strictMatching: true);
									widget70.Bind(source8);
									return widget70;
								}
								TooltipBrickPortraitAndNameView widget71 = WidgetFactory.GetWidget(config.BrickPortraitAndNameView, activate: true, strictMatching: true);
								widget71.Bind(source7);
								return widget71;
							}
							TooltipBrickPictureView widget72 = WidgetFactory.GetWidget(config.BrickPictureView, activate: true, strictMatching: true);
							widget72.Bind(source6);
							return widget72;
						}
						TooltipBrickTitleView widget73 = WidgetFactory.GetWidget(config.BrickTitleView, activate: true, strictMatching: true);
						widget73.Bind(source5);
						return widget73;
					}
					TooltipBrickSpaceView widget74 = WidgetFactory.GetWidget(config.BrickSpaceView, activate: true, strictMatching: true);
					widget74.Bind(source4);
					return widget74;
				}
				TooltipBrickSeparatorView widget75 = WidgetFactory.GetWidget(config.BrickSeparatorView, activate: true, strictMatching: true);
				widget75.Bind(source3);
				return widget75;
			}
			TooltipBrickTextView widget76 = WidgetFactory.GetWidget(config.BrickTextView, activate: true, strictMatching: true);
			widget76.Bind(source2);
			return widget76;
		}
		TooltipBrickTimerView widget77 = WidgetFactory.GetWidget(config.BrickTimerView, activate: true, strictMatching: true);
		widget77.Bind(source);
		return widget77;
	}

	public static void DestroyBrickView(MonoBehaviour view)
	{
		WidgetFactory.DisposeWidget(view);
	}

	public static void SetPivots(List<Vector2> pivots, List<Vector2> priorityPivots)
	{
		if (priorityPivots != null && priorityPivots.Count >= 0)
		{
			pivots.AddRange(priorityPivots);
		}
		if (pivots.Count <= 0)
		{
			pivots.AddRange(DefaultPivots);
		}
		float num = 10f;
		for (float num2 = 0f; num2 <= 1f; num2 += 1f)
		{
			for (float num3 = 0f; num3 <= num; num3 += 1f)
			{
				Vector2 item = new Vector2(num2, num3 / num);
				if (!pivots.Contains(item))
				{
					pivots.Add(new Vector2(num2, num3 / num));
				}
			}
		}
		for (float num4 = 0f; num4 <= 1f; num4 += 1f)
		{
			for (float num5 = 0f; num5 <= num; num5 += 1f)
			{
				Vector2 item2 = new Vector2(num5 / num, num4);
				if (!pivots.Contains(item2))
				{
					pivots.Add(new Vector2(num5 / num, num4));
				}
			}
		}
	}
}
