using Kingmaker.EntitySystem.Interfaces;
using UnityEngine;

namespace Kingmaker.Framework.EntitySystem.Interfaces.View;

public interface ICutscenePlayerView : IEntityView
{
	GameObject gameObject { get; }
}
