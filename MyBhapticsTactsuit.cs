using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using MelonLoader;

namespace MyBhapticsTactsuit
{
    public class TactsuitVR
    {
        /* A class that contains the basic functions for the bhaptics Tactsuit, like:
         * - A Heartbeat function that can be turned on/off
         * - A function to read in and register all .tact patterns in the bHaptics subfolder
         * - A logging hook to output to the Melonloader log
         * - 
         * */
        public bool suitDisabled = true;
        public bool systemInitialized = false;
        // Event to start and stop the heartbeat thread
        private static ManualResetEvent HeartBeat_mrse = new ManualResetEvent(false);
        private static ManualResetEvent TelKinL_mrse = new ManualResetEvent(false);
        private static ManualResetEvent TelKinR_mrse = new ManualResetEvent(false);
        // dictionary of all feedback patterns found in the bHaptics directory
        public Dictionary<String, FileInfo> FeedbackMap = new Dictionary<String, FileInfo>();

        private static bHapticsLib.RotationOption defaultRotationOption = new bHapticsLib.RotationOption(0.0f, 0.0f);

        public void HeartBeatFunc()
        {
            while (true)
            {
                // Check if reset event is active
                HeartBeat_mrse.WaitOne();
                bHapticsLib.bHapticsManager.PlayRegistered("HeartBeat");
                Thread.Sleep(600);
            }
        }
        public void TelKinLFunc()
        {
            while (true)
            {
                // Check if reset event is active
                TelKinL_mrse.WaitOne();
                bHapticsLib.bHapticsManager.PlayRegistered("TelekinesisArm_L");
                bHapticsLib.bHapticsManager.PlayRegistered("TelekinesisHand_L");
                Thread.Sleep(1020);
            }
        }

        public void TelKinRFunc()
        {
            while (true)
            {
                // Check if reset event is active
                TelKinR_mrse.WaitOne();
                bHapticsLib.bHapticsManager.PlayRegistered("TelekinesisArm_R");
                bHapticsLib.bHapticsManager.PlayRegistered("TelekinesisHand_R");
                Thread.Sleep(1020);
            }
        }

        public TactsuitVR()
        {
            LOG("Initializing suit");
            suitDisabled = false;
            RegisterAllTactFiles();
            LOG("Starting HeartBeat thread...");
            Thread HeartBeatThread = new Thread(HeartBeatFunc);
            HeartBeatThread.Start();
            Thread TelKinLThread = new Thread(TelKinLFunc);
            TelKinLThread.Start();
            Thread TelKinRThread = new Thread(TelKinRFunc);
            TelKinRThread.Start();
        }

        public void LOG(string logStr)
        {
#pragma warning disable CS0618 // remove warning that the logger is deprecated
            MelonLogger.Msg(logStr);
#pragma warning restore CS0618
        }



        void RegisterAllTactFiles()
        {
            // Get location of the compiled assembly and search through "bHaptics" directory and contained patterns
            string configPath = Directory.GetCurrentDirectory() + "\\Mods\\bHaptics";
            DirectoryInfo d = new DirectoryInfo(configPath);
            FileInfo[] Files = d.GetFiles("*.tact", SearchOption.AllDirectories);
            for (int i = 0; i < Files.Length; i++)
            {
                string filename = Files[i].Name;
                string fullName = Files[i].FullName;
                string prefix = Path.GetFileNameWithoutExtension(filename);
                // LOG("Trying to register: " + prefix + " " + fullName);
                if (filename == "." || filename == "..")
                    continue;
                string tactFileStr = File.ReadAllText(fullName);
                try
                {
                    bHapticsLib.bHapticsManager.RegisterPatternFromJson(prefix, tactFileStr);
                    LOG("Pattern registered: " + prefix);
                }
                catch (Exception e) { LOG(e.ToString()); }

                FeedbackMap.Add(prefix, Files[i]);
            }
            systemInitialized = true;
        }

        public void PlaybackHaptics(String key, float intensity = 1.0f, float duration = 1.0f)
        {
            //LOG("Trying to play");
            if (FeedbackMap.ContainsKey(key))
            {
                //LOG("ScaleOption");
                bHapticsLib.ScaleOption scaleOption = new bHapticsLib.ScaleOption(intensity, duration);
                //LOG("Submit");
                bHapticsLib.bHapticsManager.PlayRegistered(key, key, scaleOption, defaultRotationOption);
                // LOG("Playing back: " + key);
            }
            else
            {
                LOG("Feedback not registered: " + key);
            }
        }

