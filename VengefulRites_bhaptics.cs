using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MelonLoader;
using HarmonyLib;
using UnityEngine;
using MyBhapticsTactsuit;

namespace VengefulRites_bhaptics
{
    public class VengefulRites_bhaptics : MelonMod
    {
        public static TactsuitVR tactsuitVr;
        public static bool bladeInRightHand = true;

        public override void OnApplicationStart()
        {
            base.OnApplicationStart();
            tactsuitVr = new TactsuitVR();
            tactsuitVr.PlaybackHaptics("HeartBeat");
        }

        [HarmonyPatch(typeof(PlayerWeapon), "OnTriggerEnter", new Type[] { typeof(Collider) })]
        public class bhaptics_PlayerWeaponDrawBlood
        {
            [HarmonyPostfix]
            public static void Postfix(PlayerWeapon __instance, Collider other)
            {
                if (!__instance.inHand) return;
                if (!other.CompareTag("Enemy")) return;
                float intensity = 1.0f;
                if (!__instance.fastEnough) intensity *= 0.6f;
                if (!__instance.farEnough) intensity *= 0.6f;
                bool isRight = __instance.rightWeapon;
                tactsuitVr.Recoil("Blade", bladeInRightHand, intensity);
            }
        }

        [HarmonyPatch(typeof(PlayerWeapon), "Parry", new Type[] { })]
        public class bhaptics_PlayerWeaponParry
        {
            [HarmonyPostfix]
            public static void Postfix(PlayerWeapon __instance)
            {
                if (!__instance.inHand) return;
                // tactsuitVr.LOG("Parry: " + __instance.rightWeapon.ToString());
                bool isRight = __instance.rightWeapon;
                tactsuitVr.Recoil("Blade", bladeInRightHand);
            }
        }

        /*
        [HarmonyPatch(typeof(PlayerWeapon), "PlushieHit", new Type[] { })]
        public class bhaptics_PlayerWeaponPlushieHit
        {
            [HarmonyPostfix]
            public static void Postfix(PlayerWeapon __instance)
            {
                if (!__instance.inHand) return;
                // tactsuitVr.LOG("Plushie: " + __instance.rightWeapon.ToString());
                bool isRight = __instance.rightWeapon;
                tactsuitVr.Recoil("Blade", isRight, 0.2f);
            }
        }

        [HarmonyPatch(typeof(PlayerWeapon), "WeakHit", new Type[] { })]
        public class bhaptics_PlayerWeaponWeakHit
        {
            [HarmonyPostfix]
            public static void Postfix(PlayerWeapon __instance)
            {
                if (!__instance.inHand) return;
                // tactsuitVr.LOG("Weak: " + __instance.rightWeapon.ToString());
                bool isRight = __instance.rightWeapon;
                tactsuitVr.Recoil("Blade", isRight, 0.4f);
            }
        }
        */
        /*
        [HarmonyPatch(typeof(PlayerWeapon), "ApplyEffect", new Type[] { typeof(EnemyController), typeof(Transform), typeof(float) })]
        public class bhaptics_PlayerWeaponApplyEffect
        {
            [HarmonyPostfix]
            public static void Postfix(PlayerWeapon __instance)
            {
                if (!__instance.inHand) return;
                bool isRight = __instance.rightWeapon;
                tactsuitVr.Recoil("Blade", isRight);
            }
        }
        */

