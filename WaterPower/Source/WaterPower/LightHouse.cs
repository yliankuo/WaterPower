using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse.Sound;
using UnityEngine;
using Verse;
using RimWorld;
namespace WaterPower
{
	public class PlaceWorker_LightHouse : PlaceWorker
	{
		private bool checkBelow(IntVec3 loc, Rot4 rot)
		{
			loc = new IntVec3(loc.x - 1, 0, loc.z - 6);
			switch (Convert.ToInt32(rot.ToString()))
			{
				case 0:
					break;
			}

			IntVec3 tloc;
			for (int i = loc.x; i <= loc.x + 3; i++)
			{
				for (int j = loc.z; j <= loc.z + 11; j++)
				{
					tloc = new IntVec3(i, 0, j);
					if (base.Map.terrainGrid.TerrainAt(tloc) != TerrainDef.Named("WaterOceanShallow")&& base.Map.terrainGrid.TerrainAt(tloc) != TerrainDef.Named("WaterOceanDeep"))
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
	public class Building_LightHouse : Building
	{
		private bool destroyedFlag = false;
		protected CompPowerTrader powerComp;
		public static Graphic[] graphic = null;
		private int activeGraphicFrame = 0;
		private int ticksSinceUpdateGraphic;
		private const int arraySize = 5; // Turn animation off => set to 1
		private string graphicPathAdditionWoNumber = "_frame";
		private int updateAnimationEveryXTicks = 500;
		private CompGlower glowerComp;
		Map map;
		private int updateWeatherEveryXTicks = 25;
		private int ticksSinceUpdateWeather;
		bool disableAnimation = false;
		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			this.map = map;
			powerComp = base.GetComp<CompPowerTrader>();
			powerComp.PowerOn = true;
			glowerComp = base.GetComp<CompGlower>();
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

		private bool checkBelow(IntVec3 BotL, IntVec3 TopR)
		{
			bool ocean = true;
			IntVec3 c;
			TerrainDef terrainDef;
			for (int i = BotL.x; i <= TopR.x; i++)
			{
				for (int j = BotL.z; j <= TopR.z; j++)
				{
					c = new IntVec3(i, 0, j);
					terrainDef = base.Map.terrainGrid.TerrainAt(c);
					if (terrainDef != TerrainDef.Named("WaterOceanShallow")&& terrainDef != TerrainDef.Named("WaterOceanDeep"))
					{
						ocean = false;
						break;
					}
				}
			}
			return ocean;

		}


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
			}
			ticksSinceUpdateWeather += ticks;
			if (ticksSinceUpdateWeather >= updateWeatherEveryXTicks)
			{
				bool ocean = false;
				ticksSinceUpdateWeather = 0;

				if (checkBelow(this.OccupiedRect().BottomLeft, this.OccupiedRect().TopRight))
				{
					ocean = true;
					disableAnimation = false;
				if (powerComp.PowerNet == null || !powerComp.PowerNet.hasPowerSource)
					{
						disableAnimation = true;
						powerComp.PowerOn = false;
					}
					else {
						disableAnimation = false;
						powerComp.PowerOn = true;
					}
				}
				if (!ocean)
				{
					disableAnimation = true;
				}

			}
		}

		public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
		{
			// block further ticker work
			destroyedFlag = true;

			base.Destroy(mode);
		}

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
	}

}
