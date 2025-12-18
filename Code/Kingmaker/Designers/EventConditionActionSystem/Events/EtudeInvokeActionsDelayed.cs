using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Events;

[Obsolete]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintEtude))]
[ComponentName("Events/EtudeInvokeActionsDelayed")]
[TypeId("7282e6131e1028946b06c28e1c78ad57")]
public class EtudeInvokeActionsDelayed : BlueprintComponent, IRuntimeEntityFactComponentProvider
{
	[JsonObject]
	[OwlPackable(OwlPackableMode.Generate)]
	public class EtudeInvokeActionDelayedData : EntityFactComponent<EtudesSystem, EtudeInvokeActionsDelayed>, ITimedEvent, ISubscriber, IHashable, IOwlPackable<EtudeInvokeActionDelayedData>
	{
		[OwlPackable(OwlPackableMode.Generate)]
		public class SavableData : IEntityFactComponentSavableData, IHashable, IOwlPackable<SavableData>
		{
			[JsonProperty]
			[OwlPackInclude]
			public bool Executed;

			[JsonProperty]
			[OwlPackInclude]
			public TimeSpan LastTickTime;

			[JsonProperty]
			[OwlPackInclude]
			public TimeSpan TimeRemaining;

			public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
			{
				Name = "SavableData",
				OldNames = null,
				Fields = new FieldInfo[3]
				{
					new FieldInfo("Executed", typeof(bool)),
					new FieldInfo("LastTickTime", typeof(TimeSpan)),
					new FieldInfo("TimeRemaining", typeof(TimeSpan))
				}
			};

			public override Hash128 GetHash128()
			{
				Hash128 result = default(Hash128);
				Hash128 val = base.GetHash128();
				result.Append(ref val);
				result.Append(ref Executed);
				result.Append(ref LastTickTime);
				result.Append(ref TimeRemaining);
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
				formatter.UnmanagedField(0, "Executed", ref Executed, state);
				formatter.Field(1, "LastTickTime", ref LastTickTime, state);
				formatter.Field(2, "TimeRemaining", ref TimeRemaining, state);
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
						Executed = formatter.ReadUnmanaged<bool>(state);
						break;
					case 1:
						LastTickTime = formatter.ReadPackable<TimeSpan>(state);
						break;
					case 2:
						TimeRemaining = formatter.ReadPackable<TimeSpan>(state);
						break;
					}
				}
				formatter.LeaveObject();
			}
		}

		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "EtudeInvokeActionDelayedData",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("SourceBlueprintComponentName", typeof(string))
			}
		};

		protected override void OnInitialize()
		{
			RequestSavableData<SavableData>().TimeRemaining = TimeSpan.FromDays(base.Settings.m_Days);
		}

		protected override void OnActivateOrPostLoad()
		{
			RequestSavableData<SavableData>().LastTickTime = Game.Instance.Controllers.TimeController.GameTime;
		}

		protected override void OnDeactivate()
		{
			HandleTimePassed();
		}

		public void HandleTimePassed()
		{
			SavableData savableData = RequestSavableData<SavableData>();
			Etude etude = (Etude)base.Fact;
			if (!savableData.Executed && etude.IsPlaying)
			{
				TimeSpan gameTime = Game.Instance.Controllers.TimeController.GameTime;
				TimeSpan timeSpan = gameTime - savableData.LastTickTime;
				savableData.LastTickTime = gameTime;
				savableData.TimeRemaining -= timeSpan;
				if (!(savableData.TimeRemaining.TotalMilliseconds > 0.0))
				{
					base.Settings.m_ActionList?.Run();
					savableData.Executed = true;
				}
			}
		}

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			return result;
		}

		public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
		{
			EtudeInvokeActionDelayedData source = new EtudeInvokeActionDelayedData();
			result = Unsafe.As<EtudeInvokeActionDelayedData, TPossiblyBase>(ref source);
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
			ushort type = state.TypeLibrary.RegisterType<EtudeInvokeActionDelayedData>(OwlPackTypeInfo);
			formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
			string value = base.SourceBlueprintComponentName;
			formatter.StringField(0, "SourceBlueprintComponentName", ref value, state);
			formatter.EndObject();
		}

		public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
		{
			state.References.Register(objectId, this);
			TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<EtudeInvokeActionDelayedData>();
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

	[SerializeField]
	[Tooltip("How much in-game days should pass for ActionList to be invoked")]
	private int m_Days;

	[SerializeField]
	[Tooltip("Actions to invoke after required amount of days has passed")]
	private ActionList m_ActionList;

	public EntityFactComponent CreateRuntimeFactComponent()
	{
		return new EtudeInvokeActionDelayedData();
	}
}
