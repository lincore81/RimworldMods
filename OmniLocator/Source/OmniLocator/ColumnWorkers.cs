using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace Lincore.OmniLocator {
    public class PawnColumnWorker_Watch : PawnColumnWorker {
        public override void DoCell(Rect rect, Pawn pawn, PawnTable table) {
            if (Widgets.ButtonInvisible(rect, false)) CameraJumper.TryJump(pawn);
            var icon = Mouse.IsOver(rect) ? Global.WatchHoverIcon : Global.WatchIcon;
            var tooltip = pawn.GetTooltip();
            tooltip.text = "Click to look at this animal without closing the window.\n\n" + tooltip.text;
            Utils.DrawIcon(rect, icon, tooltip);
        }

        public override int GetMaxWidth(PawnTable table) {
            return GetOptimalWidth(table);
        }

        public override int GetMinWidth(PawnTable table) {
            return Global.ICON_SIZE;
        }

        public override int GetOptimalWidth(PawnTable table) {
            return Global.ICON_SIZE + 10;
        }
    }

    public class PawnColumnWorker_Danger : PawnColumnWorker {
       protected  enum Danger {
            Insectoid,
            Manhunter,
            Retaliation,
            Taming,
            Predator,
            None
        }

        protected struct UiElem {
            public readonly Texture2D Icon;
            public readonly Func<Pawn, string> GetTooltip;
            
            public UiElem(Texture2D icon, Func<Pawn, string> tooltip) {
                Icon = icon;
                GetTooltip = tooltip;
            }
        }

        protected static Dictionary<Danger, UiElem> DangerUiElems = new Dictionary<Danger, UiElem> {
            {Danger.None, new UiElem()},
            {Danger.Insectoid, new UiElem(Global.InsectIcon, (_) => "Infestation")},
            {Danger.Manhunter, new UiElem(Global.ManhunterIcon, (_) => "Manhunter") },
            {Danger.Retaliation, new UiElem(Global.PredatorIcon, 
                (pawn) => "MessageAnimalsGoPsychoHunted".Translate(pawn.kindDef.label)) },
            {Danger.Taming, new UiElem(Global.PredatorIcon, 
                (pawn) => "MessageAnimalManhuntsOnTameFailed".Translate(pawn.kindDef.label,
                    pawn.kindDef.RaceProps.manhunterOnTameFailChance.ToStringPercent("F2"))) },
            {Danger.Predator, new UiElem(Global.Predator2Icon, (_) => "Predator") }
        };

        protected Danger GetDangerOf(Pawn pawn) {
            var state = pawn.mindState.mentalStateHandler.CurStateDef;
            if (state != null && state.IsAggro) {
                return Danger.Manhunter;
            }
            if (pawn.Faction == Faction.OfInsects) {
                return Danger.Insectoid;
            }
            var huntDesignation = Utils.GetDesignation(pawn, DesignationDefOf.Hunt);
            var tameDesignation = Utils.GetDesignation(pawn, DesignationDefOf.Tame);
            bool huntRisk = huntDesignation != null && pawn.RaceProps.manhunterOnDamageChance > Global.MIN_RETALIATION_CHANCE_ON_HUNT;
            if (huntRisk) return Danger.Retaliation;
            bool tameRisk = tameDesignation != null && pawn.RaceProps.manhunterOnTameFailChance > Global.MIN_RETALIATION_CHANCE_ON_TAME;
            if (tameRisk) return Danger.Taming;
            if (pawn.RaceProps.predator) return Danger.Predator;
            return Danger.None;
        }

        public override void DoCell(Rect rect, Pawn pawn, PawnTable table) {
            var center = rect.center;
            rect = new Rect(rect.position, new Vector2(Global.ICON_SIZE, Global.ICON_SIZE));
            rect.center = center;
            var danger = GetDangerOf(pawn);
            UiElem ui;
            if (!DangerUiElems.TryGetValue(danger, out ui)) {
                Log.ErrorOnce(string.Format("Unknown danger: {0}"), 9235423);
                return;
            }
            var tooltip = ui.GetTooltip != null ? ui.GetTooltip(pawn) : "";
            if (ui.Icon != null) Utils.DrawIcon(rect, ui.Icon, tooltip);
        }

        public override int GetMaxWidth(PawnTable table) {
            return GetOptimalWidth(table);
        }

        public override int GetMinWidth(PawnTable table) {
            return Global.ICON_SIZE;
        }

        public override int GetOptimalWidth(PawnTable table) {
            return Global.ICON_SIZE + 10;
        }
        public override int Compare(Pawn a, Pawn b) {
            return this.GetValueToCompare(a).CompareTo(this.GetValueToCompare(b));
        }

        private int GetValueToCompare(Pawn pawn) {
            return -((int)GetDangerOf(pawn));
        }
    }

    public class PawnColumnWorker_Tame : PawnColumnWorker_Checkbox {
        protected override string GetTip(Pawn pawn) {
            return "DesignatorTameDesc".Translate();
        }

        protected override bool GetValue(Pawn pawn) {
            var designation = Utils.GetDesignation(pawn, DesignationDefOf.Tame);
            return designation != null;
        }

        protected override void SetValue(Pawn pawn, bool value) {
            if (value) {
                Utils.AddDesignation(pawn, new Designation(pawn, DesignationDefOf.Tame));
                var huntDesignation = Utils.GetDesignation(pawn, DesignationDefOf.Hunt);
                if (huntDesignation != null) Utils.RemoveDesignation(pawn, huntDesignation);
                if (pawn.RaceProps.manhunterOnTameFailChance > Global.MIN_RETALIATION_CHANCE_ON_TAME) {
                    Verse.Sound.SoundStarter.PlayOneShotOnCamera(SoundDefOf.MessageAlert);
                }
            } else {
                var tameDesignation = Utils.GetDesignation(pawn, DesignationDefOf.Tame);
                Utils.RemoveDesignation(pawn, tameDesignation);
            }
        }
        public override int GetMaxWidth(PawnTable table) {
            return GetOptimalWidth(table);
        }

        public override int GetMinWidth(PawnTable table) {
            return Global.ICON_SIZE;
        }

        public override int GetOptimalWidth(PawnTable table) {
            return Global.ICON_SIZE + 10;
        }
        
    }

    public class PawnColumnWorker_Hunt : PawnColumnWorker_Checkbox {
        protected override string GetTip(Pawn pawn) {
            return "DesignatorHuntDesc".Translate();
        }

        protected override bool GetValue(Pawn pawn) {
            var designation = Utils.GetDesignation(pawn, DesignationDefOf.Hunt);
            return designation != null;
        }

        protected override void SetValue(Pawn pawn, bool value) {
            if (value) {
                Utils.AddDesignation(pawn, new Designation(pawn, DesignationDefOf.Hunt));
                var tameDesignation = Utils.GetDesignation(pawn, DesignationDefOf.Tame);
                if (tameDesignation != null) Utils.RemoveDesignation(pawn, tameDesignation);
                if (pawn.RaceProps.manhunterOnDamageChance > Global.MIN_RETALIATION_CHANCE_ON_HUNT) {
                    Verse.Sound.SoundStarter.PlayOneShotOnCamera(SoundDefOf.MessageAlert);
                }
            } else {
                var huntDesignation = Utils.GetDesignation(pawn, DesignationDefOf.Hunt);
                Utils.RemoveDesignation(pawn, huntDesignation);
            }
        }
        public override int GetMaxWidth(PawnTable table) {
            return GetOptimalWidth(table);
        }

        public override int GetMinWidth(PawnTable table) {
            return Global.ICON_SIZE;
        }

        public override int GetOptimalWidth(PawnTable table) {
            return Global.ICON_SIZE + 10;
        }
        
    }

    public class PawnColumnWorker_Medical : PawnColumnWorker {
        protected string GetMedicalSummary(Pawn pawn) {
            var hediffs = pawn.health.hediffSet.hediffs
                .Where(h => !h.IsOld())                
                .Select(h => h.def.LabelCap)
                .Distinct();
            var c = hediffs.Count();
            if (c == 0) return null;
            return c == 1? hediffs.First() : hediffs.Aggregate((s1, s2) => s1 + ", " + s2);
        }

        public override void DoCell(Rect rect, Pawn pawn, PawnTable table) {
            var str = GetMedicalSummary(pawn);
            if (str != null) GUI.Label(rect, str);
        }
        public override int GetMinWidth(PawnTable table) {
            return 100;
        }
        public override int GetOptimalWidth(PawnTable table) {
            return 300;
        }

        public override int Compare(Pawn a, Pawn b) {
            return (GetMedicalSummary(a) ?? "").CompareTo(GetMedicalSummary(b) ?? "");
        }

    }


    public class PawnColumnWorker_InfoButton : PawnColumnWorker {
        public override void DoCell(Rect rect, Pawn pawn, PawnTable table) {
            Widgets.InfoCardButton(rect.x, rect.y, pawn);
        }
        public override int GetMaxWidth(PawnTable table) {
            return GetOptimalWidth(table);
        }

        public override int GetMinWidth(PawnTable table) {
            return Global.ICON_SIZE;
        }

        public override int GetOptimalWidth(PawnTable table) {
            return Global.ICON_SIZE + 10;
        }
        
    }
}
