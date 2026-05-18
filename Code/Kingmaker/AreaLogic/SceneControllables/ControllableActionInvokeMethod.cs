using System.Reflection;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.AreaLogic.SceneControllables;

[PlayerUpgraderAllowed(false)]
[TypeId("a7f5f66ea708d4b4b855551a2c5ac1cd")]
public class ControllableActionInvokeMethod : GameAction
{
	public ControllableReference Target;

	[Tooltip("Component type name (e.g. 'ConveyorBeltController')")]
	public string ComponentName;

	[Tooltip("Public method name to invoke (e.g. 'DeliverCrate')")]
	public string MethodName;

	[Tooltip("Optional string argument passed to the method")]
	public string Argument;

	public override string GetCaption()
	{
		return "Invoke " + ComponentName + "." + MethodName + "(" + Argument + ") on " + Target?.EntityNameInEditor;
	}

	protected override void RunAction()
	{
		if (Target == null || !Target.TryGetValue(out var controllable))
		{
			return;
		}
		Component component = (string.IsNullOrEmpty(ComponentName) ? controllable : controllable.GetComponent(ComponentName));
		if (component == null)
		{
			Debug.LogError("ControllableActionInvokeMethod: component '" + ComponentName + "' not found on '" + controllable.name + "'");
			return;
		}
		MethodInfo method = component.GetType().GetMethod(MethodName, BindingFlags.Instance | BindingFlags.Public);
		if (method == null)
		{
			Debug.LogError("ControllableActionInvokeMethod: method '" + MethodName + "' not found on '" + component.GetType().Name + "'");
			return;
		}
		ParameterInfo[] parameters = method.GetParameters();
		int result;
		if (parameters.Length == 0)
		{
			method.Invoke(component, null);
		}
		else if (parameters.Length == 1 && parameters[0].ParameterType == typeof(string))
		{
			method.Invoke(component, new object[1] { Argument });
		}
		else if (parameters.Length == 1 && parameters[0].ParameterType == typeof(int) && int.TryParse(Argument, out result))
		{
			method.Invoke(component, new object[1] { result });
		}
		else
		{
			Debug.LogError("ControllableActionInvokeMethod: method '" + MethodName + "' has unsupported parameters");
		}
	}
}
