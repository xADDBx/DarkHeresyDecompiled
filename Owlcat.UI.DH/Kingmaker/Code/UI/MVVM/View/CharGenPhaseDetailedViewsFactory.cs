using System;
using Kingmaker.Code.View.UI.UIUtilities;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

[Serializable]
public class CharGenPhaseDetailedViewsFactory : ICharGenPhaseDetailedViewsFactory
{
	[SerializeField]
	private CharGenPregenPhaseDetailedView m_PregenPhaseDetailedView;

	[SerializeField]
	private CharGenAppearancePhaseDetailedView m_AppearancePhaseDetailedView;

	[SerializeField]
	private CharGenBackgroundBasePhaseDetailedView m_SoulMarkPhaseDetailedView;

	[SerializeField]
	private CharGenBackgroundBasePhaseDetailedView m_HomeworldPhaseDetailedView;

	[SerializeField]
	private CharGenBackgroundBasePhaseDetailedView m_ImperialHomeworldChildPhaseDetailedView;

	[SerializeField]
	private CharGenBackgroundBasePhaseDetailedView m_ForgeHomeworldChildPhaseDetailedView;

	[SerializeField]
	private CharGenBackgroundBasePhaseDetailedView m_OccupationPhaseDetailedView;

	[SerializeField]
	private CharGenBackgroundBasePhaseDetailedView m_NavigatorPhaseDetailedView;

	[SerializeField]
	private CharGenBackgroundBasePhaseDetailedView m_SanctionedPsykerPhaseDetailedView;

	[SerializeField]
	private CharGenBackgroundBasePhaseDetailedView m_DarkestHourPhaseDetailedView;

	[SerializeField]
	private CharGenBackgroundBasePhaseDetailedView m_MomentOfTriumphPhaseDetailedView;

	[SerializeField]
	private CharGenCareerPhaseDetailedView m_CareerPhaseDetailedView;

	[SerializeField]
	private CharGenAttributesPhaseDetailedView m_AttributesPhaseDetailedView;

	[SerializeField]
	private CharGenSummaryPhaseDetailedView m_SummaryPhaseDetailedView;

	[SerializeField]
	private CharGenShipPhaseDetailedView m_ShipPhaseDetailedView;

	[Header("New Views")]
	[SerializeField]
	private CharGenCareerPhaseDetailedView_NEW m_CareerPhaseDetailedView_NEW;

	[Header("LevelUp")]
	[SerializeField]
	private CharGenLevelUpAbilityPhaseDetailedView m_LevelUpAbilityPhaseDetailedView;

	[SerializeField]
	private CharGenLevelUpAbilityUpgradePhaseDetailedView m_LevelUpAbilityUpgradePhaseDetailedView;

	[SerializeField]
	private CharGenLevelUpCharacteristicsPhaseDetailedView m_LevelUpCharacteristicsPhaseDetailedView;

	[SerializeField]
	private CharGenLevelUpModificationPhaseDetailedView m_LevelUpModificationPhaseDetailedView;

	[SerializeField]
	private CharGenLevelUpSkillPhaseDetailedView m_LevelUpSkillPhaseDetailedView;

	[SerializeField]
	private CharGenLevelUpTalentPhaseDetailedView m_LevelUpTalentPhaseDetailedView;

	[SerializeField]
	private CharGenLevelUpSpecializationPhaseDetailedView m_LevelUpSpecializationPhaseDetailedView;

	[SerializeField]
	private CharGenLevelUpFeaturePhaseDetailedView m_LevelUpKeystonePhaseDetailedView;

	private bool m_PaperHintsAdded;

