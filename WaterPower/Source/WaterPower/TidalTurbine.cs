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
	[StaticConstructorOnStartup]
	public class Building_TidalTurbine:Building
	{
		private bool destroyedFlag = false;
		public static Graphic[] graphic = null;
		private List<String> turbnames = new List<String>();
		private int activeGraphicFrame = 0;
		private int ticksSinceUpdateGraphic;
		private const int arraySize = 3; // Turn animation off => set to 1
		private string graphicPathAdditionWoNumber = "_frame";
		private int updateAnimationEveryXTicks = 5;
		Map map;
		private int updateWeatherEveryXTicks = 500;
		private int ticksSinceUpdateWeather;
		bool disableAnimation = false;
		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			this.map = map;

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
					if (terrainDef != TerrainDef.Named("WaterOceanShallow") && terrainDef != TerrainDef.Named("WaterOceanDeep"))
					{
						ocean = false;
						break;
					}
				}
			}
			return ocean;

		}
		public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
		{
			// block further ticker work
			destroyedFlag = true;

			base.Destroy(mode);
		}
		private int nearbyTurbines(IntVec3 BotL,IntVec3 TopR)
		{
			IntVec3 cell;
			int turbines = 0;
			for (int i = BotL.x - 4; i < TopR.x + 5; i++)
			{
				for (int j = BotL.z - 4; j < TopR.z + 5; j++)
				{
					cell = new IntVec3(i, 0, j);
					bCheck(cell);
				}
			}
			turbines = turbnames.Count;
			return turbines;
		}

		private void bCheck(IntVec3 cell)
		{
			Building edifice = cell.GetFirstBuilding(base.Map);
			if (edifice != null)
			{
				if (edifice.ToString().Contains("TidalPlant") && !turbnames.Contains(edifice.ToString()))
				{
					turbnames.Add(edifice.ToString());
				}
			}
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

				if (checkBelow(this.OccupiedRect().BottomLeft, this.OccupiedRect().TopRight) && nearbyTurbines(this.OccupiedRect().BottomLeft, this.OccupiedRect().TopRight) > 0)
				{
					ocean = true;
					disableAnimation = false;
					turbnames = new List<String>();
				}

				if (!ocean)
				{
					disableAnimation = true;
				}

			}
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
