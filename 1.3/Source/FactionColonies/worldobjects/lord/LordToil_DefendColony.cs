﻿using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace FactionColonies
{
    public class LordToil_DefendColony : LordToil
    {
        public override bool ForceHighStoryDanger => true;

        public override bool AllowSatisfyLongNeeds => false;

        public override void UpdateAllDuties()
        {
            foreach (Pawn pawn in lord.ownedPawns.Where(pawn => pawn.mindState?.duty?.def.defName != "DefendColony"))
            {
                pawn.mindState.duty = new PawnDuty(DefDatabase<DutyDef>.GetNamed("DefendColony"),
                    pawn.Position);
                pawn.mindState.canFleeIndividual = false;
                if (pawn.equipment?.Primary == null || pawn.equipment.Primary.def.IsMeleeWeapon)
                {
                    pawn.jobs.StartJob(new Job(JobDefOf.AttackMelee), JobCondition.InterruptForced);
                }
                else
                {
                    pawn.jobs.StartJob(new Job(JobDefOf.AttackStatic), JobCondition.InterruptForced);
                }
                
                if (pawn.jobs.curJob != null)
                {
                    pawn.jobs.curJob.failIfCantJoinOrCreateCaravan = true;
                }
            }
        }
    }
    
    [HarmonyPatch(typeof(JobDriver_Goto), "TryExitMap")]
    public class Patch
    {
        static bool Prefix(ref JobDriver_Goto __instance)
        {
            Pawn pawn = __instance.pawn;
            WorldSettlementFC settlementFc = pawn.Map.Parent as WorldSettlementFC;
            return !settlementFc.defenders.Contains(pawn) || settlementFc.supporting.Any(caravan => caravan.pawns.Contains(pawn));
        }
    }
}