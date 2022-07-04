﻿using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using R2API;
using static TILER2.MiscUtil;
using UnityEngine.Networking;

namespace ThinkInvisible.TinkersSatchel {
	public class Skein : Item<Skein> {

		////// Item Data //////

		public override string displayName => "Spacetime Skein";
		public override ItemTier itemTier => ItemTier.Tier3;
		public override ReadOnlyCollection<ItemTag> itemTags => new(new[] { ItemTag.Utility, ItemTag.Damage });

		protected override string GetNameString(string langid = null) => displayName;
		protected override string GetPickupString(string langid = null) =>
			"Gain mass while stationary. Lose mass while moving.";
		protected override string GetDescString(string langid = null) =>
			$"Standing still reduces the next <style=cIsDamage>damage and knockback</style> you take by up to <style=cIsDamage>{Pct(highMassFrac)} <style=cStack>(+{Pct(highMassFrac)} per stack, hyperbolic)</style></style>. Moving increasing your <style=cIsUtility>move and attack speed</style> by up to <style=cIsUtility>{Pct(lowMassFrac)} <style=cStack>(+{Pct(lowMassFrac)} per stack, linear)</style></style>. Effect ramps up over {massChangeDuration:N0} seconds, and is lost once you start or stop moving (latter has a brief grace period).";
		protected override string GetLoreString(string langid = null) => "";



		////// Config //////

		[AutoConfigRoOSlider("{0:P0}", 0f, 10f)]
		[AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
		[AutoConfig("Maximum damage/knockback to block per stack (hyperbolic).", AutoConfigFlags.PreventNetMismatch, 0f, 1f)]
		public float highMassFrac { get; private set; } = 0.5f;

		[AutoConfigRoOSlider("{0:P0}", 0f, 10f)]
		[AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage | AutoConfigUpdateActionTypes.InvalidateStats)]
		[AutoConfig("Maximum speed to add per stack (linear).", AutoConfigFlags.PreventNetMismatch, 0f, 1f)]
		public float lowMassFrac { get; private set; } = 0.5f;

