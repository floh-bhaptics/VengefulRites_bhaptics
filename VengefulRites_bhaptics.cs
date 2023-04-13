using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MelonLoader;
using HarmonyLib;
using UnityEngine;
using MyBhapticsTactsuit;

[assembly: MelonInfo(typeof(VengefulRites_bhaptics.VengefulRites_bhaptics), "VengefulRites_bhaptics", "2.0.0", "Florian Fahrenberger")]
[assembly: MelonGame("Deep Dive Interactive", "Vengeful Rites")]

namespace VengefulRites_bhaptics
{
    public class VengefulRites_bhaptics : MelonMod
    {
        public static TactsuitVR tactsuitVr;
        public static bool bladeInRightHand = true;
        public static bool pickaxeInRightHand = true;

        public override void OnInitializeMelon()
        {
            tactsuitVr = new TactsuitVR();
            tactsuitVr.PlaybackHaptics("HeartBeat");
        }

        #region Swordplay

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

        [HarmonyPatch(typeof(Pickaxe), "OnTriggerEnter", new Type[] { typeof(Collider) })]
        public class bhaptics_HitPickaxe
        {
            [HarmonyPostfix]
            public static void Postfix(Pickaxe __instance, Collider other)
            {
                //tactsuitVr.LOG("Other:" + other.isTrigger + " " + other.name + "End" + other.attachedRigidbody.name);
                if (other.isTrigger) return;
                if (other.name.Contains("Player")) return;
                if (other.name.Contains("Camera")) return;
                tactsuitVr.Recoil("Blade", pickaxeInRightHand);
            }
        }

        [HarmonyPatch(typeof(Pickaxe), "Haptics", new Type[] {  })]
        public class bhaptics_HapticsPickaxe
        {
            [HarmonyPostfix]
            public static void Postfix(Pickaxe __instance)
            {
                tactsuitVr.Recoil("Blade", pickaxeInRightHand);
            }
        }
        #endregion

        #region Health, Damage, and Death

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