        public void PlayBackHit(String key, float xzAngle, float yShift)
        {
            // two parameters can be given to the pattern to move it on the vest:
            // 1. An angle in degrees [0, 360] to turn the pattern to the left
            // 2. A shift [-0.5, 0.5] in y-direction (up and down) to move it up or down
            bHapticsLib.ScaleOption scaleOption = new bHapticsLib.ScaleOption(1f, 1f);
            bHapticsLib.RotationOption rotationOption = new bHapticsLib.RotationOption(xzAngle, yShift);
            bHapticsLib.bHapticsManager.PlayRegistered(key, key, scaleOption, rotationOption);
        }

        public void Recoil(string weaponName, bool isRightHand, float intensity = 1.0f)
        {
            // weaponName is a parameter that will go into the vest feedback pattern name
            // isRightHand is just which side the feedback is on
            // intensity should usually be between 0 and 1

            float duration = 1.0f;
            var scaleOption = new bHapticsLib.ScaleOption(intensity, duration);
            // the function needs some rotation if you want to give the scale option as well
            var rotationFront = new bHapticsLib.RotationOption(0f, 0f);
            // make postfix according to parameter
            string postfix = "_L";
            if (isRightHand) { postfix = "_R"; }

            // stitch together pattern names for Arm and Hand recoil
            string keyHands = "RecoilHand" + postfix;
            string keyArm = "RecoilArm" + postfix;
            // vest pattern name contains the weapon name. This way, you can quickly switch
            // between swords, pistols, shotguns, ... by just changing the shoulder feedback
            // and scaling via the intensity for arms and hands
            string keyVest = "Recoil" + weaponName + "Vest" + postfix;
            
            bHapticsLib.bHapticsManager.PlayRegistered(keyHands, keyHands, scaleOption, rotationFront);
            bHapticsLib.bHapticsManager.PlayRegistered(keyArm, keyArm, scaleOption, rotationFront);
            bHapticsLib.bHapticsManager.PlayRegistered(keyVest, keyVest, scaleOption, rotationFront);
        }

        public void Block(bool isRightHand)
        {
            // weaponName is a parameter that will go into the vest feedback pattern name
            // isRightHand is just which side the feedback is on
            // intensity should usually be between 0 and 1

            float duration = 1.0f;
            float intensity = 1.0f;
            var scaleOption = new bHapticsLib.ScaleOption(intensity, duration);
            // the function needs some rotation if you want to give the scale option as well
            var rotationFront = new bHapticsLib.RotationOption(0f, 0f);
            // make postfix according to parameter
            string postfix = "_L";
            if (isRightHand) { postfix = "_R"; }

            // stitch together pattern names for Arm and Hand recoil
            string keyHands = "RecoilHand" + postfix;
            string keyArm = "RecoilArm" + postfix;
            // vest pattern name contains the weapon name. This way, you can quickly switch
            // between swords, pistols, shotguns, ... by just changing the shoulder feedback
            // and scaling via the intensity for arms and hands
            string keyVest = "BlockVest" + postfix;
            bHapticsLib.bHapticsManager.PlayRegistered(keyHands, keyHands, scaleOption, rotationFront);
            bHapticsLib.bHapticsManager.PlayRegistered(keyArm, keyArm, scaleOption, rotationFront);
            bHapticsLib.bHapticsManager.PlayRegistered(keyVest, keyVest, scaleOption, rotationFront);
        }

        public void Spell(string spellName, bool isRightHand)
        {
            // weaponName is a parameter that will go into the vest feedback pattern name
            // isRightHand is just which side the feedback is on
            // intensity should usually be between 0 and 1

            float duration = 1.0f;
            float intensity = 1.0f;
            var scaleOption = new bHapticsLib.ScaleOption(intensity, duration);
            // the function needs some rotation if you want to give the scale option as well
            var rotationFront = new bHapticsLib.RotationOption(0f, 0f);
            // make postfix according to parameter
            string postfix = "_L";
            if (isRightHand) { postfix = "_R"; }

            // stitch together pattern names for Arm and Hand recoil
            string keyHands = "Spell" + spellName + "Hand" + postfix;
            string keyArm = "Spell" + spellName + "Arm" + postfix;
            // vest pattern name contains the weapon name. This way, you can quickly switch
            // between swords, pistols, shotguns, ... by just changing the shoulder feedback
            // and scaling via the intensity for arms and hands
            string keyVest = "Spell" + spellName + "Vest" + postfix;
            bHapticsLib.bHapticsManager.PlayRegistered(keyHands, keyHands, scaleOption, rotationFront);
            bHapticsLib.bHapticsManager.PlayRegistered(keyArm, keyArm, scaleOption, rotationFront);
            bHapticsLib.bHapticsManager.PlayRegistered(keyVest, keyVest, scaleOption, rotationFront);
        }

