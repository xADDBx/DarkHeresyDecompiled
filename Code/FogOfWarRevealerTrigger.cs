using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameCommands;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Logging;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
[KnowledgeDatabaseID("b88723ff06f484141a79ce5ca2b23424")]
public class FogOfWarRevealerTrigger : EntityViewBase, IUpdatable
{
	[HashNoGenerate]
	[OwlPackable(OwlPackableMode.Generate)]
	public class EntityData : Entity, IOwlPackable<EntityData>
	{
		[JsonProperty]
		[OwlPackInclude]
		private bool m_RevealComplete;

		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "EntityData",
			OldNames = null,
			Fields = new FieldInfo[11]
			{
				new FieldInfo("UniqueId", typeof(string)),
				new FieldInfo("m_IsInGame", typeof(bool)),
				new FieldInfo("m_Position", typeof(Vector3)),
				new FieldInfo("m_Orientation", typeof(float)),
				new FieldInfo("m_InitialPosition", typeof(Vector3?)),
				new FieldInfo("m_InitialOrientation", typeof(float?)),
				new FieldInfo("Facts", typeof(EntityFactsManager)),
				new FieldInfo("Parts", typeof(EntityPartsManager)),
				new FieldInfo("m_IsRevealed", typeof(bool)),
				new FieldInfo("m_ViewHandlingOnDisposePolicyOverride", typeof(ViewHandlingOnDisposePolicyType?)),
				new FieldInfo("m_RevealComplete", typeof(bool))
			}
		};

		public new FogOfWarRevealerTrigger View => (FogOfWarRevealerTrigger)base.View;

		public EntityData(EntityViewBase view)
			: base(view.UniqueId, view.IsInGameBySettings)
		{
		}

		public EntityData(OwlPackConstructorParameter _)
			: base(_)
		{
		}

		protected override IEntityView CreateViewForData()
		{
			return null;
		}

		protected override void OnPreSave()
		{
			m_RevealComplete = View.m_RevealComplete;
		}

		protected override void OnViewDidAttach()
		{
			base.OnViewDidAttach();
			View.m_RevealComplete = m_RevealComplete;
			if (m_RevealComplete)
			{
				View.gameObject.SetActive(value: false);
			}
		}

		public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
		{
			EntityData source = new EntityData(default(OwlPackConstructorParameter));
			result = Unsafe.As<EntityData, TPossiblyBase>(ref source);
		}

