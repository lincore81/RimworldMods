using UnityEngine;
using Verse;
using System.Collections.Generic;

namespace Lincore.OmniLocator {
    [StaticConstructorOnStartup]
    public static class Global {
        public static readonly Texture2D HuntIcon = ContentFinder<Texture2D>.Get("UI/Icons/Hunt");
        public static readonly Texture2D TameIcon = ContentFinder<Texture2D>.Get("UI/Icons/Tame");
        public static readonly Texture2D WatchIcon = ContentFinder<Texture2D>.Get("UI/Icons/Eye");
        public static readonly Texture2D WatchHoverIcon = ContentFinder<Texture2D>.Get("UI/Icons/Eye-Hover");
        public static readonly Texture2D WarningIcon = ContentFinder<Texture2D>.Get("UI/Icons/Warning");
        public static readonly Texture2D ManhunterIcon = ContentFinder<Texture2D>.Get("UI/Icons/Manhunter");
        public static readonly Texture2D InsectIcon = ContentFinder<Texture2D>.Get("UI/Icons/Insect");
        public static readonly Texture2D PredatorIcon = ContentFinder<Texture2D>.Get("UI/Icons/Predator");
        public static readonly Texture2D Predator2Icon = ContentFinder<Texture2D>.Get("UI/Icons/Predator2");



        public const float MIN_RETALIATION_CHANCE_ON_HUNT = 0.2f;
        public const float MIN_RETALIATION_CHANCE_ON_TAME = 0.02f;

        public const int ICON_SIZE = 24;
    }
}
