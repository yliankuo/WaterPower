using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse.Sound;
using UnityEngine;
using Verse;
using RimWorld;
using System.Reflection;
namespace WaterPower
{
	public class PlaceWorker_WaterTurbine : PlaceWorker
	{
		private bool checkBelow(IntVec3 loc, Rot4 rot)
		{
			switch (Convert.ToInt32(rot.ToString()))
			{
				case 0:
					break;
				case 1:
					loc = new IntVec3(loc.x, 0, loc.z - 1);
					break;
				case 2:

					loc = new IntVec3(loc.x - 1, 0, loc.z - 1);
					break;
				case 3:
					loc = new IntVec3(loc.x - 1, 0, loc.z);
					break;
				default:
					Log.Error("test");
					break;
			}
			IntVec3 tloc;
			for (int i = loc.x; i <= loc.x + 1; i++)
			{
				for (int j = loc.z; j <= loc.z + 1; j++)
				{
					tloc = new IntVec3(i, 0, j);
					if (Find.VisibleMap.terrainGrid.TerrainAt(tloc) != TerrainDef.Named("WaterMovingShallow"))
						return false;
				}
			}
			return true;
		}
		public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null)
		{
			return checkBelow(loc, rot);
		}
	}
    [StaticConstructorOnStartup]
    public class Building_WaterWheel : Building
    {
        private List<IntVec3> cachedAdjCellsCardinal;
        private bool destroyedFlag = false;
        private bool disableAnimation = false;
        private bool disablePowerRandomness = false;
        private int updateWeatherEveryXTicks = 1000;
        private int ticksSinceUpdateWeather;
        protected CompPowerTrader powerComp;
        public static Graphic[] graphic = null;
        private int activeGraphicFrame = 0;
        private int ticksSinceUpdateGraphic;
        private const int arraySize = 1; // Turn animation off => set to 1
        private string graphicPathAdditionWoNumber = "_frame";
        private int updateAnimationEveryXTicks = 5;
        Map map;
        Sustainer sustainer;
        protected RecipeWorker recipeworker;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref ticksSinceUpdateWeather, "updateCounter");
        }

		private List<IntVec3> AdjCellsCardinalInBounds
		{
			get
			{
				if (this.cachedAdjCellsCardinal == null)
				{
					this.cachedAdjCellsCardinal = (from c in GenAdj.CellsAdjacentCardinal(this)
												   where c.InBounds(base.Map)
												   select c).ToList<IntVec3>();
				}
				return this.cachedAdjCellsCardinal;
			}
		}

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.map = map;

            powerComp = base.GetComp<CompPowerTrader>();
            powerComp.PowerOn = true;
            //sustainer = (Sustainer)typeof(Building).GetField("sustainerAmbient", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);
            //SoundInfo info = SoundInfo.InMap(this, MaintenanceType.None);
            //sustainer = new Sustainer(this.def.building.soundAmbient, info);
            if (!checkBelow(this.OccupiedRect().BottomLeft, this.OccupiedRect().TopRight))
            {
                powerComp.PowerOutput = 0;
                powerComp.PowerOn = false;
                disableAnimation = true;
                //sustainer.End();
            }
            //recipeworker = base.Stuff

        }

		#region Area Check
		private bool checkBelow(IntVec3 BotL, IntVec3 TopR)
		{
			bool shallow = true;
			IntVec3 c;
			TerrainDef terrainDef;
			for (int i = BotL.x; i <= TopR.x; i++)
			{
				for (int j = BotL.z; j <= TopR.z; j++)
				{
					c = new IntVec3(i, 0, j);
					terrainDef = base.Map.terrainGrid.TerrainAt(c);
					if (terrainDef != TerrainDef.Named("WaterMovingShallow"))
					{
						shallow = false;
						break;
					}
				}
			}
			return shallow;

		}
		#endregion

		public override void Tick()
		{
			if (destroyedFlag) // Do nothing further, when destroyed (just a safety)
				return;

			// Call work function
			DoTickerWork(1);
			base.TickRare();
		}

		private void DoTickerWork(int ticks)
		{
			// Power off OR Roofed Position
			if (powerComp == null || !powerComp.PowerOn)
			{
				activeGraphicFrame = 0;
				powerComp.PowerOutput = 0;
				return;
			}

			if (!disableAnimation)
			{
				ticksSinceUpdateGraphic += ticks;
				if (ticksSinceUpdateGraphic >= updateAnimationEveryXTicks)
				{
					ticksSinceUpdateGraphic = 0;
					activeGraphicFrame++;
					if (activeGraphicFrame >= arraySize)
						activeGraphicFrame = 1;

					// Tell the MapDrawer that here is something thats changed
					map.mapDrawer.MapMeshDirty(Position, MapMeshFlag.Things, true, false);
				}
				ticksSinceUpdateWeather += ticks;
				if (ticksSinceUpdateWeather >= updateWeatherEveryXTicks)
				{
					bool shallow = false;
					ticksSinceUpdateWeather = 0;
					if (checkBelow(this.OccupiedRect().BottomLeft, this.OccupiedRect().TopRight))
					{
						shallow = true;
						powerComp.PowerOutput = (this.map.weatherManager.RainRate * 500 + 1000);
						if (!disablePowerRandomness)
							powerComp.PowerOutput += Rand.RangeInclusive(-250, 500);

					}

					if (!shallow)
					{
						powerComp.PowerOutput = 0;
						powerComp.PowerOn = false;
						disableAnimation = true;
						//sustainer.End();
					}

				}


			}
		}

    }
}
