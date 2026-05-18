using Kingmaker.Networking.Serialization;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Framework.DetectiveSystem;

[HashRoot]
[OwlPackable(OwlPackableMode.Generate)]
[OwlPackOldName("Kingmaker.Framework.DetectiveSystem.DetectiveSystem+CaseItemState")]
internal abstract class CaseItemState : IHashable, IOwlPackable, IOwlPackable<CaseItemState>
{
	[OwlPackInclude]
	[GameStateInclude]
	public bool Hidden { get; set; }

	protected CaseItemState()
	{
	}

	protected CaseItemState(OwlPackConstructorParameter _)
	{
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		bool val = Hidden;
		result.Append(ref val);
		return result;
	}

	public abstract void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter;

	public abstract void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter;
}
