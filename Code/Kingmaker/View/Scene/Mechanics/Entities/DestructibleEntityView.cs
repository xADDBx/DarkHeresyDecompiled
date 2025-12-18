using Kingmaker.Blueprints.JsonSystem.Helpers;
using UnityEngine;

namespace Kingmaker.View.Scene.Mechanics.Entities;

[KnowledgeDatabaseID("c7a4c5efe3084d2988c6a7bbe5ef4490")]
public sealed class DestructibleEntityView : AbstractDestructibleEntityView
{
	[SerializeField]
	private bool _canBeAttackedDirectly;

	public override bool CanBeAttackedDirectly => _canBeAttackedDirectly;
}
