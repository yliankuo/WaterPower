using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
namespace WaterDamBase
{
	public class Building_WaterDamBase : Building
	{
		private bool disablePowerRandomness = false;
		private int updateWeatherEveryXTicks = 500;
		private int ticksSinceUpdateWeather;
		private int modifier;
		protected CompPowerTrader powerComp;
	}
}