        public void hitHand(bool isRight)
        {
            float duration = 1.0f;
            float intensity = 0.5f;
            var scaleOption = new bHapticsLib.ScaleOption(intensity, duration);
            var rotationFront = new bHapticsLib.RotationOption(0f, 0f);
            string postfix = "_L";
            if (isRight) { postfix = "_R"; }

            string deviceName = "Arm";
            if (bHapticsLib.bHapticsManager.IsDeviceConnected(bHapticsLib.PositionID.HandLeft)) deviceName = "Hand";
            string key = "Recoil" + deviceName + postfix;
            bHapticsLib.bHapticsManager.PlayRegistered(key, key, scaleOption, rotationFront);
        }

        public void hitArm(bool isRight)
        {
            float duration = 1.0f;
            float intensity = 0.5f;
            var scaleOption = new bHapticsLib.ScaleOption(intensity, duration);
            var rotationFront = new bHapticsLib.RotationOption(0f, 0f);
            string postfix = "_L";
            if (isRight) { postfix = "_R"; }

            string deviceName = "Hand";
            if (bHapticsLib.bHapticsManager.IsDeviceConnected(bHapticsLib.PositionID.ArmLeft)) deviceName = "Arm";
            string key = "Recoil" + deviceName + postfix;
            bHapticsLib.bHapticsManager.PlayRegistered(key, key, scaleOption, rotationFront);
        }

        public void HeadShot(String key, float hitAngle)
        {
            // I made 4 patterns in the Tactal for fron/back/left/right headshots
            if (bHapticsLib.bHapticsManager.IsDeviceConnected(bHapticsLib.PositionID.Head))
            {
                if ((hitAngle < 45f) | (hitAngle > 315f)) { PlaybackHaptics("Headshot_F"); }
                if ((hitAngle > 45f) && (hitAngle < 135f)) { PlaybackHaptics("Headshot_L"); }
                if ((hitAngle > 135f) && (hitAngle < 225f)) { PlaybackHaptics("Headshot_B"); }
                if ((hitAngle > 225f) && (hitAngle < 315f)) { PlaybackHaptics("Headshot_R"); }
            }
            // If there is no Tactal, just forward to the vest  with angle and at the very top (0.5)
            else { PlayBackHit(key, hitAngle, 0.5f); }
        }

        public void StartHeartBeat()
        {
            HeartBeat_mrse.Set();
        }

        public void StopHeartBeat()
        {
            HeartBeat_mrse.Reset();
        }

        public void StartTelekinesis(bool isRight)
        {
            if (isRight) TelKinR_mrse.Set();
            else TelKinL_mrse.Set();
        }

        public void StopTelekinesis(bool isRight)
        {
            if (isRight)
            {
                TelKinR_mrse.Reset();
                StopHapticFeedback("TelekinesisArm_R");
                StopHapticFeedback("TelekinesisHand_R");
            }
            else
            {
                TelKinL_mrse.Reset();
                StopHapticFeedback("TelekinesisArm_L");
                StopHapticFeedback("TelekinesisHand_L");
            }
        }

        public bool IsPlaying(String effect)
        {
            return bHapticsLib.bHapticsManager.IsPlaying(effect);
        }

        public void StopHapticFeedback(String effect)
        {
            bHapticsLib.bHapticsManager.StopPlaying(effect);
        }

        public void StopAllHapticFeedback()
        {
            StopThreads();
            foreach (String key in FeedbackMap.Keys)
            {
                bHapticsLib.bHapticsManager.StopPlaying(key);
            }
        }

        public void StopThreads()
        {
            // Yes, looks silly here, but if you have several threads like this, this is
            // very useful when the player dies or starts a new level
            StopHeartBeat();
            StopTelekinesis(true);
            StopTelekinesis(false);
        }


    }
}
