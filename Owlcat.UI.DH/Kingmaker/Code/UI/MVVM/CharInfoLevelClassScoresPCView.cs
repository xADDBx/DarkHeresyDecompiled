using Kingmaker.Blueprints.Root.Strings;
using Owlcat.Runtime.Core.Utility;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoLevelClassScoresPCView : CharInfoComponentView<CharInfoLevelClassScoresVM>
{
	[SerializeField]
	private CharInfoExperiencePCView m_Experience;

	[SerializeField]
	private CharInfoAbilityScoresBlockBaseView m_AbilityScores;

	[SerializeField]
	private CharInfoClassesListPCView m_Classes;

	[Header("Add stats")]
	[SerializeField]
	protected InventoryDollAdditionalStatsPCView m_AdditionalStatsView;

	[SerializeField]
	private CharInfoGlossaryStatView m_CohesionStatView;

	[Header("Localization")]
	[SerializeField]
	private TextMeshProUGUI m_CharacterStatsLabel;

	public override void Initialize()
	{
		base.Initialize();
		m_Experience.Or(null)?.Initialize();
		m_AbilityScores.Or(null)?.Initialize();
		m_AdditionalStatsView.Or(null)?.Initialize();
	}

	protected override void OnBind()
	{
		base.OnBind();
		m_Experience.Or(null)?.Bind(base.ViewModel.Experience);
		m_AdditionalStatsView.Or(null)?.Bind(base.ViewModel.AdditionalStatsVM);
		m_CohesionStatView.Or(null)?.Bind(base.ViewModel.CohesionStatVM);
		m_CharacterStatsLabel.text = UIStrings.Instance.CharacterSheet.Stats;
	}

	protected override void RefreshView()
	{
		m_AbilityScores.Or(null)?.Bind(base.ViewModel.AbilityScores);
		m_Classes.Or(null)?.Bind(base.ViewModel.Classes);
	}
}
