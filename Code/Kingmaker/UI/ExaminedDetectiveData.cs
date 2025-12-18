using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.Networking.Serialization;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UI;

[JsonObject]
[OwlPackable(OwlPackableMode.Generate)]
public class ExaminedDetectiveData : IHashable, IOwlPackable, IOwlPackable<ExaminedDetectiveData>
{
	[JsonObject]
	[OwlPackable(OwlPackableMode.Generate)]
	public class ExaminedAnswers : IHashable, IOwlPackable, IOwlPackable<ExaminedAnswers>
	{
		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "ExaminedAnswers",
			OldNames = null,
			Fields = new FieldInfo[3]
			{
				new FieldInfo("ViewedAnswers", typeof(ExaminedData<BlueprintCaseAnswer>)),
				new FieldInfo("SelectedAnswers", typeof(Dictionary<BlueprintCase, BlueprintCaseAnswer>)),
				new FieldInfo("ViewedTier", typeof(Dictionary<BlueprintCaseAnswer, int>))
			}
		};

		[JsonProperty]
		[OwlPackInclude]
		[GameStateIgnore]
		public ExaminedData<BlueprintCaseAnswer> ViewedAnswers { get; private set; } = new ExaminedData<BlueprintCaseAnswer>();


		[JsonProperty]
		[OwlPackInclude]
		[GameStateIgnore]
		private Dictionary<BlueprintCase, BlueprintCaseAnswer> SelectedAnswers { get; set; } = new Dictionary<BlueprintCase, BlueprintCaseAnswer>();


		[JsonProperty]
		[OwlPackInclude]
		[GameStateIgnore]
		private Dictionary<BlueprintCaseAnswer, int> ViewedTier { get; set; } = new Dictionary<BlueprintCaseAnswer, int>();


		public void AddExaminedAnswerIfNeeded(BlueprintCaseAnswer bp)
		{
			ViewedAnswers.AddExaminedEntityIfNeeded(bp);
			ViewedTier.TryAdd(bp, -1);
			Game.Instance.DetectiveSystem.TryGetAnswerDegree(bp, out var degree);
			ViewedTier[bp] = Mathf.Max(degree, ViewedTier[bp]);
		}

		public int GetAnswerTier(BlueprintCaseAnswer bp)
		{
			return ViewedTier.GetValueOrDefault(bp, -1);
		}

		public bool IsEntityNew(BlueprintCaseAnswer bp)
		{
			int answerTier = GetAnswerTier(bp);
			Game.Instance.DetectiveSystem.TryGetAnswerDegree(bp, out var degree);
			return degree > answerTier;
		}

		public BlueprintCaseAnswer GetSelectedAnswer(BlueprintCase bp)
		{
			return SelectedAnswers.GetValueOrDefault(bp, null);
		}

		public void SetSelectedAnswer(BlueprintCase bp, BlueprintCaseAnswer answer)
		{
			if (!SelectedAnswers.TryAdd(bp, answer))
			{
				SelectedAnswers[bp] = answer;
			}
		}

		public void ForgetAnswerTier(BlueprintCaseAnswer bp)
		{
			ViewedAnswers.RemoveAll((BlueprintCaseAnswer a) => a.Equals(bp));
		}

		public virtual Hash128 GetHash128()
		{
			return default(Hash128);
		}

		public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
		{
			ExaminedAnswers source = new ExaminedAnswers();
			result = Unsafe.As<ExaminedAnswers, TPossiblyBase>(ref source);
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
			ushort type = state.TypeLibrary.RegisterType<ExaminedAnswers>(OwlPackTypeInfo);
			formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
			ExaminedData<BlueprintCaseAnswer> value = ViewedAnswers;
			formatter.Field(0, "ViewedAnswers", ref value, state);
			Dictionary<BlueprintCase, BlueprintCaseAnswer> value2 = SelectedAnswers;
			formatter.Field(1, "SelectedAnswers", ref value2, state);
			Dictionary<BlueprintCaseAnswer, int> value3 = ViewedTier;
			formatter.Field(2, "ViewedTier", ref value3, state);
			formatter.EndObject();
		}

