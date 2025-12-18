using UnityEngine;

namespace Owlcat.UI;

public class VirtualListVertical : VirtualListComponent
{
	[SerializeField]
	private VirtualListLayoutSettingsVertical m_LayoutSettings;

	protected override IVirtualListLayoutSettings LayoutSettings => m_LayoutSettings;
}
