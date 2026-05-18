using Kingmaker.Blueprints.Root;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.Bridge.Utils;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class CaseCardIconView : View<CaseCardVM>
{
	[Header("Elements")]
	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private TMP_Text m_TierLabel;

	[SerializeField]
	private Image m_IssueIcon;

	[SerializeField]
	private OwlcatMultiSelectable m_StateSelectable;

	[Header("Values")]
	[SerializeField]
	private Sprite m_DefaultIcon;

	protected override void OnBind()
	{
		m_Icon.sprite = ((base.ViewModel.BlueprintCase == null) ? UIConfig.Instance.DetectiveConfig.UnknownCluesIcon : (Game.Instance.DetectiveSystem.GetCaseDisplay(base.ViewModel.BlueprintCase).Icon ?? m_DefaultIcon));
		m_TierLabel.text = UtilityDetectiveDecor.GetCaseTier(base.ViewModel.BlueprintCase).ToString();
		DetectiveCaseIssueType issuingType = UtilityDetectiveDecor.GetIssuingType(base.ViewModel.BlueprintCase);
		m_IssueIcon.sprite = UIConfig.Instance.DetectiveConfig.GetIssuingTypeIcon(issuingType);
		base.ViewModel.CurrentState.Subscribe(delegate(CardState value)
		{
			m_StateSelectable.SetActiveLayer(value.ToString());
		}).AddTo(this);
	}
}
