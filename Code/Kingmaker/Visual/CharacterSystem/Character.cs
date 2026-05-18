using System;
using System.Collections.Generic;
using System.Linq;
using Code.Framework.Utility.UnityExtensions;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.View.Visual.CharacterSystem;
using Kingmaker.Code.View.Visual.CharacterSystem.EquipmentComponents;
using Kingmaker.ResourceLinks;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.Mechanics.Entities;
using Kingmaker.Visual.Animation;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.MaterialEffects;
using Kingmaker.Visual.Particles;
using Kingmaker.Visual.Utility;
using Owlcat.Runtime.Core.Registry;
using Owlcat.Runtime.Core.Updatables;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Visual.Highlighting;
using Owlcat.Runtime.Visual.OccludedObjectHighlighting;
using Owlcat.Runtime.Visual.XPBD.Authoring;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Serialization;

namespace Kingmaker.Visual.CharacterSystem;

[KnowledgeDatabaseID("49ec7da2c03301e4ca927c5c1a2e00ed")]
public class Character : RegisteredBehaviour, IUpdatable
{
	[Flags]
	public enum RenderingLayerEnum
	{
		Nothing = 0,
		[InspectorName("0: RenderingLayer1")]
		RenderingLayer1 = 1,
		[InspectorName("1: RenderingLayer2")]
		RenderingLayer2 = 2,
		[InspectorName("2: RenderingLayer3")]
		RenderingLayer3 = 4,
		[InspectorName("3: RenderingLayer4")]
		RenderingLayer4 = 8,
		[InspectorName("4: RenderingLayer5")]
		RenderingLayer5 = 0x10,
		[InspectorName("5: RenderingLayer6")]
		RenderingLayer6 = 0x20,
		[InspectorName("6: RenderingLayer7")]
		RenderingLayer7 = 0x40,
		[InspectorName("7: RenderingLayer8")]
		RenderingLayer8 = 0x80,
		Everything = 0xFF
	}

	[Flags]
	public enum CharacterRebuildMode
	{
		None = 1,
		OnlyOutfit = 2,
		OnlyAtlases = 4,
		FullUpdate = 8
	}

	public class SelectedRampIndices
	{
		public EquipmentEntity EquipmentEntity { get; set; }

		public int PrimaryIndex { get; set; }

		public int SecondaryIndex { get; set; }
	}

	[Serializable]
	public class SavedSelectedRampIndices
	{
		public EquipmentEntityLink EquipmentEntityLink;

		public int PrimaryIndex;

		public int SecondaryIndex;
	}

	private struct EquipmentModifierEntry
	{
		public Skeleton.BoneModifier Modifier;

		public Transform BoneTransform;

		public int Priority;
	}

	[HideInInspector]
	public bool PreventUpdate;

	[HideInInspector]
	public uint CurrentLayer;

	[SerializeField]
	private CharacterStudio.Gender m_Gender = CharacterStudio.Gender.NotDetermined;

	[SerializeField]
	private CharacterStudio.Race m_Race = CharacterStudio.Race.NotDetermined;

	[SerializeField]
	public CharacterAtlasData AtlasData;

	[SerializeField]
	public List<SavedSelectedRampIndices> m_SavedRampIndices = new List<SavedSelectedRampIndices>();

	[FormerlySerializedAs("BakedCharacter")]
	[SerializeField]
	private BakedCharacter m_BakedCharacter;

	[SerializeField]
	private Skeleton m_Skeleton;

	[SerializeField]
	private CharacterBonesList m_BonesList;

	[SerializeField]
	private List<EquipmentEntityLink> m_SavedEquipmentEntities = new List<EquipmentEntityLink>();

	private bool m_IsInitialized;

	private const string MaleEEMarker = "_M_";

	private const string FemaleEEMarker = "_F_";

	[NonSerialized]
	private bool m_HasMaleEE;

	[NonSerialized]
	private bool m_HasFemaleEE;

	[NonSerialized]
	private bool m_GenderMismatchWarned;

	private readonly List<EquipmentModifierEntry> m_EquipmentBoneModifiers = new List<EquipmentModifierEntry>();

	private BoneUpdateJob m_BoneUpdateJob;

	private TransformAccessArray m_BonesForJob;

	private CharacterRebuildMode m_RebuildRequest;

