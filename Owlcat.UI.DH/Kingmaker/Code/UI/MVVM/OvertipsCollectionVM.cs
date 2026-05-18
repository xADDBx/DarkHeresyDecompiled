using System;
using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Utility.DotNetExtensions;
using ObservableCollections;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public abstract class OvertipsCollectionVM<TOvertipVM> : ViewModel where TOvertipVM : OvertipEntityVM
{
	public readonly ObservableList<TOvertipVM> Overtips = new ObservableList<TOvertipVM>();

	protected virtual IEnumerable<Entity> Entities => new List<Entity>();

	protected virtual bool OvertipGetter(TOvertipVM vm, Entity entity)
	{
		return true;
	}

	protected override void OnDispose()
	{
		Clear();
	}

	protected virtual void Clear()
	{
		List<TOvertipVM> list = ListPool<TOvertipVM>.Claim();
		list.AddRange(Overtips);
		Overtips.Clear();
		list.ForEach(delegate(TOvertipVM o)
		{
			o.Dispose();
		});
		ListPool<TOvertipVM>.Release(list);
	}

	protected TOvertipVM GetOvertip(Entity entity)
	{
		Func<TOvertipVM, bool> pred = (TOvertipVM vm) => OvertipGetter(vm, entity);
		return Overtips.FirstOrDefault(pred);
	}

	protected bool ContainsOvertip(Entity entity)
	{
		Func<TOvertipVM, bool> pred = (TOvertipVM vm) => OvertipGetter(vm, entity);
		return Overtips.Contains(pred);
	}

	protected void RescanEntities()
	{
		if (Entities == null)
		{
			return;
		}
		foreach (Entity entity in Entities)
		{
			AddEntity(entity);
		}
		OnRescanEntities();
	}

	protected virtual void OnRescanEntities()
	{
	}

	protected abstract bool NeedOvertip(Entity entityData);

	protected virtual void AddEntity(Entity entityData)
	{
		if (NeedOvertip(entityData) && !ContainsOvertip(entityData))
		{
			TOvertipVM item = (TOvertipVM)Activator.CreateInstance(typeof(TOvertipVM), entityData);
			Overtips.Add(item);
		}
	}

	protected virtual void RemoveEntity(Entity entityData)
	{
		TOvertipVM overtip = GetOvertip(entityData);
		if (overtip != null)
		{
			Overtips.Remove(overtip);
			overtip.Dispose();
		}
	}
}
