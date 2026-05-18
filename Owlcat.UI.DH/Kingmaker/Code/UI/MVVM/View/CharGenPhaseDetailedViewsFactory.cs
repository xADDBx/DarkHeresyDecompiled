using System;
using Code.View.UI.MVVM;
using Kingmaker.Code.View.UI.UIUtilities;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

[Serializable]
public class CharGenPhaseDetailedViewsFactory
{
	[Header("CharGen")]
	[SerializeField]
	private CharGenAppearancePhaseDetailedView m_AppearancePhaseDetailedView;

	[SerializeField]
	private CharGenPortraitPhaseView m_PortraitPhaseView;

	[SerializeField]
	private CharGenCareerPhaseDetailedView m_CareerPhaseDetailedView_NEW;

	[SerializeField]
	private CharGenBackgroundBasePhaseDetailedView m_HomeworldPhaseDetailedView;

	[SerializeField]
	private CharGenBackgroundBasePhaseDetailedView m_OccupationPhaseDetailedView;

	[SerializeField]
	private CharGenBackgroundBasePhaseDetailedView m_NobleHomeworldChildPhaseDetailedView;

	[SerializeField]
	private CharGenVoicePhaseView m_VoicePhaseDetailedView;

	[SerializeField]
	private CharGenSummaryPhaseDetailedView m_SummaryPhaseDetailedView;

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
		if (!(viewModel is CharGenAppearanceComponentAppearancePhaseVM source))
		{
			if (!(viewModel is CharGenPortraitPhaseVM source2))
			{
				if (!(viewModel is CharGenHomeworldPhaseVM source3))
				{
					if (!(viewModel is CharGenNobleHomeworldChildPhaseVM source4))
					{
						if (!(viewModel is CharGenDeathWorldFeaturePhaseVM source5))
						{
							if (!(viewModel is CharGenOccupationPhaseVM source6))
							{
								if (!(viewModel is CharGenVoicePhaseVM source7))
								{
									if (!(viewModel is CharGenSummaryPhaseVM source8))
									{
										if (!(viewModel is CharGenCareerPhaseVM source9))
										{
											if (!(viewModel is CharGenLevelUpAbilityPhaseVM source10))
											{
												if (!(viewModel is CharGenLevelUpAbilityUpgradePhaseVM source11))
												{
													if (!(viewModel is CharGenLevelUpCharacteristicsPhaseVM source12))
													{
														if (!(viewModel is CharGenLevelUpModificationPhaseVM source13))
														{
															if (!(viewModel is CharGenLevelUpSkillPhaseVM source14))
															{
																if (!(viewModel is CharGenLevelUpSpecializationPhaseVM source15))
																{
																	if (!(viewModel is CharGenLevelUpTalentPhaseVM source16))
																	{
																		if (viewModel is CharGenLevelUpFeaturePhaseVM source17)
																		{
																			m_LevelUpKeystonePhaseDetailedView.Bind(source17);
																			return m_LevelUpKeystonePhaseDetailedView;
																		}
																		return null;
																	}
																	m_LevelUpTalentPhaseDetailedView.Bind(source16);
																	return m_LevelUpTalentPhaseDetailedView;
																}
																m_LevelUpSpecializationPhaseDetailedView.Bind(source15);
																return m_LevelUpSpecializationPhaseDetailedView;
															}
															m_LevelUpSkillPhaseDetailedView.Bind(source14);
															return m_LevelUpSkillPhaseDetailedView;
														}
														m_LevelUpModificationPhaseDetailedView.Bind(source13);
														return m_LevelUpModificationPhaseDetailedView;
													}
													m_LevelUpCharacteristicsPhaseDetailedView.Bind(source12);
													return m_LevelUpCharacteristicsPhaseDetailedView;
												}
												m_LevelUpAbilityUpgradePhaseDetailedView.Bind(source11);
												return m_LevelUpAbilityUpgradePhaseDetailedView;
											}
											m_LevelUpAbilityPhaseDetailedView.Bind(source10);
											return m_LevelUpAbilityPhaseDetailedView;
										}
										m_CareerPhaseDetailedView_NEW.Bind(source9);
										return m_CareerPhaseDetailedView_NEW;
									}
									m_SummaryPhaseDetailedView.Bind(source8);
									return m_SummaryPhaseDetailedView;
								}
								m_VoicePhaseDetailedView.Bind(source7);
								return m_VoicePhaseDetailedView;
							}
							m_OccupationPhaseDetailedView.Bind(source6);
							return m_OccupationPhaseDetailedView;
						}
						m_NobleHomeworldChildPhaseDetailedView.Bind(source5);
						return m_NobleHomeworldChildPhaseDetailedView;
					}
					m_OccupationPhaseDetailedView.Bind(source4);
					return m_OccupationPhaseDetailedView;
				}
				m_HomeworldPhaseDetailedView.Bind(source3);
				return m_HomeworldPhaseDetailedView;
			}
			m_PortraitPhaseView.Bind(source2);
			return m_PortraitPhaseView;
		}
		m_AppearancePhaseDetailedView.Bind(source);
		return m_AppearancePhaseDetailedView;
	}

	public void Initialize()
	{
		m_PaperHintsAdded = false;
		m_AppearancePhaseDetailedView.Initialize();
		m_HomeworldPhaseDetailedView.Initialize();
		m_OccupationPhaseDetailedView.Initialize();
		m_SummaryPhaseDetailedView.Initialize();
	}

	public void SetPaperHints(PaperHints paperHints)
	{
		if (UtilityNet.IsControlMainCharacter() && !m_PaperHintsAdded)
		{
			m_AppearancePhaseDetailedView.SetPaperHints(paperHints);
			m_HomeworldPhaseDetailedView.SetPaperHints(paperHints);
			m_OccupationPhaseDetailedView.SetPaperHints(paperHints);
			m_SummaryPhaseDetailedView.SetPaperHints(paperHints);
			m_PaperHintsAdded = true;
		}
	}
}
