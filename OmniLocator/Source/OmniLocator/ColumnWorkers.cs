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
        public override void DoCell(Rect rect, Pawn pawn, PawnTable table) {
            var state = pawn.mindState.mentalStateHandler.CurStateDef;
            Texture2D icon = null;
            bool warningSet = false;
            if (state != null && state.IsAggro) {
                Utils.DrawIcon(rect, Global.ManhunterIcon, "Manhunter");
                warningSet = true;
            } else if (pawn.Faction == Faction.OfInsects) {
                Utils.DrawIcon(rect, Global.InsectIcon, "Infestation");
                warningSet = true;
            }
            if (!warningSet) {
                var huntDesignation = Utils.GetDesignation(pawn, DesignationDefOf.Hunt);
                var tameDesignation = Utils.GetDesignation(pawn, DesignationDefOf.Tame);
                if (huntDesignation != null && pawn.RaceProps.manhunterOnDamageChance > Global.MIN_RETALIATION_CHANCE_ON_HUNT) {
                    var tooltip = "MessageAnimalsGoPsychoHunted".Translate(pawn.kindDef.label);
                    Utils.DrawIcon(rect, Global.PredatorIcon, tooltip);
                } else if (tameDesignation != null && pawn.RaceProps.manhunterOnTameFailChance > Global.MIN_RETALIATION_CHANCE_ON_TAME) {
                    var tooltip = "MessageAnimalManhuntsOnTameFailed".Translate(pawn.kindDef.label,
                              pawn.kindDef.RaceProps.manhunterOnTameFailChance.ToStringPercent("F2"));
                    Utils.DrawIcon(rect, Global.PredatorIcon, tooltip);
                    icon = Global.PredatorIcon;
                }
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
        public override void DoCell(Rect rect, Pawn pawn, PawnTable table) {
            var hediffs = pawn.health.hediffSet.hediffs
                .Where(h => !h.IsOld())                
                .Select(h => h.def.LabelCap)
                .Distinct();
            var c = hediffs.Count();
            if (c > 0) {
                var str = c == 1? hediffs.First() : hediffs.Aggregate((s1, s2) => s1 + ", " + s2);
                GUI.Label(rect, str);
            }
        }
        public override int GetMinWidth(PawnTable table) {
            return 100;
        }
        public override int GetOptimalWidth(PawnTable table) {
            return 200;
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
