using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Utility.StatefulRandom;

[OwlPackable(OwlPackableMode.Generate)]
[HashRoot]
public class StatefulRandom : IHashable, IOwlPackable, IOwlPackable<StatefulRandom>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "StatefulRandom",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("Name", typeof(string)),
			new FieldInfo("Rand", typeof(Rand))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	public string Name { get; private set; }

	[JsonProperty(IsReference = false)]
	[OwlPackInclude]
	public Rand Rand { get; private set; } = new Rand();


	public RandState State
	{
		get
		{
			return Rand.State;
		}
		set
		{
			Rand.State = value;
		}
	}

	public int Sign
	{
		get
		{
			if (Range(0, 2) <= 0)
			{
				return -1;
			}
			return 1;
		}
	}

	public bool YesOrNo => Range(0, 2) > 0;

	public float value => Rand.GetFloat();

	public Vector3 insideUnitSphere => RandomUnitVector(Rand) * Mathf.Pow(Rand.GetFloat(), 1f / 3f);

	public Vector2 insideUnitCircle => RandomUnitVector2(Rand) * Mathf.Sqrt(Rand.RangedRandom(0f, 1f));

	public Vector3 onUnitSphere => RandomUnitVector(Rand);

	public uint uintValue => Rand.Get();

	private StatefulRandom()
	{
	}

	public StatefulRandom(string name, uint seed = 0u)
	{
		Name = name;
		Seed(seed);
	}

	public void Seed(uint seed)
	{
		Rand.Seed = seed;
	}

	[Conditional("ALWAYS_FALSE")]
	private void DebugCollect()
	{
		CheckInit();
	}

	private void CheckInit()
	{
		if (!State.IsReady)
		{
			throw new Exception("Trying to use RND '" + Name + "' that has not been initialized!");
		}
	}

	public float Range(float minInclusive, float maxInclusive)
	{
		return Rand.RangedRandom(minInclusive, maxInclusive);
	}

	public int Range(int minInclusive, int maxExclusive)
	{
		return Rand.RangedRandom(minInclusive, maxExclusive);
	}

	private static Vector3 RandomUnitVector(Rand rand)
	{
		float num = rand.RangedRandom(-1f, 1f);
		float f = rand.RangedRandom(0f, MathF.PI * 2f);
		float num2 = Mathf.Sqrt(1f - num * num);
		float x = num2 * Mathf.Cos(f);
		float y = num2 * Mathf.Sin(f);
		return new Vector3(x, y, num);
	}

	private static Vector2 RandomUnitVector2(Rand rand)
	{
		float f = rand.RangedRandom(0f, MathF.PI * 2f);
		float x = Mathf.Cos(f);
		float y = Mathf.Sin(f);
		return new Vector2(x, y);
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(Name);
		Hash128 val = ClassHasher<Rand>.GetHash128(Rand);
		result.Append(ref val);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		StatefulRandom source = new StatefulRandom();
		result = Unsafe.As<StatefulRandom, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<StatefulRandom>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		string name = Name;
		formatter.StringField(0, "Name", ref name, state);
		Rand rand = Rand;
		formatter.Field(1, "Rand", ref rand, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<StatefulRandom>();
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
				Rand = formatter.ReadPackable<Rand>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
