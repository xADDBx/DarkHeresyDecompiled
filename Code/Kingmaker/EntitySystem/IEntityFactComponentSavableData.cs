using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.EntitySystem;

[HashRoot]
[OwlPackable(OwlPackableMode.Generate)]
public abstract class IEntityFactComponentSavableData : IHashable, IOwlPackable, IOwlPackable<IEntityFactComponentSavableData>
{
	public virtual Hash128 GetHash128()
	{
		return default(Hash128);
	}

	public abstract void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter;

	public abstract void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter;
}
