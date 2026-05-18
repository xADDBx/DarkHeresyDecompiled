using Kingmaker.Visual.Particles;
using Kingmaker.Visual.Sound;
using UnityEngine;

namespace Kingmaker.EntitySystem.Interfaces;

public interface IMechanicEntityView : IEntityView
{
	GameObject gameObject { get; }

	Transform transform { get; }

	ParticlesSnapMap ParticlesSnapMap { get; }

	UnitAsksManager Asks { get; }

	void SetVisible(bool visible, bool force = false, bool revealVisible = true);

	T[] GetComponentsInChildren<T>()
	{
		return gameObject.GetComponentsInChildren<T>();
	}
}
