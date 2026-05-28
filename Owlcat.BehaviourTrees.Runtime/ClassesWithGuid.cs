using System;
using System.Collections.Generic;
using Owlcat.BehaviourTrees;

public static class ClassesWithGuid
{
	public static readonly List<(Type Type, string Guid)> Classes = new List<(Type, string)>
	{
		(typeof(BehaviourTreeStickyNoteElement), "01f70d5a997f499cbbc0a07b7e4922c9"),
		(typeof(BooleanConditionNodeElement), "d048db281e244c72a18cf604a7e38fca"),
		(typeof(BooleanConditionPassNodeElement), "d0270e8a15224518aaa6155f93257717"),
		(typeof(RaiseExceptionNodeElement), "4f4052bcfaca4c498d58e33f22f9a821"),
		(typeof(RandomResultNodeElement), "4e3d9d03c860482db83dfcb07a3934ba"),
		(typeof(WriteLogNodeElement), "4a3fc5ea79d240108e9d4ef4066e0351"),
		(typeof(FloatConditionNodeElement), "a4ea3b3d541648018bdfbc6a1391cc70"),
		(typeof(FloatConditionPassNodeElement), "a5aa3531c1aa47f48affebd8f4f545ec"),
		(typeof(CooldownNodeElement), "7a65c972a57743bbaac8748b205e325f"),
		(typeof(FixedResultNodeElement), "a2c45b9a1a3748708c08497b9484bf74"),
		(typeof(InverterNodeElement), "ae3dec8d7b04406689ff476a41b8207d"),
		(typeof(LimitedEntryNodeElement), "ae1d2cbdf76a47de9f44e464dc030196"),
		(typeof(RandomSelectorNodeElement), "551f54b3997e4609bb1286eaf9f58918"),
		(typeof(RepeatNodeElement), "a731487fca98440eaaa2d2d3c17bb892"),
		(typeof(SelectorNodeElement), "62285927e8ed4275824e07b9fac671a2"),
		(typeof(SequenceNodeElement), "ae082473f1ba4e59ab6acfc5228df4b5"),
		(typeof(SubTreeNodeElement), "e3f41ce707834a83a9527f7b04e32a12"),
		(typeof(IncreaseFloatNodeElement), "7e01297125924e53a2824ee2333c7c09"),
		(typeof(IncreaseIntegerNodeElement), "e9b1f0f73e7340eeae66bb23c1c576aa"),
		(typeof(IntegerConditionNodeElement), "f8c92720a9b94f10839cf7b6b35fd291"),
		(typeof(IntegerConditionPassNodeElement), "9069eae1e4ed43a79fcb1ffee6e9ab83"),
		(typeof(RootNodeElement), "edf5230bd10145c69cbb7213b3a0995b"),
		(typeof(SetBooleanNodeElement), "ca1e4d8c28684ebe8c43cc866e43634e"),
		(typeof(SetFloatNodeElement), "6a7333f8ccc7483b907ad4b66d90ddf1"),
		(typeof(SetIntegerNodeElement), "4dcff6318d8d48ee9a9796793753dbc5"),
		(typeof(WaitNodeElement), "6be4e5402af84fe6aa3a245f5dc3266d"),
		(typeof(BooleanVariableElement), "ccb67a4a623a4a2893520942002943dd"),
		(typeof(FloatVariableElement), "15a41161dfaf434388122fa3d5c1023c"),
		(typeof(IntegerVariableElement), "a9446837cb6d46c5b99fd7e2eba5f7f4"),
		(typeof(PositionVariableElement), "13df1aad176e4c1f88097c714492ebb3"),
		(typeof(StringVariableElement), "622afce4f48e4ca9a4d95079920d5840")
	};
}
