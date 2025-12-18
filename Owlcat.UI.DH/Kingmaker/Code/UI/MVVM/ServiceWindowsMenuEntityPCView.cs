using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ServiceWindowsMenuEntityPCView : SelectionGroupEntityView<ServiceWindowsMenuEntityVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Label;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
	}
}
