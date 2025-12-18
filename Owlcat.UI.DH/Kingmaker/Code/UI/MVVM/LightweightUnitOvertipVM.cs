using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class LightweightUnitOvertipVM : OvertipEntityVM
{
	private readonly ReactiveProperty<Vector3> m_CameraDistance = new ReactiveProperty<Vector3>();

	private readonly ReactiveProperty<bool> m_HasSurrounding = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsChosen = new ReactiveProperty<bool>();

	private bool m_UnitScanned;

	private Transform m_Bone;

	private Bounds m_Bounds;

	public float? DeathDelay;

	public readonly MechanicEntityUIState MechanicEntityUIState;

	public readonly LightweightOvertipNameBlockVM NameBlockVM;

	public readonly OvertipBarkBlockVM BarkBlockVM;

	public ReadOnlyReactiveProperty<bool> IsBarkActive => BarkBlockVM.IsBarkActive;

	public ReadOnlyReactiveProperty<Vector3> CameraDistance => m_CameraDistance;

	public ReadOnlyReactiveProperty<bool> HasSurrounding => m_HasSurrounding;

	public ReadOnlyReactiveProperty<bool> IsChosen => m_IsChosen;

	public MechanicEntity Unit => MechanicEntityUIState.MechanicEntity.MechanicEntity;

	public MechanicEntityUIWrapper MechanicEntityUIWrapper => MechanicEntityUIState.MechanicEntity;

	public bool ForceOnScreen => IsBarkActive.CurrentValue;

	public bool HideFromScreen
	{
		get
		{
			if (Unit != null)
			{
				if (Game.Instance.IsControllerGamepad)
				{
					if (Unit.IsInState)
					{
						if (!Unit.IsVisibleForPlayer)
						{
							return !Unit.IsDirectlyControllable;
						}
						return false;
					}
					return true;
				}
				if (Unit.IsInState && Unit is AbstractUnitEntity { IsAwake: not false })
				{
					if (!Unit.IsVisibleForPlayer)
					{
						return !Unit.IsDirectlyControllable;
					}
					return false;
				}
				return true;
			}
			return true;
		}
	}

	public bool IsInCameraFrustum => Unit?.IsInCameraFrustum ?? false;

	public LightweightUnitOvertipVM(LightweightUnitEntity unit)
	{
		MechanicEntityUIState = UnitUIStateHolder.Instance.GetOrCreateUnitState(unit);
		NameBlockVM = new LightweightOvertipNameBlockVM(MechanicEntityUIState).AddTo(this);
		BarkBlockVM = new OvertipBarkBlockVM().AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
	}

	protected override Vector3 GetEntityPosition()
	{
		return GetEntityPositionInternal();
	}

	protected override void OnUpdateHandler()
	{
		base.OnUpdateHandler();
		m_CameraDistance.Value = base.Position - CameraRig.Instance.GetTargetPointPosition();
	}

	protected override void OnDispose()
	{
		if (Unit != null)
		{
			UnitUIStateHolder.Instance.RemoveUnitState(Unit);
		}
		else
		{
			UnitUIStateHolder.Instance.RemoveUnitState(MechanicEntityUIWrapper.UniqueId);
		}
	}

	public void ShowBark(string text)
	{
		BarkBlockVM.ShowBark(text);
	}

	public void HideBark()
	{
		BarkBlockVM.HideBark();
	}

	public void SetDeathDelay(float val)
	{
		DeathDelay = val;
	}

	public void HandleSurroundingObjectsChanged(bool moreThenOne, bool isChosen)
	{
		m_HasSurrounding.Value = moreThenOne;
		m_IsChosen.Value = isChosen;
	}

	private Vector3 GetEntityPositionInternal()
	{
		if (!m_UnitScanned && Unit?.View != null)
		{
			m_Bone = Unit.View.ViewTransform.FindChildRecursive("UI_Overtip_Bone");
			m_UnitScanned = true;
		}
		if (m_Bone != null)
		{
			return m_Bone.position;
		}
		if (Unit is AbstractUnitEntity { Position: var position } abstractUnitEntity)
		{
			position.y += abstractUnitEntity.View?.CameraOrientedBoundsSize.y ?? Vector2.zero.y;
			return position;
		}
		return Vector3.zero;
	}
}
