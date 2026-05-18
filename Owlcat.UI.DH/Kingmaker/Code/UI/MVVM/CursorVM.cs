using System;
using Kingmaker.Blueprints.Root;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Pointer;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

[Serializable]
public class CursorVM : ViewModel, ICursorControllerHandler, ISubscriber
{
	public SerializableReactiveProperty<bool> Enabled = new SerializableReactiveProperty<bool>(value: true);

	public SerializableReactiveProperty<CursorType> Type = new SerializableReactiveProperty<CursorType>();

	public SerializableReactiveProperty<Sprite> Icon = new SerializableReactiveProperty<Sprite>();

	public SerializableReactiveProperty<bool> Software = new SerializableReactiveProperty<bool>();

	public SerializableReactiveProperty<float> Scale = new SerializableReactiveProperty<float>(1f);

	public SerializableReactiveProperty<string> TextUpper = new SerializableReactiveProperty<string>();

	public SerializableReactiveProperty<string> TextLower = new SerializableReactiveProperty<string>();

	public SerializableReactiveProperty<bool> HasNoMove = new SerializableReactiveProperty<bool>();

	public CursorVM(CursorController controller)
	{
		EventBus.Subscribe(this).AddTo(this);
		Enabled.Value = controller.IsCursorActive;
	}

	void ICursorControllerHandler.HandleActiveChanged(bool value)
	{
		Enabled.Value = value;
	}

	void ICursorControllerHandler.HandleTypeChanged(CursorType type)
	{
		Type.Value = type;
		Icon.Value = null;
	}

	void ICursorControllerHandler.HandleTypeChanged(CursorType type, Sprite icon)
	{
		Type.Value = type;
		Icon.Value = icon;
	}

	void ICursorControllerHandler.HandleTextsChanged(string upper, string lower)
	{
		TextUpper.Value = upper;
		TextLower.Value = lower;
	}

	void ICursorControllerHandler.HandleNoMoveChanged(bool value)
	{
		HasNoMove.Value = value;
	}
}
