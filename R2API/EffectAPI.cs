﻿using UnityEngine.Networking;
using RoR2.Projectile;
using RoR2;
using UnityEngine;
using R2API.Utils;
namespace R2API {
    public static class EffectAPI {
        /// <summary>
        /// Please don't touch this value, Used by ice and Fire ring since they share the SAME roll
        /// 0 mean Unset, 1 = True, 2 = False
        /// </summary>
        public static int ringBuffer = 0;


        public static void ModdedHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim) {

            if (damageInfo.procCoefficient == 0 || !NetworkServer.active || (!(bool)damageInfo.attacker || damageInfo.procCoefficient <= 0))
                return;

            var Attacker = damageInfo.attacker.GetComponent<CharacterBody>();
            var characterBody = victim ? victim.GetComponent<CharacterBody>() : null;

            if (!Attacker)
                return;
            var master = Attacker.master;
            if (!master)
                return;
            damageInfo.procChainMask.LinkToManager();

            var inventory = master.inventory;
            var Team = Attacker.GetComponent<TeamComponent>();
            var attackerTeamIndex = Team ? Team.teamIndex : TeamIndex.Neutral;

            var aimOrigin = Attacker.aimOrigin;

            CustomItemAPI.OnHitEnemyEffects(self, damageInfo, victim);


            //SetOnFire . Can't really do much for this one
            var DamageType = (uint)(damageInfo.damageType & RoR2.DamageType.IgniteOnHit) > 0U ? 1 : 0;
            var CanSetFire = (damageInfo.damageType & RoR2.DamageType.IgniteOnHit) != RoR2.DamageType.Generic || Attacker.HasBuff(BuffIndex.AffixRed);
            //bool CanSetFire = (damageInfo.damageType & RoR2.DamageType.PercentIgniteOnHit) != RoR2.DamageType.Generic || Attacker.HasBuff(BuffIndex.AffixRed); //Depend on Dll version
            var num2 = CanSetFire ? 1 : 0;
            if ((DamageType | num2) != 0)
                DotController.InflictDot(victim, damageInfo.attacker, CanSetFire ? DotController.DotIndex.Burn : DotController.DotIndex.Burn, 4f * damageInfo.procCoefficient, 1f);
            //DotController.InflictDot(victim, damageInfo.attacker, CanSetFire ? DotController.DotIndex.PercentBurn : DotController.DotIndex.Burn, 4f * damageInfo.procCoefficient, 1f); //Depend on Dll version

            //Apply Ice Elite (Will have to wait for Buff Change for that)
            if ((Attacker.HasBuff(BuffIndex.AffixWhite) ? 1 : 0) > 0 && (bool)((Object)characterBody))
                characterBody.AddTimedBuff(BuffIndex.Slow80, 1.5f * damageInfo.procCoefficient);

            damageInfo.procChainMask.UnlinkToManager();
        }
        public static void ModdedHitAll(On.RoR2.GlobalEventManager.orig_OnHitAll orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim) {

            if ((double)damageInfo.procCoefficient == 0.0)
                return;
            var Host = NetworkServer.active ? 1 : 0;
            if (!(bool)((Object)damageInfo.attacker))
                return;
            var component = damageInfo.attacker.GetComponent<CharacterBody>();
            if (!(bool)((Object)component))
                return;
            var master = component.master;
            if (!(bool)((Object)master))
                return;
            var inventory = master.inventory;
            if (!(bool)((Object)master.inventory))
                return;
            damageInfo.procChainMask.LinkToManager();


            CustomItemAPI.OnHitAllEffects(self, damageInfo, victim);

            //Buff
            if ((component.HasBuff(BuffIndex.AffixBlue) ? 1 : 0) <= 0)
                return;
            var damage = damageInfo.damage * 0.5f;
            var force = 0.0f;
            var position = damageInfo.position;
#pragma warning disable CS0618 //Obsolete warning
            ProjectileManager.instance.FireProjectile(Resources.Load<GameObject>("Prefabs/Projectiles/LightningStake"), position, Quaternion.identity, damageInfo.attacker, damage, force, damageInfo.crit, DamageColorIndex.Item, (GameObject)null, -1f);
#pragma warning restore CS0618

            damageInfo.procChainMask.UnlinkToManager();
        }



        internal static void InitHooks() {

            On.RoR2.GlobalEventManager.OnHitEnemy += ModdedHitEnemy;
            On.RoR2.GlobalEventManager.OnHitAll += ModdedHitAll;

        }
    }
}
