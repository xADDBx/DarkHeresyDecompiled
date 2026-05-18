using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Items;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[JsonObject]
[OwlPackable(OwlPackableMode.Generate)]
public abstract class PartItemsCollection : EntityPart, IItemsCollection, IEnumerable<ItemEntity>, IEnumerable, IHashable, IOwlPackable<PartItemsCollection>
{
	[CanBeNull]
	private ItemsCollection m_Collection;

	[CanBeNull]
	[JsonProperty(PropertyName = "items", NullValueHandling = NullValueHandling.Ignore)]
	[OwlPackInclude]
	protected ItemsCollection CollectionConverter
	{
		get
		{
			if (!IsCollectionOwner)
			{
				return null;
			}
			return m_Collection;
		}
		set
		{
			m_Collection = value;
		}
	}

	private bool IsCollectionOwner => m_Collection?.Owner == OwnerUnit;

	[NotNull]
	public ItemsCollection Collection => m_Collection ?? throw new NullReferenceException();

	public BaseUnitEntity OwnerUnit => base.Owner as BaseUnitEntity;

	public ReadonlyList<ItemEntity> Items => Collection.Items;

	public bool ForceStackable => Collection.ForceStackable;

	public bool IsVendorTable => Collection.IsVendorTable;

	public bool IsPlayerInventory => Collection.IsPlayerInventory;

	public bool IsSharedStash => Collection.IsSharedStash;

	public bool KeepItemsWithZeroCount => Collection.KeepItemsWithZeroCount;

	public virtual bool HasLoot => Collection.HasLoot;

	protected override void OnAttach()
	{
		Setup();
	}

	protected override void OnSubscribe()
	{
		if (IsCollectionOwner)
		{
			m_Collection?.Subscribe();
		}
	}

	protected override void OnUnsubscribe()
	{
		if (IsCollectionOwner)
		{
			m_Collection?.Unsubscribe();
		}
	}

	protected override void OnPreSave()
	{
		if (IsCollectionOwner)
		{
			m_Collection?.PreSave();
		}
	}

	protected override void OnPrePostLoad()
	{
		if (m_Collection == null)
		{
			Setup();
		}
		else if (IsCollectionOwner)
		{
			m_Collection.PrePostLoad();
		}
	}

	protected override void OnPostLoad()
	{
		if (IsCollectionOwner)
		{
			m_Collection?.PostLoad();
		}
	}

	protected override void OnDetach()
	{
		if (IsCollectionOwner)
		{
			m_Collection?.Dispose();
		}
	}

	public void Setup()
	{
		ItemsCollection itemsCollection = SetupInternal(m_Collection);
		if (itemsCollection != m_Collection)
		{
			m_Collection = itemsCollection;
			OnCollectionChanged();
		}
	}

	protected override void OnApplyPostLoadFixes()
	{
		if (IsCollectionOwner)
		{
			m_Collection?.ApplyPostLoadFixes();
		}
	}

	protected abstract ItemsCollection SetupInternal([CanBeNull] ItemsCollection currentCollection);

	protected virtual void OnCollectionChanged()
	{
	}

	public bool Contains(BlueprintItem item, int count = 1)
	{
		return Collection.Contains(item, count);
	}

	public bool ContainsAny(IList<BlueprintItem> items)
	{
		return Collection.ContainsAny(items);
	}

	public ItemEntity Add(ItemEntity newItem, bool noAutoMerge = false)
	{
		return Collection.Add(newItem, noAutoMerge);
	}

	public void Add(BlueprintItem newBpItem, int count, Action<ItemEntity> callback = null, bool noAutoMerge = false, int? equipmentCR = null)
	{
		Collection.Add(newBpItem, count, callback, noAutoMerge, equipmentCR);
	}

	public ItemEntity Add(BlueprintItem newBpItem, int? equipmentCR = null)
	{
		return Collection.Add(newBpItem, equipmentCR);
	}

	public void AddStartingInventory(BlueprintUnit originalBlueprint)
	{
		if ((bool)ContextData<UnitHelper.DoNotCreateItems>.Current)
		{
			return;
		}
		using (ProfileScope.New("Add Item"))
		{
			using (ContextData<ItemsCollection.SuppressEvents>.Request())
			{
				foreach (BlueprintItem item in originalBlueprint.StartingInventory.NotNull())
				{
					Collection.Add(item);
				}
			}
		}
	}

	public ItemEntity Remove(ItemEntity item, int? count = null)
	{
		return Collection.Remove(item, count);
	}

	public void Remove(BlueprintItem bpItem, int count = 1)
	{
		Collection.Remove(bpItem, count);
	}

	public void RemoveAll()
	{
		Collection.RemoveAll();
	}

	public ItemEntity Transfer(ItemEntity item, int count, ItemsCollection to)
	{
		return Collection.Transfer(item, count, to);
	}

	public ItemEntity TransferWithoutMerge(ItemEntity item, int count, ItemsCollection to)
	{
		return Collection.TransferWithoutMerge(item, count, to);
	}

	public ItemEntity Transfer(ItemEntity item, ItemsCollection to)
	{
		return Collection.Transfer(item, to);
	}

	public IEnumerator<ItemEntity> GetEnumerator()
	{
		return Collection.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return Collection.GetEnumerator();
	}

	public ItemEntity Transfer(ItemEntity item, int count, PartInventory to)
	{
		return Collection.Transfer(item, count, to.Collection);
	}

	public ItemEntity TransferWithoutMerge(ItemEntity item, int count, PartInventory to)
	{
		return Collection.TransferWithoutMerge(item, count, to.Collection);
	}

	public ItemEntity Transfer(ItemEntity item, PartInventory to)
	{
		return Collection.Transfer(item, to.Collection);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Hash128 val2 = ClassHasher<ItemsCollection>.GetHash128(CollectionConverter);
		result.Append(ref val2);
		return result;
	}
}
