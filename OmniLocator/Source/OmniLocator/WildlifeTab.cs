using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;

namespace Lincore.OmniLocator {

    [StaticConstructorOnStartup]
    public class MainTabWindow_Wildlife : MainTabWindow_PawnTable {
        [DefOf]
        public static class PawnTableDefs {
            public static PawnTableDef Wildlife;
        }

        public const float MIN_RETALIATION_CHANCE_ON_HUNT = 0.2f;
        public const float MIN_RETALIATION_CHANCE_ON_TAME = 0.02f;
  
        public const float ICON_SIZE = 24f;
        public const float CELL_SPACING = 5f;        
        public const float COL_GENDER_X = 165f + ICON_SIZE + CELL_SPACING;
        public const float COL_WATCH_X = COL_GENDER_X + ICON_SIZE + CELL_SPACING;
        public const float COL_WARNING_X = COL_WATCH_X + ICON_SIZE + CELL_SPACING;
        public const float COL_HUNT_X = COL_WARNING_X + ICON_SIZE + CELL_SPACING;
        public const float COL_TAME_X = COL_HUNT_X + ICON_SIZE + CELL_SPACING;
        public const float COL_HEALTH_X = COL_TAME_X + ICON_SIZE + CELL_SPACING + 10f;
        public const float COL_INFO_X = 580 - ICON_SIZE - CELL_SPACING;
        public const float HEADER_HEIGHT = 35f;
        public const float VERTICAL_MARGIN = 12f;
        public const float UPDATE_INTERVAL = 5f;




        protected override IEnumerable<Pawn> Pawns {
            get {
              return from p in Find.VisibleMap.mapPawns.AllPawnsSpawned
                     where p.RaceProps.Animal &&
                           !Find.VisibleMap.fogGrid.IsFogged(p.Position) &&
                           p.Faction == null || p.Faction == Faction.OfInsects &&
                           (p.mindState.Active || p.Dead)
                     orderby (p.Name != null && !p.Name.Numerical) ? p.Name.ToStringShort : p.Label
                     select p;
            }
        }


        protected override PawnTableDef PawnTableDef {
            get {
                return PawnTableDefs.Wildlife;
            }
        }

