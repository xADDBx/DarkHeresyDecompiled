using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Animancer;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Kingmaker.View.Mechanics.Entities;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.Particles;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Visual.CharactersRigidbody;

public class RigidbodyCreatureController : MonoBehaviour, Kingmaker.Controllers.IUpdatable
{
	public class ImpulseData
	{
		public Vector3 Direction;

		public float MagnitudeModifier;

		public TimeSpan Time;

		public DamageType DamageType;
	}

	public enum RagdollState
	{
		Off,
		Falling,
		Lying,
		Standing
	}

	[Serializable]
	public struct BoneImpulseMultiplier
	{
		public Rigidbody bone;

		public float multiplier;
	}

	[OwlPackable(OwlPackableMode.Generate)]
	public class BoneData : IHashable, IOwlPackable, IOwlPackable<BoneData>
	{
		[JsonProperty]
		[OwlPackInclude]
		public string Name = "";

		[JsonProperty(IsReference = false)]
		[OwlPackInclude]
		public Vector3 Positions;

		[JsonProperty(IsReference = false)]
		[OwlPackInclude]
		public Quaternion Rotations;

		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "BoneData",
			OldNames = null,
			Fields = new FieldInfo[3]
			{
				new FieldInfo("Name", typeof(string)),
				new FieldInfo("Positions", typeof(Vector3)),
				new FieldInfo("Rotations", typeof(Quaternion))
			}
		};

