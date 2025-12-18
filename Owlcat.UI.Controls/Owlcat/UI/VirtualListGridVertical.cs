using UnityEngine;

namespace Owlcat.UI;

public class VirtualListGridVertical : VirtualListComponent
{
	[SerializeField]
	private VirtualListLayoutSettingsGrid m_LayoutSettings;

	protected override IVirtualListLayoutSettings LayoutSettings => m_LayoutSettings;
}
