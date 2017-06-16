
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using System;
namespace WaterDamGen
{
	public class Building_WaterDamGen : Building
	{
		private bool disablePowerRandomness = false;
		private int updateWeatherEveryXTicks = 500;
		private int ticksSinceUpdateWeather;
		private int modifier;
		protected CompPowerTrader powerComp;
		Map map;
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<int>(ref ticksSinceUpdateWeather, "updateCounter");
		}

	}

}
