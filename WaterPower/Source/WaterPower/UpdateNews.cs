using System;
using Verse;
using System.Collections.Generic;
namespace RimWorld
{
	public class IncidentWorker_WaterPowerUpdate : IncidentWorker
	{
		public override bool TryExecute(IncidentParms parms){
			Map map = (Map)parms.target;
			Find.LetterStack.ReceiveLetter("LetterLabelWaterPowerUpdate".Translate(),"WaterPowerUpdate".Translate(new object[0]), LetterDefOf.Good,null);
			Find.TickManager.slower.SignalForceNormalSpeedShort();
			return true;
		}
	}
}
