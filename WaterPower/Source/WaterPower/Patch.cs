using System;
using Harmony;
using Verse;
using System.Reflection;
using UnityEngine;
using RimWorld;
namespace WaterPower
{
	[StaticConstructorOnStartup]
	static class HarmonyPatches
	{
		static HarmonyPatches()
		{
			HarmonyInstance harmony = HarmonyInstance.Create("rimworld.swenzi.waterpower");
			MethodInfo targetmethod = AccessTools.Method(typeof(RimWorld.StatDefOf), "StatDefOf");	
			HarmonyMethod prefixmethod = new HarmonyMethod(typeof(WaterPower.HarmonyPatches).GetMethod("StatDefOf_Prefix"));
			harmony.Patch(targetmethod, prefixmethod, null);

			MethodInfo targetmethod2 = AccessTools.Method(typeof(RimWorld.StatDefOf), "StatDefOf");


		}
		public static void StatDefOf_Prefix()
		{
			StatDef PowerGenFactor;
		}
	}
	              
}
