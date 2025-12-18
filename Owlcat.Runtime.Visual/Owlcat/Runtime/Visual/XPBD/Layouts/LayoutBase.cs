using Owlcat.Runtime.Visual.XPBD.Bodies;
using UnityEngine;

namespace Owlcat.Runtime.Visual.XPBD.Layouts;

public abstract class LayoutBase : ScriptableObject
{
	public BodyStructure BodyStructure = new BodyStructure();
}
