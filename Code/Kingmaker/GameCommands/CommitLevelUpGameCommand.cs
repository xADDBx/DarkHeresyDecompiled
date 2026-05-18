using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Code.View.Bridge.Facades;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Levelup.Selections.CharacterGender;
using Kingmaker.UnitLogic.Levelup.Selections.CharacterName;
using Kingmaker.UnitLogic.Levelup.Selections.Doll;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.UnitLogic.Levelup.Selections.Portrait;
using Kingmaker.UnitLogic.Levelup.Selections.Stats;
using Kingmaker.UnitLogic.Levelup.Selections.Voice;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.Progression.Paths;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public class CommitLevelUpGameCommand : GameCommand, IMemoryPackable<CommitLevelUpGameCommand>, IMemoryPackFormatterRegister, IOwlPackable<CommitLevelUpGameCommand>
{
	[Preserve]
	private sealed class CommitLevelUpGameCommandFormatter : MemoryPackFormatter<CommitLevelUpGameCommand>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref CommitLevelUpGameCommand value)
		{
			CommitLevelUpGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref CommitLevelUpGameCommand value)
		{
			CommitLevelUpGameCommand.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CommitLevelUpGameCommand value)
		{
			CommitLevelUpGameCommand.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref CommitLevelUpGameCommand value)
		{
			CommitLevelUpGameCommand.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private UnitReference m_UnitRef;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private BlueprintPath m_CareerPath;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private List<SelectionFeature> m_FeatureSelections;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private List<StatsEntry> m_StatSelections;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private PortraitEntry m_PortraitSelection;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private CharacterNameEntry m_CharacterNameSelection;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private CharacterVoiceEntry m_CharacterVoiceSelection;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private CharacterGenderEntry m_CharacterGenderSelection;

	[JsonProperty]
	[OwlPackInclude]
	[MemoryPackInclude]
	private CharacterDollEntry m_CharacterDollSelection;

	public static readonly TypeInfo OwlPackTypeInfo;

	public override bool IsSynchronized => true;

	public override bool IsForcedSynced => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	public CommitLevelUpGameCommand()
	{
	}

	public CommitLevelUpGameCommand([NotNull] LevelUpManager levelUpManager)
	{
		m_UnitRef = levelUpManager.TargetUnit.FromBaseUnitEntity();
		m_CareerPath = levelUpManager.Path;
		m_FeatureSelections = new List<SelectionFeature>();
		m_StatSelections = new List<StatsEntry>();
		foreach (SelectionState selection in levelUpManager.Selections)
		{
			if (!(selection is SelectionStateFeature { SelectionItem: var selectionItem } selectionStateFeature))
			{
				if (!(selection is SelectionStateStats selectionStateStats))
				{
					if (!(selection is SelectionStatePortrait selectionStatePortrait))
					{
						if (!(selection is SelectionStateCharacterName selectionStateCharacterName))
						{
							if (!(selection is SelectionStateVoice selectionStateVoice))
							{
								if (!(selection is SelectionStateGender selectionStateGender))
								{
									if (!(selection is SelectionStateDoll selectionStateDoll))
									{
										throw new ArgumentOutOfRangeException("selectionState");
									}
									DollData dollData = selectionStateDoll.CreateData();
									if (dollData != null)
									{
										m_CharacterDollSelection = new CharacterDollEntry(dollData);
									}
								}
								else
								{
									m_CharacterGenderSelection = new CharacterGenderEntry(selectionStateGender.Gender);
								}
							}
							else
							{
								m_CharacterVoiceSelection = new CharacterVoiceEntry(selectionStateVoice.Asks.ToReference<BlueprintUnitAsksListReference>());
							}
						}
						else
						{
							m_CharacterNameSelection = new CharacterNameEntry(selectionStateCharacterName.CharacterName);
						}
					}
					else
					{
						m_PortraitSelection = new PortraitEntry(selectionStatePortrait.Portrait.ToReference<BlueprintPortraitReference>());
					}
					continue;
				}
				foreach (StatType stat in selectionStateStats.Stats)
				{
					if (selectionStateStats.GetPointsSpent(stat) != 0)
					{
						int pointsTotal = selectionStateStats.GetPointsTotal(stat);
						m_StatSelections.Add(new StatsEntry(selectionStateStats.Blueprint.ToReference<BlueprintSelectionReference>(), selectionStateStats.PathRank, stat, pointsTotal));
					}
				}
			}
			else
			{
				BlueprintFeature blueprintFeature = selectionItem?.Feature;
				if (blueprintFeature != null)
				{
					m_FeatureSelections.Add(new SelectionFeature(selectionStateFeature.Blueprint.ToReference<BlueprintSelectionReference>(), selectionStateFeature.PathRank, blueprintFeature.ToReference<BlueprintFeatureReference>()));
				}
			}
		}
	}

	protected override void ExecuteInternal()
	{
		AbstractUnitEntity abstractUnitEntity = m_UnitRef.Entity as AbstractUnitEntity;
		DollData oldDoll = abstractUnitEntity?.ViewSettings.Doll;
		LevelUpManager levelUpManager = new LevelUpManager(m_UnitRef.Entity.ToIBaseUnitEntity(), m_CareerPath, autoCommit: false);
		foreach (SelectionState selection2 in levelUpManager.Selections)
		{
			BlueprintSelection selection = selection2.Blueprint;
			int pathRank = selection2.PathRank;
			if (!(selection2 is SelectionStateFeature selectionStateFeature))
			{
				if (!(selection2 is SelectionStateStats selectionStateStats))
				{
					if (!(selection2 is SelectionStatePortrait selectionStatePortrait))
					{
						if (!(selection2 is SelectionStateCharacterName selectionStateCharacterName))
						{
							if (!(selection2 is SelectionStateVoice selectionStateVoice))
							{
								if (!(selection2 is SelectionStateGender selectionStateGender))
								{
									if (!(selection2 is SelectionStateDoll selectionStateDoll))
									{
										throw new ArgumentOutOfRangeException("selectionState");
									}
									if (m_CharacterDollSelection != null)
									{
										selectionStateDoll.Select(m_CharacterDollSelection.ToDollData());
									}
								}
								else
								{
									selectionStateGender.SelectGender(m_CharacterGenderSelection.Gender);
								}
							}
							else
							{
								selectionStateVoice.SelectVoice(m_CharacterVoiceSelection.Asks);
							}
						}
						else
						{
							selectionStateCharacterName.SelectName(m_CharacterNameSelection.Name);
						}
					}
					else
					{
						selectionStatePortrait.SelectPortrait(m_PortraitSelection.Portrait);
					}
					continue;
				}
				foreach (StatsEntry item in m_StatSelections.Where((StatsEntry i) => i.Selection.Blueprint == selection && i.PathRank == pathRank))
				{
					selectionStateStats.SetPoints(item.StatType, item.Points);
				}
			}
			else
			{
				SelectionFeature selectionEntry = m_FeatureSelections.FirstItem((SelectionFeature i) => i.Selection.Blueprint == selection && i.PathRank == pathRank);
				FeatureSelectionItem selectionItem = selectionStateFeature.Items.FirstItem((FeatureSelectionItem i) => i.Feature == selectionEntry?.Feature.Blueprint);
				if (selectionItem.Feature != null)
				{
					selectionStateFeature.Select(selectionItem);
				}
			}
		}
		levelUpManager.Commit();
		levelUpManager.Dispose();
		if (abstractUnitEntity != null && SkeletonChanged(oldDoll, abstractUnitEntity.ViewSettings.Doll))
		{
			RebuildUnitView(abstractUnitEntity);
		}
		EventBus.RaiseEvent(delegate(ILevelUpManagerUIHandler h)
		{
			h.HandleUICommitChanges();
		});
	}

	private static bool SkeletonChanged([CanBeNull] DollData oldDoll, [CanBeNull] DollData newDoll)
	{
		if (oldDoll == null || newDoll == null)
		{
			return oldDoll != newDoll;
		}
		if (oldDoll.Gender == newDoll.Gender)
		{
			return oldDoll.RacePreset != newDoll.RacePreset;
		}
		return true;
	}

	private static void RebuildUnitView(AbstractUnitEntity unit)
	{
		UnitEntityView unitEntityView = unit.View as UnitEntityView;
		if (!(unitEntityView == null))
		{
			UnitEntityView unitEntityView2 = unit.ViewSettings.Instantiate(ignorePolymorph: true);
			if (!(unitEntityView2 == null))
			{
				unit.AttachView(unitEntityView2);
				Scene scene = unitEntityView.ViewTransform.gameObject.scene;
				SceneManager.MoveGameObjectToScene(unitEntityView2.gameObject, scene);
				unitEntityView2.ViewTransform.position = unitEntityView.ViewTransform.position;
				unitEntityView2.ViewTransform.rotation = unitEntityView.ViewTransform.rotation;
				UnityEngine.Object.Destroy(unitEntityView.gameObject);
				SelectionManagerFacade.ForceCreateMarks();
				Game.Instance.Controllers.SelectionCharacter.ReselectCurrentUnit();
			}
		}
	}

	static CommitLevelUpGameCommand()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "CommitLevelUpGameCommand",
			OldNames = null,
			Fields = new FieldInfo[9]
			{
				new FieldInfo("m_UnitRef", typeof(UnitReference)),
				new FieldInfo("m_CareerPath", typeof(BlueprintPath)),
				new FieldInfo("m_FeatureSelections", typeof(List<SelectionFeature>)),
				new FieldInfo("m_StatSelections", typeof(List<StatsEntry>)),
				new FieldInfo("m_PortraitSelection", typeof(PortraitEntry)),
				new FieldInfo("m_CharacterNameSelection", typeof(CharacterNameEntry)),
				new FieldInfo("m_CharacterVoiceSelection", typeof(CharacterVoiceEntry)),
				new FieldInfo("m_CharacterGenderSelection", typeof(CharacterGenderEntry)),
				new FieldInfo("m_CharacterDollSelection", typeof(CharacterDollEntry))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CommitLevelUpGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new CommitLevelUpGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CommitLevelUpGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CommitLevelUpGameCommand>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<List<SelectionFeature>>())
		{
			MemoryPackFormatterProvider.Register(new ListFormatter<SelectionFeature>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<List<StatsEntry>>())
		{
			MemoryPackFormatterProvider.Register(new ListFormatter<StatsEntry>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref CommitLevelUpGameCommand? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(9);
		writer.WritePackable(in value.m_UnitRef);
		writer.WriteValue(in value.m_CareerPath);
		ListFormatter.SerializePackable(ref writer, value.m_FeatureSelections);
		ListFormatter.SerializePackable(ref writer, value.m_StatSelections);
		writer.WritePackable(in value.m_PortraitSelection);
		writer.WritePackable(in value.m_CharacterNameSelection);
		writer.WritePackable(in value.m_CharacterVoiceSelection);
		writer.WritePackable(in value.m_CharacterGenderSelection);
		writer.WritePackable(in value.m_CharacterDollSelection);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CommitLevelUpGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		UnitReference value2;
		BlueprintPath value3;
		List<SelectionFeature> value4;
		List<StatsEntry> value5;
		PortraitEntry value6;
		CharacterNameEntry value7;
		CharacterVoiceEntry value8;
		CharacterGenderEntry value9;
		CharacterDollEntry value10;
		if (memberCount == 9)
		{
			if (value != null)
			{
				value2 = value.m_UnitRef;
				value3 = value.m_CareerPath;
				value4 = value.m_FeatureSelections;
				value5 = value.m_StatSelections;
				value6 = value.m_PortraitSelection;
				value7 = value.m_CharacterNameSelection;
				value8 = value.m_CharacterVoiceSelection;
				value9 = value.m_CharacterGenderSelection;
				value10 = value.m_CharacterDollSelection;
				reader.ReadPackable(ref value2);
				reader.ReadValue(ref value3);
				ListFormatter.DeserializePackable(ref reader, ref value4);
				ListFormatter.DeserializePackable(ref reader, ref value5);
				reader.ReadPackable(ref value6);
				reader.ReadPackable(ref value7);
				reader.ReadPackable(ref value8);
				reader.ReadPackable(ref value9);
				reader.ReadPackable(ref value10);
				goto IL_01fd;
			}
			value2 = reader.ReadPackable<UnitReference>();
			value3 = reader.ReadValue<BlueprintPath>();
			value4 = ListFormatter.DeserializePackable<SelectionFeature>(ref reader);
			value5 = ListFormatter.DeserializePackable<StatsEntry>(ref reader);
			value6 = reader.ReadPackable<PortraitEntry>();
			value7 = reader.ReadPackable<CharacterNameEntry>();
			value8 = reader.ReadPackable<CharacterVoiceEntry>();
			value9 = reader.ReadPackable<CharacterGenderEntry>();
			value10 = reader.ReadPackable<CharacterDollEntry>();
		}
		else
		{
			if (memberCount > 9)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CommitLevelUpGameCommand), 9, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = default(UnitReference);
				value3 = null;
				value4 = null;
				value5 = null;
				value6 = null;
				value7 = null;
				value8 = null;
				value9 = null;
				value10 = null;
			}
			else
			{
				value2 = value.m_UnitRef;
				value3 = value.m_CareerPath;
				value4 = value.m_FeatureSelections;
				value5 = value.m_StatSelections;
				value6 = value.m_PortraitSelection;
				value7 = value.m_CharacterNameSelection;
				value8 = value.m_CharacterVoiceSelection;
				value9 = value.m_CharacterGenderSelection;
				value10 = value.m_CharacterDollSelection;
			}
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				if (memberCount != 1)
				{
					reader.ReadValue(ref value3);
					if (memberCount != 2)
					{
						ListFormatter.DeserializePackable(ref reader, ref value4);
						if (memberCount != 3)
						{
							ListFormatter.DeserializePackable(ref reader, ref value5);
							if (memberCount != 4)
							{
								reader.ReadPackable(ref value6);
								if (memberCount != 5)
								{
									reader.ReadPackable(ref value7);
									if (memberCount != 6)
									{
										reader.ReadPackable(ref value8);
										if (memberCount != 7)
										{
											reader.ReadPackable(ref value9);
											if (memberCount != 8)
											{
												reader.ReadPackable(ref value10);
												_ = 9;
											}
										}
									}
								}
							}
						}
					}
				}
			}
			if (value != null)
			{
				goto IL_01fd;
			}
		}
		value = new CommitLevelUpGameCommand
		{
			m_UnitRef = value2,
			m_CareerPath = value3,
			m_FeatureSelections = value4,
			m_StatSelections = value5,
			m_PortraitSelection = value6,
			m_CharacterNameSelection = value7,
			m_CharacterVoiceSelection = value8,
			m_CharacterGenderSelection = value9,
			m_CharacterDollSelection = value10
		};
		return;
		IL_01fd:
		value.m_UnitRef = value2;
		value.m_CareerPath = value3;
		value.m_FeatureSelections = value4;
		value.m_StatSelections = value5;
		value.m_PortraitSelection = value6;
		value.m_CharacterNameSelection = value7;
		value.m_CharacterVoiceSelection = value8;
		value.m_CharacterGenderSelection = value9;
		value.m_CharacterDollSelection = value10;
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref CommitLevelUpGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("m_UnitRef");
		writer.WritePackable(value.m_UnitRef);
		writer.WriteProperty("m_CareerPath");
		writer.WriteValue(value.m_CareerPath);
		writer.WriteProperty("m_FeatureSelections");
		ListFormatter.SerializePackableJson(ref writer, value.m_FeatureSelections);
		writer.WriteProperty("m_StatSelections");
		ListFormatter.SerializePackableJson(ref writer, value.m_StatSelections);
		writer.WriteProperty("m_PortraitSelection");
		writer.WritePackable(value.m_PortraitSelection);
		writer.WriteProperty("m_CharacterNameSelection");
		writer.WritePackable(value.m_CharacterNameSelection);
		writer.WriteProperty("m_CharacterVoiceSelection");
		writer.WritePackable(value.m_CharacterVoiceSelection);
		writer.WriteProperty("m_CharacterGenderSelection");
		writer.WritePackable(value.m_CharacterGenderSelection);
		writer.WriteProperty("m_CharacterDollSelection");
		writer.WritePackable(value.m_CharacterDollSelection);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref CommitLevelUpGameCommand? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		UnitReference val;
		BlueprintPath val2;
		List<SelectionFeature> value2;
		List<StatsEntry> value3;
		PortraitEntry val3;
		CharacterNameEntry val4;
		CharacterVoiceEntry val5;
		CharacterGenderEntry val6;
		CharacterDollEntry val7;
		if (value == null)
		{
			val = default(UnitReference);
			val2 = null;
			value2 = null;
			value3 = null;
			val3 = null;
			val4 = null;
			val5 = null;
			val6 = null;
			val7 = null;
		}
		else
		{
			val = value.m_UnitRef;
			val2 = value.m_CareerPath;
			value2 = value.m_FeatureSelections;
			value3 = value.m_StatSelections;
			val3 = value.m_PortraitSelection;
			val4 = value.m_CharacterNameSelection;
			val5 = value.m_CharacterVoiceSelection;
			val6 = value.m_CharacterGenderSelection;
			val7 = value.m_CharacterDollSelection;
		}
		bool[] array = new bool[9];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				switch (text)
				{
				case "m_UnitRef":
					val = reader.ReadPackable<UnitReference>();
					array[0] = true;
					break;
				case "m_CareerPath":
					val2 = reader.ReadValue<BlueprintPath>();
					array[1] = true;
					break;
				case "m_FeatureSelections":
					value2 = ListFormatter.DeserializePackableJson<SelectionFeature>(ref reader);
					array[2] = true;
					break;
				case "m_StatSelections":
					value3 = ListFormatter.DeserializePackableJson<StatsEntry>(ref reader);
					array[3] = true;
					break;
				case "m_PortraitSelection":
					val3 = reader.ReadPackable<PortraitEntry>();
					array[4] = true;
					break;
				case "m_CharacterNameSelection":
					val4 = reader.ReadPackable<CharacterNameEntry>();
					array[5] = true;
					break;
				case "m_CharacterVoiceSelection":
					val5 = reader.ReadPackable<CharacterVoiceEntry>();
					array[6] = true;
					break;
				case "m_CharacterGenderSelection":
					val6 = reader.ReadPackable<CharacterGenderEntry>();
					array[7] = true;
					break;
				case "m_CharacterDollSelection":
					val7 = reader.ReadPackable<CharacterDollEntry>();
					array[8] = true;
					break;
				}
			}
			else
			{
				switch (text)
				{
				case "m_UnitRef":
					reader.ReadPackable(ref val);
					break;
				case "m_CareerPath":
					reader.ReadValue(ref val2);
					break;
				case "m_FeatureSelections":
					ListFormatter.DeserializePackableJson(ref reader, ref value2);
					break;
				case "m_StatSelections":
					ListFormatter.DeserializePackableJson(ref reader, ref value3);
					break;
				case "m_PortraitSelection":
					reader.ReadPackable(ref val3);
					break;
				case "m_CharacterNameSelection":
					reader.ReadPackable(ref val4);
					break;
				case "m_CharacterVoiceSelection":
					reader.ReadPackable(ref val5);
					break;
				case "m_CharacterGenderSelection":
					reader.ReadPackable(ref val6);
					break;
				case "m_CharacterDollSelection":
					reader.ReadPackable(ref val7);
					break;
				}
			}
		}
		if (value != null)
		{
			value.m_UnitRef = val;
			value.m_CareerPath = val2;
			value.m_FeatureSelections = value2;
			value.m_StatSelections = value3;
			value.m_PortraitSelection = val3;
			value.m_CharacterNameSelection = val4;
			value.m_CharacterVoiceSelection = val5;
			value.m_CharacterGenderSelection = val6;
			value.m_CharacterDollSelection = val7;
		}
		else
		{
			value = new CommitLevelUpGameCommand
			{
				m_UnitRef = val,
				m_CareerPath = val2,
				m_FeatureSelections = value2,
				m_StatSelections = value3,
				m_PortraitSelection = val3,
				m_CharacterNameSelection = val4,
				m_CharacterVoiceSelection = val5,
				m_CharacterGenderSelection = val6,
				m_CharacterDollSelection = val7
			};
		}
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CommitLevelUpGameCommand source = new CommitLevelUpGameCommand();
		result = Unsafe.As<CommitLevelUpGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<CommitLevelUpGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_UnitRef", ref m_UnitRef, state);
		formatter.Field(1, "m_CareerPath", ref m_CareerPath, state);
		formatter.Field(2, "m_FeatureSelections", ref m_FeatureSelections, state);
		formatter.Field(3, "m_StatSelections", ref m_StatSelections, state);
		formatter.Field(4, "m_PortraitSelection", ref m_PortraitSelection, state);
		formatter.Field(5, "m_CharacterNameSelection", ref m_CharacterNameSelection, state);
		formatter.Field(6, "m_CharacterVoiceSelection", ref m_CharacterVoiceSelection, state);
		formatter.Field(7, "m_CharacterGenderSelection", ref m_CharacterGenderSelection, state);
		formatter.Field(8, "m_CharacterDollSelection", ref m_CharacterDollSelection, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CommitLevelUpGameCommand>();
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
				m_UnitRef = formatter.ReadPackable<UnitReference>(state);
				break;
			case 1:
				m_CareerPath = formatter.ReadPackable<BlueprintPath>(state);
				break;
			case 2:
				m_FeatureSelections = formatter.ReadPackable<List<SelectionFeature>>(state);
				break;
			case 3:
				m_StatSelections = formatter.ReadPackable<List<StatsEntry>>(state);
				break;
			case 4:
				m_PortraitSelection = formatter.ReadPackable<PortraitEntry>(state);
				break;
			case 5:
				m_CharacterNameSelection = formatter.ReadPackable<CharacterNameEntry>(state);
				break;
			case 6:
				m_CharacterVoiceSelection = formatter.ReadPackable<CharacterVoiceEntry>(state);
				break;
			case 7:
				m_CharacterGenderSelection = formatter.ReadPackable<CharacterGenderEntry>(state);
				break;
			case 8:
				m_CharacterDollSelection = formatter.ReadPackable<CharacterDollEntry>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
