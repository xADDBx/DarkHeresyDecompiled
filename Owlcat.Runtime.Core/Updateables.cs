using System;
using System.Collections.Generic;
using Owlcat.Runtime.Core.Updatables;

public static class Updateables
{
	public static readonly List<Type> IUpdateables = new List<Type>
	{
		typeof(UpdateableBehaviour),
		typeof(UpdateableInEditorBehaviour)
	};

	public static readonly List<Type> ILateUpdateables = new List<Type> { typeof(LateUpdateableBehaviour) };
}
