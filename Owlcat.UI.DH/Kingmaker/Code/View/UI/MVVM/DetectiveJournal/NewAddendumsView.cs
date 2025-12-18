using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class NewAddendumsView : View<NewAddendumsVM>
{
	[Header("Elements")]
	[SerializeField]
	private NewAddendumView m_AddendumView;

	[SerializeField]
	private TMP_Text m_AddendumTitle;

	protected override void OnBind()
	{
		m_AddendumTitle.text = UIStrings.Instance.DetectiveJournal.NewAddendumLabel;
		ObservableSubscribeExtensions.Subscribe(base.ViewModel.RefreshAddendumsCommand, delegate
		{
			RefreshView();
		}).AddTo(this);
		RefreshView();
	}

	private void RefreshView()
	{
		m_AddendumView.Bind(base.ViewModel.NewAddendums.FirstOrDefault());
	}
}
