using Kingmaker.Code.View.Bridge.OBSOLETE;
using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class RankEntryDescriptionView : VirtualListElementViewBase<RankEntryDescriptionVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Description;

	[SerializeField]
	private VirtualListLayoutElementSettings m_LayoutSettings;

	public override VirtualListLayoutElementSettings LayoutSettings => m_LayoutSettings;

	protected override void BindViewImplementation()
	{
		m_Description.text = base.ViewModel.Description;
		DelayedInvoker.InvokeInFrames(delegate
		{
			m_LayoutSettings.Height = m_Description.rectTransform.rect.height;
		}, 1);
	}

	protected override void DestroyViewImplementation()
	{
	}
}
