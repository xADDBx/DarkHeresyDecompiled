using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public abstract class CombatUnitView<TCombatUnitVM> : View<TCombatUnitVM> where TCombatUnitVM : CombatMechanicEntityVM
{
	[Header("Button for Interact")]
	[SerializeField]
	protected OwlcatMultiButton Button;

	[Header("PortraitZone")]
	[SerializeField]
	private SurfaceCombatUnitPortraitZone m_CharacetrPortraitZone;

	[Header("No Portrait Zone")]
	[SerializeField]
	private SurfaceCombatUnitPortraitZone m_NoPortraitZone;

	[Header("Buffs")]
	[SerializeField]
	protected bool HasBuffsView;

	[ShowIf("HasBuffsView")]
	[SerializeField]
	private BuffsBlockView m_BuffsBlockView;

	[Header("Health")]
	[SerializeField]
	protected UnitHealthPartProgressView UnitHealthPartProgressPCView;

	[SerializeField]
	private UnitHealthPartTextPCView m_HealthTextView;

	private UnitFractionViewMode m_FractionViewMode;

	private bool m_IsInit;

	private void Awake()
	{
		if (!m_IsInit)
		{
			m_CharacetrPortraitZone.Hide();
			m_NoPortraitZone.Hide();
			m_IsInit = true;
		}
	}

	protected override void OnBind()
	{
		InternalBind();
	}

	protected void InternalBind()
	{
		if (!base.ViewModel.HasUnit)
		{
			return;
		}
		SetupPortrait();
		if (HasBuffsView)
		{
			m_BuffsBlockView.Bind(base.ViewModel.IsInitiativeHolder ? null : base.ViewModel.UnitBuffs);
		}
		base.ViewModel.UnitHealthPartVM.Subscribe(delegate(UnitHealthPartVM h)
		{
			UnitHealthPartProgressPCView.SetVisible(!base.ViewModel.IsInitiativeHolder);
			UnitHealthPartProgressPCView.Bind(h);
			m_HealthTextView.Bind(h);
		}).AddTo(this);
		if (Button != null)
		{
			ObservableSubscribeExtensions.Subscribe(Button.OnLeftDoubleClickAsObservable(), delegate
			{
				HandleLeftClick(isDoubleClick: true);
			}).AddTo(this);
			ObservableSubscribeExtensions.Subscribe(Button.OnSingleLeftClickAsObservable(), delegate
			{
				HandleLeftClick();
			}).AddTo(this);
		}
	}

	protected override void OnUnbind()
	{
		m_NoPortraitZone.Hide();
		m_CharacetrPortraitZone.Hide();
	}

	private void HandleLeftClick(bool isDoubleClick = false)
	{
		HandleLeftClickImpl(isDoubleClick);
	}

	protected virtual void HandleLeftClickImpl(bool isDoubleClick = false)
	{
		base.ViewModel.HandleUnitClick(isDoubleClick);
	}

	private void SetupPortrait()
	{
		if (base.ViewModel.UsedSubtypeIcon)
		{
			m_CharacetrPortraitZone.Hide();
			m_NoPortraitZone.SetUnit(base.ViewModel.MechanicEntity);
		}
		else
		{
			m_CharacetrPortraitZone.SetUnit(base.ViewModel.MechanicEntity);
			m_NoPortraitZone.Hide();
		}
		if (base.ViewModel.IsEnemy.CurrentValue)
		{
			m_FractionViewMode = UnitFractionViewMode.Enemy;
		}
		else if (base.ViewModel.IsPlayer.CurrentValue)
		{
			m_FractionViewMode = UnitFractionViewMode.Companion;
		}
		else
		{
			m_FractionViewMode = UnitFractionViewMode.Friend;
		}
		Button.Or(null)?.SetActiveLayer(m_FractionViewMode.ToString());
	}
}
