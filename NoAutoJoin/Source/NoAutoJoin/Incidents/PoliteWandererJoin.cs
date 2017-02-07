using System;
using System.Linq;
using System.Collections.Generic;
using Verse;
using RimWorld;

namespace NoAutoJoin {
    public class IncidentWorker_PoliteWandererJoin : IncidentWorker {

        public override bool TryExecute(IncidentParms parms) {
            // can we spawn a pawn?
            var map = (Map)parms.target;
            IntVec3 loc;
            if (!CellFinder.TryFindRandomEdgeCellWith((IntVec3 cell) => 
                map.reachability.CanReachColony(cell), map, out loc)) return false;

            // if yes, generate the pawn
            var pawnKindDef = new List<PawnKindDef> {PawnKindDefOf.Villager}.RandomElement();
            var request = new PawnGenerationRequest(
                pawnKindDef, 
                Faction.OfPlayer, 
                PawnGenerationContext.NonPlayer, 
                null,  // Map map
                false, // bool forceGenerateNewPawn 
                false, // bool newborn
                false, // bool allowDead
                false, // bool allowDowned
                true,  // bool canGeneratePawnRelations
                false, // bool mustBeCapableOfViolence
                GetColonistRelationProbability(),   // float colonistRelationChangeFactor
                false, // bool forceAddFreeWarmLayerIfNeeded
                true,  // bool allowGay
                true,  // bool allowFood
                null,  // Predicate<Pawn> validator
                null,  // float? fixedBiologicalAge
                null,  // float? fixedChronologicalAge
                null,  // Gender? fixedGender
                null,  // float? fixedMelanin
                null); // string fixedLastName
            Pawn wanderer = PawnGenerator.GeneratePawn(request);

            // instead of simply spawning the pawn, ask the player:
            ShowDialog(wanderer, () => {
                GenSpawn.Spawn(wanderer, loc, map);
                wanderer.SetFaction(Faction.OfPlayer);
                Find.CameraDriver.JumpTo(loc);
            });
            return true;
        }

        private static void ShowDialog(Pawn wanderer, Action accept) {
            string text = "PoliteWandererJoin".Translate(wanderer.story.Title.ToLower(), wanderer.ageTracker.AgeBiologicalYears);
            text = text.AdjustedFor(wanderer);
            PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, wanderer);
            var dlg = new DiaNode(text);
            var acceptOption = new DiaOption("PoliteWandererJoinAccept".Translate());
            var rejectOption = new DiaOption("PoliteWandererJoinReject".Translate());
            acceptOption.action = accept;
            acceptOption.resolveTree = true;
            rejectOption.resolveTree = true;
            dlg.options.Add(acceptOption);
            dlg.options.Add(rejectOption);
            Find.WindowStack.Add(new Dialog_NodeTree(dlg, true));
        }

       /* private static void GetCharacterSkillDescription(Pawn p) {
            var mostSkilled = p.skills.skills.Aggregate((skillA, skillB) => skillA.Level > skillB.Level ? skillA : skillB);
            var mostPassion = p.skills.skills.Aggregate((skillA, skillB) => {
                var passionA = (int)skillA.passion;
                var passionB = (int)skillB.passion;
                if (passionA > passionB) return skillA;
                if (passionB > passionA) return skillB;
                return skillA.Level > skillB.Level ? skillA : skillB;
            });
            
        }*/

        private float GetColonistRelationProbability() {
            var baseProbability = PawnsFinder.AllMaps_FreeColonists
                .Where(p => p.relations.everSeenByPlayer)
                .Select(p => {
                    var bioage = p.ageTracker.AgeBiologicalYearsFloat;
                    var chronoage = p.ageTracker.AgeChronologicalYearsFloat;
                    if (chronoage == 0f) chronoage = 1f;
                    return Math.Max(0.2f, bioage / chronoage);
                 })
                .Sum();
            var techlevel = Faction.OfPlayer.def.techLevel;
            float techFactor = 1f;
            if (techlevel == TechLevel.Industrial) techFactor = 2f;
            if (techlevel == TechLevel.Neolithic || 
                techlevel == TechLevel.Animal || 
                techlevel == TechLevel.Medieval) techFactor = 5f;

            var ans = Math.Max(1f, Math.Min(baseProbability * techFactor, 50f));
            Log.Message("relation probability: " + ans);
            return ans;
        }
    }
}
