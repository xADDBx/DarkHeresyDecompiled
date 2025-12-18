using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class AddendumTitleView : View<AddendumTitleVM>
{
	[Header("Elements")]
	[SerializeField]
	private TMP_Text m_AddendumLabel;

	[SerializeField]
	private OwlcatMultiSelectable m_TitleSelectable;

	[SerializeField]
	private TMP_Text m_AddendumId;

	protected override void OnBind()
	{
		m_AddendumLabel.text = UIStrings.Instance.DetectiveJournal.NewAddendumsReceived.Text;
		TMP_Text addendumId = m_AddendumId;
		int infoId = base.ViewModel.InfoId;
		addendumId.text = infoId.ToString();
		base.ViewModel.IsViewed.Subscribe(delegate(bool value)
		{
			m_TitleSelectable.SetActiveLayer((!value) ? 1 : 0);
		}).AddTo(this);
	}
}
