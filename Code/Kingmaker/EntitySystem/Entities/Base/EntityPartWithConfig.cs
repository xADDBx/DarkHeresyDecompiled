using Kingmaker.EntitySystem.Interfaces;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.EntitySystem.Entities.Base;

[OwlPackable(OwlPackableMode.Generate)]
public abstract class EntityPartWithConfig : EntityPart<Entity>, IHashable, IOwlPackable<EntityPartWithConfig>
{
	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	public string SourceType { get; protected set; }

	public IEntityPartConfig Source { get; private set; }

	public IEntityView View => base.Owner.View;

	public virtual bool ShouldCheckSourceComponent => true;

	public virtual void SetConfig(IEntityPartConfig source)
	{
		Source = source;
		SourceType = source.GetType().Name;
	}

	public bool IsFromSource(IEntityPartConfig source)
	{
		return SourceType == source.GetType().Name;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(SourceType);
		return result;
	}
}
[OwlPackable(OwlPackableMode.Generate)]
public abstract class EntityPartWithConfig<TSettings> : EntityPartWithConfig, IHashable, IOwlPackable<EntityPartWithConfig<TSettings>> where TSettings : class, new()
{
	public TSettings Settings { get; private set; } = new TSettings();


	public override void SetConfig(IEntityPartConfig source)
	{
		IEntityPartConfig source2 = base.Source;
		base.SetConfig(source);
		Settings = source.GetSettings() as TSettings;
		OnConfigDidSet(source2 != source);
	}

	protected virtual void OnConfigDidSet(bool isNewConfig)
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