		public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
		{
			state.References.Register(objectId, this);
			TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<ExaminedAnswers>();
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
					ViewedAnswers = formatter.ReadPackable<ExaminedData<BlueprintCaseAnswer>>(state);
					break;
				case 1:
					SelectedAnswers = formatter.ReadPackable<Dictionary<BlueprintCase, BlueprintCaseAnswer>>(state);
					break;
				case 2:
					ViewedTier = formatter.ReadPackable<Dictionary<BlueprintCaseAnswer, int>>(state);
					break;
				}
			}
			formatter.LeaveObject();
		}
	}

	[JsonObject]
	[OwlPackable(OwlPackableMode.Generate)]
	public class ExaminedData<T> : IHashable, IOwlPackable, IOwlPackable<ExaminedData<T>>
	{
		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "ExaminedData",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("ExaminedEntities", typeof(List<T>))
			}
		};

		private static IOutputFormatter.FieldDelegate<List<T>> m_Serializer_List_ = null;

		private static IInputFormatter.FieldDelegate<List<T>> m_Deserializer_List_ = null;

		[JsonProperty]
		[OwlPackInclude]
		[GameStateIgnore]
		private List<T> ExaminedEntities { get; set; } = new List<T>();


		public void AddExaminedEntityIfNeeded(T bp)
		{
			if (IsEntityNew(bp))
			{
				ExaminedEntities.Add(bp);
			}
		}

		public bool IsEntityNew(T bp)
		{
			return !ExaminedEntities.Contains(bp);
		}

		public void RemoveAll(Func<T, bool> predicate)
		{
			ExaminedEntities.RemoveAll((T e) => predicate(e));
		}

		public List<T> GetEntities()
		{
			return ExaminedEntities;
		}

		public virtual Hash128 GetHash128()
		{
			return default(Hash128);
		}

		public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
		{
			ExaminedData<T> source = new ExaminedData<T>();
			result = Unsafe.As<ExaminedData<T>, TPossiblyBase>(ref source);
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
			ushort type = state.TypeLibrary.RegisterType<ExaminedData<T>>(OwlPackTypeInfo);
			OutputFormatter.CreateFieldDelegate(formatter, ref m_Serializer_List_);
			formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
			List<T> value = ExaminedEntities;
			formatter.Field(0, "ExaminedEntities", ref value, state);
			formatter.EndObject();
		}

		public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
		{
			InputFormatter.CreateFieldDelegate(formatter, ref m_Deserializer_List_);
			state.References.Register(objectId, this);
			TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<ExaminedData<T>>();
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
					ExaminedEntities = formatter.ReadPackable<List<T>>(state);
					break;
				}
			}
			formatter.LeaveObject();
		}
	}

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "ExaminedDetectiveData",
		OldNames = null,
		Fields = new FieldInfo[11]
		{
			new FieldInfo("CaseInfoData", typeof(Dictionary<BlueprintCase, bool>)),
			new FieldInfo("ExaminedCases", typeof(ExaminedData<BlueprintCase>)),
			new FieldInfo("ExaminedClues", typeof(ExaminedData<BlueprintClue>)),
			new FieldInfo("ExaminedAddendums", typeof(ExaminedData<BlueprintClueAddendum>)),
			new FieldInfo("ExaminedStudies", typeof(ExaminedData<BlueprintClueStudy>)),
			new FieldInfo("ExaminedConclusions", typeof(ExaminedData<BlueprintConclusion>)),
			new FieldInfo("WatchedConclusions", typeof(ExaminedData<BlueprintConclusion>)),
			new FieldInfo("SelectedConclusions", typeof(ExaminedData<BlueprintConclusion>)),
			new FieldInfo("ViewedReportsConclusions", typeof(ExaminedData<BlueprintConclusion>)),
			new FieldInfo("ExaminedAnswersData", typeof(ExaminedAnswers)),
			new FieldInfo("SelectedConclusionSource", typeof(ExaminedData<ConclusionSourceWrapper>))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	[GameStateIgnore]
	private Dictionary<BlueprintCase, bool> CaseInfoData { get; set; } = new Dictionary<BlueprintCase, bool>();


	[JsonProperty]
	[OwlPackInclude]
	[GameStateIgnore]
	public ExaminedData<BlueprintCase> ExaminedCases { get; private set; } = new ExaminedData<BlueprintCase>();


	[JsonProperty]
	[OwlPackInclude]
	[GameStateIgnore]
	public ExaminedData<BlueprintClue> ExaminedClues { get; private set; } = new ExaminedData<BlueprintClue>();


	[JsonProperty]
	[OwlPackInclude]
	[GameStateIgnore]
	public ExaminedData<BlueprintClueAddendum> ExaminedAddendums { get; private set; } = new ExaminedData<BlueprintClueAddendum>();


	[JsonProperty]
	[OwlPackInclude]
	[GameStateIgnore]
	public ExaminedData<BlueprintClueStudy> ExaminedStudies { get; private set; } = new ExaminedData<BlueprintClueStudy>();


	[JsonProperty]
	[OwlPackInclude]
	[GameStateIgnore]
	public ExaminedData<BlueprintConclusion> ExaminedConclusions { get; private set; } = new ExaminedData<BlueprintConclusion>();


	[JsonProperty]
	[OwlPackInclude]
	[GameStateIgnore]
	public ExaminedData<BlueprintConclusion> WatchedConclusions { get; private set; } = new ExaminedData<BlueprintConclusion>();


	[JsonProperty]
	[OwlPackInclude]
	[GameStateIgnore]
	public ExaminedData<BlueprintConclusion> SelectedConclusions { get; private set; } = new ExaminedData<BlueprintConclusion>();


	[JsonProperty]
	[OwlPackInclude]
	[GameStateIgnore]
	public ExaminedData<BlueprintConclusion> ViewedReportsConclusions { get; private set; } = new ExaminedData<BlueprintConclusion>();


	[JsonProperty]
	[OwlPackInclude]
	[GameStateIgnore]
	public ExaminedAnswers ExaminedAnswersData { get; private set; } = new ExaminedAnswers();


	[JsonProperty]
	[OwlPackInclude]
	[GameStateIgnore]
	public ExaminedData<ConclusionSourceWrapper> SelectedConclusionSource { get; private set; } = new ExaminedData<ConclusionSourceWrapper>();


	public bool IsCaseInfoExpanded(BlueprintCase blueprintCase)
	{
		if (blueprintCase == null)
		{
			return false;
		}
		bool value;
		return CaseInfoData.TryGetValue(blueprintCase, out value) && value;
	}

	public void ToggleInfoExpanded(BlueprintCase blueprintCase)
	{
		if (blueprintCase != null && !CaseInfoData.TryAdd(blueprintCase, value: true))
		{
			CaseInfoData[blueprintCase] = !CaseInfoData[blueprintCase];
		}
	}

	public virtual Hash128 GetHash128()
	{
		return default(Hash128);
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		ExaminedDetectiveData source = new ExaminedDetectiveData();
		result = Unsafe.As<ExaminedDetectiveData, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<ExaminedDetectiveData>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		Dictionary<BlueprintCase, bool> value = CaseInfoData;
		formatter.Field(0, "CaseInfoData", ref value, state);
		ExaminedData<BlueprintCase> value2 = ExaminedCases;
		formatter.Field(1, "ExaminedCases", ref value2, state);
		ExaminedData<BlueprintClue> value3 = ExaminedClues;
		formatter.Field(2, "ExaminedClues", ref value3, state);
		ExaminedData<BlueprintClueAddendum> value4 = ExaminedAddendums;
		formatter.Field(3, "ExaminedAddendums", ref value4, state);
		ExaminedData<BlueprintClueStudy> value5 = ExaminedStudies;
		formatter.Field(4, "ExaminedStudies", ref value5, state);
		ExaminedData<BlueprintConclusion> value6 = ExaminedConclusions;
		formatter.Field(5, "ExaminedConclusions", ref value6, state);
		ExaminedData<BlueprintConclusion> value7 = WatchedConclusions;
		formatter.Field(6, "WatchedConclusions", ref value7, state);
		ExaminedData<BlueprintConclusion> value8 = SelectedConclusions;
		formatter.Field(7, "SelectedConclusions", ref value8, state);
		ExaminedData<BlueprintConclusion> value9 = ViewedReportsConclusions;
		formatter.Field(8, "ViewedReportsConclusions", ref value9, state);
		ExaminedAnswers value10 = ExaminedAnswersData;
		formatter.Field(9, "ExaminedAnswersData", ref value10, state);
		ExaminedData<ConclusionSourceWrapper> value11 = SelectedConclusionSource;
		formatter.Field(10, "SelectedConclusionSource", ref value11, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<ExaminedDetectiveData>();
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
				CaseInfoData = formatter.ReadPackable<Dictionary<BlueprintCase, bool>>(state);
				break;
			case 1:
				ExaminedCases = formatter.ReadPackable<ExaminedData<BlueprintCase>>(state);
				break;
			case 2:
				ExaminedClues = formatter.ReadPackable<ExaminedData<BlueprintClue>>(state);
				break;
			case 3:
				ExaminedAddendums = formatter.ReadPackable<ExaminedData<BlueprintClueAddendum>>(state);
				break;
			case 4:
				ExaminedStudies = formatter.ReadPackable<ExaminedData<BlueprintClueStudy>>(state);
				break;
			case 5:
				ExaminedConclusions = formatter.ReadPackable<ExaminedData<BlueprintConclusion>>(state);
				break;
			case 6:
				WatchedConclusions = formatter.ReadPackable<ExaminedData<BlueprintConclusion>>(state);
				break;
			case 7:
				SelectedConclusions = formatter.ReadPackable<ExaminedData<BlueprintConclusion>>(state);
				break;
			case 8:
				ViewedReportsConclusions = formatter.ReadPackable<ExaminedData<BlueprintConclusion>>(state);
				break;
			case 9:
				ExaminedAnswersData = formatter.ReadPackable<ExaminedAnswers>(state);
				break;
			case 10:
				SelectedConclusionSource = formatter.ReadPackable<ExaminedData<ConclusionSourceWrapper>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