		public override void Serialize<TFormatter>(TFormatter formatter, SerializerState state)
		{
			(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
			var (objectId, _) = orRegister;
			if (orRegister.isRef)
			{
				formatter.ObjectRef(objectId);
				return;
			}
			ushort type = state.TypeLibrary.RegisterType<EntityData>(OwlPackTypeInfo);
			formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
			string value = base.UniqueId;
			formatter.StringField(0, "UniqueId", ref value, state);
			formatter.UnmanagedField(1, "m_IsInGame", ref m_IsInGame, state);
			formatter.Field(2, "m_Position", ref m_Position, state);
			formatter.UnmanagedField(3, "m_Orientation", ref m_Orientation, state);
			formatter.NullableField(4, "m_InitialPosition", ref m_InitialPosition, state);
			formatter.UnmanagedNullableField(5, "m_InitialOrientation", ref m_InitialOrientation, state);
			formatter.Field(6, "Facts", ref Facts, state);
			formatter.Field(7, "Parts", ref Parts, state);
			formatter.UnmanagedField(8, "m_IsRevealed", ref m_IsRevealed, state);
			formatter.EnumNullableField(9, "m_ViewHandlingOnDisposePolicyOverride", ref m_ViewHandlingOnDisposePolicyOverride, state);
			formatter.UnmanagedField(10, "m_RevealComplete", ref m_RevealComplete, state);
			formatter.EndObject();
		}

		public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
		{
			state.References.Register(objectId, this);
			TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<EntityData>();
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
					base.UniqueId = formatter.ReadString(state);
					break;
				case 1:
					m_IsInGame = formatter.ReadUnmanaged<bool>(state);
					break;
				case 2:
					m_Position = formatter.ReadPackable<Vector3>(state);
					break;
				case 3:
					m_Orientation = formatter.ReadUnmanaged<float>(state);
					break;
				case 4:
					m_InitialPosition = formatter.ReadNullablePackable<Vector3>(state);
					break;
				case 5:
					m_InitialOrientation = formatter.ReadNullableUnmanaged<float>(state);
					break;
				case 6:
					Facts = formatter.ReadPackable<EntityFactsManager>(state);
					break;
				case 7:
					Parts = formatter.ReadPackable<EntityPartsManager>(state);
					break;
				case 8:
					m_IsRevealed = formatter.ReadUnmanaged<bool>(state);
					break;
				case 9:
					m_ViewHandlingOnDisposePolicyOverride = formatter.ReadNullableEnum<ViewHandlingOnDisposePolicyType>(state);
					break;
				case 10:
					m_RevealComplete = formatter.ReadUnmanaged<bool>(state);
					break;
				}
			}
			formatter.LeaveObject();
		}
	}

	private enum AnimationMode
	{
		FromCode,
		UsingAnimator
	}

	[SerializeField]
	private AnimationMode Mode;

	[SerializeField]
	private List<Animator> Animators;

	[SerializeField]
	private string TriggerName = "fow";

	[SerializeField]
	private int Layer;

	[SerializeField]
	private List<FogOfWarRevealerSettings> revealers = new List<FogOfWarRevealerSettings>();

	[SerializeField]
	private float ProceduralAnimationScaleSpeed = 0.05f;

	[SerializeField]
	private float ProceduralAnimationScaleLimit = 50f;

	[SerializeField]
	private float ProceduralAnimationScaleStep = 20f;

	[SerializeField]
	private bool KeepRevealerAliveAfterAnimation;

	private Rigidbody m_TriggerRigidBody;

	private List<Collider> m_TriggerColliders = new List<Collider>();

	private bool m_ShowLinkedRevealers;

	private bool m_RevealStarted;

	private bool m_RevealComplete;

	private bool m_SubscribedForUpdates;

	public static readonly Dictionary<string, FogOfWarRevealerTrigger> AllTriggers = new Dictionary<string, FogOfWarRevealerTrigger>();

	private Vector3 LocalScaleControl = Vector3.zero;

	private void Reset()
	{
		Layer = LayerMask.NameToLayer("Selectable");
	}

	protected override void OnEnable()
	{
		AllTriggers[UniqueId] = this;
		base.OnEnable();
	}

	protected override void OnDisable()
	{
		AllTriggers.Remove(UniqueId);
		base.OnDisable();
	}

	private void Start()
	{
		m_TriggerRigidBody = GetComponentInChildren<Rigidbody>();
		m_TriggerColliders = GetComponentsInChildren<Collider>().ToList();
		if (!m_TriggerRigidBody)
		{
			base.enabled = false;
			UberDebug.LogError("Fow Revealer Animation Trigger Error : No Rigidbody on trigger : " + base.name);
		}
		base.gameObject.layer = LayerMask.NameToLayer("FXRaycast");
		if (m_TriggerColliders.Count < 1)
		{
			base.enabled = false;
			UberDebug.LogError("Fow Revealer Animation Trigger Error : No trigger colliders on trigger : " + base.name);
		}
		m_TriggerRigidBody.isKinematic = true;
		foreach (Collider triggerCollider in m_TriggerColliders)
		{
			triggerCollider.includeLayers = 1 << Layer;
			triggerCollider.isTrigger = true;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		UnsubscribeFromUpdates();
	}

	private void OnTriggerEnter(Collider trespasser)
	{
		if (trespasser.gameObject.layer == Layer && !(trespasser.transform.parent == null) && !(trespasser.transform.parent.GetComponent<UnitEntityView>() == null) && trespasser.transform.parent.GetComponent<UnitEntityView>().EntityData != null && trespasser.transform.parent.GetComponent<UnitEntityView>().EntityData.GetCompanionOptional() != null && trespasser.transform.parent.GetComponent<UnitEntityView>().EntityData.GetCompanionOptional().State == CompanionState.InParty)
		{
			Game.Instance.GameCommandQueue.AddCommand(new FogOfWarRevealerTriggerGameCommand(UniqueId));
		}
	}

	public void Reveal()
	{
		if (m_RevealStarted || m_RevealComplete)
		{
			return;
		}
		if (revealers.Count <= 0)
		{
			UberDebug.LogError("No revealers in " + base.name);
			return;
		}
		switch (Mode)
		{
		case AnimationMode.UsingAnimator:
			if (Animators.Count <= 0)
			{
				UberDebug.LogError("No animators for trigger in " + base.name);
				break;
			}
			if (TriggerName == "")
			{
				UberDebug.LogError("Empty animation trigger name in " + base.name);
				break;
			}
			foreach (Animator animator in Animators)
			{
				animator.SetTrigger(TriggerName);
			}
			base.enabled = false;
			break;
		case AnimationMode.FromCode:
			foreach (FogOfWarRevealerSettings revealer in revealers)
			{
				revealer.transform.localScale = Vector3.zero;
			}
			SubscribeForUpdates();
			m_RevealStarted = true;
			break;
		}
	}

	private void AnimateRevealer(float deltaTime)
	{
		if (LocalScaleControl.x >= ProceduralAnimationScaleLimit)
		{
			if (!KeepRevealerAliveAfterAnimation)
			{
				foreach (FogOfWarRevealerSettings revealer in revealers)
				{
					FogOfWarControllerData.RemoveRevealer(revealer.transform);
				}
			}
			if ((bool)m_TriggerRigidBody)
			{
				m_TriggerRigidBody.Sleep();
			}
			foreach (Collider triggerCollider in m_TriggerColliders)
			{
				if ((bool)triggerCollider)
				{
					triggerCollider.enabled = false;
				}
			}
			OnComplete();
			return;
		}
		foreach (FogOfWarRevealerSettings revealer2 in revealers)
		{
			if (!revealer2.RevealManual)
			{
				FogOfWarControllerData.AddRevealer(revealer2.transform);
				revealer2.RevealManual = true;
			}
			if (!revealer2.enabled)
			{
				revealer2.enabled = true;
			}
			Vector3 vector = ProceduralAnimationScaleStep * deltaTime * Vector3.one;
			Vector3 vector2 = revealer2.transform.localScale + vector;
			revealer2.transform.localScale = vector2;
			LocalScaleControl = vector2;
		}
	}

	void IUpdatable.Tick(float delta)
	{
		try
		{
			float gameDeltaTime = Game.Instance.Controllers.TimeController.GameDeltaTime;
			gameDeltaTime = 1f / 60f * gameDeltaTime / Mathf.Max(1f / 60f, ProceduralAnimationScaleSpeed);
			AnimateRevealer(gameDeltaTime);
		}
		catch (Exception ex)
		{
			PFLog.Default.Exception(ex);
			OnComplete();
			base.gameObject.SetActive(value: false);
		}
	}

	private void OnComplete()
	{
		base.enabled = false;
		m_RevealComplete = true;
		UnsubscribeFromUpdates();
	}

	public override Entity CreateEntityData(bool load)
	{
		return Entity.Initialize(new EntityData(this));
	}

	private void SubscribeForUpdates()
	{
		if (!m_SubscribedForUpdates)
		{
			Game.Instance.Controllers.FogOfWarRevealerTriggerController.Add(this);
			m_SubscribedForUpdates = true;
		}
	}

	private void UnsubscribeFromUpdates()
	{
		if (m_SubscribedForUpdates)
		{
			Game.Instance.Controllers.FogOfWarRevealerTriggerController.Remove(this);
			m_SubscribedForUpdates = false;
		}
	}

	protected override void OnDrawGizmos()
	{
		if (!m_ShowLinkedRevealers)
		{
			return;
		}
		foreach (FogOfWarRevealerSettings revealer in revealers)
		{
			Debug.DrawLine(revealer.transform.position, base.transform.position, Color.cyan);
		}
	}
}
