using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Fx;
using Kingmaker.Controllers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.ResourceLinks;
using Kingmaker.View;
using Kingmaker.View.Covers;
using Kingmaker.View.Equipment;
using Kingmaker.Visual.Animation;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.CharacterSystem;
using Kingmaker.Visual.Particles;
using Owlcat.Runtime.Visual.OccludedObjectHighlighting;
using UnityEngine;

namespace Kingmaker.UnitLogic;

public class UnitHologram : MonoBehaviour
{
	private Character m_OriginalAvatar;

	private BaseUnitEntity m_OriginalBaseUnit;

	private Character m_HologramAvatar;

	private UnitViewHandsEquipment m_AvatarHands;

	private GameObject m_Shading;

	private UnitEntityView m_HologramEntityView;

	private bool m_IsStarshipHologram;

	private UnitAnimationManager AnimationManager => m_HologramAvatar?.AnimationManager;

	public LosCalculations.CoverType CoverType
	{
		get
		{
			return AnimationManager?.CoverType ?? LosCalculations.CoverType.Obstacle;
		}
		set
		{
			if (AnimationManager != null)
			{
				AnimationManager.CoverType = value;
			}
		}
	}

	public Vector3 Direction
	{
		get
		{
			return base.transform.forward;
		}
		set
		{
			base.transform.forward = value;
			if (AnimationManager != null)
			{
				AnimationManager.Orientation = Quaternion.LookRotation(value).eulerAngles.y;
			}
		}
	}

	public UnitEntityView HologramEntityView => m_HologramEntityView;

	public BaseUnitEntity Parent => m_OriginalBaseUnit;

	private void OnEnable()
	{
		EventBus.Subscribe(this);
	}

	private void OnDisable()
	{
		EventBus.Unsubscribe(this);
		Object.Destroy(m_Shading);
	}

	public void Setup([NotNull] UnitEntityView hologramUnit, [NotNull] UnitEntityView originalUnit)
	{
		m_OriginalBaseUnit = originalUnit.EntityData;
		m_HologramEntityView = hologramUnit;
		m_AvatarHands = originalUnit.HandsEquipment;
		Character characterAvatar = hologramUnit.CharacterAvatar;
		m_OriginalAvatar = characterAvatar;
		m_HologramAvatar = SetupAvatar(m_OriginalAvatar);
		hologramUnit.Blueprint = originalUnit.Blueprint;
		SetupShading(ConfigRoot.Instance.FxRoot.Hologram.MainFx);
		OccludedObjectHighlighter component = hologramUnit.GetComponent<OccludedObjectHighlighter>();
		Color color = component.Color;
		color.a = 0f;
		component.Color = color;
	}

	[NotNull]
	private Character SetupAvatar(Character originalAvatar)
	{
		Character component = base.gameObject.GetComponent<Character>();
		component.transform.localScale = originalAvatar.transform.localScale;
		component.DisplayOptions.IsInDollRoom = true;
		component.ForbidBeltItemVisualization = originalAvatar.ForbidBeltItemVisualization;
		component.AnimatorPrefab = originalAvatar.AnimatorPrefab;
		component.AnimationSet = originalAvatar.AnimationSet;
		component.SavedEquipmentEntities = originalAvatar.SavedEquipmentEntities;
		component.InitAnimator();
		if (component.Animator != null)
		{
			if (!component.Animator.gameObject.GetComponent<UnitAnimationCallbackReceiver>())
			{
				component.Animator.gameObject.AddComponent<UnitAnimationCallbackReceiver>();
			}
			component.Animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
		}
		if (component.AnimationManager != null)
		{
			component.AnimationManager.AttachToView(m_HologramEntityView, m_OriginalBaseUnit?.Progression.Race);
			component.AnimationManager.ActiveMainHandWeaponType = m_AvatarHands.ActiveMainHandWeaponType;
			component.AnimationManager.ActiveOffHandWeaponType = m_AvatarHands.ActiveOffHandWeaponType;
			component.AnimationManager.IsInCombat = true;
			component.AnimationManager.Tick(RealTimeController.SystemStepDurationSeconds);
		}
		return component;
	}

	private void Update()
	{
		if (!m_IsStarshipHologram && m_HologramAvatar.AnimationManager != null)
		{
			m_HologramAvatar.AnimationManager.ActiveMainHandWeaponType = m_AvatarHands.ActiveMainHandWeaponType;
			m_HologramAvatar.AnimationManager.ActiveOffHandWeaponType = m_AvatarHands.ActiveOffHandWeaponType;
			m_HologramAvatar.AnimationManager.Tick(Time.deltaTime);
			UnitViewHandsEquipment avatarHands = m_AvatarHands;
			if (avatarHands != null && !avatarHands.InCombat)
			{
				m_AvatarHands.IsUsingHologram = true;
				m_AvatarHands.SetCombatVisualState(inCombat: true);
				m_AvatarHands.MatchWithCurrentCombatState();
				m_AvatarHands.ForceEndChangeEquipment();
				m_AvatarHands.AnimationManager.CustomUpdate(2f);
				m_AvatarHands.AnimationManager.CustomUpdate(2f);
			}
		}
	}

	public void LookAt(Vector3 position)
	{
		Direction = (position - base.transform.position).normalized;
	}

	public void LookAt(MechanicEntity entity)
	{
		LookAt(entity.Position);
	}

	public void SetupShading(PrefabLink fx = null, bool threatening = false)
	{
		if (m_Shading != null)
		{
			Object.Destroy(m_Shading);
		}
		GameObject prefab = fx.Load();
		if (fx != null && m_HologramEntityView != null)
		{
			m_Shading = FxHelper.SpawnFxOnEntity(prefab, m_HologramEntityView);
		}
		HologramEntry hologram = ConfigRoot.Instance.FxRoot.Hologram;
		Material material = null;
		if (threatening)
		{
			MaterialLink threateningHoloMaterial = hologram.ThreateningHoloMaterial;
			if ((object)threateningHoloMaterial != null && threateningHoloMaterial.Exists())
			{
				material = hologram.ThreateningHoloMaterial.Load();
			}
		}
		if ((object)material == null)
		{
			material = hologram.HoloMaterial.Load();
		}
		Renderer[] componentsInChildren = m_HologramEntityView.GetComponentsInChildren<Renderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].sharedMaterials = new Material[1] { material };
		}
	}
}