        [HarmonyPatch(typeof(HealingAura), "FinishSpell", new Type[] {  })]
        public class bhaptics_HealingAura
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                tactsuitVr.PlaybackHaptics("Healing");
            }
        }

        [HarmonyPatch(typeof(HealingOrb), "SpawnAura", new Type[] {  })]
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

        #endregion

        #region Magic

        [HarmonyPatch(typeof(MagicController), "BeginKinesis", new Type[] { })]
        public class bhaptics_BeginKinesis
        {
            [HarmonyPostfix]
            public static void Postfix(MagicController __instance)
            {
                if (__instance.grapplePoint != null) return;
                tactsuitVr.StartTelekinesis(__instance.isRightController);
            }
        }

        [HarmonyPatch(typeof(KineticThrust), "PerformThrust", new Type[] { })]
        public class bhaptics_KineticThrust
        {
            [HarmonyPostfix]
            public static void Postfix(KineticThrust __instance)
            {
                tactsuitVr.PlaybackHaptics("KineticThrust");
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
                tactsuitVr.Spell("Fire", __instance.isRightController);
            }
        }

        [HarmonyPatch(typeof(MagicController), "ConjureHealingOrb", new Type[] { })]
        public class bhaptics_HealingOrbConjure
        {
            [HarmonyPostfix]
            public static void Postfix(MagicController __instance)
            {
                tactsuitVr.Spell("Heal", __instance.isRightController);
            }
        }

        [HarmonyPatch(typeof(MagicController), "ConjureProtectionOrb", new Type[] { })]
        public class bhaptics_Shield
        {
            [HarmonyPostfix]
            public static void Postfix(MagicController __instance)
            {
                tactsuitVr.Spell("Shield", __instance.isRightController);
            }
        }

        [HarmonyPatch(typeof(MagicController), "ConjureStoneSkin", new Type[] { })]
        public class bhaptics_StoneSkin
        {
            [HarmonyPostfix]
            public static void Postfix(MagicController __instance)
            {
                tactsuitVr.Spell("Fire", __instance.isRightController);
            }
        }

        [HarmonyPatch(typeof(MagicController_Oculus), "BeginKinesis", new Type[] { })]
        public class bhaptics_BeginKinesisOculus
        {
            [HarmonyPostfix]
            public static void Postfix(MagicController_Oculus __instance)
            {
                if (__instance.grapplePoint != null) return;
                tactsuitVr.StartTelekinesis(__instance.isRightController);
            }
        }

        [HarmonyPatch(typeof(MagicController_Oculus), "EndKinesis", new Type[] { })]
        public class bhaptics_EndKinesisOculus
        {
            [HarmonyPostfix]
            public static void Postfix(MagicController_Oculus __instance)
            {
                tactsuitVr.StopTelekinesis(__instance.isRightController);
            }
        }

        [HarmonyPatch(typeof(MagicController_Oculus), "ConjureFireball", new Type[] { })]
        public class bhaptics_FireballOculus
        {
            [HarmonyPostfix]
            public static void Postfix(MagicController_Oculus __instance)
            {
                tactsuitVr.Spell("Fire", __instance.isRightController);
            }
        }

        [HarmonyPatch(typeof(MagicController_Oculus), "ConjureHealingOrb", new Type[] { })]
        public class bhaptics_HealingOrbConjureOculus
        {
            [HarmonyPostfix]
            public static void Postfix(MagicController_Oculus __instance)
            {
                tactsuitVr.Spell("Heal", __instance.isRightController);
            }
        }

        [HarmonyPatch(typeof(MagicController_Oculus), "ConjureProtectionOrb", new Type[] { })]
        public class bhaptics_ShieldOculus
        {
            [HarmonyPostfix]
            public static void Postfix(MagicController_Oculus __instance)
            {
                tactsuitVr.Spell("Shield", __instance.isRightController);
            }
        }

        [HarmonyPatch(typeof(MagicController_Oculus), "ConjureStoneSkin", new Type[] { })]
        public class bhaptics_StoneSkinOculus
        {
            [HarmonyPostfix]
            public static void Postfix(MagicController_Oculus __instance)
            {
                tactsuitVr.Spell("Fire", __instance.isRightController);
            }
        }

        #endregion

        #region Controller interaction

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

        [HarmonyPatch(typeof(ControllerInteraction), "GrabObject", new Type[] { })]
        public class bhaptics_GrabObject
        {
            [HarmonyPostfix]
            public static void Postfix(ControllerInteraction __instance)
            {
                //bool isRight = (__instance.controller.index == SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Rightmost, Valve.VR.ETrackedDeviceClass.Controller));
                //tactsuitVr.LOG("Object: " + __instance.heldObject.name);
                if (!__instance.heldObject.name.Contains("Pickaxe")) return;
                if (__instance.hand.name == "RightHand") pickaxeInRightHand = true;
                if (__instance.hand.name == "LeftHand") pickaxeInRightHand = false;
                //tactsuitVr.LOG("GrabWeapon: " + __instance.hand.name + " " + __instance.controller.index.ToString() + " " + isRight.ToString());
            }
        }

        [HarmonyPatch(typeof(ControllerInteraction_Oculus), "FireArrow", new Type[] { })]
        public class bhaptics_ReleaseBowOculus
        {
            [HarmonyPostfix]
            public static void Postfix(ControllerInteraction_Oculus __instance)
            {
                //bool isRight = (__instance.controller.index == SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Rightmost, Valve.VR.ETrackedDeviceClass.Controller));
                //tactsuitVr.LOG("FireArrow: " + __instance.hand.name + " " + __instance.controller.index.ToString() + " " + isRight.ToString());
                if (__instance.hand.name == "LeftHand") tactsuitVr.PlaybackHaptics("RecoilBowVest_L");
                else tactsuitVr.PlaybackHaptics("RecoilBowVest_R");
            }
        }

        [HarmonyPatch(typeof(ControllerInteraction_Oculus), "GrabWeapon", new Type[] { })]
        public class bhaptics_GrabWeaponOculus
        {
            [HarmonyPostfix]
            public static void Postfix(ControllerInteraction_Oculus __instance)
            {
                //bool isRight = (__instance.controller.index == SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Rightmost, Valve.VR.ETrackedDeviceClass.Controller));
                if (__instance.hand.name == "RightHand") bladeInRightHand = true;
                if (__instance.hand.name == "LeftHand") bladeInRightHand = false;
                //tactsuitVr.LOG("GrabWeapon: " + __instance.hand.name + " " + __instance.controller.index.ToString() + " " + isRight.ToString());
            }
        }

        [HarmonyPatch(typeof(ControllerInteraction_Oculus), "GrabObject", new Type[] { })]
        public class bhaptics_GrabObjectOculus
        {
            [HarmonyPostfix]
            public static void Postfix(ControllerInteraction_Oculus __instance)
            {
                //bool isRight = (__instance.controller.index == SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Rightmost, Valve.VR.ETrackedDeviceClass.Controller));
                //tactsuitVr.LOG("Object: " + __instance.heldObject.name);
                if (!__instance.heldObject.name.Contains("Pickaxe")) return;
                if (__instance.hand.name == "RightHand") pickaxeInRightHand = true;
                if (__instance.hand.name == "LeftHand") pickaxeInRightHand = false;
                //tactsuitVr.LOG("GrabWeapon: " + __instance.hand.name + " " + __instance.controller.index.ToString() + " " + isRight.ToString());
            }
        }

        #endregion
    }
}