		public virtual Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			result.Append(Name);
			result.Append(ref Positions);
			result.Append(ref Rotations);
			return result;
		}

		public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
		{
			BoneData source = new BoneData();
			result = Unsafe.As<BoneData, TPossiblyBase>(ref source);
		}

		public virtual void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter
		{
			(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
			var (objectId, _) = orRegister;
			if (orRegister.isRef)
			{
				formatter.ObjectRef(objectId);
				return;
			}
			ushort type = state.TypeLibrary.RegisterType<BoneData>(OwlPackTypeInfo);
			formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
			formatter.StringField(0, "Name", ref Name, state);
			formatter.Field(1, "Positions", ref Positions, state);
			formatter.Field(2, "Rotations", ref Rotations, state);
			formatter.EndObject();
		}

		public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
		{
			state.References.Register(objectId, this);
			TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<BoneData>();
			List<byte> mappingForType = state.GetMappingForType(OwlPackTypeInfo, typeInfo);
			formatter.EnterObject();
			for (int i = 0; i < typeInfo.Fields.Length; i++)
			{
				formatter.ReadFieldHeader(typeInfo, out var fieldID, out var size);
				switch (mappingForType[fieldID])
				{
				case byte.MaxValue:
					formatter.SkipField(size);
					break;
				case 0:
					Name = formatter.ReadString(state);
					break;
				case 1:
					Positions = formatter.ReadPackable<Vector3>(state);
					break;
				case 2:
					Rotations = formatter.ReadPackable<Quaternion>(state);
					break;
				}
			}
			formatter.LeaveObject();
		}
	}

	private static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("RigidbodyCreatureController");

	[HideInInspector]
	public bool RagdollOnlyOnDeath = true;

	public GameObject RootBone;

	public List<BoneImpulseMultiplier> BoneImpulseMultipliers = new List<BoneImpulseMultiplier>();

	public List<BoneImpulseMultiplier> WeaponBoneImpulseMultipliers = new List<BoneImpulseMultiplier>();

	public bool RandomNegativeValueOnMultiplier;

	public float BaseImpulseValue = 10f;

	public float AdditionalImpulseMin;

	public float AdditionalImpulseMax;

	public float MultiplyVectorYAxis = 1f;

	public float InProneMultiplier = 1f;

	public float ImpulseValueMultiplierToParents;

	public float ImpulseValueMultiplierToChildren;

	public bool ApplyImpulseToAllBones;

	[Tooltip("Character objects with rigidbody component")]
	public List<Rigidbody> RigidBones = new List<Rigidbody>();

	[Tooltip("Time after ragdoll simulation will stop")]
	public float RagdollTime = 3f;

	public bool CheckForEarlyStopRagdoll;

	public float MinRootPosition;

	public float MinAllPosition;

	public float MinTimeToStop = 9f;

	[HideInInspector]
	[Tooltip("Main bones which should be returned to source positions on creature stand up")]
	public List<Transform> BonesToReturn = new List<Transform>();

	public AbstractUnitEntityView EntityView;

	private TimeSpan m_StartTime;

	private Vector3 m_PreviousRootPosition;

	private Vector3 m_CurRootPosition;

	private TimeSpan m_StartTimeToStop;

	private TimeSpan m_LastSlowCheckTime;

	private bool m_LastSlowCheckResult;

	private readonly List<BoneData> m_BonesData = new List<BoneData>();

	public List<BoneData> RagdollCurrentPositions = new List<BoneData>();

	private ImpulseData m_LastImpulse;

	public bool PostEventWithSurface = true;

	public List<RagdollPostEventWithSurface> EventTargets = new List<RagdollPostEventWithSurface>();

	private RagdollState m_State;

	private Vector3 m_DeathPoint;

	private bool m_LootDropped;

	private Collider[] m_CachedColliders;

	private Dictionary<string, int> m_BoneNameToIndex;

	public float minRagdollValue = 1f;

	public float maxRagdollValue = 80f;

	private GameObject m_ActiveRagdoll;

	private List<GameObject> m_ActiveRagdollChildrens = new List<GameObject>();

	public List<GameObject> skeletonBones;

	private const float RagdollProjectionAngle = 10f;

	public RagdollState State
	{
		get
		{
			return m_State;
		}
		private set
		{
			if (m_State != value)
			{
				m_State = value;
				base.enabled = value == RagdollState.Falling;
				switch (value)
				{
				case RagdollState.Falling:
				case RagdollState.Lying:
					DisableAnimationComponents();
					break;
				case RagdollState.Off:
					EnableAnimationComponents();
					break;
				}
				if (value == RagdollState.Falling || value == RagdollState.Lying)
				{
					PrepareRenderersForRagdoll();
				}
			}
		}
	}

	public bool IsControllingRigidbody
	{
		get
		{
			if (State != RagdollState.Falling)
			{
				return State == RagdollState.Standing;
			}
			return true;
		}
	}

	public bool RagdollWorking => State == RagdollState.Falling;

	public bool IsActive => State != RagdollState.Off;

	public bool IsRagdollPositionsRestored { get; private set; }

	public void SaveBonesPosition(List<BoneData> targetData)
	{
		targetData.Clear();
		foreach (Rigidbody rigidBone in RigidBones)
		{
			if ((bool)base.transform)
			{
				BoneData item = new BoneData
				{
					Name = rigidBone.name,
					Positions = rigidBone.transform.position,
					Rotations = rigidBone.transform.rotation
				};
				targetData.Add(item);
			}
		}
	}

	private void SwitchKinematic(bool enable)
	{
		foreach (Rigidbody rigidBone in RigidBones)
		{
			if (!rigidBone.isKinematic)
			{
				rigidBone.linearVelocity = Vector3.zero;
			}
			rigidBone.ResetCenterOfMass();
			rigidBone.detectCollisions = !enable;
			rigidBone.isKinematic = enable;
			rigidBone.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
		}
	}

	private void ApplyJointProjection()
	{
		foreach (Rigidbody rigidBone in RigidBones)
		{
			CharacterJoint component = rigidBone.GetComponent<CharacterJoint>();
			if (component != null)
			{
				component.enableProjection = true;
				component.projectionAngle = 10f;
			}
		}
	}

	public void InitRigidbodyCreatureController()
	{
		EntityView = GetComponentInParent<AbstractUnitEntityView>();
		if (EntityView == null)
		{
			Logger.Error(this, "No EntityView found {0}", this);
			return;
		}
		if (EntityView.RigidbodyController == null)
		{
			EntityView.RigidbodyController = this;
		}
		base.enabled = false;
		State = RagdollState.Off;
		if (!RootBone)
		{
			Logger.Error(this, "No root bone in RagdollCharacter {0}", this);
			return;
		}
		if (RigidBones.Count <= 0)
		{
			Logger.Error(this, "No rigid bones in RagdollCharacter {0}", this);
			return;
		}
		EntityView.GetComponent<SnapMapBase>().Init();
		RigidBones.RemoveAll((Rigidbody b) => b == null);
		foreach (Rigidbody rigidBone in RigidBones)
		{
			if (rigidBone.gameObject.layer != 9 && rigidBone.gameObject.layer != 29)
			{
				Logger.Error(this, "Rigidbone {0} without Unit layer. Headwalking will occur. {1}", rigidBone.name, this);
				rigidBone.gameObject.layer = 9;
			}
			rigidBone.ResetInertiaTensor();
			rigidBone.solverIterations = 6;
			rigidBone.maxDepenetrationVelocity = 1f;
			rigidBone.solverVelocityIterations = 1;
			rigidBone.maxAngularVelocity = 7f;
			rigidBone.detectCollisions = false;
			rigidBone.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
			rigidBone.isKinematic = true;
		}
		m_CachedColliders = new Collider[RigidBones.Count];
		m_BoneNameToIndex = new Dictionary<string, int>(RigidBones.Count);
		for (int i = 0; i < RigidBones.Count; i++)
		{
			m_CachedColliders[i] = RigidBones[i].GetComponent<Collider>();
			m_BoneNameToIndex[RigidBones[i].name] = i;
		}
		SwitchRigidbodiesSleep(enable: false);
	}

	public void CancelRagdoll()
	{
		StopRagdoll();
		State = RagdollState.Off;
	}

	public void ReturnToAnimationState()
	{
		StopRagdoll();
		State = RagdollState.Standing;
	}

	private void SwitchRigidbodies(bool enable)
	{
		foreach (Rigidbody rigidBone in RigidBones)
		{
			rigidBone.gameObject.SetActive(enable);
		}
	}

	private void SwitchRigidbodiesSleep(bool enable)
	{
		for (int i = 0; i < RigidBones.Count; i++)
		{
			Rigidbody rigidbody = RigidBones[i];
			if (!enable)
			{
				rigidbody.Sleep();
			}
			else
			{
				rigidbody.WakeUp();
			}
			Collider collider = ((m_CachedColliders != null && i < m_CachedColliders.Length) ? m_CachedColliders[i] : rigidbody.GetComponent<Collider>());
			if ((bool)collider)
			{
				collider.enabled = enable;
			}
		}
	}

	public void StopRagdoll()
	{
		if (State == RagdollState.Falling)
		{
			SwitchKinematic(enable: true);
			SwitchRigidbodies(enable: false);
			SaveBonesPosition(RagdollCurrentPositions);
			base.gameObject.GetComponent<HumanoidRagdollManager>()?.Enabled(flag: false);
		}
		UnitAnimationManager component = GetComponent<UnitAnimationManager>();
		if (component != null && component.IsProne)
		{
			UnitAnimationActionHandle unitAnimationActionHandle = GetComponent<UnitAnimationManager>()?.ExclusiveHandle;
			unitAnimationActionHandle?.Action.OnUpdate(unitAnimationActionHandle, 0.1f);
		}
		State = RagdollState.Lying;
	}

	public void StartRagdoll()
	{
		if ((bool)RootBone && RigidBones.Count > 0 && State != RagdollState.Falling)
		{
			InitBakedCharactersBonesPosition();
			SaveBonesPosition(m_BonesData);
			State = RagdollState.Falling;
			m_DeathPoint = base.transform.position;
			ApplySavedBonePositions();
			SwitchRigidbodies(enable: true);
			SwitchKinematic(enable: false);
			ApplyJointProjection();
			SwitchRigidbodiesSleep(enable: true);
			m_StartTime = Game.Instance.Controllers.TimeController.GameTime;
			base.gameObject.GetComponent<HumanoidRagdollManager>()?.Enabled(flag: true);
			if (m_LastImpulse != null && Game.Instance.Controllers.TimeController.GameTime - m_LastImpulse.Time < 0.2f.Seconds())
			{
				ApplyImpulseDirectly(m_LastImpulse.Direction, m_LastImpulse.MagnitudeModifier);
			}
		}
	}

	private void ApplySavedBonePositions()
	{
		if (m_BonesData.Count == 0 || m_BoneNameToIndex == null)
		{
			return;
		}
		foreach (BoneData bonesDatum in m_BonesData)
		{
			if (m_BoneNameToIndex.TryGetValue(bonesDatum.Name, out var value))
			{
				Rigidbody rigidbody = RigidBones[value];
				rigidbody.transform.position = bonesDatum.Positions;
				rigidbody.transform.rotation = bonesDatum.Rotations;
			}
		}
	}

	public void ApplyImpulse(Vector3 direction, float additionalMagnitude)
	{
		if (State == RagdollState.Falling)
		{
			ApplyImpulseDirectly(direction, additionalMagnitude);
			return;
		}
		m_LastImpulse = m_LastImpulse ?? new ImpulseData();
		m_LastImpulse.Direction = direction;
		m_LastImpulse.MagnitudeModifier = additionalMagnitude;
		m_LastImpulse.Time = Game.Instance.Controllers.TimeController.GameTime;
	}

	private void ApplyImpulseDirectly(Vector3 direction, float additionalMagnitude)
	{
		if (BoneImpulseMultipliers == null || BoneImpulseMultipliers.Count == 0)
		{
			return;
		}
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		int num4 = 1;
		if (RandomNegativeValueOnMultiplier && PFStatefulRandom.Visuals.Rigidbody.value > 0.5f)
		{
			num4 = -1;
		}
		if (ApplyImpulseToAllBones)
		{
			foreach (BoneImpulseMultiplier boneImpulseMultiplier in BoneImpulseMultipliers)
			{
				Rigidbody bone = boneImpulseMultiplier.bone;
				num = ((0f != boneImpulseMultiplier.multiplier) ? boneImpulseMultiplier.multiplier : 1f);
				AbstractUnitEntity entityData = EntityView.EntityData;
				num3 = ((entityData != null && entityData.IsProne) ? InProneMultiplier : 1f);
				num2 = (BaseImpulseValue + PFStatefulRandom.Visuals.Rigidbody.Range(AdditionalImpulseMin, AdditionalImpulseMax)) * (float)num4 * (1f + additionalMagnitude) * num * num3;
				num2 = Mathf.Clamp(num2, minRagdollValue, maxRagdollValue);
				Vector3 impulseDirection = num2 * direction;
				float impulseValueMultiplierToParents = ImpulseValueMultiplierToParents;
				float impulseValueMultiplierToChildren = ImpulseValueMultiplierToChildren;
				float multiplyVectorYAxis = MultiplyVectorYAxis;
				ApplyImpulseToRagdoll(bone, impulseDirection, default(Vector3), zeroVerticalVector: false, default(Vector3), impulseValueMultiplierToParents, impulseValueMultiplierToChildren, multiplyVectorYAxis);
			}
		}
		else
		{
			int num5 = PFStatefulRandom.Visuals.Rigidbody.Range(0, BoneImpulseMultipliers.Count);
			Rigidbody bone2 = BoneImpulseMultipliers[num5].bone;
			num = ((num5 < BoneImpulseMultipliers.Count) ? BoneImpulseMultipliers[num5].multiplier : 1f);
			AbstractUnitEntity entityData2 = EntityView.EntityData;
			num3 = ((entityData2 != null && entityData2.IsProne) ? InProneMultiplier : 1f);
			num2 = (BaseImpulseValue + PFStatefulRandom.Visuals.Rigidbody.Range(AdditionalImpulseMin, AdditionalImpulseMax)) * (float)num4 * (1f + additionalMagnitude) * num * num3;
			num2 = Mathf.Clamp(num2, minRagdollValue, maxRagdollValue);
			Vector3 impulseDirection2 = num2 * direction;
			float multiplyVectorYAxis = ImpulseValueMultiplierToParents;
			float impulseValueMultiplierToChildren = ImpulseValueMultiplierToChildren;
			float impulseValueMultiplierToParents = MultiplyVectorYAxis;
			ApplyImpulseToRagdoll(bone2, impulseDirection2, default(Vector3), zeroVerticalVector: false, default(Vector3), multiplyVectorYAxis, impulseValueMultiplierToChildren, impulseValueMultiplierToParents);
		}
		ApplyWeaponImpulse(direction, additionalMagnitude, num4);
	}

	private void ApplyWeaponImpulse(Vector3 direction, float additionalMagnitude, int negativeCoefficient)
	{
		if (WeaponBoneImpulseMultipliers == null || WeaponBoneImpulseMultipliers.Count == 0)
		{
			return;
		}
		foreach (BoneImpulseMultiplier weaponBoneImpulseMultiplier in WeaponBoneImpulseMultipliers)
		{
			if (!(weaponBoneImpulseMultiplier.bone == null))
			{
				float num = ((weaponBoneImpulseMultiplier.multiplier != 0f) ? weaponBoneImpulseMultiplier.multiplier : 1f);
				float value = BaseImpulseValue * (float)negativeCoefficient * (1f + additionalMagnitude) * num;
				value = Mathf.Clamp(value, minRagdollValue, maxRagdollValue);
				Vector3 vector = direction + Vector3.down * 0.5f;
				weaponBoneImpulseMultiplier.bone.AddForce(vector.normalized * value, ForceMode.Impulse);
				Vector3 torque = new Vector3(PFStatefulRandom.Visuals.Rigidbody.Range(-1f, 1f), PFStatefulRandom.Visuals.Rigidbody.Range(-0.5f, 0.5f), PFStatefulRandom.Visuals.Rigidbody.Range(-1f, 1f)) * value * 0.3f;
				weaponBoneImpulseMultiplier.bone.AddTorque(torque, ForceMode.Impulse);
			}
		}
	}

	private void DisableAnimationComponents()
	{
		AnimancerComponent component = GetComponent<AnimancerComponent>();
		if (component != null)
		{
			component.enabled = false;
		}
		Animator component2 = GetComponent<Animator>();
		if (component2 != null)
		{
			component2.enabled = false;
		}
		UnitAnimationManager component3 = GetComponent<UnitAnimationManager>();
		if (component3 != null)
		{
			component3.Disabled = true;
		}
	}

	private void EnableAnimationComponents()
	{
		UnitAnimationManager component = GetComponent<UnitAnimationManager>();
		if (component != null)
		{
			component.Disabled = false;
		}
		Animator component2 = GetComponent<Animator>();
		if (component2 != null)
		{
			component2.enabled = true;
		}
		AnimancerComponent component3 = GetComponent<AnimancerComponent>();
		if (component3 != null)
		{
			component3.enabled = true;
		}
	}

	public static void ApplyImpulseToRagdoll([NotNull] Rigidbody impulseTarget, Vector3 impulseDirection, Vector3 additionalDirection = default(Vector3), bool zeroVerticalVector = false, Vector3 torque = default(Vector3), float impulseValueMultiplierToParents = 0f, float impulseValueMultiplierToChildren = 0f, float multiplyVectorYAxis = 1f)
	{
		if (zeroVerticalVector)
		{
			impulseDirection = new Vector3(impulseDirection.x, 0f, impulseDirection.z);
		}
		impulseDirection = new Vector3(impulseDirection.x, impulseDirection.y * multiplyVectorYAxis, impulseDirection.z);
		impulseDirection += additionalDirection * impulseDirection.magnitude;
		impulseTarget.AddForce(impulseDirection, ForceMode.Impulse);
		if (torque != Vector3.zero)
		{
			impulseTarget.AddTorque(torque.x, torque.y, torque.z);
		}
		Rigidbody rigidbody = impulseTarget;
		float num = impulseValueMultiplierToParents;
		if (impulseValueMultiplierToParents > Mathf.Epsilon)
		{
			while ((bool)rigidbody)
			{
				rigidbody = rigidbody.transform.parent.GetComponent<Rigidbody>();
				if (rigidbody != null)
				{
					rigidbody.AddForce(impulseDirection * num, ForceMode.Impulse);
					num *= impulseValueMultiplierToParents;
				}
			}
		}
		if (impulseValueMultiplierToChildren > Mathf.Epsilon)
		{
			rigidbody = impulseTarget;
			Vector3 childImpulseVec = impulseDirection * impulseValueMultiplierToChildren;
			ImpulseChildren(rigidbody, childImpulseVec, impulseValueMultiplierToChildren);
		}
	}

	private static void ImpulseChildren(Rigidbody curBone, Vector3 childImpulseVec, float childImpulseValueMultiplier)
	{
		int childCount = curBone.transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			Rigidbody component = curBone.transform.GetChild(i).GetComponent<Rigidbody>();
			if (!(component == null))
			{
				component.AddForce(childImpulseVec, ForceMode.Impulse);
				ImpulseChildren(component, childImpulseVec * childImpulseValueMultiplier, childImpulseValueMultiplier);
			}
		}
	}

	public void RestoreRagdollPositions()
	{
		if (RagdollCurrentPositions.Count <= 0)
		{
			return;
		}
		foreach (BoneData ragdollCurrentPosition in RagdollCurrentPositions)
		{
			if (m_BoneNameToIndex != null && m_BoneNameToIndex.TryGetValue(ragdollCurrentPosition.Name, out var value))
			{
				Rigidbody rigidbody = RigidBones[value];
				Transform obj = rigidbody.transform;
				Quaternion rotation = (rigidbody.rotation = ragdollCurrentPosition.Rotations);
				obj.rotation = rotation;
				Transform obj2 = rigidbody.transform;
				Vector3 position = (rigidbody.position = ragdollCurrentPosition.Positions);
				obj2.position = position;
			}
		}
		GetComponent<HumanoidRagdollManager>().Or(null)?.CopyPoseFromRagdoll();
		State = RagdollState.Lying;
		SwitchRigidbodies(enable: false);
		SwitchKinematic(enable: true);
		IsRagdollPositionsRestored = true;
	}

	private bool ReturnBonesToAnimatePosition()
	{
		if (BonesToReturn.Count <= 0)
		{
			return true;
		}
		SwitchKinematic(enable: true);
		bool flag = false;
		bool flag2 = false;
		foreach (Transform item in BonesToReturn)
		{
			BoneData bone = GetBone(item.transform);
			if (bone == null)
			{
				Logger.Error(this, "Bone data is null. Ragdoll stopped. {0}", this);
				return false;
			}
			if (item.transform.rotation != bone.Rotations)
			{
				Transform obj = item.transform;
				Quaternion rotation = (item.rotation = Quaternion.RotateTowards(item.transform.rotation, bone.Rotations, 80f * Game.Instance.Controllers.TimeController.GameDeltaTime));
				obj.rotation = rotation;
				flag = false;
			}
			else
			{
				flag = true;
			}
		}
		foreach (Transform item2 in BonesToReturn)
		{
			BoneData bone2 = GetBone(item2.transform);
			if (bone2 == null)
			{
				Logger.Error(this, "Bone data is null. Ragdoll stopped. {0}", this);
				return false;
			}
			if (item2.transform.position != bone2.Positions)
			{
				Transform obj2 = item2.transform;
				Vector3 position = (item2.position = Vector3.MoveTowards(item2.transform.position, bone2.Positions, 2f * Game.Instance.Controllers.TimeController.GameDeltaTime));
				obj2.position = position;
				flag2 = false;
			}
			else
			{
				flag2 = true;
			}
		}
		return flag && flag2;
	}

	private BoneData GetBone(Transform targetBone)
	{
		BoneData result = null;
		foreach (BoneData bonesDatum in m_BonesData)
		{
			if (targetBone.name == bonesDatum.Name)
			{
				result = bonesDatum;
			}
		}
		return result;
	}

	private void OnEnable()
	{
		Game.Instance.Controllers.UpdateRigidbodyCreatureController.Add(this);
	}

	private void OnDisable()
	{
		Game.Instance.Controllers.UpdateRigidbodyCreatureController.Remove(this);
	}

	void Kingmaker.Controllers.IUpdatable.Tick(float delta)
	{
		if (!EntityView)
		{
			Logger.Error(this, "{0} EntityView link is empty. Abort!", this);
			return;
		}
		if (Game.Instance.Controllers.TimeController.GameTime > m_StartTime + RagdollTime.Seconds())
		{
			MaybeSpawnLootAtDeathPoint();
			StopRagdoll();
		}
		if (!CheckForEarlyStopRagdoll)
		{
			return;
		}
		TimeSpan gameTime = Game.Instance.Controllers.TimeController.GameTime;
		if (gameTime > m_LastSlowCheckTime + 0.15f.Seconds())
		{
			m_LastSlowCheckResult = CheckRagdollIsSlow();
			m_PreviousRootPosition = RootBone.transform.position;
			m_LastSlowCheckTime = gameTime;
		}
		if (m_LastSlowCheckResult)
		{
			if (gameTime > m_StartTimeToStop + MinTimeToStop.Seconds())
			{
				MaybeSpawnLootAtDeathPoint();
				StopRagdoll();
			}
		}
		else
		{
			m_StartTimeToStop = gameTime;
		}
	}

	private void MaybeSpawnLootAtDeathPoint()
	{
		if (!m_LootDropped && !(EntityView == null) && EntityView.EntityData != null && EntityView.EntityData.IsDeadAndHasLoot && !(m_DeathPoint == Vector3.zero))
		{
			float sqrMagnitude = (RootBone.transform.position - m_DeathPoint).sqrMagnitude;
			float ragdollDistanceForLootBag = ConfigRoot.Instance.HitSystemRoot.RagdollDistanceForLootBag;
			if (sqrMagnitude > ragdollDistanceForLootBag * ragdollDistanceForLootBag)
			{
				EntityView.EntityData.GetOptional<PartInventory>()?.DropLootToGround(dismember: false, m_DeathPoint, dropAttached: true);
				m_LootDropped = true;
			}
		}
	}

	private bool CheckRagdollIsSlow()
	{
		Vector3 vector = RootBone.transform.position - m_PreviousRootPosition;
		bool flag = Mathf.Max(Mathf.Max(Mathf.Abs(vector.x), Mathf.Abs(vector.y)), Mathf.Abs(vector.z)) <= MinRootPosition * (Game.Instance.Controllers.TimeController.GameDeltaTime * 30f);
		if (flag)
		{
			float num = MinAllPosition * 30f * (MinAllPosition * 30f);
			foreach (Rigidbody rigidBone in RigidBones)
			{
				if (rigidBone.linearVelocity.sqrMagnitude > num)
				{
					flag = false;
					break;
				}
			}
		}
		return flag;
	}

	public void InitBakedCharactersBonesPosition()
	{
		if (skeletonBones.Count == 0)
		{
			skeletonBones = GetAllChildrenInGO(base.gameObject);
		}
		if (m_ActiveRagdoll != null)
		{
			Dictionary<string, GameObject> dictionary = new Dictionary<string, GameObject>(skeletonBones.Count);
			foreach (GameObject skeletonBone in skeletonBones)
			{
				dictionary[skeletonBone.name] = skeletonBone;
			}
			{
				foreach (GameObject activeRagdollChildren in m_ActiveRagdollChildrens)
				{
					activeRagdollChildren.SetActive(value: true);
					if (dictionary.TryGetValue(activeRagdollChildren.name, out var value))
					{
						activeRagdollChildren.transform.position = value.transform.position;
						activeRagdollChildren.transform.rotation = value.transform.rotation;
					}
				}
				return;
			}
		}
		if (RigidBones.Count <= 0)
		{
			return;
		}
		foreach (Rigidbody rigidBone in RigidBones)
		{
			m_ActiveRagdollChildrens.Add(rigidBone.gameObject);
		}
		SkinnedMeshRenderer componentInChildren = EntityView.GetComponentInChildren<SkinnedMeshRenderer>();
		if (!(componentInChildren != null) || componentInChildren.bones.Length == 0)
		{
			return;
		}
		Dictionary<string, Transform> dictionary2 = new Dictionary<string, Transform>(componentInChildren.bones.Length);
		Transform[] bones = componentInChildren.bones;
		foreach (Transform transform in bones)
		{
			dictionary2[transform.name] = transform;
		}
		foreach (GameObject activeRagdollChildren2 in m_ActiveRagdollChildrens)
		{
			activeRagdollChildren2.SetActive(value: true);
			if (dictionary2.TryGetValue(activeRagdollChildren2.name, out var value2) && value2.gameObject != activeRagdollChildren2)
			{
				activeRagdollChildren2.transform.position = value2.position;
				activeRagdollChildren2.transform.rotation = value2.rotation;
			}
		}
	}

	public void SetActiveRagdollGO(GameObject ragdoll_right_handed_go, List<GameObject> skeletonBones1)
	{
		m_ActiveRagdoll = ragdoll_right_handed_go;
		foreach (Rigidbody rigidBone in m_ActiveRagdoll.GetComponent<RigidbodyCreatureController>().RigidBones)
		{
			m_ActiveRagdollChildrens.Add(rigidBone.gameObject);
		}
		skeletonBones = skeletonBones1;
	}

	public void FindSkeleton()
	{
		skeletonBones = GetAllChildrenInGO(base.gameObject);
	}

	private List<GameObject> GetAllChildrenInGO(GameObject gameObject)
	{
		Transform transform = gameObject.transform;
		Transform[] componentsInChildren = gameObject.GetComponentsInChildren<Transform>(includeInactive: true);
		List<GameObject> list = new List<GameObject>();
		Transform[] array = componentsInChildren;
		foreach (Transform transform2 in array)
		{
			if (transform != transform2)
			{
				list.Add(transform2.gameObject);
			}
		}
		return list;
	}

	private void PrepareRenderersForRagdoll()
	{
		if (!Application.isPlaying || RootBone == null || EntityView == null)
		{
			return;
		}
		SkinnedMeshRenderer componentInChildren = EntityView.GetComponentInChildren<SkinnedMeshRenderer>();
		if (!(componentInChildren == null))
		{
			Mesh sharedMesh = componentInChildren.sharedMesh;
			if (!(sharedMesh == null))
			{
				componentInChildren.rootBone = RootBone.transform;
				Vector3 extents = sharedMesh.bounds.extents;
				float num = Mathf.Max(Mathf.Max(extents.x, extents.y), extents.z);
				float num2 = 2f * num;
				componentInChildren.localBounds = new Bounds(default(Vector3), new Vector3(num2, num2, num2));
			}
		}
	}
}
