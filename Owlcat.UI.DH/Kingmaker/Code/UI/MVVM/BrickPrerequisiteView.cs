using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickPrerequisiteView : BrickBaseView<BrickPrerequisiteVM>
{
	[SerializeField]
	protected WidgetList m_WidgetList;

	[Header("Views")]
	[SerializeField]
	private PrerequisiteEntryView m_PrerequisiteEntryView;

	protected override void OnBind()
	{
		DrawEntries();
	}

	private void DrawEntries()
	{
		m_WidgetList.DrawEntries(base.ViewModel.PrerequisiteEntries.ToArray(), m_PrerequisiteEntryView);
	}
}