        [HarmonyPatch(typeof(PlayerStats), "Die", new Type[] {  })]
        public class bhaptics_PlayerDies
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                tactsuitVr.StopThreads();
            }
        }

        [HarmonyPatch(typeof(PlayerStats), "GainHealth", new Type[] { typeof(float) })]
        public class bhaptics_GainHealth
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                tactsuitVr.PlaybackHaptics("Healing");
            }
        }

        [HarmonyPatch(typeof(HealingAura), "FinishSpell", new Type[] { typeof(float) })]
        public class bhaptics_HealingAura
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                tactsuitVr.PlaybackHaptics("Healing");
            }
        }

        [HarmonyPatch(typeof(HealingOrb), "SpawnAura", new Type[] { typeof(float) })]
        public class bhaptics_HealingOrb
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                tactsuitVr.PlaybackHaptics("Healing");
            }
        }

        [HarmonyPatch(typeof(PlayerStats), "TakeDamage", new Type[] { typeof(float) })]
        public class bhaptics_TakeDamage
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                tactsuitVr.PlaybackHaptics("Impact");
            }
        }

        [HarmonyPatch(typeof(PlayerStats), "Update", new Type[] {  })]
        public class bhaptics_HealthUpdate
        {
            [HarmonyPostfix]
            public static void Postfix(PlayerStats __instance)
            {
                if (PlayerStats.playerHP <= 0.2f * PlayerStats.maxHP) tactsuitVr.StartHeartBeat();
                else tactsuitVr.StopHeartBeat();
            }
        }

        [HarmonyPatch(typeof(MagicController), "BeginKinesis", new Type[] { })]
        public class bhaptics_BeginKinesis
        {
            [HarmonyPostfix]
            public static void Postfix(MagicController __instance)
            {
                tactsuitVr.StartTelekinesis(__instance.isRightController);
            }
        }

        [HarmonyPatch(typeof(MagicController), "EndKinesis", new Type[] { })]
        public class bhaptics_EndKinesis
        {
            [HarmonyPostfix]
            public static void Postfix(MagicController __instance)
            {
                tactsuitVr.StopTelekinesis(__instance.isRightController);
            }
        }

        [HarmonyPatch(typeof(MagicController), "ConjureFireball", new Type[] { })]
        public class bhaptics_Fireball
        {
            [HarmonyPostfix]
            public static void Postfix(MagicController __instance)
            {
                //tactsuitVr.LOG("Fireball: " + __instance.isRightController.ToString() + " " + __instance.element);
                tactsuitVr.Spell("Fire", __instance.isRightController);
            }
        }

        [HarmonyPatch(typeof(MagicController), "ConjureHealingOrb", new Type[] { })]
        public class bhaptics_HealingOrbConjure
        {
            [HarmonyPostfix]
            public static void Postfix(MagicController __instance)
            {
                //tactsuitVr.LOG("Fireball: " + __instance.isRightController.ToString() + " " + __instance.element);
                tactsuitVr.Spell("Heal", __instance.isRightController);
            }
        }

        [HarmonyPatch(typeof(MagicController), "ConjureProtectionOrb", new Type[] { })]
        public class bhaptics_Shield
        {
            [HarmonyPostfix]
            public static void Postfix(MagicController __instance)
            {
                //tactsuitVr.LOG("Fireball: " + __instance.isRightController.ToString() + " " + __instance.element);
                tactsuitVr.Spell("Shield", __instance.isRightController);
            }
        }

        [HarmonyPatch(typeof(MagicController), "ConjureStoneSkin", new Type[] { })]
        public class bhaptics_StoneSkin
        {
            [HarmonyPostfix]
            public static void Postfix(MagicController __instance)
            {
                //tactsuitVr.LOG("Fireball: " + __instance.isRightController.ToString() + " " + __instance.element);
                tactsuitVr.Spell("Fire", __instance.isRightController);
            }
        }


        [HarmonyPatch(typeof(ControllerInteraction), "FireArrow", new Type[] { })]
        public class bhaptics_ReleaseBow
        {
            [HarmonyPostfix]
            public static void Postfix(ControllerInteraction __instance)
            {
                //bool isRight = (__instance.controller.index == SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Rightmost, Valve.VR.ETrackedDeviceClass.Controller));
                //tactsuitVr.LOG("FireArrow: " + __instance.hand.name + " " + __instance.controller.index.ToString() + " " + isRight.ToString());
                if (__instance.hand.name == "LeftHand") tactsuitVr.PlaybackHaptics("RecoilBowVest_L");
                else tactsuitVr.PlaybackHaptics("RecoilBowVest_R");
            }
        }

        [HarmonyPatch(typeof(ControllerInteraction), "GrabWeapon", new Type[] { })]
        public class bhaptics_GrabWeapon
        {
            [HarmonyPostfix]
            public static void Postfix(ControllerInteraction __instance)
            {
                //bool isRight = (__instance.controller.index == SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Rightmost, Valve.VR.ETrackedDeviceClass.Controller));
                if (__instance.hand.name == "RightHand") bladeInRightHand = true;
                if (__instance.hand.name == "LeftHand") bladeInRightHand = false;
                //tactsuitVr.LOG("GrabWeapon: " + __instance.hand.name + " " + __instance.controller.index.ToString() + " " + isRight.ToString());
            }
        }

    }
}
