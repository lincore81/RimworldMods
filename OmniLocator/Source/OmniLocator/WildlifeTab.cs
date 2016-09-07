using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;

namespace Lincore.OmniLocator {

    [StaticConstructorOnStartup]
    public class MainTabWindow_Wildlife : MainTabWindow_PawnList {
        public const float ICON_SIZE = 24f;
        public const float CELL_SPACING = 5f;
        public const float COL_GENDER_X = 165f;
        public const float COL_WATCH_X = COL_GENDER_X + ICON_SIZE + CELL_SPACING;
        public const float COL_WARNING_X = COL_WATCH_X + ICON_SIZE + CELL_SPACING;
        public const float COL_HUNT_X = COL_WARNING_X + ICON_SIZE + CELL_SPACING;
        public const float COL_TAME_X = COL_HUNT_X + ICON_SIZE + CELL_SPACING;
        public const float COL_HEALTH_X = COL_TAME_X + ICON_SIZE + CELL_SPACING + 10f;
        public const float HEADER_HEIGHT = 35f;
        public const float UPDATE_INTERVAL = 5f;

        public static readonly Texture2D HuntIcon = ContentFinder<Texture2D>.Get("UI/Icons/Hunt");
        public static readonly Texture2D TameIcon = ContentFinder<Texture2D>.Get("UI/Icons/Tame");
        public static readonly Texture2D WatchIcon = ContentFinder<Texture2D>.Get("UI/Icons/Eye");
        public static readonly Texture2D WatchHoverIcon = ContentFinder<Texture2D>.Get("UI/Icons/Eye-Hover");
        public static readonly Texture2D WarningIcon = ContentFinder<Texture2D>.Get("UI/Icons/Warning");
        public static readonly Texture2D ManhunterIcon = ContentFinder<Texture2D>.Get("UI/Icons/Manhunter");
        public static readonly Texture2D InsectIcon = ContentFinder<Texture2D>.Get("UI/Icons/Insect");



        protected float lastUpdate = -1f;

        
        protected override void BuildPawnList() {
            pawns = (from p in Find.MapPawns.AllPawnsSpawned
                     where p.RaceProps.Animal &&                 
                           !Find.FogGrid.IsFogged(p.Position) &&
                           p.Faction == null || p.Faction == Faction.OfInsects &&
                           (p.mindState.Active || p.Dead)
                     orderby (p.Name != null && !p.Name.Numerical)? p.Name.ToStringShort : p.Label
                     select p).ToList();
            lastUpdate = Time.time;
        }

        public override Vector2 RequestedTabSize {
            get {
                return new Vector2(610f, HEADER_HEIGHT + pawns.Count * 30f);
            }
        }

        public override void DoWindowContents(Rect inRect) {            
            base.DoWindowContents(inRect);

            if (Time.time - lastUpdate >= UPDATE_INTERVAL) {
                // pawns are filtered by criteria that do not cause the list to be marked as dirty when changed
                // e.g. if a mega spider was revealed it would not appear in the list unless it is manually repopulated.
                Notify_PawnsChanged();                
            }
                        
            Rect headerBounds = new Rect(0f, 0f, inRect.width, HEADER_HEIGHT);
            DrawTableHeader(headerBounds);

            Rect listBounds = new Rect(0f, HEADER_HEIGHT, inRect.width, inRect.height - HEADER_HEIGHT);
            DrawRows(listBounds);
        }

        protected override void DrawPawnRow(Rect bounds, Pawn p) {
            GUI.BeginGroup(bounds);

            var rect = new Rect(COL_WATCH_X, bounds.height - ICON_SIZE, ICON_SIZE, ICON_SIZE);

            // watch
            if (Widgets.ButtonInvisible(rect, false)) JumpToTargetUtility.TryJump(p);
            var icon = Mouse.IsOver(rect)? WatchHoverIcon : WatchIcon;
            var tooltip = p.GetTooltip();
            tooltip.text = "Click jump to animal while keeping this tab open:\n\n" + tooltip.text;
            DrawIcon(rect, icon, tooltip);

            // gender
            rect.x = COL_GENDER_X;
            DrawIcon(rect, p.gender.GetIcon(), p.gender.GetLabel().CapitalizeFirst());

            // manhunter?
            var state = p.mindState.mentalStateHandler.CurStateDef;
            if (state != null && state.IsAggro) {
                rect.x = COL_WARNING_X;
                DrawIcon(rect, ManhunterIcon, "Manhunter");
            } else if (p.Faction == Faction.OfInsects) {
                rect.x = COL_WARNING_X;
                DrawIcon(rect, InsectIcon, "Infestation");
            }
            
            // designations:                
            var huntDesignation = Find.DesignationManager.DesignationOn(p, DesignationDefOf.Hunt);
            var tameDesignation = Find.DesignationManager.DesignationOn(p, DesignationDefOf.Tame);
            var doHunt = huntDesignation != null;
            var doTame = tameDesignation != null;

            // hunt?
            rect.x = COL_HUNT_X;
            if (DrawCheckbox(rect, ref doHunt, "DesignatorHuntDesc".Translate())) {
                if (doHunt) {
                    huntDesignation = new Designation(p, DesignationDefOf.Hunt);
                    Find.DesignationManager.AddDesignation(huntDesignation);
                    if (doTame) Find.DesignationManager.RemoveDesignation(tameDesignation);
                } else {
                    Find.DesignationManager.RemoveDesignation(huntDesignation);
                }
            }
            
            // tame?
            rect.x = COL_TAME_X;
            if (DrawCheckbox(rect, ref doTame, "DesignatorTameDesc".Translate())) {
                if (doTame) {
                    tameDesignation = new Designation(p, DesignationDefOf.Tame);
                    Find.DesignationManager.AddDesignation(tameDesignation);
                    if (doHunt) Find.DesignationManager.RemoveDesignation(huntDesignation);
                }
                else {
                    Find.DesignationManager.RemoveDesignation(tameDesignation);
                }
            }

            // medical conditions
            var hediffs = p.health.hediffSet.hediffs
                .Where(h => !h.IsOld())                
                .Select(h => h.def.LabelCap)
                .Distinct();
            var c = hediffs.Count();
            if (c > 0) {
                var str = c == 1? hediffs.First() : hediffs.Aggregate((s1, s2) => s1 + ", " + s2);
                rect.x = COL_HEALTH_X;
                rect.width = bounds.width - rect.x;
                GUI.Label(rect, str);
            }
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

        public static void DrawIcon(Rect r, Texture icon, TipSignal? tooltip = null) {
            GUI.DrawTexture(r, icon);
            if (tooltip.HasValue) TooltipHandler.TipRegion(r, tooltip.Value);
        }

        public static bool DrawCheckbox(Rect rect, ref bool flag, TipSignal? tooltip = null) {
            var oldFlag = flag;            
            Widgets.Checkbox(rect.position, ref flag, ICON_SIZE);
            if (tooltip.HasValue) TooltipHandler.TipRegion(rect, tooltip.Value);
            return flag != oldFlag;
        }
    }
}
