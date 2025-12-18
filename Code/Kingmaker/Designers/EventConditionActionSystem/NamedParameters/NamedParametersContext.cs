using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.NamedParameters;

[HashNoGenerate]
[OwlPackable(OwlPackableMode.Generate)]
public class NamedParametersContext : IOwlPackable, IOwlPackable<NamedParametersContext>
{
	public static class Hasher
	{
		[HasherFor(Type = typeof(NamedParametersContext))]
		public static Hash128 GetHash128(NamedParametersContext obj)
		{
			if (obj == null)
			{
				return default(Hash128);
			}
			Hash128 result = default(Hash128);
			if (RecursiveReferences.TryGetValue(obj, out var index))
			{
				result.Append(ref index);
				return result;
			}
			RecursiveReferences.Add(obj);
			int val = 0;
			foreach (KeyValuePair<string, INamedParameterValue> param in obj.Params)
			{
				param.Deconstruct(out var key, out var value);
				string text = key;
				INamedParameterValue par = value;
				Hash128 hash = default(Hash128);
				if (text != null)
				{
					Hash128 val2 = StringHasher.GetHash128(text);
					hash.Append(ref val2);
				}
				string text2 = Normalize(par);
				if (text2 != null)
				{
					Hash128 val3 = StringHasher.GetHash128(text2);
					hash.Append(ref val3);
				}
				val ^= hash.GetHashCode();
			}
			result.Append(ref val);
			return result;
		}
	}

	public class ContextData : ContextData<ContextData>
	{
		public NamedParametersContext Context { get; private set; }

		public ContextData Setup([NotNull] NamedParametersContext context)
		{
			Context = context;
			return this;
		}

		protected override void Reset()
		{
			Context = null;
		}
	}

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "NamedParametersContext",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("Params", typeof(Dictionary<string, INamedParameterValue>))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	public Dictionary<string, INamedParameterValue> Params { get; private set; } = new Dictionary<string, INamedParameterValue>();


	[CanBeNull]
	public CutscenePlayerData Cutscene { get; set; }

	public IDisposable RequestContextData()
	{
		return ContextData<ContextData>.Request().Setup(this);
	}

	public bool IsTheSame(NamedParametersContext other)
	{
		foreach (KeyValuePair<string, INamedParameterValue> param in Params)
		{
			if (!other.Params.TryGetValue(param.Key, out var value))
			{
				return false;
			}
			if (Normalize(value) != Normalize(param.Value))
			{
				return false;
			}
		}
		return true;
	}

	private static string Normalize(object par)
	{
		if (!(par is string result))
		{
			if (!(par is Entity entity))
			{
				if (!(par is IEntityRef entityRef))
				{
					if (!(par is ITypedEntityRef typedEntityRef))
					{
						if (!(par is SimpleBlueprint simpleBlueprint))
						{
							if (par is INamedParameterValue namedParameterValue)
							{
								return Normalize(namedParameterValue.Value);
							}
							if (par == null)
							{
								return null;
							}
							return Convert.ToString(par, CultureInfo.InvariantCulture);
						}
						return simpleBlueprint.AssetGuid.ToString();
					}
					return typedEntityRef.GetId();
				}
				return entityRef.Id;
			}
			return entity.UniqueId;
		}
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		NamedParametersContext source = new NamedParametersContext();
		result = Unsafe.As<NamedParametersContext, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<NamedParametersContext>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		Dictionary<string, INamedParameterValue> value = Params;
		formatter.Field(0, "Params", ref value, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<NamedParametersContext>();
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
				Params = formatter.ReadPackable<Dictionary<string, INamedParameterValue>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
