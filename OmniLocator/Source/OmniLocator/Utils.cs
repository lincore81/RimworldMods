using UnityEngine;
using Verse;

namespace Lincore.OmniLocator {
    public static class Utils {

        public static void DrawIcon(Rect r, Texture icon, TipSignal? tooltip = null) {
            GUI.DrawTexture(r, icon);
            if (tooltip.HasValue) TooltipHandler.TipRegion(r, tooltip.Value);
        }

        public static bool DrawCheckbox(Rect rect, ref bool flag, TipSignal? tooltip = null, float iconSize = 24f) {
            var oldFlag = flag;
            Widgets.Checkbox(rect.position, ref flag, iconSize);
            if (tooltip.HasValue && Mouse.IsOver(rect)) {
                TooltipHandler.TipRegion(rect, tooltip.Value);
            }
            return flag != oldFlag;
        }

        public static bool DrawSlidingCheckbox(Rect rect, ref bool flag, TipSignal? tooltip = null, float iconSize = 24f) {
            var oldFlag = flag;
            if (!Mouse.IsOver(rect)) {
                return false;
            }

            bool lmb = Input.GetKey(KeyCode.Mouse0);


            Widgets.Checkbox(rect.position, ref flag, iconSize);
            if (tooltip.HasValue && Mouse.IsOver(rect)) {
                TooltipHandler.TipRegion(rect, tooltip.Value);
            }
            return flag != oldFlag;
        }

        public static Designation GetDesignation(Pawn pawn, DesignationDef designation) {
            return pawn.MapHeld.designationManager.DesignationOn(pawn, designation);
        }

        public static void RemoveDesignation(Pawn pawn, Designation designation) {
            pawn.MapHeld.designationManager.RemoveDesignation(designation);
        }
        public static void AddDesignation(Pawn pawn, Designation designation) {
            pawn.MapHeld.designationManager.AddDesignation(designation);
        }
    }
}
