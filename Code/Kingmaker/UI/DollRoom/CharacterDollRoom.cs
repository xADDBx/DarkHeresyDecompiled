using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.ResourceLinks;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Kingmaker.View.Equipment;
using Kingmaker.View.Mechadendrites;
using Kingmaker.Visual.Animation;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.CharacterSystem;
using Kingmaker.Visual.MaterialEffects;
using Kingmaker.Visual.MaterialEffects.Dissolve;
using Kingmaker.Visual.Particles;
using Kingmaker.Visual.Particles.GameObjectsPooling;
using Kingmaker.Visual.Trails;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Visual.XPBD;
using R3;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering;

namespace Kingmaker.UI.DollRoom;

public class CharacterDollRoom : DollRoomBase
{
	private sealed class UnitEquipmentSync : IUnitEquipmentHandler<EntitySubscriber>, IUnitEquipmentHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IUnitEquipmentHandler, EntitySubscriber>, IEntitySubscriber, IUnitActiveEquipmentSetHandler<EntitySubscriber>, IUnitActiveEquipmentSetHandler, ISubscriber<IBaseUnitEntity>, IEventTag<IUnitActiveEquipmentSetHandler, EntitySubscriber>, IUnitVisualChangeHandler
	{
		private readonly CharacterDollRoom m_DollRoom;

		public UnitEquipmentSync(CharacterDollRoom dollRoom)
		{
			m_DollRoom = dollRoom;
		}

		public void Subscribe()
		{
			EventBus.Subscribe(this);
		}

		public void Unsubscribe()
		{
			EventBus.Unsubscribe(this);
		}

		public IEntity GetSubscribingEntity()
		{
			return m_DollRoom.m_Unit;
		}

		public void HandleEquipmentSlotUpdated(ItemSlot slot, ItemEntity previousItem)
		{
			if (slot.Owner != m_DollRoom.m_Unit)
			{
				return;
			}
			Character avatar = m_DollRoom.Avatar;
			Character originalAvatar = m_DollRoom.m_OriginalAvatar;
			IEnumerable<EquipmentEntity> enumerable = originalAvatar.EquipmentEntities.Concat(originalAvatar.SavedEquipmentEntities.Select((EquipmentEntityLink ee) => ee.Load()));
			IEnumerable<EquipmentEntity> enumerable2 = avatar.EquipmentEntities.Concat(avatar.SavedEquipmentEntities.Select((EquipmentEntityLink ee) => ee.Load()));
			EquipmentEntity[] ees = enumerable2.Except(enumerable).ToArray();
			EquipmentEntity[] ees2 = enumerable.Except(enumerable2).ToArray();
			IEnumerable<Character.SelectedRampIndices> collection = originalAvatar.RampIndices.Except(avatar.RampIndices);
			avatar.RampIndices.Clear();
			avatar.RampIndices.AddRange(collection);
			avatar.RequestRebuild(Character.CharacterRebuildMode.OnlyAtlases);
			avatar.RemoveEquipmentEntities(ees);
			avatar.AddEquipmentEntities(ees2);
			if (slot is HandSlot slot2)
			{
				m_DollRoom.m_AvatarHands?.HandleEquipmentSlotUpdated(slot2, previousItem);
			}
			else
			{
				PartUnitBody bodyOptional = slot.Owner.GetBodyOptional();
				if (bodyOptional != null && bodyOptional.QuickSlots.Contains(slot))
				{
					m_DollRoom.m_AvatarHands?.UpdateBeltPrefabs();
				}
			}
			if ((bool)originalAvatar && m_DollRoom.m_Unit != null && !originalAvatar.gameObject.activeInHierarchy && m_DollRoom.m_Unit.View != null)
			{
				m_DollRoom.m_Unit.View.HandleEquipmentSlotUpdated(slot, previousItem);
			}
		}

		public void HandleUnitChangeActiveEquipmentSet()
		{
			m_DollRoom.m_AvatarHands?.HandleEquipmentSetChanged();
		}

		public void HandleUnitChangeEquipmentColor(int rampIndex, bool secondary)
		{
			Character originalAvatar = m_DollRoom.m_Unit.View?.CharacterAvatar;
			if (originalAvatar != null)
			{
				OwlcatR3UnitExtensions.Subscribe(Observable.NextFrame(), delegate
				{
					m_DollRoom.Avatar.CopyRampIndicesFrom(originalAvatar);
				}).AddTo(m_DollRoom);
			}
		}
	}

