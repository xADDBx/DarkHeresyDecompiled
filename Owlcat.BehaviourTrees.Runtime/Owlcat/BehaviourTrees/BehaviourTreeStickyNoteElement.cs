using System;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Owlcat.BehaviourTrees;

[Serializable]
[TypeId("01f70d5a997f499cbbc0a07b7e4922c9")]
public class BehaviourTreeStickyNoteElement : BehaviourTreeElement
{
	public string Text;

	public Rect Rect;

	public override string GetCaption()
	{
		return Text;
	}

	public static BehaviourTreeStickyNoteElement Create(Vector2 position)
	{
		BehaviourTreeStickyNoteElement behaviourTreeStickyNoteElement = BehaviourTreeElement.CreateInstance<BehaviourTreeStickyNoteElement>();
		behaviourTreeStickyNoteElement.Text = "Your text here...";
		behaviourTreeStickyNoteElement.Rect = new Rect(position, new Vector2(250f, 110f));
		return behaviourTreeStickyNoteElement;
	}
}
