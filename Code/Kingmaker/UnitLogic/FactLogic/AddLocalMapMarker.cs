using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("f76fb89c2e514ffeb5b5ecf695390890")]
public class AddLocalMapMarker : BlueprintComponent, IRuntimeEntityFactComponentProvider
{
	[OwlPackable(OwlPackableMode.Generate)]
	public class Runtime : EntityFactComponent<MechanicEntity, AddLocalMapMarker>, ILocalMapMarker, IHashable, IOwlPackable<Runtime>
	{
		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "Runtime",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("SourceBlueprintComponentName", typeof(string))
			}
		};

		public bool IsDisposed => !base.IsInitialized;

		protected override void OnActivateOrPostLoad()
		{
			base.OnActivateOrPostLoad();
			LocalMapModel.Add(this);
		}

		protected override void OnDeactivate()
		{
			base.OnDeactivate();
			LocalMapModel.Remove(this);
		}

		protected override void OnDispose()
		{
			base.OnDispose();
			LocalMapModel.Remove(this);
		}

		public LocalMapMarkType GetMarkerType()
		{
			base.Settings.Type = ((base.Owner is BaseUnitEntity) ? LocalMapMarkType.VeryImportantPerson : (base.Settings.Type = LocalMapMarkType.PointOfInterest));
			return base.Settings.Type;
		}

		public string GetDescription()
		{
			return base.Owner?.GetDescriptionOptional()?.Name ?? "";
		}

		public Vector3 GetPosition()
		{
			return base.Owner?.Position ?? Vector3.zero;
		}

		public bool IsVisible()
		{
			bool num = base.Owner?.IsInGame ?? false;
			bool flag = base.Owner?.IsRevealed ?? false;
			bool flag2 = base.Owner?.IsDeadOrUnconscious ?? false;
			if (num && (flag || base.Settings.ShowIfNotRevealed))
			{
				return !flag2;
			}
			return false;
		}

		public bool IsMapObject()
		{
			return false;
		}

		public Entity GetEntity()
		{
			return base.Owner;
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
			Runtime source = new Runtime();
			result = Unsafe.As<Runtime, TPossiblyBase>(ref source);
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
			ushort type = state.TypeLibrary.RegisterType<Runtime>(OwlPackTypeInfo);
			formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
			string value = base.SourceBlueprintComponentName;
			formatter.StringField(0, "SourceBlueprintComponentName", ref value, state);
			formatter.EndObject();
		}

		public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
		{
			state.References.Register(objectId, this);
			TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<Runtime>();
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

	public LocalMapMarkType Type = LocalMapMarkType.VeryImportantPerson;

	public bool ShowIfNotRevealed;

	public EntityFactComponent CreateRuntimeFactComponent()
	{
		return new Runtime();
	}
}
