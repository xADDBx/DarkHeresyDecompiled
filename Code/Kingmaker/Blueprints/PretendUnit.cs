using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem;
using Kingmaker.QA;
using Kingmaker.UnitLogic;
using Newtonsoft.Json;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Blueprints;

[AllowedOn(typeof(BlueprintUnit))]
[AllowedOn(typeof(BlueprintUnitFact))]
[ComponentName("LD/PretendUnit")]
[TypeId("cea12929263741faadf5bfc2bcfb92d2")]
public class PretendUnit : UnitFactComponentDelegate
{
	[OwlPackable(OwlPackableMode.Generate)]
	public sealed class ComponentData : IEntityFactComponentSavableData, IHashable, IOwlPackable<ComponentData>
	{
		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "ComponentData",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("Applied", typeof(bool))
			}
		};

		[JsonProperty]
		[OwlPackInclude]
		public bool Applied { get; set; }

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			bool val2 = Applied;
			result.Append(ref val2);
			return result;
		}

		public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
		{
			ComponentData source = new ComponentData();
			result = Unsafe.As<ComponentData, TPossiblyBase>(ref source);
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
			ushort type = state.TypeLibrary.RegisterType<ComponentData>(OwlPackTypeInfo);
			formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
			bool value = Applied;
			formatter.UnmanagedField(0, "Applied", ref value, state);
			formatter.EndObject();
		}

		public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
		{
			state.References.Register(objectId, this);
			TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<ComponentData>();
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
					Applied = formatter.ReadUnmanaged<bool>(state);
					break;
				}
			}
			formatter.LeaveObject();
		}
	}

	[SerializeField]
	[ValidateNotNull]
	private BlueprintUnitReference m_Unit;

	public BlueprintUnit Unit => m_Unit?.Get();

	protected override void OnActivate()
	{
		if (base.Owner.Blueprint != base.Owner.OriginalBlueprint)
		{
			PFLog.Default.ErrorWithReport("PretendUnit.OnActivate: Owner.Blueprint != Owner.OriginalBlueprint");
			return;
		}
		base.Owner.SetFakeBlueprint(Unit);
		RequestSavableData<ComponentData>().Applied = true;
	}

	protected override void OnDeactivate()
	{
		ComponentData componentData = RequestSavableData<ComponentData>();
		if (componentData.Applied)
		{
			base.Owner.SetFakeBlueprint(null);
			componentData.Applied = false;
		}
	}
}
