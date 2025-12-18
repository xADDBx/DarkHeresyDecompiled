using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class SurfaceOvertipsPCView : View<SurfaceOvertipsVM>
{
	[SerializeField]
	private UnitOvertipsView m_UnitOvertipsPCView;

	[SerializeField]
	private LightweightUnitOvertipsCollectionView m_LightweightUnitOvertipsPCView;

	[SerializeField]
	private MapObjectOvertipsPCView m_MapObjectOvertipsPCView;

	public void Awake()
	{
		m_UnitOvertipsPCView.Initialize();
		m_LightweightUnitOvertipsPCView.Initialize();
		m_MapObjectOvertipsPCView.Initialize();
	}

	protected override void OnBind()
	{
		m_UnitOvertipsPCView.Bind(base.ViewModel.UnitOvertipsCollectionVM);
		m_LightweightUnitOvertipsPCView.Bind(base.ViewModel.LightweightUnitOvertipsCollectionVM);
		m_MapObjectOvertipsPCView.Bind(base.ViewModel.MapObjectOvertipsVM);
	}
}