	private BaseUnitEntity m_Unit;

	private UnitViewHandsEquipment m_AvatarHands;

	private UnitViewMechadendritesEquipment m_Mechadendrites;

	[SerializeField]
	protected GameObject m_PlatformPrefab;

	private UnitEntityView m_SimpleAvatar;

	private GameObject m_SnowDecal;

	private GameObject m_AppearFxInstance;

	private readonly List<Renderer> m_TempRenderers = new List<Renderer>();

	private readonly List<ParticleSystem> m_TempParticles = new List<ParticleSystem>();

	private readonly List<ParticlesMaterialController> m_TempMatControllers = new List<ParticlesMaterialController>();

	private readonly List<CompositeTrailRenderer> m_TempTrails = new List<CompositeTrailRenderer>();

	protected GameObject m_PlatformInstance;

	private UnitEquipmentSync m_EquipmentSync;

	private DollRoomAvatarManager<BaseUnitEntity> m_AvatarManager;

	protected Character m_OriginalAvatar;

	public BaseUnitEntity Unit => m_Unit;

	private Character Avatar => m_AvatarManager?.Active;

	public override void Show()
	{
		base.Show();
		SetupDollPostProcessAndAnimation(isCharGen: false);
		ServiceWindowsSounds.Instance.Character.DollAnimationShow.Play();
		if ((bool)m_PlatformPrefab && m_PlatformInstance == null)
		{
			m_PlatformInstance = Object.Instantiate(m_PlatformPrefab, m_TargetPlaceholder.transform);
			m_PlatformInstance.transform.localPosition = Vector3.zero;
			m_PlatformInstance.transform.rotation = Quaternion.identity;
		}
		PlayAppearEffect();
	}

	public override void Hide()
	{
		base.Hide();
		if ((bool)m_PlatformInstance)
		{
			Object.Destroy(m_PlatformInstance);
		}
		if ((bool)m_OriginalAvatar)
		{
			m_OriginalAvatar.enabled = true;
			m_OriginalAvatar = null;
		}
	}

	public virtual void SetupUnit(BaseUnitEntity player)
	{
		if (m_Unit == player)
		{
			return;
		}
		PFLog.UI.Log("SetupInfo");
		Cleanup();
		m_Unit = player;
		if (m_EquipmentSync == null)
		{
			m_EquipmentSync = new UnitEquipmentSync(this);
		}
		m_EquipmentSync.Unsubscribe();
		m_EquipmentSync.Subscribe();
		m_OriginalAvatar = player.View?.CharacterAvatar;
		if (m_OriginalAvatar == null)
		{
			CreateSimpleAvatar(player);
			UnitAnimationManager componentInChildren = m_SimpleAvatar.GetComponentInChildren<UnitAnimationManager>();
			if ((bool)componentInChildren)
			{
				SetupAnimationManagerStandalone(componentInChildren);
			}
			PlayAppearEffect();
			return;
		}
		if (m_AvatarManager == null)
		{
			m_AvatarManager = new DollRoomAvatarManager<BaseUnitEntity>(m_TargetPlaceholder, OnCharacterAvatarUpdated);
		}
		bool hasBallisticMechadendrite = m_Unit.HasMechadendriteOfType(MechadendritesType.Ballistic);
		m_AvatarManager.GetOrCreate(player, m_OriginalAvatar, m_Unit.ToString(), delegate(Character avatar)
		{
			IKController iKController = avatar.gameObject.AddComponent<IKController>();
			iKController.IsDollRoom = true;
			iKController.CharacterSystem = avatar;
			if ((bool)m_OriginalAvatar.GetComponent<UnitEntityView>())
			{
				iKController.CharacterUnitEntity = m_OriginalAvatar.GetComponent<UnitEntityView>();
			}
			m_AvatarManager.SetupAnimationManager(avatar.AnimationManager, hasBallisticMechadendrite);
			HashSet<UnitAnimationManager> mechsAnimationManagers = avatar.MechsAnimationManagers;
			if (mechsAnimationManagers != null && mechsAnimationManagers.Count > 0)
			{
				foreach (UnitAnimationManager mechsAnimationManager in avatar.MechsAnimationManagers)
				{
					if (mechsAnimationManager != null)
					{
						m_AvatarManager.SetupAnimationManager(mechsAnimationManager);
					}
				}
			}
		});
		m_AvatarManager.SetActive(player);
		if ((bool)m_Camera)
		{
			Character avatar2 = Avatar;
			string text = avatar2.Skeleton?.DollRoomZoomPreset.TargetBoneName ?? "Head";
			Transform targetTransform = avatar2.transform.FindChildRecursive(text);
			m_Camera.LookAt(targetTransform, avatar2.Skeleton?.DollRoomZoomPreset);
		}
		m_AvatarHands?.Dispose();
		m_AvatarHands = null;
		m_Mechadendrites = null;
		UpdateCharacter();
		m_Camera.SetCameraFovAndYpos(m_Unit);
		PlayAppearEffect();
	}