		[AutoConfigRoOSlider("{0:N0} s", 0f, 30f)]
		[AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
		[AutoConfig("Time required to reach maximum buff, in seconds.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
		public float massChangeDuration { get; private set; } = 5f;

		[AutoConfigRoOSlider("{0:N0} s", 0f, 5f)]
		[AutoConfig("Time required to register a movement stop, in seconds.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
		public float moveGracePeriod { get; private set; } = 0.25f;



		////// Other Fields/Properties //////
		
		public BuffDef speedBuff { get; private set; }
		public BuffDef resistBuff { get; private set; }
		internal static UnlockableDef unlockable;
		public GameObject idrPrefab { get; private set; }



		////// TILER2 Module Setup //////

		public Skein() {
			modelResource = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Skein.prefab");
			iconResource = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/ItemIcons/skeinIcon.png");
			idrPrefab = TinkersSatchelPlugin.resources.LoadAsset<GameObject>("Assets/TinkersSatchel/Prefabs/Items/Display/Skein.prefab");
		}

		public override void SetupModifyItemDef() {
			base.SetupModifyItemDef();

			CommonCode.RetrieveDefaultMaterials(idrPrefab.GetComponent<ItemDisplay>());

			#region ItemDisplayRule Definitions

			/// Survivors ///
			displayRules.Add("Bandit2Body", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "Chest",
				localPos = new Vector3(0.16662F, 0.23603F, -0.2328F),
				localAngles = new Vector3(283.0797F, 259.6789F, 87.20558F),
				localScale = new Vector3(0.23F, 0.23F, 0.23F)
			});
			displayRules.Add("CaptainBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "Head",
				localPos = new Vector3(-0.21289F, 0.14872F, -0.10543F),
				localAngles = new Vector3(53.26719F, 171.0046F, 197.6588F),
				localScale = new Vector3(0.12F, 0.12F, 0.12F)
			});
			displayRules.Add("CommandoBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "Chest",
				localPos = new Vector3(-0.00118F, 0.178F, -0.28754F),
				localAngles = new Vector3(359.0837F, 93.76512F, 332.8747F),
				localScale = new Vector3(0.5F, 0.5F, 0.5F)
			});
			displayRules.Add("CrocoBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "SpineChest1",
				localPos = new Vector3(0.06337F, 1.26103F, -0.69081F),
				localAngles = new Vector3(2.39033F, 269.3893F, 47.91893F),
				localScale = new Vector3(4F, 4F, 4F)
			});
			displayRules.Add("EngiBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "Pelvis",
				localPos = new Vector3(0.24835F, 0.13692F, 0.12219F),
				localAngles = new Vector3(19.74273F, 338.7649F, 343.2596F),
				localScale = new Vector3(0.3F, 0.3F, 0.3F)
			});
			displayRules.Add("HuntressBody", new ItemDisplayRule {
				childName = "Stomach",
				localPos = new Vector3(0.17437F, -0.01902F, 0.11239F),
				localAngles = new Vector3(14.62809F, 338.0782F, 18.2589F),
				localScale = new Vector3(0.3F, 0.3F, 0.3F),
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab
			});
			displayRules.Add("LoaderBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "MechBase",
				localPos = new Vector3(0.28481F, -0.22564F, -0.12889F),
				localAngles = new Vector3(0.98176F, 51.91312F, 23.00177F),
				localScale = new Vector3(0.3F, 0.3F, 0.3F)
			});
			displayRules.Add("MageBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "Pelvis",
				localPos = new Vector3(0.16876F, -0.10376F, 0.02998F),
				localAngles = new Vector3(357.5521F, 355.006F, 105.9485F),
				localScale = new Vector3(0.25F, 0.25F, 0.25F)
			});
			displayRules.Add("MercBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "ThighR",
				localPos = new Vector3(-0.08794F, 0.03176F, -0.06409F),
				localAngles = new Vector3(350.6662F, 317.2625F, 21.97947F),
				localScale = new Vector3(0.3F, 0.3F, 0.3F)
			});
			displayRules.Add("ToolbotBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "Chest",
				localPos = new Vector3(2.33895F, -0.34548F, 0.80107F),
				localAngles = new Vector3(311.4177F, 7.89006F, 354.1869F),
				localScale = new Vector3(3F, 3F, 3F)
			});
			displayRules.Add("TreebotBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "PlatformBase",
				localPos = new Vector3(0.75783F, -0.10773F, 0.00385F),
				localAngles = new Vector3(308.2326F, 10.8672F, 329.0782F),
				localScale = new Vector3(1F, 1F, 1F)
			});
			displayRules.Add("RailgunnerBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "Backpack",
				localPos = new Vector3(0.28636F, -0.3815F, -0.06912F),
				localAngles = new Vector3(352.4358F, 63.85439F, 6.83272F),
				localScale = new Vector3(0.3F, 0.3F, 0.3F)
			});
			displayRules.Add("VoidSurvivorBody", new ItemDisplayRule {
				ruleType = ItemDisplayRuleType.ParentedPrefab,
				followerPrefab = idrPrefab,
				childName = "Stomach",
				localPos = new Vector3(0.17554F, -0.13447F, -0.0436F),
				localAngles = new Vector3(15.08189F, 9.51543F, 15.89409F),
				localScale = new Vector3(0.3F, 0.3F, 0.3F)
			});
			#endregion
		}

		public override void SetupAttributes() {
			base.SetupAttributes();

			speedBuff = ScriptableObject.CreateInstance<BuffDef>();
			speedBuff.buffColor = Color.white;
			speedBuff.canStack = true;
			speedBuff.isDebuff = false;
			speedBuff.name = "TKSATSkeinSpeed";
			speedBuff.iconSprite = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/MiscIcons/skeinSpeedBuffIcon.png");
			ContentAddition.AddBuffDef(speedBuff);

			resistBuff = ScriptableObject.CreateInstance<BuffDef>();
			resistBuff.buffColor = Color.white;
			resistBuff.canStack = true;
			resistBuff.isDebuff = false;
			resistBuff.name = "TKSATSkeinResist";
			resistBuff.iconSprite = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/MiscIcons/skeinResistBuffIcon.png");
			ContentAddition.AddBuffDef(resistBuff);

			var achiNameToken = $"ACHIEVEMENT_TKSAT_{name.ToUpper(System.Globalization.CultureInfo.InvariantCulture)}_NAME";
			var achiDescToken = $"ACHIEVEMENT_TKSAT_{name.ToUpper(System.Globalization.CultureInfo.InvariantCulture)}_DESCRIPTION";
			unlockable = ScriptableObject.CreateInstance<UnlockableDef>();
			unlockable.cachedName = $"TkSat_{name}Unlockable";
			unlockable.sortScore = 200;
			unlockable.achievementIcon = TinkersSatchelPlugin.resources.LoadAsset<Sprite>("Assets/TinkersSatchel/Textures/UnlockIcons/skeinIcon.png");
			ContentAddition.AddUnlockableDef(unlockable);
			LanguageAPI.Add(achiNameToken, "Phenomenal Cosmic Power");
			LanguageAPI.Add(achiDescToken, "Complete all 4 Item Set achievements from Tinker's Satchel.");
			itemDef.unlockableDef = unlockable;
		}

		public override void Install() {
			base.Install();
			CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
			RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
		}

        public override void Uninstall() {
			base.Uninstall();
			CharacterBody.onBodyInventoryChangedGlobal -= CharacterBody_onBodyInventoryChangedGlobal;
			RecalculateStatsAPI.GetStatCoefficients -= RecalculateStatsAPI_GetStatCoefficients;
			On.RoR2.HealthComponent.TakeDamage -= HealthComponent_TakeDamage;
		}



		////// Hooks //////
		
		private void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody body) {
			if(GetCount(body) > 0 && !body.GetComponent<SkeinTracker>())
				body.gameObject.AddComponent<SkeinTracker>();
		}

		private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args) {
			if(!sender) return;
			var count = GetCount(sender);
			var cpt = sender.GetComponent<SkeinTracker>();
			if(count > 0 && cpt) {
				var fac = cpt.GetMovementScalar() * count * lowMassFrac;
				args.moveSpeedMultAdd += fac;
				args.attackSpeedMultAdd += fac;
            }
		}

		private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo) {
			if(self && self.body) {
				var count = GetCount(self.body);
				var cpt = self.GetComponent<SkeinTracker>();
				if(count > 0 && cpt) {
					var fac = 1f - (1f - Mathf.Pow(highMassFrac, count)) * cpt.GetResistanceScalar();
					damageInfo.damage *= fac;
					if(damageInfo.canRejectForce)
						damageInfo.force *= fac;
					cpt.ForceResetStopped();
                }
            }
			orig(self, damageInfo);
		}
	}

	[RequireComponent(typeof(CharacterBody))]
	public class SkeinTracker : MonoBehaviour {
		const float RECALC_TICK_RATE = 0.2f;

		float movingStopwatch = 0f;
		float shortNotMovingStopwatch = 0f;
		float tickStopwatch = 0f;
		bool isStopped = false;

		Vector3 prevPos;

		CharacterBody body;

		public float GetMovementScalar() {
			if(isStopped) return 0;
			return Mathf.Clamp01(movingStopwatch / Skein.instance.massChangeDuration);
        }

		public float GetResistanceScalar() {
			if(!isStopped) return 0;
			return Mathf.Clamp01(shortNotMovingStopwatch / Skein.instance.massChangeDuration);
		}

		public void ForceResetStopped() {
			shortNotMovingStopwatch = 0f;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
		void Awake() {
			body = GetComponent<CharacterBody>();
			prevPos = body.transform.position;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Unity Engine.")]
		void FixedUpdate() {
			if(!body || !NetworkServer.active) return;
			float minMove = 0.1f * Time.fixedDeltaTime;
			if((body.transform.position - prevPos).sqrMagnitude <= minMove * minMove) {
				shortNotMovingStopwatch += Time.fixedDeltaTime;
				if(!isStopped) {
					if(shortNotMovingStopwatch > Skein.instance.moveGracePeriod) {
						movingStopwatch = 0f;
						isStopped = true;
						body.statsDirty = true;
						body.SetBuffCount(Skein.instance.speedBuff.buffIndex, 0);
					} else movingStopwatch += Time.fixedDeltaTime;
                }
			} else {
				if(isStopped) {
					body.SetBuffCount(Skein.instance.resistBuff.buffIndex, 0);
					isStopped = false;
				}
				movingStopwatch += Time.fixedDeltaTime;
				shortNotMovingStopwatch = 0f;
			}

			prevPos = body.transform.position;

			tickStopwatch -= Time.fixedDeltaTime;
			if(tickStopwatch <= 0f) {
				tickStopwatch = RECALC_TICK_RATE;
				if(!isStopped) body.statsDirty = true;
				body.SetBuffCount((isStopped ? Skein.instance.resistBuff : Skein.instance.speedBuff).buffIndex,
					Mathf.FloorToInt((isStopped ? GetResistanceScalar() : GetMovementScalar()) * 100));
			}
        }
    }

	[RegisterAchievement("TkSat_Skein", "TkSat_SkeinUnlockable", "")]
	public class TkSatSkeinAchievement : RoR2.Achievements.BaseAchievement {
		public override void OnInstall() {
			base.OnInstall();
            On.RoR2.RoR2Application.Update += RoR2Application_Update;
		}

        public override void OnUninstall() {
			base.OnUninstall();
			On.RoR2.RoR2Application.Update -= RoR2Application_Update;
		}

		float stopwatch = 0f;
		private void RoR2Application_Update(On.RoR2.RoR2Application.orig_Update orig, RoR2Application self) {
			orig(self);
			stopwatch -= Time.deltaTime;
			if(stopwatch <= 0f) {
				stopwatch = 1f;
				if(userProfile.HasUnlockable(Defib.unlockable)
					&& userProfile.HasUnlockable(ShootToHeal.unlockable)
					&& userProfile.HasUnlockable(Pinball.unlockable)
					&& userProfile.HasUnlockable(Lodestone.unlockable))
					Grant();
			}
		}
	}
}