using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Debugging;

public class OwlcatObjectField : DebugUI.Field<UnityEngine.Object>
{
	public Type type = typeof(UnityEngine.Object);
}
