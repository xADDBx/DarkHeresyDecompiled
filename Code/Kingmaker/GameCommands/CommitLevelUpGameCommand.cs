using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.Progression.Paths;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public class CommitLevelUpGameCommand : GameCommand, IOwlPackable<CommitLevelUpGameCommand>
{
	[JsonProperty]
	[OwlPackInclude]
	private UnitReference m_UnitRef;

	[JsonProperty]
	[OwlPackInclude]
	private BlueprintPath m_CareerPath;

	[JsonProperty]
	[OwlPackInclude]
	private List<SelectionEntry> m_Selections;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "CommitLevelUpGameCommand",
		OldNames = null,
		Fields = new FieldInfo[3]
		{
			new FieldInfo("m_UnitRef", typeof(UnitReference)),
			new FieldInfo("m_CareerPath", typeof(BlueprintPath)),
			new FieldInfo("m_Selections", typeof(List<SelectionEntry>))
		}
	};

	public override bool IsSynchronized => true;

	[JsonConstructor]
	public CommitLevelUpGameCommand()
	{
	}

	public CommitLevelUpGameCommand([NotNull] LevelUpManager levelUpManager)
	{
		m_UnitRef = levelUpManager.TargetUnit.FromBaseUnitEntity();
		m_CareerPath = levelUpManager.Path;
		m_Selections = new List<SelectionEntry>();
		foreach (SelectionState selection in levelUpManager.Selections)
		{
			if (!(selection is SelectionStateFeature { SelectionItem: var selectionItem } selectionStateFeature))
			{
				throw new ArgumentOutOfRangeException("selectionState");
			}
			BlueprintFeature blueprintFeature = selectionItem?.Feature;
			if (blueprintFeature != null)
			{
				m_Selections.Add(new SelectionEntry(selectionStateFeature.Blueprint, selectionStateFeature.PathRank, blueprintFeature));
			}
		}
	}

	protected override void ExecuteInternal()
	{
		LevelUpManager levelUpManager = new LevelUpManager(m_UnitRef.Entity.ToIBaseUnitEntity(), m_CareerPath, autoCommit: false);
		foreach (SelectionState selection2 in levelUpManager.Selections)
		{
			if (!(selection2 is SelectionStateFeature selectionStateFeature))
			{
				throw new ArgumentOutOfRangeException("selectionState");
			}
			BlueprintSelection selection = selection2.Blueprint;
			int pathRank = selection2.PathRank;
			SelectionEntry selectionEntry = m_Selections.FirstItem((SelectionEntry i) => i.Selection == selection && i.PathRank == pathRank);
			FeatureSelectionItem selectionItem = selectionStateFeature.Items.FirstItem((FeatureSelectionItem i) => i.Feature == selectionEntry?.Feature);
			if (selectionItem.Feature != null)
			{
				selectionStateFeature.Select(selectionItem);
			}
		}
		levelUpManager.Commit();
		levelUpManager.Dispose();
		EventBus.RaiseEvent(delegate(ILevelUpManagerUIHandler h)
		{
			h.HandleUICommitChanges();
		});
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
		formatter.Field(2, "m_Selections", ref m_Selections, state);
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
				m_Selections = formatter.ReadPackable<List<SelectionEntry>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
