using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.QA;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

[AllowedOn(typeof(BlueprintComponentList))]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintEtude))]
[TypeId("4d95171e41fd4447a47bf00f82bb5070")]
public abstract class EtudeBracketTrigger : EntityFactComponentDelegate<EtudesSystem>
{
	[OwlPackable(OwlPackableMode.Generate)]
	public class EtudeBracketRuntime : ComponentRuntime, IEtudesUpdateHandler, ISubscriber, IAreaActivationHandler, IHashable, IOwlPackable<EtudeBracketRuntime>
	{
		[OwlPackable(OwlPackableMode.Generate)]
		public class SavableData : IEntityFactComponentSavableData, IHashable, IOwlPackable<SavableData>
		{
			[JsonProperty]
			[OwlPackInclude]
			public bool AlreadyProcessedActivation;

			public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
			{
				Name = "SavableData",
				OldNames = null,
				Fields = new FieldInfo[1]
				{
					new FieldInfo("AlreadyProcessedActivation", typeof(bool))
				}
			};

			public override Hash128 GetHash128()
			{
				Hash128 result = default(Hash128);
				Hash128 val = base.GetHash128();
				result.Append(ref val);
				result.Append(ref AlreadyProcessedActivation);
				return result;
			}

			public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
			{
				SavableData source = new SavableData();
				result = Unsafe.As<SavableData, TPossiblyBase>(ref source);
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
				ushort type = state.TypeLibrary.RegisterType<SavableData>(OwlPackTypeInfo);
				formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
				formatter.UnmanagedField(0, "AlreadyProcessedActivation", ref AlreadyProcessedActivation, state);
				formatter.EndObject();
			}

			public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
			{
				state.References.Register(objectId, this);
				TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<SavableData>();
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
						AlreadyProcessedActivation = formatter.ReadUnmanaged<bool>(state);
						break;
					}
				}
				formatter.LeaveObject();
			}
		}

		private bool m_NeedResume;

		public new static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "EtudeBracketRuntime",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("SourceBlueprintComponentName", typeof(string))
			}
		};

		public Etude Etude => (Etude)base.Fact;

		private EtudeBracketTrigger Delegate => (EtudeBracketTrigger)base.SourceBlueprintComponent;

		private bool LinkedAreaUnavailable => IsLinkedAreaUnavailable(Etude.Blueprint, Game.Instance.CurrentlyLoadedAreaPart);

		private bool LinkedAreaUnavailableOnExit
		{
			get
			{
				if (IsLinkedAreaUnavailable(Etude.Blueprint, Game.Instance.CurrentlyLoadedAreaPart))
				{
					return IsLinkedAreaUnavailable(Etude.Blueprint, Game.Instance.EtudesSystem.AreaPartBeingExited);
				}
				return false;
			}
		}

		public bool AlreadyProcessedActivation => RequestSavableData<SavableData>().AlreadyProcessedActivation;

		private static bool IsLinkedAreaUnavailable(BlueprintEtude etude, BlueprintAreaPart area)
		{
			if (!etude.HasLinkedAreaPart || etude.IsLinkedAreaPart(area))
			{
				if (!etude.Parent.IsEmpty())
				{
					return IsLinkedAreaUnavailable(etude.Parent, area);
				}
				return false;
			}
			return true;
		}

		protected override void OnActivate()
		{
			base.OnActivate();
			RequestSavableData<SavableData>().AlreadyProcessedActivation = false;
			m_NeedResume = false;
		}

		protected override void OnDeactivate()
		{
			base.OnDeactivate();
			SavableData savableData = RequestSavableData<SavableData>();
			if (savableData.AlreadyProcessedActivation)
			{
				savableData.AlreadyProcessedActivation = false;
				m_NeedResume = false;
				Exit();
			}
		}

		private void MaybeEnter()
		{
			if (LinkedAreaUnavailable)
			{
				return;
			}
			SavableData savableData = RequestSavableData<SavableData>();
			if (savableData.AlreadyProcessedActivation)
			{
				if (m_NeedResume)
				{
					m_NeedResume = false;
					Resume();
				}
			}
			else
			{
				savableData.AlreadyProcessedActivation = true;
				Enter();
			}
		}

		public void OnEtudesUpdate()
		{
			MaybeEnter();
		}

		public void OnAreaActivated()
		{
			MaybeEnter();
		}

		protected override void OnPostLoad()
		{
			base.OnPostLoad();
			SavableData savableData = RequestSavableData<SavableData>();
			if (base.Fact.Active && savableData.AlreadyProcessedActivation)
			{
				if (LinkedAreaUnavailable)
				{
					m_NeedResume = true;
				}
				else
				{
					Resume();
				}
			}
		}

		private void Enter()
		{
			try
			{
				using (SetScope())
				{
					Delegate.OnEnter();
				}
			}
			catch (Exception exception)
			{
				PFLog.Etudes.ExceptionWithReport(exception, null);
			}
		}

		private void Exit()
		{
			try
			{
				if (LinkedAreaUnavailableOnExit)
				{
					PFLog.Etudes.ErrorWithReport(Etude.Blueprint, "EtudeBracketTrigger.OnExit: skip, because of linked area is unavailable. Probably etude was changed by designer and should be deactivated after game loaded.");
					return;
				}
				using (SetScope())
				{
					Delegate.OnExit();
				}
			}
			catch (Exception exception)
			{
				PFLog.Etudes.ExceptionWithReport(exception, null);
			}
		}

		private void Resume()
		{
			try
			{
				using (SetScope())
				{
					Delegate.OnResume();
				}
			}
			catch (Exception exception)
			{
				PFLog.Etudes.ExceptionWithReport(exception, null);
			}
		}

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			return result;
		}

		public new static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
		{
			EtudeBracketRuntime source = new EtudeBracketRuntime();
			result = Unsafe.As<EtudeBracketRuntime, TPossiblyBase>(ref source);
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
			ushort type = state.TypeLibrary.RegisterType<EtudeBracketRuntime>(OwlPackTypeInfo);
			formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
			string value = base.SourceBlueprintComponentName;
			formatter.StringField(0, "SourceBlueprintComponentName", ref value, state);
			formatter.EndObject();
		}

		public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
		{
			state.References.Register(objectId, this);
			TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<EtudeBracketRuntime>();
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
					base.SourceBlueprintComponentName = formatter.ReadString(state);
					break;
				}
			}
			formatter.LeaveObject();
		}
	}

	public virtual bool RequireLinkedArea => false;

	protected static Etude Etude => (SimpleContextData<EntityFactComponent, EntityFactComponent.Scope>.Current?.Fact as Etude) ?? throw new InvalidOperationException();

	public new EtudeBracketRuntime Runtime => (EtudeBracketRuntime)base.Runtime;

	protected bool AlreadyProcessedActivation => Runtime.AlreadyProcessedActivation;

	protected virtual void OnEnter()
	{
	}

	protected virtual void OnExit()
	{
	}

	protected virtual void OnResume()
	{
	}

	public override EntityFactComponent CreateRuntimeFactComponent()
	{
		return new EtudeBracketRuntime();
	}
}
