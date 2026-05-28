using System;
using UnityEngine;

namespace Owlcat.BehaviourTrees;

public abstract class BehaviourTreeElement
{
	[HideInInspector]
	public string name;

	public string Id => name;

	public BehaviourTreeSerializableData BehaviourTree { get; set; }

	public void InitName()
	{
		name = BehaviourTreeNameGenerator.GenerateName(GetType().Name);
	}

	public static BehaviourTreeElement CreateInstance(Type t)
	{
		BehaviourTreeElement obj = (BehaviourTreeElement)Activator.CreateInstance(t);
		obj.InitName();
		return obj;
	}

	public static T CreateInstance<T>() where T : BehaviourTreeElement, new()
	{
		T val = new T();
		val.InitName();
		return val;
	}

	public virtual string GetCaption(bool useLineBreaks)
	{
		return GetCaption();
	}

	public abstract string GetCaption();
}
