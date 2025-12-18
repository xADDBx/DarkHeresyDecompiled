using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Data;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class CaseCardHeaderView : View<CaseCardVM>
{
	[Header("Elements")]
	[SerializeField]
	private TMP_Text m_CaseLabel;

	[SerializeField]
	private OwlcatMultiSelectable m_StateSelectable;

	[Header("Values")]
	[SerializeField]
	private TextStyle m_TextStyle;

	protected override void OnBind()
	{
		SetupLabel();
		base.ViewModel.CurrentState.Subscribe(delegate(CardState value)
		{
			m_StateSelectable.SetActiveLayer(value.ToString());
		}).AddTo(this);
	}

	private void SetupLabel()
	{
		m_CaseLabel.styleSheet = m_TextStyle.StyleSheet;
		if (base.ViewModel.BlueprintCase == null)
		{
			m_CaseLabel.text = string.Empty;
			return;
		}
		using (GameLogContext.Scope)
		{
			GameLogContext.Case = base.ViewModel.BlueprintCase;
			GameLogContext.TextStyle = m_TextStyle;
			m_CaseLabel.text = UIStrings.Instance.DetectiveDecor.CaseNumber.Text;
			GameLogContext.TextStyle = UIConfig.Instance.DefaultTextStyle;
		}
	}
}
