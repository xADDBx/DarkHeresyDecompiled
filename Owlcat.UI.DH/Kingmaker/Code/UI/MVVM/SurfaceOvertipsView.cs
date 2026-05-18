using Owlcat.UI;
using Owlcat.UI.Commands;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

[RequireComponent(typeof(FocusLayer))]
public class SurfaceOvertipsView : View<SurfaceOvertipsVM>
{
	[SerializeField]
	private UnitOvertipsView m_UnitOvertipsPCView;

	[SerializeField]
	private LightweightUnitOvertipsCollectionView m_LightweightUnitOvertipsPCView;

	[SerializeField]
	private MapObjectOvertipsView m_MapObjectOvertipsPCView;

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
		this.AddCommand("gamepad:leftShoulder", delegate
		{
			base.ViewModel.Navigate(-1);
		}).AddTo(this);
		this.AddCommand("gamepad:rightShoulder", delegate
		{
			base.ViewModel.Navigate(1);
		}).AddTo(this);
		this.AddCommand("gamepad:buttonSouth", delegate
		{
			base.ViewModel.Interact();
		});
	}
}
