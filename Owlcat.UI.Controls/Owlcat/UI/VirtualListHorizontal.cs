using UnityEngine;

namespace Owlcat.UI;

public class VirtualListHorizontal : VirtualListComponent
{
	[SerializeField]
	private VirtualListLayoutSettingsHorizontal m_LayoutSettings;

	protected override IVirtualListLayoutSettings LayoutSettings => m_LayoutSettings;
}