	protected override void Cleanup()
	{
		m_TargetPlaceholder.rotation = Quaternion.identity;
		BaseUnitEntity unit = m_Unit;
		if (Avatar != null)
		{
			m_AvatarManager?.Cleanup();
			m_AvatarManager = null;
			m_AvatarHands?.Dispose();
			m_AvatarHands = null;
			m_Mechadendrites = null;
			m_Unit = null;
			if (unit?.View != null)
			{
				unit?.View.HandsEquipment.UpdateVisibility(unit.View.IsVisible);
			}
		}
		if ((bool)m_SimpleAvatar)
		{
			Object.Destroy(m_SimpleAvatar.gameObject);
			m_SimpleAvatar = null;
		}
		m_EquipmentSync?.Unsubscribe();
		base.Cleanup();
	}

	private void UpdateAvatarRenderers()
	{
		GameObject gameObject = (Avatar ? Avatar.gameObject : (m_SimpleAvatar ? m_SimpleAvatar.gameObject : null));
		if (!gameObject)
		{
			return;
		}
		gameObject.GetComponentsInChildren(m_TempRenderers);
		foreach (Renderer tempRenderer in m_TempRenderers)
		{
			tempRenderer.gameObject.layer = 15;
		}
		UnscaleFxTimes(gameObject);
		RefreshAppearEffectForLateRenderers(gameObject);
	}

	private void RefreshAppearEffectForLateRenderers(GameObject avatarGO)
	{
		if (!(m_AppearFxInstance == null))
		{
			StandardMaterialController[] componentsInChildren = avatarGO.GetComponentsInChildren<StandardMaterialController>(includeInactive: true);
			foreach (StandardMaterialController obj in componentsInChildren)
			{
				obj.UpdateRenderers();
				obj.DissolveController.InvalidateCache();
				obj.DoUpdate();
			}
		}
	}

