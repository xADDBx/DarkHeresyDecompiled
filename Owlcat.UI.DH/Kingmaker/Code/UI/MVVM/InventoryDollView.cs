using System;
using System.Collections;
using System.Collections.Generic;
using Code.View.UI.UIUtils;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.DollRoom;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public abstract class InventoryDollView<TSlotView> : CharInfoComponentView<InventoryDollVM> where TSlotView : ItemSlotView<EquipSlotVM>
{
	[SerializeField]
	private FadeAnimator m_DollFadeAnimator;

	[Header("Doll")]
	[SerializeField]
	protected DollRoomTargetController m_CharacterController;

	[Header("Slot groups")]
	[SerializeField]
	private GameObject m_LeftSlots;

	[SerializeField]
	private GameObject m_RightSlots;

	[Header("Body slots")]
	[SerializeField]
	protected TSlotView m_BodyArmor;

	[SerializeField]
	protected TSlotView m_HeadArmor;

	[SerializeField]
	protected TSlotView m_Gloves;

	[SerializeField]
	protected TSlotView m_Boots;

	[SerializeField]
	protected TSlotView m_Back;

	[SerializeField]
	protected TSlotView m_Neck;

	[SerializeField]
	protected TSlotView m_Ring1;

	[SerializeField]
	protected TSlotView m_Ring2;

	[Header("Quick slots")]
	[SerializeField]
	protected TSlotView[] m_QuickSlots;

	[Header("Weapon sets")]
	[SerializeField]
	protected WeaponSetSelectorPCView m_WeaponSetSelector;

	[SerializeField]
	private FadeAnimator m_WeaponSetForegroundFade;

	[Header("Ruler")]
	[SerializeField]
	private InventoryRuler m_Ruler;

	[SerializeField]
	private OwlcatButton m_ResetRulerTargetButton;

	[Header("Mask")]
	[SerializeField]
	private RawImage m_Mask;

	[SerializeField]
	private Material m_TwoSlotsMaskMaterial;

	[SerializeField]
	private Material m_OneSlotMaskMaterial;

	private List<TSlotView> m_AllSlots;

	private bool m_DollRoomInitialized;

	private IEnumerator m_SetupCoroutine;

	private bool m_BindCompleted;

	private CharacterDollRoom Room => UIDollRooms.Instance.Or(null)?.CharacterDollRoom;

	public override void Initialize()
	{
		base.Initialize();
		SetSlotGroupsVisibility(isVisible: false);
		if (m_AllSlots == null)
		{
			m_AllSlots = new List<TSlotView> { m_BodyArmor, m_HeadArmor, m_Gloves, m_Boots, m_Back, m_Neck, m_Ring1, m_Ring2 };
			m_AllSlots.AddRange(m_QuickSlots);
		}
	}

	public void ClearViewIfNeeded()
	{
		if (!base.IsBinded)
		{
			SetSlotGroupsVisibility(isVisible: false);
		}
	}

	protected override void OnBind()
	{
		SetSlotGroupsVisibility(isVisible: true);
		BindSlots();
		UpdateMask();
		base.OnBind();
		m_SetupCoroutine = DollSetupCoroutine();
		m_BindCompleted = true;
		m_Ruler.SetHint(UIStrings.Instance.InventoryScreen.RulerHint);
		m_CharacterController.CurrentZoomNormalized.Subscribe(m_Ruler.SetZoom).AddTo(this);
		m_CharacterController.CurrentRotationAngle.Subscribe(m_Ruler.SetRotation).AddTo(this);
		m_CharacterController.IsHoveredOver.Subscribe(m_Ruler.SetHighlight).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_ResetRulerTargetButton.OnLeftClickAsObservable(), delegate
		{
			m_CharacterController.ResetTargetView();
			ServiceWindowsSounds.Instance.Inventory.ResetDollPosition.Play();
		}).AddTo(this);
		m_WeaponSetForegroundFade.AppearAnimation();
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_BindCompleted = false;
		m_SetupCoroutine = null;
		m_DollRoomInitialized = false;
		m_WeaponSetForegroundFade.DisappearInstant();
	}

	protected override void RefreshView()
	{
		base.RefreshView();
		if (!(Room == null) && m_DollRoomInitialized && base.ViewModel.Unit.CurrentValue != null)
		{
			UpdateRoom();
			BindSlots();
			UpdateMask();
		}
	}

	protected override void OnShow()
	{
		base.OnShow();
		SetSlotGroupsVisibility(isVisible: true);
	}

	protected override void OnHide(UnityAction onHideCallback = null)
	{
		base.OnHide(onHideCallback);
		SwitchOffDoll();
		SetSlotGroupsVisibility(isVisible: false);
	}

	private void InitializeDollRoom()
	{
		try
		{
			Room.Initialize(m_CharacterController);
			m_DollRoomInitialized = true;
			Room.Show();
		}
		catch (Exception ex)
		{
			PFLog.UI.Exception(ex);
		}
	}

	private void UpdateRoom()
	{
		try
		{
			Room.SetupUnit(base.ViewModel.Unit.CurrentValue);
		}
		catch (Exception ex)
		{
			PFLog.UI.Exception(ex);
		}
	}

	private void BindSlots()
	{
		m_BodyArmor.Bind(base.ViewModel.Armor);
		m_HeadArmor.Bind(base.ViewModel.Head);
		m_Gloves.Bind(base.ViewModel.Gloves);
		m_Boots.Bind(base.ViewModel.Feet);
		m_Back.Bind(base.ViewModel.Shoulders);
		m_Neck.Bind(base.ViewModel.Neck);
		m_Ring1.Bind(base.ViewModel.Ring1);
		m_Ring2.Bind(base.ViewModel.Ring2);
		for (int i = 0; i < m_QuickSlots.Length; i++)
		{
			m_QuickSlots[i].Bind(base.ViewModel.QuickSlots[i]);
		}
		m_WeaponSetSelector.Bind(base.ViewModel.WeaponSetSelector);
	}

	private void SwitchOffDoll()
	{
		try
		{
			if ((bool)Room && m_DollRoomInitialized)
			{
				Room.Hide();
			}
		}
		catch (Exception ex)
		{
			PFLog.UI.Exception(ex);
		}
	}

	private void SetSlotGroupsVisibility(bool isVisible)
	{
		m_LeftSlots.Or(null)?.SetActive(isVisible);
		m_RightSlots.Or(null)?.SetActive(isVisible);
	}

	private void UpdateMask()
	{
		int weaponSetsCount = UIUtilityUnit.GetWeaponSetsCount(base.ViewModel.Unit.CurrentValue);
		m_Mask.material = ((weaponSetsCount == 1) ? m_OneSlotMaskMaterial : m_TwoSlotsMaskMaterial);
	}

	private IEnumerator DollSetupCoroutine()
	{
		m_DollFadeAnimator.DisappearInstant();
		InitializeDollRoom();
		m_DollFadeAnimator.AppearAnimation();
		yield return null;
		RefreshView();
		yield return null;
		m_AllSlots.ForEach(delegate(TSlotView s)
		{
			s.RefreshItem();
		});
		yield return null;
		m_WeaponSetSelector.RefreshItems();
	}

	private void MoveCoroutine()
	{
		if (m_BindCompleted && m_SetupCoroutine != null && !m_SetupCoroutine.MoveNext())
		{
			m_SetupCoroutine = null;
		}
	}

	private void Update()
	{
		MoveCoroutine();
	}
}
