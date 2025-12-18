using Kingmaker.Code.View.Bridge.Utils;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Code.View.UI.MVVM.Dialog;

public class EpilogTrueAnswerView : View<EpilogTrueAnswerVM>
{
	[Header("Elements")]
	[SerializeField]
	private TMP_Text m_NumberDecor;

	[Header("Views")]
	[SerializeField]
	private TrueAnswerEntityView m_TrueAnswerEntityView;

	[SerializeField]
	private EpilogTrueAnswerDecorView m_DecorView;

	protected override void OnBind()
	{
		base.ViewModel.TrueAnswer.Subscribe(m_TrueAnswerEntityView.Bind).AddTo(this);
		base.gameObject.SetActive(value: true);
		m_DecorView.Bind(default(Unit));
		m_NumberDecor.text = UtilityDetectiveDecor.GetCaseUniqueId(base.ViewModel.BlueprintCase);
	}

	protected override void OnUnbind()
	{
		base.gameObject.SetActive(value: false);
	}
}