	private void UnscaleFxTimes(GameObject obj)
	{
		if (obj == null)
		{
			return;
		}
		PooledGameObject component = obj.GetComponent<PooledGameObject>();
		if ((bool)component)
		{
			Object.Destroy(component);
		}
		obj.GetComponentsInChildren(m_TempRenderers);
		foreach (Renderer tempRenderer in m_TempRenderers)
		{
			tempRenderer.gameObject.layer = 15;
			tempRenderer.lightProbeUsage = LightProbeUsage.Off;
		}
		obj.GetComponentsInChildren(m_TempParticles);
		foreach (ParticleSystem tempParticle in m_TempParticles)
		{
			ParticleSystem.MainModule main = tempParticle.main;
			main.useUnscaledTime = true;
		}
		obj.GetComponentsInChildren(m_TempTrails);
		foreach (CompositeTrailRenderer tempTrail in m_TempTrails)
		{
			tempTrail.Emitters.ForEach(delegate(TrailEmitter e)
			{
				e.UseUnscaledTime = true;
			});
		}
		obj.GetComponentsInChildren(m_TempMatControllers);
		ParticlesMaterialController[] componentsInChildren = obj.GetComponentsInChildren<ParticlesMaterialController>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].UnscaledTime = true;
		}
		FxFadeOut component2 = obj.GetComponent<FxFadeOut>();
		if ((bool)component2)
		{
			component2.Duration = 0f;
		}
	}

	private void CreateSimpleAvatar(BaseUnitEntity player)
	{
		m_SimpleAvatar = Object.Instantiate(player.View.AsUnitEntityView(), m_TargetPlaceholder, worldPositionStays: false);
		Transform viewTransform = m_SimpleAvatar.ViewTransform;
		viewTransform.localPosition = Vector3.zero;
		viewTransform.localRotation = Quaternion.identity;
		viewTransform.localScale = player.View.ViewTransform.localScale;
		if (!m_SimpleAvatar.gameObject.activeSelf)
		{
			m_SimpleAvatar.gameObject.SetActive(value: true);
			Renderer[] componentsInChildren = m_SimpleAvatar.GetComponentsInChildren<Renderer>();
			if (componentsInChildren.Any((Renderer r) => !r.enabled))
			{
				PFLog.UI.Warning("SimpleAvatar has disabled renderers " + player.View.gameObject.name);
			}
			Renderer[] array = componentsInChildren;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = true;
			}
		}
		UpdateAvatarRenderers();
		Bounds bounds = (from r in m_SimpleAvatar.GetComponentsInChildren<Renderer>()
			select r.bounds).Aggregate(SumBounds);
		if (bounds.size.y * viewTransform.localScale.y > 2.5f)
		{
			float num = 2.5f / (bounds.size.y * viewTransform.localScale.y);
			viewTransform.localScale *= num;
		}
		if ((bool)m_Camera)
		{
			string text = m_SimpleAvatar.CharacterAvatar?.Skeleton?.DollRoomZoomPreset.TargetBoneName ?? "Head";
			Transform targetTransform = viewTransform.FindChildRecursive(text);
			m_Camera.LookAt(targetTransform, m_SimpleAvatar.CharacterAvatar?.Skeleton?.DollRoomZoomPreset);
		}
	}

	private static Bounds SumBounds(Bounds b1, Bounds b2)
	{
		b1.Encapsulate(b2.min);
		b1.Encapsulate(b2.max);
		return b1;
	}

	protected virtual void OnCharacterAvatarUpdated(Character character)
	{
		character.GetComponentsInChildren(m_TempRenderers);
		if (character == Avatar)
		{
			UpdateCharacter();
		}
	}

	private void UpdateCharacter()
	{
		Character avatar = Avatar;
		avatar.UpdateSkeleton();
		if (m_Mechadendrites == null && m_Unit != null)
		{
			m_Mechadendrites = new UnitViewMechadendritesEquipment(m_Unit.View.AsUnitEntityView(), avatar);
			m_Mechadendrites.UpdateAll();
		}
		if (m_AvatarHands?.Owner == null)
		{
			m_AvatarHands?.Dispose();
			m_AvatarHands = null;
			if (m_Unit != null && m_Unit.View != null)
			{
				m_AvatarHands = new UnitViewHandsEquipment(m_Unit.View.AsUnitEntityView(), avatar);
				m_AvatarHands.OnModelSpawned += UpdateAvatarRenderers;
				m_AvatarHands.UpdateAll();
			}
		}
		Animator[] componentsInChildren = avatar.GetComponentsInChildren<Animator>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].updateMode = AnimatorUpdateMode.UnscaledTime;
		}
		UpdateAvatarRenderers();
	}

	private void PlayAppearEffect()
	{
		GameObject gameObject = BlueprintCharGenRoot.Instance?.CharacterAppearEffectPrefab;
		if (gameObject == null)
		{
			return;
		}
		GameObject gameObject2 = ((Avatar != null) ? Avatar.gameObject : ((m_SimpleAvatar != null && m_SimpleAvatar.CharacterAvatar != null) ? m_SimpleAvatar.CharacterAvatar.gameObject : null));
		if (gameObject2 == null)
		{
			return;
		}
		StandardMaterialController[] componentsInChildren = gameObject2.GetComponentsInChildren<StandardMaterialController>(includeInactive: true);
		if (componentsInChildren.Length == 0)
		{
			return;
		}
		m_AppearFxInstance = Object.Instantiate(gameObject, gameObject2.transform, worldPositionStays: false);
		DissolveSetup[] componentsInChildren2 = m_AppearFxInstance.GetComponentsInChildren<DissolveSetup>(includeInactive: true);
		if (componentsInChildren2.Length == 0)
		{
			return;
		}
		DissolveSetup[] array = componentsInChildren2;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Settings.UseUnscaledTime = true;
		}
		StandardMaterialController[] array2 = componentsInChildren;
		foreach (StandardMaterialController standardMaterialController in array2)
		{
			standardMaterialController.UpdateRenderers();
			array = componentsInChildren2;
			foreach (DissolveSetup dissolveSetup in array)
			{
				standardMaterialController.DissolveController.Animations.Add(dissolveSetup.Settings);
			}
		}
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		Hide();
	}

	[UsedImplicitly]
	private void Update()
	{
		if (Game.HasInstance && Game.Instance.CurrentModeType == GameModeType.None)
		{
			TempList.Release();
			TempHashSet.Release();
		}
		UpdateInternal();
		Character avatar = Avatar;
		if (!m_OriginalAvatar || avatar == null)
		{
			if ((bool)m_SimpleAvatar)
			{
				UnitAnimationManager componentInChildren = m_SimpleAvatar.GetComponentInChildren<UnitAnimationManager>();
				if ((bool)componentInChildren)
				{
					componentInChildren.Tick(Time.deltaTime);
				}
			}
			return;
		}
		if ((bool)m_PlatformInstance)
		{
			m_PlatformInstance.transform.rotation = avatar.transform.rotation;
		}
		if (avatar.AnimationManager.HasBallisticMechadendrite)
		{
			if (m_AvatarHands != null)
			{
				BlueprintItem obj = m_AvatarHands.GetSelectedWeaponSet().MainHand.VisibleItem?.Blueprint;
				BlueprintItem blueprintItem = m_AvatarHands.GetSelectedWeaponSet().OffHand.VisibleItem?.Blueprint;
				WeaponType activeMainHandWeaponType = ((obj is BlueprintItemWeapon { IsMelee: not false }) ? m_AvatarHands.ActiveMainHandWeaponType : WeaponType.Fist);
				WeaponType activeOffHandWeaponType = ((blueprintItem is BlueprintItemWeapon { IsMelee: not false }) ? m_AvatarHands.ActiveOffHandWeaponType : WeaponType.Fist);
				WeaponType activeMainHandWeaponType2 = ((obj is BlueprintItemWeapon { IsMelee: false }) ? m_AvatarHands.ActiveMainHandWeaponType : ((blueprintItem is BlueprintItemWeapon { IsMelee: false }) ? m_AvatarHands.ActiveOffHandWeaponType : WeaponType.Fist));
				avatar.AnimationManager.ActiveMainHandWeaponType = activeMainHandWeaponType;
				avatar.AnimationManager.ActiveOffHandWeaponType = activeOffHandWeaponType;
				avatar.AnimationManager.Tick(Time.deltaTime);
				foreach (UnitAnimationManager mechsAnimationManager in avatar.MechsAnimationManagers)
				{
					mechsAnimationManager.ActiveMainHandWeaponType = activeMainHandWeaponType2;
					mechsAnimationManager.ActiveOffHandWeaponType = WeaponType.Fist;
					mechsAnimationManager.Tick(Time.deltaTime);
				}
			}
			else
			{
				avatar.AnimationManager.Tick(Time.deltaTime);
			}
		}
		else
		{
			if (m_AvatarHands != null)
			{
				avatar.AnimationManager.ActiveMainHandWeaponType = m_AvatarHands.ActiveMainHandWeaponType;
				avatar.AnimationManager.ActiveOffHandWeaponType = m_AvatarHands.ActiveOffHandWeaponType;
			}
			avatar.AnimationManager.Tick(Time.deltaTime);
		}
		if (m_AvatarHands != null && !m_AvatarHands.InCombat)
		{
			m_AvatarHands.SetCombatVisualState(inCombat: true);
			m_AvatarHands.MatchWithCurrentCombatState();
		}
		if (Time.timeScale == 0f)
		{
			XPBD.TickByScript();
		}
	}

	protected virtual void UpdateInternal()
	{
	}

	private static void SetupAnimationManagerStandalone(UnitAnimationManager animationManager, bool hasBallisticMechadendrite = false)
	{
		animationManager.UpdateMode = DirectorUpdateMode.UnscaledGameTime;
		animationManager.IsInCombat = true;
		animationManager.HasBallisticMechadendrite = hasBallisticMechadendrite;
		animationManager.UpdateActiveWeaponStyle();
		animationManager.Tick(RealTimeController.SystemStepDurationSeconds);
	}
}
