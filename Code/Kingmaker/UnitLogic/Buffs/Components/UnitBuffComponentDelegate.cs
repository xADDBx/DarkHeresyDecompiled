using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Buffs.Components;

[AllowedOn(typeof(BlueprintBuff))]
[TypeId("1a257c2f62219924aa445b0058430d08")]
public abstract class UnitBuffComponentDelegate : UnitFactComponentDelegate
{
	[OwlPackable(OwlPackableMode.Generate)]
	public class UnitBuffComponentRuntime : ComponentRuntime, IBuffRemoved, IHashable, IOwlPackable<UnitBuffComponentRuntime>
	{
		public new static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "UnitBuffComponentRuntime",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("SourceBlueprintComponentName", typeof(string))
			}
		};

		private UnitBuffComponentDelegate Delegate => (UnitBuffComponentDelegate)base.SourceBlueprintComponent;

		void IBuffRemoved.OnRemoved()
		{
			using (SetScope())
			{
				try
				{
					Delegate.OnRemoved();
				}
				catch (Exception ex)
				{
					PFLog.EntityFact.Exception(ex);
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

		public new static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
		{
			UnitBuffComponentRuntime source = new UnitBuffComponentRuntime();
			result = Unsafe.As<UnitBuffComponentRuntime, TPossiblyBase>(ref source);
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
			ushort type = state.TypeLibrary.RegisterType<UnitBuffComponentRuntime>(OwlPackTypeInfo);
			formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
			string value = base.SourceBlueprintComponentName;
			formatter.StringField(0, "SourceBlueprintComponentName", ref value, state);
			formatter.EndObject();
		}

		public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
		{
			state.References.Register(objectId, this);
			TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitBuffComponentRuntime>();
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

	protected Buff Buff => base.Fact as Buff;

	protected virtual void OnRemoved()
	{
	}

	public override EntityFactComponent CreateRuntimeFactComponent()
	{
		return new UnitBuffComponentRuntime();
	}
}
