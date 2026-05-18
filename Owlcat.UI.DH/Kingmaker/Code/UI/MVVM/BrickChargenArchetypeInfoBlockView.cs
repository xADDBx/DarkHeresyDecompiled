using Kingmaker.Blueprints.Root.Strings;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickChargenArchetypeInfoBlockView : BrickBaseView<BrickChargenArchetypeInfoBlockVM>
{
	[SerializeField]
	private TMP_Text m_DescriptionText;

	[SerializeField]
	private TMP_Text m_MovementLabel;

	[SerializeField]
	private TMP_Text m_BuffLabel;

	[SerializeField]
	private TMP_Text m_RangeLabel;

	[SerializeField]
	private TMP_Text m_PsykanaLabel;

	[SerializeField]
	private TMP_Text m_MeleeLabel;

	[SerializeField]
	private TMP_Text m_DefenceLabel;

	[SerializeField]
	private TooltipElementStatPieWidgetView m_StatWidget;

	protected override void OnBind()
	{
		base.OnBind();
		m_MovementLabel.text = UIStrings.Instance.CharGen.Movement;
		m_BuffLabel.text = UIStrings.Instance.CharGen.Buff;
		m_RangeLabel.text = UIStrings.Instance.CharGen.Range;
		m_PsykanaLabel.text = UIStrings.Instance.CharGen.Psykana;
		m_MeleeLabel.text = UIStrings.Instance.CharGen.Melee;
		m_DefenceLabel.text = UIStrings.Instance.CharacterSheet.DefenceLabel;
		m_DescriptionText.text = base.ViewModel.Description;
		m_StatWidget.SegmentCount = base.ViewModel.SchemeData.Count;
		m_StatWidget.Values = base.ViewModel.SchemeData.ToArray();
		m_StatWidget.SetVerticesDirty();
	}
}