	public ICharGenPhaseDetailedView GetDetailedPhaseView(CharGenPhaseBaseVM viewModel)
	{
		if (!(viewModel is CharGenPregenPhaseVM source))
		{
			if (!(viewModel is CharGenAppearanceComponentAppearancePhaseVM source2))
			{
				if (!(viewModel is CharGenHomeworldPhaseVM source3))
				{
					if (!(viewModel is CharGenOccupationPhaseVM source4))
					{
						if (!(viewModel is CharGenNavigatorPhaseVM source5))
						{
							if (!(viewModel is CharGenDarkestHourPhaseVM source6))
							{
								if (!(viewModel is CharGenMomentOfTriumphPhaseVM source7))
								{
									if (!(viewModel is CharGenCareerPhaseVM source8))
									{
										if (!(viewModel is CharGenAttributesPhaseVM source9))
										{
											if (!(viewModel is CharGenSummaryPhaseVM source10))
											{
												if (!(viewModel is CharGenShipPhaseVM source11))
												{
													if (!(viewModel is CharGenSoulMarkPhaseVM source12))
													{
														if (!(viewModel is CharGenImperialHomeworldChildPhaseVM source13))
														{
															if (!(viewModel is CharGenForgeHomeworldChildPhaseVM source14))
															{
																if (!(viewModel is CharGenSanctionedPsykerChildPhaseVM source15))
																{
																	if (!(viewModel is CharGenCareerPhaseVM_NEW source16))
																	{
																		if (!(viewModel is CharGenLevelUpAbilityPhaseVM source17))
																		{
																			if (!(viewModel is CharGenLevelUpAbilityUpgradePhaseVM source18))
																			{
																				if (!(viewModel is CharGenLevelUpCharacteristicsPhaseVM source19))
																				{
																					if (!(viewModel is CharGenLevelUpModificationPhaseVM source20))
																					{
																						if (!(viewModel is CharGenLevelUpSkillPhaseVM source21))
																						{
																							if (!(viewModel is CharGenLevelUpSpecializationPhaseVM source22))
																							{
																								if (!(viewModel is CharGenLevelUpTalentPhaseVM source23))
																								{
																									if (viewModel is CharGenLevelUpFeaturePhaseVM source24)
																									{
																										m_LevelUpKeystonePhaseDetailedView.Bind(source24);
																										return m_LevelUpKeystonePhaseDetailedView;
																									}
																									return null;
																								}
																								m_LevelUpTalentPhaseDetailedView.Bind(source23);
																								return m_LevelUpTalentPhaseDetailedView;
																							}
																							m_LevelUpSpecializationPhaseDetailedView.Bind(source22);
																							return m_LevelUpSpecializationPhaseDetailedView;
																						}
																						m_LevelUpSkillPhaseDetailedView.Bind(source21);
																						return m_LevelUpSkillPhaseDetailedView;
																					}
																					m_LevelUpModificationPhaseDetailedView.Bind(source20);
																					return m_LevelUpModificationPhaseDetailedView;
																				}
																				m_LevelUpCharacteristicsPhaseDetailedView.Bind(source19);
																				return m_LevelUpCharacteristicsPhaseDetailedView;
																			}
																			m_LevelUpAbilityUpgradePhaseDetailedView.Bind(source18);
																			return m_LevelUpAbilityUpgradePhaseDetailedView;
																		}
																		m_LevelUpAbilityPhaseDetailedView.Bind(source17);
																		return m_LevelUpAbilityPhaseDetailedView;
																	}
																	m_CareerPhaseDetailedView_NEW.Bind(source16);
																	return m_CareerPhaseDetailedView_NEW;
																}
																m_SanctionedPsykerPhaseDetailedView.Bind(source15);
																return m_SanctionedPsykerPhaseDetailedView;
															}
															m_ForgeHomeworldChildPhaseDetailedView.Bind(source14);
															return m_ForgeHomeworldChildPhaseDetailedView;
														}
														m_ImperialHomeworldChildPhaseDetailedView.Bind(source13);
														return m_ImperialHomeworldChildPhaseDetailedView;
													}
													m_SoulMarkPhaseDetailedView.Bind(source12);
													return m_SoulMarkPhaseDetailedView;
												}
												m_ShipPhaseDetailedView.Bind(source11);
												return m_ShipPhaseDetailedView;
											}
											m_SummaryPhaseDetailedView.Bind(source10);
											return m_SummaryPhaseDetailedView;
										}
										m_AttributesPhaseDetailedView.Bind(source9);
										return m_AttributesPhaseDetailedView;
									}
									m_CareerPhaseDetailedView.Bind(source8);
									return m_CareerPhaseDetailedView;
								}
								m_MomentOfTriumphPhaseDetailedView.Bind(source7);
								return m_MomentOfTriumphPhaseDetailedView;
							}
							m_DarkestHourPhaseDetailedView.Bind(source6);
							return m_DarkestHourPhaseDetailedView;
						}
						m_NavigatorPhaseDetailedView.Bind(source5);
						return m_NavigatorPhaseDetailedView;
					}
					m_OccupationPhaseDetailedView.Bind(source4);
					return m_OccupationPhaseDetailedView;
				}
				m_HomeworldPhaseDetailedView.Bind(source3);
				return m_HomeworldPhaseDetailedView;
			}
			m_AppearancePhaseDetailedView.Bind(source2);
			return m_AppearancePhaseDetailedView;
		}
		m_PregenPhaseDetailedView.Bind(source);
		return m_PregenPhaseDetailedView;
	}

