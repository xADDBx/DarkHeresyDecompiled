using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CombatUnitEntityView : View<CombatMechanicEntityVM>
{
	[SerializeField]
	private CombatUnitPortraitWidget m_PortraitWidget;

	[SerializeField]
	private SurfaceCombatActionView m_ConcentrationView;

	[SerializeField]
	private InitiativeTrackerMoraleView m_MoraleView;

	[SerializeField]
	private UnitHealthPartProgressView m_HealthView;

	[SerializeField]
	private BuffsBlockView m_BuffsBlockView;

	[SerializeField]
	private UnitSelectionButtonView m_SelectionButton;

	[SerializeField]
	private RectTransform m_NameAnchor;

	public RectTransform UnitNameAnchor => m_NameAnchor;

	public ReadOnlyReactiveProperty<bool> IsHovered => m_SelectionButton.IsHovered;

	public void SetActive(bool isActive)
	{
		base.gameObject.SetActive(isActive);
	}

	public void SetFocused(bool isFocused)
	{
		m_SelectionButton.SetSelectedExternal(isFocused);
	}

	protected override void OnBind()
	{
		if (!base.ViewModel.HasUnit || base.ViewModel.IsInitiativeHolder)
		{
			SetActive(isActive: false);
			return;
		}
		base.ViewModel.FactionInfo.Subscribe(HandleUnitFactionChanged).AddTo(this);
		m_PortraitWidget.SetPortrait(base.ViewModel.MechanicEntity, !base.ViewModel.UsedSubtypeIcon);
		base.ViewModel.ConcentrationVM.Subscribe(m_ConcentrationView.Bind).AddTo(this);
		base.ViewModel.OvertipMoraleVM.Subscribe(m_MoraleView.Bind).AddTo(this);
		base.ViewModel.UnitHealthPartVM.Subscribe(m_HealthView.Bind).AddTo(this);
		m_BuffsBlockView.Bind(base.ViewModel.UnitBuffs);
		m_SelectionButton.Bind(base.ViewModel);
		SetActive(isActive: true);
	}

	protected override void OnUnbind()
	{
		m_BuffsBlockView.Unbind();
		m_SelectionButton.Unbind();
		m_PortraitWidget.ClearPortrait();
	}

	private void HandleUnitFactionChanged((bool isEnemy, bool isPlayerFaction) data)
	{
		m_PortraitWidget.SetVisualLayer(data.isEnemy, data.isPlayerFaction);
	}
}
