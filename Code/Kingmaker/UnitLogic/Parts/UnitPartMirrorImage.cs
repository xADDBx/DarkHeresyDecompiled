using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Code.Framework.Utility.UnityExtensions;
using JetBrains.Annotations;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Visual.Particles;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class UnitPartMirrorImage : BaseUnitPart, IHashable, IOwlPackable<UnitPartMirrorImage>
{
	[JsonProperty]
	[OwlPackInclude]
	public List<int> VisualImages = new List<int>();

	[JsonProperty]
	[OwlPackInclude]
	public List<int> MechanicsImages = new List<int>();

	[CanBeNull]
	public MirrorImageFX Fx;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnitPartMirrorImage",
		OldNames = null,
		Fields = new FieldInfo[3]
		{
			new FieldInfo("VisualImages", typeof(List<int>)),
			new FieldInfo("MechanicsImages", typeof(List<int>)),
			new FieldInfo("Source", typeof(Buff))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	public Buff Source { get; private set; }

	public void Init(int imagesCount, Buff source)
	{
		if (Source == null)
		{
			VisualImages.Clear();
			MechanicsImages.Clear();
			for (int i = 1; i <= imagesCount; i++)
			{
				VisualImages.Add(i);
				MechanicsImages.Add(i);
			}
			Source = source;
		}
	}

	public int TryAbsorbHit(bool force = false)
	{
		if (MechanicsImages.Count <= 0)
		{
			return 0;
		}
		int num = (force ? MechanicsImages.Count : base.Owner.Random.Range(0, MechanicsImages.Count + 1));
		if (num <= 0)
		{
			return 0;
		}
		int result = MechanicsImages[num - 1];
		MechanicsImages.RemoveAt(num - 1);
		return result;
	}

	public void SpendReservedImage(int imageIndex)
	{
		if (Fx != null)
		{
			Fx.DestroyImage(imageIndex);
		}
		VisualImages.Remove(imageIndex);
		if (VisualImages.Count <= 0)
		{
			Source?.Remove();
		}
	}

	protected override void OnDetach()
	{
		Source = null;
		if (Fx != null)
		{
			Utils.EditorSafeDestroy(Fx.gameObject);
		}
		Fx = null;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		List<int> visualImages = VisualImages;
		if (visualImages != null)
		{
			for (int i = 0; i < visualImages.Count; i++)
			{
				int obj = visualImages[i];
				Hash128 val2 = UnmanagedHasher<int>.GetHash128(ref obj);
				result.Append(ref val2);
			}
		}
		List<int> mechanicsImages = MechanicsImages;
		if (mechanicsImages != null)
		{
			for (int j = 0; j < mechanicsImages.Count; j++)
			{
				int obj2 = mechanicsImages[j];
				Hash128 val3 = UnmanagedHasher<int>.GetHash128(ref obj2);
				result.Append(ref val3);
			}
		}
		Hash128 val4 = ClassHasher<Buff>.GetHash128(Source);
		result.Append(ref val4);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		UnitPartMirrorImage source = new UnitPartMirrorImage();
		result = Unsafe.As<UnitPartMirrorImage, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UnitPartMirrorImage>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "VisualImages", ref VisualImages, state);
		formatter.Field(1, "MechanicsImages", ref MechanicsImages, state);
		Buff value = Source;
		formatter.Field(2, "Source", ref value, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitPartMirrorImage>();
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
				VisualImages = formatter.ReadPackable<List<int>>(state);
				break;
			case 1:
				MechanicsImages = formatter.ReadPackable<List<int>>(state);
				break;
			case 2:
				Source = formatter.ReadPackable<Buff>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
