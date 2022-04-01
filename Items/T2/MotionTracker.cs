﻿using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;
using static R2API.RecalculateStatsAPI;
using R2API;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using System.Linq;

namespace ThinkInvisible.TinkersSatchel {
    public class MotionTracker : Item<MotionTracker> {

        ////// Item Data //////
        
        public override string displayName => "Motion Tracker";
        public override ItemTier itemTier => ItemTier.Tier2;
        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] {ItemTag.Healing});

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Deal increasing damage during combat.";
        protected override string GetDescString(string langid = null) => $"While in combat with an enemy, deal up to <style=cIsDamage>{Pct(damageFrac)} more damage</style> to that enemy <style=cStack>(+{Pct(damageFrac)} per stack)</style>, ramping up over {damageTime:N0} seconds.";
        protected override string GetLoreString(string langid = null) => "";



        ////// Config //////

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Maximum damage bonus per stack.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float damageFrac { get; private set; } = 0.2f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Time in combat required to reach maximum damage bonus.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float damageTime { get; private set; } = 15f;



        ////// TILER2 Module Setup //////
        
        public MotionTracker() {
            modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/MotionTracker.prefab");
            iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/Icons/motionTrackerIcon.png");
        }

        public override void SetupAttributes() {
            base.SetupAttributes();
        }

        public override void Install() {
            base.Install();
            CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
        }

        public override void Uninstall() {
            base.Uninstall();
            CharacterBody.onBodyInventoryChangedGlobal -= CharacterBody_onBodyInventoryChangedGlobal;
        }



        ////// Hooks //////

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo) {
            if(self && damageInfo.attacker) {
                var mtt = damageInfo.attacker.GetComponent<MotionTrackerTracker>();
                var count = GetCount(damageInfo.attacker.GetComponent<CharacterBody>());
                if(mtt && count > 0) {
                    mtt.SetInCombat(self.gameObject);
                    damageInfo.damage *= 1f + mtt.GetCombatBonusScalar(self.gameObject) * count;
                }
            }

            orig(self, damageInfo);

            if(self && self.body && damageInfo.attacker) {
                var mtt = self.body.GetComponent<MotionTrackerTracker>();
                if(mtt)
                    mtt.SetInCombat(damageInfo.attacker);
            }
        }

        private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody body) {
            if(GetCount(body) > 0 && !body.GetComponent<MotionTrackerTracker>())
                body.gameObject.AddComponent<MotionTrackerTracker>();
        }

    }

    public class MotionTrackerTracker : MonoBehaviour {
        const float COMBAT_TIMER = 6f;

        Dictionary<GameObject, (float stopwatch, float duration)> activeCombatants = new Dictionary<GameObject, (float, float)>();

        public float GetCombatBonusScalar(GameObject with) {
            if(!activeCombatants.ContainsKey(with))
                return 0f;
            return Mathf.Clamp01(activeCombatants[with].duration / MotionTracker.instance.damageTime) * MotionTracker.instance.damageFrac;
        }

        public void SetInCombat(GameObject with) {
            if(activeCombatants.ContainsKey(with))
                activeCombatants[with] = (COMBAT_TIMER, activeCombatants[with].duration);
            else
                activeCombatants[with] = (COMBAT_TIMER, 0f);
        }

        void FixedUpdate() {
            var frozenCombatants = activeCombatants.ToArray();
            foreach(var kvp in frozenCombatants) {
                var nsw = kvp.Value.stopwatch - Time.fixedDeltaTime;
                if(nsw <= 0f)
                    activeCombatants.Remove(kvp.Key);
                else
                    activeCombatants[kvp.Key] = (nsw, kvp.Value.duration + Time.fixedDeltaTime);
            }
        }
    }
}