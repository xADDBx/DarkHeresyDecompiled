using System.Collections.Generic;
using Kingmaker.Blueprints.Area;
using Kingmaker.Code.Framework.VO;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using UnityEngine;

namespace Kingmaker.EntitySystem.Interfaces;

public interface IMapObjectView : IMechanicEntityView, IEntityView
{
	FactHolder FactHolder { get; }

	AbstractInteractionPart InteractionComponent { get; }

	bool NeedsVoiceOver { get; }

	VoIdField VoId { get; }

	List<Renderer> Renderers { get; }

	bool Highlighted { get; }

	new MapObjectEntity Data { get; }

	float FogOfWarFudgeRadius { get; set; }

	bool OnlySilentHighlighting { get; }

	void OnEntityNoticed(BaseUnitEntity character);

	void MarkHighlightedAndNoticed();

	void UpdateHighlight();

	void SetGlobalHighlight(bool value);

	void ForceHighlightExternal(bool value);

	bool IsOwnerOf(EntityFactComponent component);

	T GetComponent<T>();

	T GetComponentInChildren<T>();

	T[] GetComponents<T>();
}