	private CharacterBuilder m_CharacterBuilder;

	public readonly List<SelectedRampIndices> RampIndices = new List<SelectedRampIndices>();

	public AnimationSet OverrideAnimationSet;

	public AnimationSet m_AnimationSet;

	public Animator AnimatorPrefab;

	[Tooltip("Галка, которая дает возможность собирать кричу в чаргене без необходимости экипировать ее оружием")]
	public bool IsCreatureAsCharacter;

	[Tooltip("Sometimes we need to forbid visualization of belt items, for Example on Ulfar")]
	public bool ForbidBeltItemVisualization;

	public bool SaveRagdoll;

	public CharacterAtlasSize MaxAtlasSize = CharacterAtlasSize.Default;

	public readonly List<EquipmentEntity> SavedBeforeCutsceneEquipment = new List<EquipmentEntity>();

	public readonly List<SelectedRampIndices> SavedBeforeCutsceneRampIndices = new List<SelectedRampIndices>();

	public CharacterDisplayOptions DisplayOptions = CharacterDisplayOptions.Default;

	public HashSet<UnitAnimationManager> MechsAnimationManagers = new HashSet<UnitAnimationManager>();

	private AbstractUnitEntityView m_Owner;

	public static RenderingLayerEnum DefaultCharacterRenderingLayer => RenderingLayerEnum.RenderingLayer2;

	public (CharacterStudio.Gender gender, CharacterStudio.Race race) GenderAndRace
	{
		get
		{
			return (gender: m_Gender, race: m_Race);
		}
		set
		{
			(m_Gender, m_Race) = value;
		}
	}

	public CharacterBonesList BonesList => m_BonesList;

	public SkinnedMeshRenderer CharacterRenderer { get; private set; }

	public AnimationSet AnimationSet
	{
		get
		{
			if (!(m_AnimationSet == null))
			{
				return m_AnimationSet;
			}
			return ConfigRoot.Instance.SystemMechanics.HumanAnimationSet;
		}
		set
		{
			m_AnimationSet = value;
		}
	}

	public BakedCharacter BakedCharacter
	{
		get
		{
			return m_BakedCharacter;
		}
		set
		{
			m_BakedCharacter = value;
		}
	}

	public bool IsBaked => m_BakedCharacter != null;

	public bool HasBonesList => m_BonesList != null;

	public bool IsTextureCompressionInProgress
	{
		get
		{
			if (!IsBaked)
			{
				return !m_CharacterBuilder.IsAtlasCompressed();
			}
			return false;
		}
	}

	public bool IsReadyForBaking
	{
		get
		{
			if (!IsBaked)
			{
				return m_CharacterBuilder?.IsReadyForBaking() ?? false;
			}
			return false;
		}
	}

	public List<EquipmentEntityLink> SavedEquipmentEntities
	{
		get
		{
			return m_SavedEquipmentEntities;
		}
		set
		{
			m_SavedEquipmentEntities = value;
		}
	}

	public UnitAnimationManager AnimationManager { get; private set; }

	public List<EquipmentEntity> EquipmentEntities { get; } = new List<EquipmentEntity>();


	public int EquipmentEntityCount => EquipmentEntities.Count;

	public ParticlesSnapMap ParticlesSnapMap { get; private set; }

	public Skeleton Skeleton
	{
		get
		{
			return m_Skeleton;
		}
		set
		{
			m_Skeleton = value;
			if (m_IsInitialized)
			{
				CacheSkeletonBones();
			}
		}
	}

	public Animator Animator { get; private set; }

	public bool PeacefulMode => DisplayOptions.IsPeacefulMode;

	private bool IsInCharacterStudio => false;

	public event Action OnBackEquipmentUpdated;

	public event Action<Character> OnUpdated;

	public event Action<Character> OnOutfitOnlyUpdated;

