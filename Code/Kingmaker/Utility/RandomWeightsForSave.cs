using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.Utility;

[OwlPackable(OwlPackableMode.Generate)]
public class RandomWeightsForSave<T> : IOwlPackable, IOwlPackable<RandomWeightsForSave<T>> where T : BlueprintReferenceBase
{
	[JsonProperty]
	[OwlPackInclude]
	private List<WeightPair<T>> m_InitialWeights = new List<WeightPair<T>>();

	[JsonProperty]
	[OwlPackInclude]
	private List<WeightPair<T>> m_CurrentWeights = new List<WeightPair<T>>();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "RandomWeightsForSave",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("m_InitialWeights", typeof(List<WeightPair<T>>)),
			new FieldInfo("m_CurrentWeights", typeof(List<WeightPair<T>>))
		}
	};

	public RandomWeightsForSave(RandomWeights<T> blueprint)
	{
		WeightPair<T>[] weights = blueprint.Weights;
		foreach (WeightPair<T> weightPair in weights)
		{
			m_InitialWeights.Add(new WeightPair<T>(weightPair.Object, weightPair.Weight));
			m_CurrentWeights.Add(new WeightPair<T>(weightPair.Object, weightPair.Weight));
		}
	}

	public RandomWeightsForSave(JsonConstructorMark _)
	{
	}

	public RandomWeightsForSave()
	{
	}

	private int WeightsSum(List<WeightPair<T>> weights)
	{
		int num = 0;
		foreach (WeightPair<T> weight in weights)
		{
			num += weight.Weight;
		}
		return num;
	}

	public int WeightsSum()
	{
		return WeightsSum(m_CurrentWeights);
	}

	[CanBeNull]
	public T GetRandomObject(PersistentRandom.Generator generator)
	{
		return GetRandomObject(generator, m_CurrentWeights);
	}

	[CanBeNull]
	private T GetRandomObject(PersistentRandom.Generator generator, List<WeightPair<T>> weights)
	{
		int num = generator.NextRange(1, WeightsSum(weights));
		int num2 = 0;
		foreach (WeightPair<T> weight in weights)
		{
			if (num2 <= num && num < num2 + weight.Weight)
			{
				T @object = weight.Object;
				RecalculateWeights(@object);
				return @object;
			}
			num2 += weight.Weight;
		}
		if (num2 == num)
		{
			RecalculateWeights(weights[weights.Count - 1].Object);
			return weights[weights.Count - 1].Object;
		}
		RecalculateAllWeights();
		return null;
	}

	[CanBeNull]
	public T GetRandomObjectExcept(PersistentRandom.Generator generator, List<T> except)
	{
		List<WeightPair<T>> weights = m_CurrentWeights.Where((WeightPair<T> obj) => !except.Contains(obj.Object)).ToList();
		return GetRandomObject(generator, weights);
	}

	private void RecalculateWeights(T chosenObject)
	{
		foreach (WeightPair<T> currentWeight in m_CurrentWeights)
		{
			if (currentWeight.Object != chosenObject)
			{
				currentWeight.Weight++;
			}
			else
			{
				currentWeight.Weight = 0;
			}
		}
	}

	private void RecalculateAllWeights()
	{
		foreach (WeightPair<T> currentWeight in m_CurrentWeights)
		{
			currentWeight.Weight++;
		}
	}

	public bool Empty()
	{
		return m_CurrentWeights.Empty();
	}

	public void RemoveObject(T obj)
	{
		WeightPair<T> weightPair = m_CurrentWeights.FirstOrDefault((WeightPair<T> pair) => pair.Object == obj);
		if (weightPair != null)
		{
			m_CurrentWeights.Remove(weightPair);
		}
	}

	public List<T> Keys()
	{
		return m_CurrentWeights.Select((WeightPair<T> pair) => pair.Object).ToList();
	}

	public void CopyCurrentWeights(RandomWeightsForSave<T> other)
	{
		foreach (WeightPair<T> obj in other.m_CurrentWeights)
		{
			m_CurrentWeights.First((WeightPair<T> o) => o.Object.Equals(obj.Object)).Weight = obj.Weight;
		}
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		RandomWeightsForSave<T> source = new RandomWeightsForSave<T>();
		result = Unsafe.As<RandomWeightsForSave<T>, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<RandomWeightsForSave<T>>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_InitialWeights", ref m_InitialWeights, state);
		formatter.Field(1, "m_CurrentWeights", ref m_CurrentWeights, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<RandomWeightsForSave<T>>();
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
				m_InitialWeights = formatter.ReadPackable<List<WeightPair<T>>>(state);
				break;
			case 1:
				m_CurrentWeights = formatter.ReadPackable<List<WeightPair<T>>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
