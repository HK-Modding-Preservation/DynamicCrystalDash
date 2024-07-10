using System.Collections.Generic;
using Modding;
using UnityEngine;
using HutongGames.PlayMaker;
using Satchel;

namespace DynamicCrystalDash {
    public class DynamicCrystalDash: Mod {
        public static float chargeStartTime;
        public static float totalChargeTime;
        public static bool charging;
        new public string GetName() => "DynamicCrystalDash";
        public override string GetVersion() => "1.0.0.0";
        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects) {
            On.PlayMakerFSM.OnEnable += updateFSMs;
        }

        private void updateFSMs(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self) {
            orig(self);
            if(self.gameObject.name == "Knight" && self.FsmName == "Superdash") {
                self.GetState("Relinquish Control").AddAction(new setChargeStart());
                
                FsmState wallChargeState = self.GetState("Wall Charge");
                wallChargeState.ChangeTransition("BUTTON UP", "Wall Charged");
                wallChargeState.RemoveAction(15);
                FsmState groundChargeState = self.GetState("Ground Charge");
                groundChargeState.ChangeTransition("BUTTON UP", "Ground Charged");
                groundChargeState.RemoveAction(10);

                FsmState leftState = self.GetState("Left");
                leftState.RemoveAction(0);
                leftState.InsertAction(new setDynamicSpeed(self, -1), 0);
                FsmState rightState = self.GetState("Right");
                rightState.RemoveAction(0);
                rightState.InsertAction(new setDynamicSpeed(self, 1), 0);

                self.GetState("Cancelable").RemoveAction(1);
            }
            else if(self.gameObject.name.StartsWith("SD Crystal Gen") && self.FsmName == "superdash_crystal_gen") {
                int index = (self.gameObject.name.StartsWith("SD Crystal Gen G") ? 3 : 1);
                FsmState chargingState = self.GetState("Charging");
                chargingState.RemoveAction(index);
                chargingState.InsertAction(new fakeiTweenMoveBy(self.gameObject),index);
            }
        }
    }

    public class setChargeStart: FsmStateAction {
        public override void OnEnter() {
            DynamicCrystalDash.charging = true;
            DynamicCrystalDash.chargeStartTime = Time.time;
            Finish();
        }
    }

    public class setDynamicSpeed: FsmStateAction {
        private PlayMakerFSM self;
        private int positiveDirection;
        public setDynamicSpeed(PlayMakerFSM fsm, int positiveDirection) {
            self = fsm;
            this.positiveDirection = positiveDirection;
        }
        public override void OnEnter() {
            float totalTime = Time.time - DynamicCrystalDash.chargeStartTime;
            DynamicCrystalDash.totalChargeTime = totalTime;
            float speed = 37.5f * totalTime * positiveDirection;
            self.FsmVariables.GetFsmFloat("Current SD Speed").SafeAssign(speed);
            self.FsmVariables.GetFsmFloat(positiveDirection > 0 ? "Superdash Speed" : "Superdash Speed neg").SafeAssign(speed);
            DynamicCrystalDash.charging = false;
            Finish();
        }
    }

    public class fakeiTweenMoveBy: FsmStateAction {
        private Vector3 movement;
        private GameObject gameObject;
        public fakeiTweenMoveBy(GameObject gameObject) {
            this.gameObject = gameObject;
            switch(gameObject.name) {
                case "SD Crystal Gen G1":
                    movement = new Vector3(3.75f, 0, 0);
                    break;
                case "SD Crystal Gen G2":
                    movement = new Vector3(-3.75f, 0, 0);
                    break;
                case "SD Crystal Gen W1":
                    movement = new Vector3(0, 3.75f, 0);
                    break;
                case "SD Crystal Gen W2":
                    movement = new Vector3(0, -3.75f, 0);
                    break;
            }
        }
        public override void OnUpdate() {
            this.gameObject.transform.position += movement * Time.deltaTime;
            if(!DynamicCrystalDash.charging)
                Finish();
        }
    }
}