	public void Initialize()
	{
		if (m_IsInitialized)
		{
			return;
		}
		if (IsInCharacterStudio)
		{
			Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				UnityEngine.Object.DestroyImmediate(componentsInChildren[i].gameObject);
			}
		}
		if (IsBaked)
		{
			base.gameObject.EnsureComponent<StandardMaterialController>();
			InitBakedCharacter();
			InitAnimator();
			CacheSkeletonBones();
			SetUpCharacterRenderingLayerMask();
		}
		else
		{
			base.gameObject.EnsureComponent<StandardMaterialController>();
			SkeletonUpdateService.Ensure();
			InitRuntimeCharacter();
			InitAnimator();
			RestoreSavedEquipment();
			m_RebuildRequest = CharacterRebuildMode.FullUpdate;
		}
		m_IsInitialized = true;
	}

	private void OnAddEquipmentEntity(EquipmentEntity equipmentEntity)
	{
		foreach (IAddEquipmentEntityHandler featureComponent in equipmentEntity.GetFeatureComponents<IAddEquipmentEntityHandler>())
		{
			featureComponent.HandleEquipmentEntityAdded(m_Owner);
		}
	}

	private void OnDisposeBakedCharacter()
	{
		foreach (EquipmentEntityLink savedEquipmentEntity in SavedEquipmentEntities)
		{
			EquipmentEntity equipmentEntity = savedEquipmentEntity.Load();
			if (!(equipmentEntity == null))
			{
				OnRemoveEquipmentEntity(equipmentEntity);
			}
		}
	}

	private void OnRemoveEquipmentEntity(EquipmentEntity equipmentEntity)
	{
		foreach (IRemoveEquipmentEntityHandler featureComponent in equipmentEntity.GetFeatureComponents<IRemoveEquipmentEntityHandler>())
		{
			featureComponent.HandleEquipmentEntityRemoved(m_Owner);
		}
	}

	private void InitBakedCharacter()
	{
		foreach (BakedCharacter.RendererDescription rendererDescription in m_BakedCharacter.RendererDescriptions)
		{
			if (rendererDescription.Mesh != null)
			{
				rendererDescription.Mesh.UploadMeshData(markNoLongerReadable: true);
			}
		}
		SkinnedMeshRenderer[] componentsInChildren = GetComponentsInChildren<SkinnedMeshRenderer>();
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in componentsInChildren)
		{
			if (skinnedMeshRenderer?.sharedMesh?.name == "Character")
			{
				CharacterRenderer = skinnedMeshRenderer;
				break;
			}
		}
		MeshBodyBase[] componentsInChildren2 = GetComponentsInChildren<MeshBodyBase>();
		foreach (MeshBodyBase comp in componentsInChildren2)
		{
			comp.EnsureComponent<HighlighterBlocker>();
			comp.EnsureComponent<OccludedObjectHighlighterBlocker>();
		}
		foreach (EquipmentEntityLink savedEquipmentEntity in SavedEquipmentEntities)
		{
			EquipmentEntity equipmentEntity = savedEquipmentEntity.Load();
			if (!(equipmentEntity == null))
			{
				OnAddEquipmentEntity(equipmentEntity);
			}
		}
	}

	private void InitRuntimeCharacter()
	{
		if (AnimatorPrefab != null)
		{
			Animator = GetComponentInChildren<Animator>();
			if (Animator != null)
			{
				PFLog.TechArt.Error($"Incorrect assembly! {base.gameObject.name} already has an Animator component, but we try to create a new one from AnimatorPrefab {AnimatorPrefab}");
				Utils.EditorSafeDestroy(Animator.gameObject);
			}
			Animator animator = UnityEngine.Object.Instantiate(AnimatorPrefab, base.transform);
			Transform obj = animator.transform;
			obj.localPosition = Vector3.zero;
			obj.localRotation = Quaternion.identity;
			obj.localScale = Vector3.one;
			animator.gameObject.name = base.name + ".animator";
			Animator = animator;
		}
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Utils.EditorSafeDestroy(componentsInChildren[i].gameObject);
		}
		if (m_Skeleton != null && m_Skeleton.CharacterFxBonesMap != null)
		{
			ParticlesSnapMap = this.EnsureComponent<ParticlesSnapMap>();
			ParticlesSnapMap.CharacterFxBonesMap = m_Skeleton.CharacterFxBonesMap;
			ParticlesSnapMap.Init();
		}
		m_CharacterBuilder = new CharacterBuilder(base.gameObject);
	}

	public void InitAnimSet()
	{
		if (Skeleton.AnimationSetOverride != null)
		{
			AnimationSet = Skeleton.AnimationSetOverride;
		}
		if (OverrideAnimationSet != null)
		{
			AnimationSet = OverrideAnimationSet;
		}
		AnimationManager.AnimationSet = AnimationSet;
	}

	public void InitAnimator()
	{
		if ((object)Animator == null)
		{
			Animator animator = (Animator = GetComponentInChildren<Animator>());
		}
		if (Animator == null)
		{
			PFLog.TechArt.Error("Animator is null in character " + base.gameObject.name + ". Animator will not be initialized.");
			return;
		}
		if (Skeleton == null)
		{
			PFLog.TechArt.Error("SkeletonConfig is null in character " + base.gameObject.name + ". Animator will not be initialized.");
			return;
		}
		Animator.runtimeAnimatorController = null;
		Animator.enabled = true;
		AnimationManager = Animator.EnsureComponent<UnitAnimationManager>();
		AnimationManager.IsInDollRoom = DisplayOptions.IsInDollRoom;
		InitAnimSet();
		if (AnimationSet == null)
		{
			PFLog.TechArt.Error("AnimationSet is null in character " + base.gameObject.name + ". Animator will not be initialized.");
			return;
		}
		m_BonesList = Animator.EnsureComponent<CharacterBonesList>();
		if (m_BonesList.Bones == null)
		{
			m_BonesList.UpdateCache(CharacterBonesSetup.Instance);
		}
		Animator.EnsureComponent<AimIKController>();
		Animator.EnsureComponent<LookAtIKController>();
	}

	private void OnDestroy()
	{
		m_CharacterBuilder?.Cleanup();
		if (m_BonesForJob.isCreated)
		{
			m_BonesForJob.Dispose();
		}
		if (IsBaked)
		{
			OnDisposeBakedCharacter();
		}
	}

	public void DoUpdate()
	{
		if (m_IsInitialized && Game.HasInstance && !IsBaked && !PreventUpdate)
		{
			m_RebuildRequest |= DisplayOptions.CharacterRebuildRequest;
			DisplayOptions.CharacterRebuildRequest = CharacterRebuildMode.None;
			if (m_RebuildRequest != CharacterRebuildMode.None)
			{
				BuildCharacter();
			}
		}
	}

	private void BuildCharacter()
	{
		try
		{
			m_CharacterBuilder.BuildCharacter(EquipmentEntities, RampIndices, AtlasData, MaxAtlasSize, DisplayOptions, m_RebuildRequest);
			CharacterRenderer = m_CharacterBuilder.CharacterMesh;
			if ((m_RebuildRequest & CharacterRebuildMode.FullUpdate) == CharacterRebuildMode.FullUpdate)
			{
				if (Animator != null)
				{
					Animator.Rebind();
					Animator.Update(0f);
				}
				CacheSkeletonBones();
				SetUpCharacterRenderingLayerMask();
				List<SkinnedMeshRenderer> renderers = GetComponentsInChildren<SkinnedMeshRenderer>().ToList();
				ProbeAnchorOverrider.UpdateProbeAnchorsOnObject(base.gameObject, renderers);
				this.OnUpdated?.Invoke(this);
				this.OnBackEquipmentUpdated?.Invoke();
			}
			if ((m_RebuildRequest & CharacterRebuildMode.OnlyOutfit) == CharacterRebuildMode.OnlyOutfit)
			{
				this.OnOutfitOnlyUpdated?.Invoke(this);
			}
			if ((m_RebuildRequest & CharacterRebuildMode.OnlyAtlases) != 0)
			{
				m_CharacterBuilder.UpdateOutfitColors(RampIndices);
			}
		}
		catch (Exception ex)
		{
			PFLog.TechArt.Exception(ex);
		}
		finally
		{
			m_RebuildRequest = CharacterRebuildMode.None;
			DisplayOptions.CharacterRebuildRequest = CharacterRebuildMode.None;
		}
	}

	public void RequestRebuild(CharacterRebuildMode mode)
	{
		if (!IsBaked)
		{
			m_RebuildRequest |= mode;
		}
	}

	public void SetPrimaryRampIndex(EquipmentEntity ee, int primaryRampIndex, bool saved = false)
	{
		SetRampIndices(ee, primaryRampIndex, null, saved);
	}

	public void SetSecondaryRampIndex(EquipmentEntity ee, int secondaryRampIndex, bool saved = false)
	{
		SetRampIndices(ee, null, secondaryRampIndex, saved);
	}

	public void SetRampIndices(EquipmentEntity ee, int? primaryRampIndex, int? secondaryRampIndex, bool saved = false)
	{
		if (!(ee == null) && (primaryRampIndex.HasValue || secondaryRampIndex.HasValue))
		{
			SelectedRampIndices selectedRampIndices = RampIndices.FirstOrDefault((SelectedRampIndices i) => i.EquipmentEntity == ee);
			if (selectedRampIndices == null)
			{
				selectedRampIndices = CreateRampIndices(ee);
			}
			if (primaryRampIndex.HasValue)
			{
				selectedRampIndices.PrimaryIndex = primaryRampIndex.Value;
			}
			if (secondaryRampIndex.HasValue)
			{
				selectedRampIndices.SecondaryIndex = secondaryRampIndex.Value;
			}
			RequestRebuild(CharacterRebuildMode.OnlyAtlases);
		}
	}

	private SelectedRampIndices CreateRampIndices(EquipmentEntity ee)
	{
		SelectedRampIndices selectedRampIndices = new SelectedRampIndices
		{
			EquipmentEntity = ee
		};
		RampIndices.Add(selectedRampIndices);
		return selectedRampIndices;
	}

	private void CacheSkeletonBones()
	{
		if (m_BonesForJob.isCreated)
		{
			m_BonesForJob.Dispose();
		}
		if (!HasBonesList)
		{
			PFLog.TechArt.Error(base.gameObject, "Failed to cache skeleton bones! BonesList is NULL in character " + base.gameObject.name);
			return;
		}
		Transform[] array = new Transform[Skeleton.Bones.Count];
		for (int i = 0; i < Skeleton.Bones.Count; i++)
		{
			Skeleton.Bone bone = Skeleton.Bones[i];
			array[i] = m_BonesList.GetByName(bone.Name);
		}
		m_BonesForJob = new TransformAccessArray(array);
		m_BoneUpdateJob = new BoneUpdateJob
		{
			Scales = Skeleton.GetBoneData()
		};
		if (IsBaked)
		{
			return;
		}
		m_EquipmentBoneModifiers.Clear();
		foreach (EquipmentEntity equipmentEntity in EquipmentEntities)
		{
			int num = 0;
			num = (equipmentEntity.HasFeature(EquipmentFeatureFlag.IsBackpack) ? 30 : (equipmentEntity.HasFeature(EquipmentFeatureFlag.IsCloak) ? 20 : (equipmentEntity.HasFeature(EquipmentFeatureFlag.IsArmor) ? 10 : 0)));
			foreach (Skeleton.BoneModifier skeletonModifier in equipmentEntity.SkeletonModifiers)
			{
				if (skeletonModifier.IsValid() && !skeletonModifier.HasConflictsWithEquipment(EquipmentEntities))
				{
					Transform byName = m_BonesList.GetByName(skeletonModifier.BoneType.BoneName);
					if (byName == null)
					{
						PFLog.TechArt.Error("Can't find bone for EE skeleton modifier " + equipmentEntity.name + ", " + skeletonModifier.BoneType.BoneName);
						continue;
					}
					m_EquipmentBoneModifiers.Add(new EquipmentModifierEntry
					{
						Modifier = skeletonModifier,
						BoneTransform = byName,
						Priority = num
					});
				}
			}
		}
		m_EquipmentBoneModifiers.Sort((EquipmentModifierEntry a, EquipmentModifierEntry b) => a.Priority.CompareTo(b.Priority));
	}

	public void RestoreSavedEquipment()
	{
		AddEquipmentEntities(m_SavedEquipmentEntities.Select((EquipmentEntityLink eel) => eel.Load()));
		foreach (SavedSelectedRampIndices savedRampIndex in m_SavedRampIndices)
		{
			EquipmentEntity ee = savedRampIndex.EquipmentEntityLink.Load();
			SetRampIndices(ee, savedRampIndex.PrimaryIndex, savedRampIndex.SecondaryIndex);
		}
	}

	public void RestoreEquipment()
	{
		AddEquipmentEntities(SavedBeforeCutsceneEquipment);
		foreach (SavedSelectedRampIndices savedRampIndex in m_SavedRampIndices)
		{
			EquipmentEntity ee = savedRampIndex.EquipmentEntityLink.Load();
			SetRampIndices(ee, savedRampIndex.PrimaryIndex, savedRampIndex.SecondaryIndex);
		}
	}

	public void AddEquipmentEntity(EquipmentEntityLink eel, bool saved = false)
	{
		AddEquipmentEntity(eel.Load(), saved);
	}

	public void RemoveEquipmentEntity(EquipmentEntityLink eel, bool saved = false)
	{
		RemoveEquipmentEntity(eel.Load(), saved);
	}

	public void AddEquipmentEntity(EquipmentEntity ee, bool saved = false)
	{
		if (ee == null)
		{
			return;
		}
		DetectMixedGenderEquipment(ee);
		if (EquipmentEntities.Contains(ee))
		{
			return;
		}
		EquipmentEntities.Add(ee);
		OnAddEquipmentEntity(ee);
		if (ee.HasPrimaryRamps || ee.HasSecondaryRamps)
		{
			if (!RampIndices.Contains((SelectedRampIndices rampIndices) => rampIndices.EquipmentEntity == ee))
			{
				CreateRampIndices(ee);
			}
			RequestRebuild(CharacterRebuildMode.OnlyAtlases);
		}
		RequestRebuild(CharacterRebuildMode.FullUpdate);
	}

	private void DetectMixedGenderEquipment(EquipmentEntity ee)
	{
		if (m_GenderMismatchWarned || string.IsNullOrEmpty(ee.name))
		{
			return;
		}
		bool flag = ee.name.Contains("_M_");
		bool flag2 = ee.name.Contains("_F_");
		if (flag != flag2)
		{
			if (flag)
			{
				m_HasMaleEE = true;
			}
			else
			{
				m_HasFemaleEE = true;
			}
			if (m_HasMaleEE && m_HasFemaleEE)
			{
				m_GenderMismatchWarned = true;
				PFLog.TechArt.Error(base.gameObject, "Mixed-gender EquipmentEntities on character '" + base.gameObject.name + "': adding '" + ee.name + "' but list already contains opposite-gender items. Breaks bone-binding (mesh parts stuck in bind-pose, mismatched skeleton). Likely cause: gender-dependent equipment was resolved before Owner.Gender was set on the unit.");
			}
		}
	}

	public void AddEquipmentEntities(IEnumerable<EquipmentEntity> ees, bool saved = false)
	{
		using (ProfileScope.NewScope("AddEquipmentEntities"))
		{
			ees.ForEach(delegate(EquipmentEntity ee)
			{
				AddEquipmentEntity(ee, saved);
			});
		}
	}

	public void AddEquipmentEntities(IEnumerable<EquipmentEntityLink> ees, bool saved = false)
	{
		using (ProfileScope.NewScope("AddEquipmentEntities"))
		{
			ees.ForEach(delegate(EquipmentEntityLink ee)
			{
				AddEquipmentEntity(ee);
			});
		}
	}

	public void RemoveEquipmentEntity(EquipmentEntity ee, bool saved = false)
	{
		if (!(ee == null))
		{
			RampIndices.Remove((SelectedRampIndices rampIndices) => rampIndices.EquipmentEntity == ee);
			if (EquipmentEntities.Remove(ee))
			{
				OnRemoveEquipmentEntity(ee);
				RequestRebuild(CharacterRebuildMode.FullUpdate);
			}
		}
	}

	public void RemoveAllEquipmentEntities(bool saved = false)
	{
		if (EquipmentEntities.Any())
		{
			RequestRebuild(CharacterRebuildMode.FullUpdate);
		}
		EquipmentEntities.Clear();
		RampIndices.Clear();
	}

	public void RemoveEquipmentEntities(IEnumerable<EquipmentEntityLink> ees, bool saved = false)
	{
		using (ProfileScope.NewScope("RemoveEquipmentEntities"))
		{
			ees.ForEach(delegate(EquipmentEntityLink ee)
			{
				RemoveEquipmentEntity(ee, saved);
			});
		}
	}

	public void RemoveEquipmentEntities(IEnumerable<EquipmentEntity> ees, bool saved = false)
	{
		using (ProfileScope.NewScope("RemoveEquipmentEntities"))
		{
			ees.ForEach(delegate(EquipmentEntity ee)
			{
				RemoveEquipmentEntity(ee, saved);
			});
		}
	}

	public void CopyEquipmentFrom(Character originalAvatar)
	{
		RemoveAllEquipmentEntities();
		AddEquipmentEntities(originalAvatar.EquipmentEntities);
		AddEquipmentEntities(originalAvatar.m_SavedEquipmentEntities.Select((EquipmentEntityLink eel) => eel.Load()), saved: true);
		CopyRampIndicesFrom(originalAvatar);
		DisplayOptions = originalAvatar.DisplayOptions;
	}

	public void CopyRampIndicesFrom(Character originalAvatar)
	{
		foreach (SelectedRampIndices rampIndex in originalAvatar.RampIndices)
		{
			SetRampIndices(rampIndex.EquipmentEntity, rampIndex.PrimaryIndex, rampIndex.SecondaryIndex);
		}
		foreach (SavedSelectedRampIndices savedRampIndex in originalAvatar.m_SavedRampIndices)
		{
			EquipmentEntity ee = savedRampIndex.EquipmentEntityLink.Load();
			SetRampIndices(ee, savedRampIndex.PrimaryIndex, savedRampIndex.SecondaryIndex);
		}
		RequestRebuild(CharacterRebuildMode.OnlyAtlases);
	}

	public JobHandle ScheduleBoneUpdateJob()
	{
		if (!m_BonesForJob.isCreated)
		{
			CacheSkeletonBones();
		}
		return m_BoneUpdateJob.Schedule(m_BonesForJob);
	}

	public void UpdateSkeleton(bool runJob = true)
	{
		using (ProfileScope.New("UpdateSkeleton"))
		{
			if (runJob)
			{
				ScheduleBoneUpdateJob().Complete();
			}
			if (!IsBaked && HasBonesList)
			{
				for (int i = 0; i < m_EquipmentBoneModifiers.Count; i++)
				{
					Transform boneTransform = m_EquipmentBoneModifiers[i].BoneTransform;
					if (boneTransform != null)
					{
						boneTransform.localPosition = Vector3.zero;
						boneTransform.localRotation = Quaternion.identity;
						boneTransform.localScale = Vector3.one;
					}
				}
				for (int j = 0; j < m_EquipmentBoneModifiers.Count; j++)
				{
					m_EquipmentBoneModifiers[j].Modifier.Apply(m_EquipmentBoneModifiers[j].BoneTransform);
				}
			}
			if ((bool)Skeleton && Skeleton.IsDirty())
			{
				Skeleton.ResetDirty();
			}
		}
	}

	public void UpdateSkeletonDirectly(Transform root = null)
	{
		using (ProfileScope.New("UpdateSkeletonEditor"))
		{
			foreach (Skeleton.Bone bone in Skeleton.Bones)
			{
				Transform byName = m_BonesList.GetByName(bone.Name);
				if ((bool)byName)
				{
					byName.localScale = bone.Scale;
					if (bone.ApplyOffset)
					{
						byName.localPosition = bone.Offset;
					}
				}
			}
			if ((bool)Skeleton && Skeleton.IsDirty())
			{
				Skeleton.ResetDirty();
			}
		}
	}

	public static Transform FindBone(Transform parent, string name)
	{
		if (parent.name == name)
		{
			return parent;
		}
		int childCount = parent.childCount;
		for (int i = 0; i < childCount; i++)
		{
			Transform transform = FindBone(parent.GetChild(i), name);
			if (transform != null)
			{
				return transform;
			}
		}
		return null;
	}

	private void SetUpCharacterRenderingLayerMask()
	{
		base.gameObject.layer = LayerMask.NameToLayer("Unit");
		byte b = 1;
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			renderer.gameObject.layer = LayerMask.NameToLayer("Unit");
			uint renderingLayerMask = renderer.renderingLayerMask;
			if (renderingLayerMask != 0)
			{
				uint num = (uint)(((renderingLayerMask & (1 << b - 1)) == 0L) ? renderingLayerMask : (renderingLayerMask ^ (1 << b - 1)));
				if (num > 254)
				{
					num &= 0xFEu;
				}
				if (num == 0)
				{
					renderer.renderingLayerMask = (byte)DefaultCharacterRenderingLayer;
				}
				else
				{
					renderer.renderingLayerMask = num;
				}
				CurrentLayer = renderer.renderingLayerMask;
			}
		}
	}

	public void SetupOwner(AbstractUnitEntityView owner)
	{
		m_Owner = owner;
	}
}