	public void Initialize()
	{
		m_PaperHintsAdded = false;
		m_PregenPhaseDetailedView.Initialize();
		m_AppearancePhaseDetailedView.Initialize();
		m_SoulMarkPhaseDetailedView.Initialize();
		m_HomeworldPhaseDetailedView.Initialize();
		m_OccupationPhaseDetailedView.Initialize();
		m_DarkestHourPhaseDetailedView.Initialize();
		m_MomentOfTriumphPhaseDetailedView.Initialize();
		m_NavigatorPhaseDetailedView.Initialize();
		m_CareerPhaseDetailedView.Initialize();
		m_AttributesPhaseDetailedView.Initialize();
		m_SummaryPhaseDetailedView.Initialize();
		m_ShipPhaseDetailedView.Initialize();
		m_ImperialHomeworldChildPhaseDetailedView.Initialize();
		m_ForgeHomeworldChildPhaseDetailedView.Initialize();
		m_SanctionedPsykerPhaseDetailedView.Initialize();
	}

	public void SetPaperHints(PaperHints paperHints)
	{
		if (UtilityNet.IsControlMainCharacter() && !m_PaperHintsAdded)
		{
			m_PregenPhaseDetailedView.SetPaperHints(paperHints);
			m_AppearancePhaseDetailedView.SetPaperHints(paperHints);
			m_SoulMarkPhaseDetailedView.SetPaperHints(paperHints);
			m_HomeworldPhaseDetailedView.SetPaperHints(paperHints);
			m_OccupationPhaseDetailedView.SetPaperHints(paperHints);
			m_DarkestHourPhaseDetailedView.SetPaperHints(paperHints);
			m_MomentOfTriumphPhaseDetailedView.SetPaperHints(paperHints);
			m_NavigatorPhaseDetailedView.SetPaperHints(paperHints);
			m_CareerPhaseDetailedView.SetPaperHints(paperHints);
			m_AttributesPhaseDetailedView.SetPaperHints(paperHints);
			m_SummaryPhaseDetailedView.SetPaperHints(paperHints);
			m_ShipPhaseDetailedView.SetPaperHints(paperHints);
			m_ImperialHomeworldChildPhaseDetailedView.SetPaperHints(paperHints);
			m_ForgeHomeworldChildPhaseDetailedView.SetPaperHints(paperHints);
			m_SanctionedPsykerPhaseDetailedView.SetPaperHints(paperHints);
			m_PaperHintsAdded = true;
		}
	}
}
