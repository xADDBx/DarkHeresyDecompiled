using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Code.UI.MVVM;

public class LocalMapCommonMarkerVM : LocalMapMarkerVM
{
	private readonly ILocalMapMarker m_Marker;

	public LocalMapCommonMarkerVM(ILocalMapMarker marker)
	{
		m_Marker = marker;
		m_Position.Value = marker.GetPosition();
		m_IsVisible.Value = marker.IsVisible();
		m_Description.Value = marker.GetDescription();
		m_IsMapObject.Value = marker.IsMapObject();
		base.MarkerType = marker.GetMarkerType();
		if (base.MarkerType == LocalMapMarkType.Loot && base.Description.CurrentValue.Empty())
		{
			m_Description.Value = ConfigRoot.Instance.LocalizedTexts.UserInterfacesText.Tooltips.Loot;
		}
	}

	protected override void OnUpdateHandler()
	{
		ILocalMapMarker marker = m_Marker;
		if (marker != null && !marker.IsDisposed)
		{
			m_Position.Value = m_Marker.GetPosition();
			m_IsVisible.Value = m_Marker.IsVisible();
		}
	}

	public override Entity GetEntity()
	{
		return m_Marker.GetEntity();
	}
}
