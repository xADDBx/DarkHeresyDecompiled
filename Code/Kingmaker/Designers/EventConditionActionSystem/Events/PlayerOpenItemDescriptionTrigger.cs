using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Items;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Events;

[ComponentName("Items/PlayerOpenItemDescriptionTrigger")]
[TypeId("4eed9274a7d420c40a17f7982062b98b")]
public class PlayerOpenItemDescriptionTrigger : EntityFactComponentDelegate, IPlayerOpenItemDescriptionHandler, ISubscriber<IItemEntity>, ISubscriber
{
	[OwlPackable(OwlPackableMode.Generate)]
	public class SavableData : IEntityFactComponentSavableData, IHashable, IOwlPackable<SavableData>
	{
		[JsonProperty]
		[OwlPackInclude]
		public List<BlueprintItem> OpenedItems = new List<BlueprintItem>();

		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "SavableData",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("OpenedItems", typeof(List<BlueprintItem>))
			}
		};

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			List<BlueprintItem> openedItems = OpenedItems;
			if (openedItems != null)
			{
				for (int i = 0; i < openedItems.Count; i++)
				{
					Hash128 val2 = SimpleBlueprintHasher.GetHash128(openedItems[i]);
					result.Append(ref val2);
				}
			}
			return result;
		}

		public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
		{
			SavableData source = new SavableData();
			result = Unsafe.As<SavableData, TPossiblyBase>(ref source);
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
			ushort type = state.TypeLibrary.RegisterType<SavableData>(OwlPackTypeInfo);
			formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
			formatter.Field(0, "OpenedItems", ref OpenedItems, state);
			formatter.EndObject();
		}

		public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
		{
			state.References.Register(objectId, this);
			TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<SavableData>();
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
					OpenedItems = formatter.ReadPackable<List<BlueprintItem>>(state);
					break;
				}
			}
			formatter.LeaveObject();
		}
	}

	public bool OnlyOnce = true;

	public List<ItemToActions> Items;

	public void HandlePlayerOpenItemDescription()
	{
		if (Items == null)
		{
			return;
		}
		SavableData savableData = RequestSavableData<SavableData>();
		BlueprintItem blueprintItem = EventInvokerExtensions.GetEntity<ItemEntity>()?.Blueprint;
		if (!OnlyOnce || !savableData.OpenedItems.Contains(blueprintItem))
		{
			ItemToActions itemToActions = Items.Find((ItemToActions itoa) => itoa.Item == blueprintItem);
			if (itemToActions != null)
			{
				itemToActions.Actions.Run();
				savableData.OpenedItems.AddUnique(itemToActions.Item);
			}
		}
	}
}
