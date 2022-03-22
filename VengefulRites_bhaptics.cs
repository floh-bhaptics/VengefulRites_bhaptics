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

        public override void OnApplicationStart()
        {
            base.OnApplicationStart();
            tactsuitVr = new TactsuitVR();
            tactsuitVr.PlaybackHaptics("HeartBeat");
        }

        [HarmonyPatch(typeof(PlayerWeapon), "OnTriggerEnter", new Type[] { typeof(Collider) })]
        public class bhaptics_PlayerWeaponCollide
        {
            [HarmonyPostfix]
            public static void Postfix(PlayerWeapon __instance, Collider other)
            {
                if (!__instance.inHand) return;
                bool isRight = __instance.rightWeapon;
                tactsuitVr.Recoil("Blade", isRight);
            }
        }


    }
}
