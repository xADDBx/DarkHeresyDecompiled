using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CombatUnitDelayedActionView : View<CombatMechanicEntityVM>
{
	[SerializeField]
	private CombatUnitPortraitWidget m_PortraitWidget;

	[SerializeField]
	private SurfaceCombatActionView m_ActionView;

	[SerializeField]
	private UnitSelectionButtonView m_SelectionButton;

	[SerializeField]
	private RectTransform m_NameAnchor;

	public RectTransform UnitNameAnchor => m_NameAnchor;

	public void SetActive(bool isActive)
	{
		base.gameObject.SetActive(isActive);
	}

	protected override void OnBind()
	{
		if (!base.ViewModel.HasUnit || !base.ViewModel.IsInitiativeHolder)
		{
			SetActive(isActive: false);
			return;
		}
		m_PortraitWidget.SetPortrait(base.ViewModel.MechanicEntity, !base.ViewModel.UsedSubtypeIcon);
		m_PortraitWidget.SetVisualLayer(base.ViewModel.IsEnemy.CurrentValue, base.ViewModel.IsPlayerFaction);
		m_SelectionButton.Bind(base.ViewModel);
		base.ViewModel.ConcentrationVM.Subscribe(m_ActionView.Bind).AddTo(this);
		SetActive(isActive: true);
	}

	protected override void OnUnbind()
	{
		m_SelectionButton.Unbind();
		m_PortraitWidget.ClearPortrait();
	}
}
