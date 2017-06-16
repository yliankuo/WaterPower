using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse.Sound;
using UnityEngine;
using Verse;
using RimWorld;
using System.Reflection;
namespace WaterTurbine
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

					loc = new IntVec3(loc.x - 1, 0, loc.z-1);
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
					if (base.Map.terrainGrid.TerrainAt(tloc) != TerrainDef.Named("WaterMovingDeep"))
						return false;
				}
			}
			return true;
		}
		public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Thing thingToIgnore = null)
		{
			return checkBelow(loc, rot);
		}
	}
	[StaticConstructorOnStartup]
	public class Building_WaterTurbine : Building
	{
		private bool destroyedFlag = false;
		private bool disableAnimation = false;	
		private bool disablePowerRandomness = false;
		private int updateWeatherEveryXTicks = 500;
		private int ticksSinceUpdateWeather;
		protected CompPowerTrader powerComp;
		public static Graphic[] graphic = null;
		private int activeGraphicFrame = 0;
		private int ticksSinceUpdateGraphic;
		private const int arraySize = 3; // Turn animation off => set to 1
		private string graphicPathAdditionWoNumber = "_frame";
		private int updateAnimationEveryXTicks = 5;
		Map map;
		Sustainer sustainer;
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<int>(ref ticksSinceUpdateWeather, "updateCounter");
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			this.map = map;

			powerComp = base.GetComp<CompPowerTrader>();
			powerComp.PowerOn = true;
			sustainer = (Sustainer)typeof(Building).GetField("sustainerAmbient", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);
			SoundInfo info = SoundInfo.InMap(this, MaintenanceType.None);
			sustainer = new Sustainer(this.def.building.soundAmbient, info);
			if (!checkBelow(this.OccupiedRect().BottomLeft, this.OccupiedRect().TopRight))
			{
				powerComp.PowerOutput = 0;
				powerComp.PowerOn = false;
				disableAnimation = true;
				sustainer.End();
			}

		}

		private void UpdateGraphics()
		{
			// Check if graphic is already filled
			if (graphic != null && graphic.Length > 0 && graphic[0] != null)
				return;

			// resize the graphic array
			graphic = new Graphic_Single[arraySize];

			// Get the base path (without _frameXX)
			int indexOf_frame = def.graphicData.texPath.ToLower().LastIndexOf(graphicPathAdditionWoNumber);
			string graphicRealPathBase = def.graphicData.texPath.Remove(indexOf_frame);

			// fill the graphic array
			for (int i = 0; i < arraySize; i++)
			{
				string graphicRealPath = graphicRealPathBase + graphicPathAdditionWoNumber + (i + 1).ToString();

				// Set the graphic, use additional info from the xml data
				graphic[i] = GraphicDatabase.Get<Graphic_Single>(graphicRealPath, def.graphic.Shader, def.graphic.drawSize, def.graphic.Color, def.graphic.ColorTwo);
			}
		}

		#region Area Check
		private bool checkBelow(IntVec3 BotL, IntVec3 TopR)
		{
			bool deep = true;
			IntVec3 c;
			TerrainDef terrainDef;
			for (int i = BotL.x; i <= TopR.x; i++)
			{
				for (int j = BotL.z; j <= TopR.z; j++)
				{
					c = new IntVec3(i, 0, j);
					terrainDef = base.Map.terrainGrid.TerrainAt(c);
					if (terrainDef != TerrainDef.Named("WaterMovingDeep"))
					{
						deep = false;
						break;
					}
				}
			}
			return deep;

		}
		#endregion

		#region Destroy
		public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
		{
			base.Destroy(mode);
			destroyedFlag = true;
			sustainer.End();
		}
		#endregion

		#region Ticker
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
					bool deep = false;
					ticksSinceUpdateWeather = 0;
					if (checkBelow(this.OccupiedRect().BottomLeft, this.OccupiedRect().TopRight))
					{
						deep = true;
						powerComp.PowerOutput = (this.map.weatherManager.RainRate * 500 + 2000);
						if (!disablePowerRandomness)
							powerComp.PowerOutput += Rand.RangeInclusive(-750, 500);

					}

					if (!deep)
					{
						powerComp.PowerOutput = 0;
						powerComp.PowerOn = false;
						disableAnimation = true;
						sustainer.End();
					}

				}

			}
		}
		#endregion
		#region Graphics
		public override Graphic Graphic
		{
			get
			{
				if (disableAnimation)
					return base.Graphic;

				if (graphic == null || graphic[0] == null)
				{
					UpdateGraphics();
					// Graphic couldn't be loaded? (Happends after load for a while)
					if (graphic == null || graphic[0] == null)
						return base.Graphic;
				}

				if (graphic[activeGraphicFrame] != null)
					return graphic[activeGraphicFrame];

				return base.Graphic;
			}
		}
		#endregion


	}
}
