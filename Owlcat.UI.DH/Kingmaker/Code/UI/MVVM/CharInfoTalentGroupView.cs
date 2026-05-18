using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoTalentGroupView : View<CharInfoTalentGroupVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Title;

	[SerializeField]
	private WidgetList m_TalentsList;

	[SerializeField]
	private CharInfoTalentItemView m_TalentView;

	protected override void OnBind()
	{
		base.OnBind();
		m_Title.text = base.ViewModel.Title;
		base.ViewModel.TalentList.SubscribeToWidgetList(m_TalentsList, m_TalentView).AddTo(this);
	}
}
