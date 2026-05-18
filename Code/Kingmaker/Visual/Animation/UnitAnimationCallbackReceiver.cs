using System;
using Animancer;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.View;
using Kingmaker.View.Equipment;
using Kingmaker.View.Mechanics.Entities;
using Kingmaker.Visual.Animation.Decorators;
using Kingmaker.Visual.Animation.Kingmaker;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Visual.Animation;

[DisallowMultipleComponent]
public class UnitAnimationCallbackReceiver : MonoBehaviour
{
	[CanBeNull]
	private AbstractUnitEntityView m_UnitView;

	[CanBeNull]
	private UnitAnimationManager m_AnimationManager;

	[UsedImplicitly]
	private void OnEnable()
	{
		m_UnitView = GetComponentInParent<AbstractUnitEntityView>();
		m_AnimationManager = GetComponentInParent<UnitAnimationManager>();
	}

	public void AnimateWeaponTrail(float duration)
	{
		if (m_UnitView != null && m_UnitView is UnitEntityView unitEntityView)
		{
			unitEntityView.AnimateWeaponTrail(duration);
		}
	}

	public void FxAnimatorToggleAction(string objectName)
	{
		Transform transform = base.transform.parent.FindChildRecursive(objectName);
		if (transform != null && transform.GetComponent<Animator>() != null)
		{
			transform.GetComponent<Animator>().enabled = true;
		}
	}

	public void PostCommandActEvent()
	{
		m_AnimationManager?.OnCommandActEvent();
	}

	public void PostDecoratorObject(UnitAnimationDecoratorObject decorator, AnimancerState animancerState, float duration)
	{
		if (m_UnitView == null)
		{
			m_UnitView = GetComponentInParent<AbstractUnitEntityView>();
		}
		if (decorator != null && duration > 0f && decorator.Entries.Length != 0 && m_AnimationManager != null && m_UnitView != null)
		{
			UnitAnimationDecoratorManager decoratorManager = m_AnimationManager.DecoratorManager;
			Gender gender = m_UnitView.EntityData.Gender;
			if (!decorator.UseGender || gender == decorator.gender)
			{
				decoratorManager.ShowDecorator(decorator, animancerState, duration);
			}
		}
	}

	public void PlaceFootprintEvent(string locator, int footIndex)
	{
		AbstractUnitEntityView unitView = m_UnitView;
		if (unitView == null)
		{
			return;
		}
		AbstractUnitEntity entityData = unitView.EntityData;
		if (entityData != null)
		{
			EventBus.RaiseEvent((IAbstractUnitEntity)entityData, (Action<IUnitFootstepAnimationEventHandler>)delegate(IUnitFootstepAnimationEventHandler h)
			{
				h.HandleUnitFootstepAnimationEvent(locator, footIndex);
			}, isCheckRuntime: true);
		}
	}

	public void ChangeShowingWeapon(bool isMainWeaponVisible, bool isOffWeaponVisible)
	{
		if (!(m_UnitView == null) && m_UnitView is UnitEntityView unitEntityView)
		{
			WeaponSet weaponSet = unitEntityView.HandsEquipment?.GetSelectedWeaponSet();
			if (weaponSet != null)
			{
				weaponSet.MainHand?.ShowItem(isMainWeaponVisible);
				weaponSet.OffHand?.ShowItem(isOffWeaponVisible);
			}
		}
	}

	public void ChangeAttachPointForMainHandWeapon(bool inMainHand)
	{
		if (!(m_UnitView == null) && m_UnitView is UnitEntityView unitEntityView && !(unitEntityView.HandsEquipment?.GetSelectedWeaponSet()?.MainHand?.VisualModel == null))
		{
			unitEntityView.HandsEquipment.GetSelectedWeaponSet().MainHand.VisualModel.transform.SetParent(inMainHand ? unitEntityView.HandsEquipment.GetSelectedWeaponSet().MainHand.MainHandTransform : unitEntityView.HandsEquipment.GetSelectedWeaponSet().MainHand.OffHandTransform, worldPositionStays: true);
		}
	}
}