        /*
        public override void DoWindowContents(Rect inRect) {
            base.DoWindowContents(inRect);

            if (Time.time - lastUpdate >= UPDATE_INTERVAL) {
                // pawns are filtered by criteria that do not cause the list to be marked as dirty when changed
                // e.g. if a mega spider was revealed it would not appear in the list unless it is manually repopulated.
                Notify_PawnsChanged();                
            }

            if (PawnsCount == 0) {
                Widgets.Label(inRect, "Whichever animal did not succumb to these harsh conditions is long gone.");
                return;
            }
                        
            Rect headerBounds = new Rect(0f, 0f, inRect.width, HEADER_HEIGHT);
            DrawTableHeader(headerBounds);

            Rect listBounds = new Rect(0f, HEADER_HEIGHT, inRect.width, inRect.height - HEADER_HEIGHT);
            DrawRows(listBounds);
        }

        protected override void DrawPawnRow(Rect bounds, Pawn p) {
            GUI.BeginGroup(bounds);

            var rect = new Rect(COL_GENDER_X, bounds.height - ICON_SIZE, ICON_SIZE, ICON_SIZE);

            // gender
            DrawIcon(rect, p.gender.GetIcon(), p.gender.GetLabel().CapitalizeFirst());

            // watch
            rect.x = COL_WATCH_X;
            if (Widgets.ButtonInvisible(rect, false)) JumpToTargetUtility.TryJump(p);
            var icon = Mouse.IsOver(rect) ? WatchHoverIcon : WatchIcon;
            var tooltip = p.GetTooltip();
            tooltip.text = "Click jump to animal while keeping this tab open:\n\n" + tooltip.text;
            DrawIcon(rect, icon, tooltip);

            // manhunter/insectoid?
            var state = p.mindState.mentalStateHandler.CurStateDef;
            bool warningSet = false;
            if (state != null && state.IsAggro) {
                rect.x = COL_WARNING_X;
                DrawIcon(rect, ManhunterIcon, "Manhunter");
                warningSet = true;
            } else if (p.Faction == Faction.OfInsects) {
                rect.x = COL_WARNING_X;
                DrawIcon(rect, InsectIcon, "Infestation");
                warningSet = true;
            }
            
            // designations:
            var huntDesignation = Find.VisibleMap.designationManager.DesignationOn(p, DesignationDefOf.Hunt);
            var tameDesignation = Find.VisibleMap.designationManager.DesignationOn(p, DesignationDefOf.Tame);
            var doHunt = huntDesignation != null;
            var doTame = tameDesignation != null;


            // hunted?
            rect.x = COL_HUNT_X;
            if (DrawCheckbox(rect, ref doHunt, "DesignatorHuntDesc".Translate())) {
                if (doHunt) {
                    huntDesignation = new Designation(p, DesignationDefOf.Hunt);
                    Find.VisibleMap.designationManager.AddDesignation(huntDesignation);
                    if (doTame) Find.VisibleMap.designationManager.RemoveDesignation(tameDesignation);
                    if (p.RaceProps.manhunterOnDamageChance > MIN_RETALIATION_CHANCE_ON_HUNT) {
                        Verse.Sound.SoundStarter.PlayOneShotOnCamera(SoundDefOf.MessageAlert);
                    }
                } else {
                    Find.VisibleMap.designationManager.RemoveDesignation(huntDesignation);
                    huntDesignation = null;
                }
            }

            // taming?
            rect.x = COL_TAME_X;
            if (DrawCheckbox(rect, ref doTame, "DesignatorTameDesc".Translate())) {
                if (doTame) {
                    tameDesignation = new Designation(p, DesignationDefOf.Tame);
                    Find.VisibleMap.designationManager.AddDesignation(tameDesignation);
                    if (doHunt) Find.VisibleMap.designationManager.RemoveDesignation(huntDesignation);
                    if (p.RaceProps.manhunterOnTameFailChance > MIN_RETALIATION_CHANCE_ON_TAME) {
                        Verse.Sound.SoundStarter.PlayOneShotOnCamera(SoundDefOf.MessageAlert);
                    }
                }
                else {
                    Find.VisibleMap.designationManager.RemoveDesignation(tameDesignation);
                }
            }

            // retaliation warning:
            rect.x = COL_WARNING_X;
            if (!warningSet && huntDesignation != null && p.RaceProps.manhunterOnDamageChance > MIN_RETALIATION_CHANCE_ON_HUNT) {
                tooltip = "MessageAnimalsGoPsychoHunted".Translate(p.kindDef.label);
                DrawIcon(rect, PredatorIcon, tooltip);
            } else if (!warningSet && tameDesignation != null && p.RaceProps.manhunterOnTameFailChance > MIN_RETALIATION_CHANCE_ON_TAME) {
                tooltip = "MessageAnimalManhuntsOnTameFailed".Translate(p.kindDef.label,
                          p.kindDef.RaceProps.manhunterOnTameFailChance.ToStringPercent("F2"));
                DrawIcon(rect, PredatorIcon, tooltip);
            }


            // medical conditions:
            var hediffs = p.health.hediffSet.hediffs
                .Where(h => !h.IsOld())                
                .Select(h => h.def.LabelCap)
                .Distinct();
            var c = hediffs.Count();
            if (c > 0) {
                var str = c == 1? hediffs.First() : hediffs.Aggregate((s1, s2) => s1 + ", " + s2);
                rect.x = COL_HEALTH_X;
                rect.width = COL_INFO_X - rect.x;
                GUI.Label(rect, str);
            }

            // info button
            Widgets.InfoCardButton(COL_INFO_X, rect.y, p);

            GUI.EndGroup();
        }

        protected void DrawTableHeader(Rect bounds) {
            GUI.BeginGroup(bounds);
            var rect = new Rect(COL_WARNING_X, bounds.height - ICON_SIZE, ICON_SIZE, ICON_SIZE);
            DrawIcon(rect, WarningIcon, "Danger");

            rect.x = COL_HUNT_X;
            DrawIcon(rect, HuntIcon, "DesignatorHunt".Translate());

            rect.x = COL_TAME_X;
            DrawIcon(rect, TameIcon, "DesignatorTame".Translate());

            rect.x = COL_HEALTH_X;
            rect.width = bounds.width - rect.width;
            Text.Font = GameFont.Medium;
            GUI.Label(rect, "Medical conditions");

            GUI.EndGroup();
        }


        // utility:

        
        */
    }
}
