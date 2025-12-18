using System;
using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Utility.DotNetExtensions;
using ObservableCollections;
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
		Overtips.ForEach(delegate(TOvertipVM o)
		{
			o.Dispose();
		});
		Overtips.Clear();
	}

	protected virtual TOvertipVM GetOvertip(Entity entity)
	{
		Func<TOvertipVM, bool> pred = (TOvertipVM vm) => OvertipGetter(vm, entity);
		return Overtips.FirstOrDefault(pred);
	}

	protected virtual bool ContainsOvertip(Entity entity)
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
			overtip.Dispose();
			Overtips.Remove(overtip);
		}
	}
}
