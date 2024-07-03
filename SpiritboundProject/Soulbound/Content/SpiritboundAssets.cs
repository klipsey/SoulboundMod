using RoR2;
using UnityEngine;
using SpiritboundMod.Modules;
using RoR2.Projectile;
using RoR2.UI;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using R2API;
using UnityEngine.Rendering.PostProcessing;
using ThreeEyedGames;
using SpiritboundMod.Spiritbound.Components;
using System.Reflection;
using System;
using UnityEngine.Diagnostics;

namespace SpiritboundMod.Spiritbound.Content
{
    public static class SpiritboundAssets
    {
        //AssetBundle
        internal static AssetBundle mainAssetBundle;

        //Materials
        internal static Material commandoMat;
        internal static Material fireMat;
        internal static Material fireMatInFront;
        internal static Material spiritBodyMat;
        internal static Material spiritFurMat;
        internal static Material absorbMaterial;

        //Shader
        internal static Shader hotpoo = Resources.Load<Shader>("Shaders/Deferred/HGStandard");

        //Effects
        internal static GameObject bloodSplatterEffect;
        internal static GameObject bloodExplosionEffect;
        internal static GameObject bloodSpurtEffect;

        internal static GameObject batHitEffectRed;
        internal static GameObject spiritboundHitEffect;
        internal static GameObject dashEffect;
        internal static GameObject spiritBiteEffect;

        internal static GameObject chargeEffect;
        internal static GameObject fullChargeEffect;
        internal static GameObject arrowMuzzleFlashEffect;

        internal static GameObject consumeOrb;

        //Models

        //Projectiles
        internal static GameObject arrowPrefab;
        internal static GameObject arrowGhostPrefab;

        internal static GameObject chargedArrowPrefab;
        internal static GameObject chargedArrowGhostPrefab;
        //Sounds
        internal static NetworkSoundEventDef biteImpactSoundEvent;
        internal static NetworkSoundEventDef swordImpactSoundEvent;

        //Colors
        internal static Color spiritBoundColor = new Color(166f / 255f, 138f / 255f, 242f / 255f);
        internal static Color spiritBoundSecondaryColor = new Color(186f / 255f, 202 / 255f, 255f / 255f);

        //Crosshair
        internal static GameObject spiritboundCrosshair;
        public static void Init(AssetBundle assetBundle)
        {
            mainAssetBundle = assetBundle;

            CreateMaterials();

            CreateModels();

            CreateEffects();

            CreateSounds();

            CreateProjectiles();

            CreateUI();
        }

        private static void CleanChildren(Transform startingTrans)
        {
            for (int num = startingTrans.childCount - 1; num >= 0; num--)
            {
                if (startingTrans.GetChild(num).childCount > 0)
                {
                    CleanChildren(startingTrans.GetChild(num));
                }
                UnityEngine.Object.DestroyImmediate(startingTrans.GetChild(num).gameObject);
            }
        }

        private static void CreateMaterials()
        {
            fireMat = UnityEngine.Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/GreaterWisp/matGreaterWispFire.mat").WaitForCompletion());
            fireMat.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture>("RoR2/Base/Common/ColorRamps/texRampWispSoul.png").WaitForCompletion());

            fireMatInFront = UnityEngine.Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/GreaterWisp/matGreaterWispFire.mat").WaitForCompletion());
            fireMatInFront.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture>("RoR2/Base/Common/ColorRamps/texRampWispSoul.png").WaitForCompletion());
            fireMatInFront.SetFloat("_DepthOffset", -10f);

            spiritBodyMat = UnityEngine.Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Gravekeeper/matGravekeeperDiffuse.mat").WaitForCompletion());
            spiritBodyMat.SetTexture("_MainTex", mainAssetBundle.LoadAsset<Texture>("texSpiritDiffuse"));

            spiritFurMat = UnityEngine.Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Gravekeeper/matGravekeeperFurDiffuse.mat").WaitForCompletion());
            spiritFurMat.SetTexture("_MainTex", mainAssetBundle.LoadAsset<Texture>("texSpiritFurDiffuse"));

            absorbMaterial = Addressables.LoadAssetAsync<Material>("RoR2/Base/Merc/matMercEvisTarget.mat").WaitForCompletion();
        }

        private static void CreateModels()
        {
        }
        #region effects
        private static void CreateEffects()
        {
            consumeOrb = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/Effects/OrbEffects/InfusionOrbEffect"), "SpiritboundConsumeOrbEffect", true);
            if (!consumeOrb.GetComponent<NetworkIdentity>()) consumeOrb.AddComponent<NetworkIdentity>();

            TrailRenderer trail = consumeOrb.transform.Find("TrailParent").Find("Trail").GetComponent<TrailRenderer>();
            trail.widthMultiplier = 0.35f;
            trail.material = Addressables.LoadAssetAsync<Material>("RoR2/Base/moon2/matBloodSiphon.mat").WaitForCompletion();

            consumeOrb.transform.Find("VFX").Find("Core").GetComponent<ParticleSystemRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matBloodHumanLarge.mat").WaitForCompletion();
            consumeOrb.transform.Find("VFX").localScale = Vector3.one * 0.5f;

            consumeOrb.transform.Find("VFX").Find("Core").localScale = Vector3.one * 4.5f;

            consumeOrb.transform.Find("VFX").Find("PulseGlow").GetComponent<ParticleSystemRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matOmniRing2Generic.mat").WaitForCompletion();


            SpiritboundMod.Modules.Content.CreateAndAddEffectDef(consumeOrb);

            bloodExplosionEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ImpBoss/ImpBossBlink.prefab").WaitForCompletion().InstantiateClone("SpiritbombExplosion", false);

            Material bloodMat2 = UnityEngine.Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matBloodHumanLarge.mat").WaitForCompletion());
            Material bloodMat4 = UnityEngine.Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/moon2/matBloodSiphon.mat").WaitForCompletion());

            bloodExplosionEffect.transform.Find("Particles/LongLifeNoiseTrails").GetComponent<ParticleSystemRenderer>().material = bloodMat2;
            bloodExplosionEffect.transform.Find("Particles/LongLifeNoiseTrails").GetComponent<ParticleSystemRenderer>().material.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture>("RoR2/Base/Common/ColorRamps/texRampHuntressSoft.png").WaitForCompletion());
            bloodExplosionEffect.transform.Find("Particles/LongLifeNoiseTrails, Bright").GetComponent<ParticleSystemRenderer>().material = bloodMat2;
            bloodExplosionEffect.transform.Find("Particles/LongLifeNoiseTrails, Bright").GetComponent<ParticleSystemRenderer>().material.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture>("RoR2/Base/Common/ColorRamps/texRampHuntressSoft.png").WaitForCompletion());

            bloodExplosionEffect.transform.Find("Particles/Dash").GetComponent<ParticleSystemRenderer>().material = bloodMat2;
            bloodExplosionEffect.transform.Find("Particles/Dash").GetComponent<ParticleSystemRenderer>().material.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture>("RoR2/Base/Common/ColorRamps/texRampHuntressSoft.png").WaitForCompletion());
            bloodExplosionEffect.transform.Find("Particles/Dash, Bright").GetComponent<ParticleSystemRenderer>().material = bloodMat2;
            bloodExplosionEffect.transform.Find("Particles/Dash, Bright").GetComponent<ParticleSystemRenderer>().material.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture>("RoR2/Base/Common/ColorRamps/texRampHuntressSoft.png").WaitForCompletion());
            bloodExplosionEffect.transform.Find("Particles/DashRings").GetComponent<ParticleSystemRenderer>().material = bloodMat4;
            bloodExplosionEffect.transform.Find("Particles/DashRings").GetComponent<ParticleSystemRenderer>().material.SetColor("_TintColor", Color.cyan);
            bloodExplosionEffect.GetComponentInChildren<Light>().gameObject.SetActive(false);

            bloodExplosionEffect.GetComponentInChildren<PostProcessVolume>().sharedProfile = Addressables.LoadAssetAsync<PostProcessProfile>("RoR2/Base/title/ppLocalGold.asset").WaitForCompletion();

            Modules.Content.CreateAndAddEffectDef(bloodExplosionEffect);

            bloodSpurtEffect = mainAssetBundle.LoadAsset<GameObject>("BloodSpurtEffect");

            bloodSpurtEffect.transform.Find("Blood").GetComponent<ParticleSystemRenderer>().material = bloodMat2;
            bloodSpurtEffect.transform.Find("Trails").GetComponent<ParticleSystemRenderer>().trailMaterial = bloodMat2;

            dashEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherDashEffect.prefab").WaitForCompletion().InstantiateClone("SpiritboundDashEffect");
            dashEffect.AddComponent<NetworkIdentity>();
            UnityEngine.Object.Destroy(dashEffect.transform.Find("Point light").gameObject);
            UnityEngine.Object.Destroy(dashEffect.transform.Find("Flash, White").gameObject);
            UnityEngine.Object.Destroy(dashEffect.transform.Find("NoiseTrails").gameObject);
            dashEffect.transform.Find("Donut").localScale *= 0.5f;
            dashEffect.transform.Find("Donut, Distortion").localScale *= 0.5f;
            dashEffect.transform.Find("Dash").GetComponent<ParticleSystemRenderer>().material.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture>("RoR2/Base/Common/ColorRamps/texRampDefault.png").WaitForCompletion());
            dashEffect.transform.Find("Dash").GetComponent<ParticleSystemRenderer>().material.SetColor("_TintColor", spiritBoundColor);
            Modules.Content.CreateAndAddEffectDef(dashEffect);

            bloodSplatterEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherSlamImpact.prefab").WaitForCompletion().InstantiateClone("SpiritboundSplat", true);
            bloodSplatterEffect.AddComponent<NetworkIdentity>();
            bloodSplatterEffect.transform.GetChild(0).gameObject.SetActive(false);
            bloodSplatterEffect.transform.GetChild(1).gameObject.SetActive(false);
            bloodSplatterEffect.transform.GetChild(2).gameObject.SetActive(false);
            bloodSplatterEffect.transform.GetChild(3).gameObject.SetActive(false);
            bloodSplatterEffect.transform.GetChild(4).gameObject.SetActive(false);
            bloodSplatterEffect.transform.GetChild(5).gameObject.SetActive(false);
            bloodSplatterEffect.transform.GetChild(6).gameObject.SetActive(false);
            bloodSplatterEffect.transform.GetChild(7).gameObject.SetActive(false);
            bloodSplatterEffect.transform.GetChild(8).gameObject.SetActive(false);
            bloodSplatterEffect.transform.GetChild(9).gameObject.SetActive(false);
            bloodSplatterEffect.transform.GetChild(10).gameObject.SetActive(false);
            bloodSplatterEffect.transform.Find("Decal").GetComponent<Decal>().Material = Addressables.LoadAssetAsync<Material>("RoR2/Base/Imp/matImpDecal.mat").WaitForCompletion();
            bloodSplatterEffect.transform.Find("Decal").GetComponent<AnimateShaderAlpha>().timeMax = 10f;
            bloodSplatterEffect.transform.GetChild(12).gameObject.SetActive(false);
            bloodSplatterEffect.transform.GetChild(13).gameObject.SetActive(false);
            bloodSplatterEffect.transform.GetChild(14).gameObject.SetActive(false);
            bloodSplatterEffect.transform.GetChild(15).gameObject.SetActive(false);
            bloodSplatterEffect.transform.localScale = Vector3.one;
            SpiritboundMod.Modules.Content.CreateAndAddEffectDef(bloodSplatterEffect);

            spiritboundHitEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Merc/OmniImpactVFXSlashMerc.prefab").WaitForCompletion().InstantiateClone("SpiritboundImpact", false);
            if(!spiritboundHitEffect.GetComponent<NetworkIdentity>()) spiritboundHitEffect.AddComponent<NetworkIdentity>();
            spiritboundHitEffect.GetComponent<OmniEffect>().enabled = false;
            spiritboundHitEffect.GetComponent<EffectComponent>().soundName = "Play_acrid_m2_bite_hit";
            Material material = UnityEngine.Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Merc/matOmniHitspark3Merc.mat").WaitForCompletion());
            material.SetColor("_TintColor", Color.red);
            spiritboundHitEffect.transform.GetChild(1).gameObject.GetComponent<ParticleSystemRenderer>().material = material;
            spiritboundHitEffect.transform.GetChild(2).localScale = Vector3.one * 1.5f;
            spiritboundHitEffect.transform.GetChild(2).gameObject.GetComponent<ParticleSystemRenderer>().material = UnityEngine.Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/DLC1/VoidSurvivor/matVoidSurvivorBlasterFireCorrupted.mat").WaitForCompletion());
            material = UnityEngine.Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Imp/matImpSlashImpact.mat").WaitForCompletion());
            spiritboundHitEffect.transform.GetChild(5).gameObject.GetComponent<ParticleSystemRenderer>().material = material;
            spiritboundHitEffect.transform.GetChild(4).localScale = Vector3.one * 3f;
            spiritboundHitEffect.transform.GetChild(4).gameObject.GetComponent<ParticleSystemRenderer>().material = UnityEngine.Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Imp/matImpDust.mat").WaitForCompletion());
            spiritboundHitEffect.transform.GetChild(6).GetChild(0).gameObject.GetComponent<ParticleSystemRenderer>().material = UnityEngine.Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/DLC1/Common/Void/matOmniHitspark1Void.mat").WaitForCompletion());
            spiritboundHitEffect.transform.GetChild(6).gameObject.GetComponent<ParticleSystemRenderer>().material = UnityEngine.Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/DLC1/Common/Void/matOmniHitspark2Void.mat").WaitForCompletion());
            spiritboundHitEffect.transform.GetChild(1).localScale = Vector3.one * 1.5f;
            spiritboundHitEffect.transform.GetChild(1).gameObject.SetActive(true);
            spiritboundHitEffect.transform.GetChild(2).gameObject.SetActive(true);
            spiritboundHitEffect.transform.GetChild(3).gameObject.SetActive(true);
            spiritboundHitEffect.transform.GetChild(4).gameObject.SetActive(true);
            spiritboundHitEffect.transform.GetChild(5).gameObject.SetActive(true);
            spiritboundHitEffect.transform.GetChild(6).gameObject.SetActive(true);
            spiritboundHitEffect.transform.GetChild(6).GetChild(0).gameObject.SetActive(true);
            spiritboundHitEffect.transform.GetChild(6).transform.localScale = new Vector3(1f, 1f, 3f);
            spiritboundHitEffect.transform.localScale = Vector3.one * 1.5f;
            Modules.Content.CreateAndAddEffectDef(spiritboundHitEffect);

            spiritBiteEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Vermin/VerminBiteEffect.prefab").WaitForCompletion().InstantiateClone("SpiritboundBiteEffect", true);
            if (!spiritBiteEffect.GetComponent<NetworkIdentity>()) spiritBiteEffect.AddComponent<NetworkIdentity>();
            spiritBiteEffect.transform.Find("SwingTrail").gameObject.GetComponent<ParticleSystemRenderer>().material.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture>("RoR2/DLC1/Common/ColorRamps/texRampVoidArenaShield.png").WaitForCompletion());
            spiritBiteEffect.transform.Find("Goo").gameObject.GetComponent<ParticleSystemRenderer>().material.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture>("RoR2/DLC1/Common/ColorRamps/texRampVoidArenaShield.png").WaitForCompletion());
            spiritBiteEffect.transform.Find("Point Light").gameObject.GetComponent<Light>().color = spiritBoundColor;

            chargeEffect = mainAssetBundle.LoadAsset<GameObject>("ChargingEffect");
            var objectScaleCurve = chargeEffect.AddComponent<ObjectScaleCurve>();
            objectScaleCurve.overallCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
            objectScaleCurve.useOverallCurveOnly = true;
            objectScaleCurve.timeMax = 3f;
            fullChargeEffect = mainAssetBundle.LoadAsset<GameObject>("ChargeFullEffect");

            arrowMuzzleFlashEffect = mainAssetBundle.LoadAsset<GameObject>("ChargeMuzzleFlash");
            var effectComponent = arrowMuzzleFlashEffect.AddComponent<EffectComponent>();
            effectComponent.soundName = "Play_mage_m1_shoot";
            effectComponent.positionAtReferencedTransform = true;
            effectComponent.parentToReferencedTransform = true;
            arrowMuzzleFlashEffect.AddComponent<DestroyOnTimer>().duration = 1f;

            Modules.Content.CreateAndAddEffectDef(arrowMuzzleFlashEffect);
        }

        #endregion

        #region projectiles
        private static void CreateProjectiles()
        {
            arrowGhostPrefab = mainAssetBundle.CreateProjectileGhostPrefab("ArrowGhost", false);

            arrowPrefab = CreateBlankPrefab("SpiritboundArrow", true);
            arrowPrefab.GetComponent<NetworkIdentity>().localPlayerAuthority = true;
            CopyChildren(mainAssetBundle.LoadAsset<GameObject>("ArrowProjectile"), arrowPrefab);
            if (!arrowPrefab.GetComponent<NetworkIdentity>()) arrowPrefab.AddComponent<NetworkIdentity>();
            var projectileController = arrowPrefab.AddComponent<ProjectileController>();
            projectileController.allowPrediction = true;
            projectileController.ghostPrefab = arrowGhostPrefab;
            arrowPrefab.AddComponent<ProjectileNetworkTransform>();
            var projectileSimple = arrowPrefab.AddComponent<ProjectileSimple>();
            projectileSimple.desiredForwardSpeed = 50f;
            projectileSimple.lifetime = 10f;
            var projectileDamage = arrowPrefab.AddComponent<ProjectileDamage>();
            var projectileSingleTargetImpact = arrowPrefab.AddComponent<ProjectileSingleTargetImpact>();
            projectileSingleTargetImpact.destroyOnWorld = true;
            projectileSingleTargetImpact.impactEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Huntress/OmniImpactVFXHuntress.prefab").WaitForCompletion();
            projectileSingleTargetImpact.hitSoundString = "Play_MULT_m1_smg_impact";
            arrowPrefab.AddComponent<AlignArrowComponent>();

            Modules.Content.AddProjectilePrefab(arrowPrefab);


            chargedArrowGhostPrefab = mainAssetBundle.CreateProjectileGhostPrefab("ArrowChargedGhost", false);

            chargedArrowPrefab = CreateBlankPrefab("ChargedSpiritboundArrow", true);
            chargedArrowPrefab.GetComponent<NetworkIdentity>().localPlayerAuthority = true;
            CopyChildren(mainAssetBundle.LoadAsset<GameObject>("ArrowProjectile"), chargedArrowPrefab);
            if (!chargedArrowPrefab.GetComponent<NetworkIdentity>()) chargedArrowPrefab.AddComponent<NetworkIdentity>();
            var projectileController2 = chargedArrowPrefab.AddComponent<ProjectileController>();
            projectileController2.allowPrediction = true;
            projectileController2.ghostPrefab = chargedArrowGhostPrefab;
            chargedArrowPrefab.AddComponent<ProjectileNetworkTransform>();
            var projectileSimple2 = chargedArrowPrefab.AddComponent<ProjectileSimple>();
            projectileSimple2.desiredForwardSpeed = 150f;
            projectileSimple2.lifetime = 10f;
            var projectileDamage2 = chargedArrowPrefab.AddComponent<ProjectileDamage>();
            projectileDamage2.damageType = DamageType.Generic;
            var projectileSingleTargetImpact2 = chargedArrowPrefab.AddComponent<ProjectileSingleTargetImpact>();
            projectileSingleTargetImpact2.destroyOnWorld = true;
            projectileSingleTargetImpact2.impactEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Huntress/OmniImpactVFXHuntress.prefab").WaitForCompletion();
            projectileSingleTargetImpact2.hitSoundString = "Play_MULT_m1_smg_impact";
            var aa = chargedArrowPrefab.AddComponent<AlignArrowComponent>();
            chargedArrowPrefab.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>();

            Modules.Content.AddProjectilePrefab(chargedArrowPrefab);
        }
        #endregion

        #region sounds
        private static void CreateSounds()
        {
            biteImpactSoundEvent = Modules.Content.CreateAndAddNetworkSoundEventDef("Play_acrid_m2_bite_hit");
            swordImpactSoundEvent = Modules.Content.CreateAndAddNetworkSoundEventDef("Play_merc_sword_impact");
        }
        #endregion

        private static void CreateUI()
        {
            spiritboundCrosshair = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Captain/CaptainCrosshair.prefab").WaitForCompletion().InstantiateClone("SpiritboundCrosshair", false);
            spiritboundCrosshair.transform.Find("Circle").gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(200f, 200f);
            spiritboundCrosshair.transform.Find("Circle (1)").gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(150f, 150f);
        }

        #region helpers
        private static GameObject CreateImpactExplosionEffect(string effectName, Material bloodMat, Material decal, float scale = 1f)
        {
            GameObject newEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherSlamImpact.prefab").WaitForCompletion().InstantiateClone(effectName, true);

            newEffect.transform.Find("Spikes, Small").gameObject.SetActive(false);

            newEffect.transform.Find("PP").gameObject.SetActive(false);
            newEffect.transform.Find("Point light").gameObject.SetActive(false);
            newEffect.transform.Find("Flash Lines").GetComponent<ParticleSystemRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matOpaqueDustLargeDirectional.mat").WaitForCompletion();

            newEffect.transform.GetChild(3).GetComponent<ParticleSystemRenderer>().material = bloodMat;
            newEffect.transform.Find("Flash Lines, Fire").GetComponent<ParticleSystemRenderer>().material = bloodMat;
            newEffect.transform.GetChild(6).GetComponent<ParticleSystemRenderer>().material = bloodMat;
            newEffect.transform.Find("Fire").GetComponent<ParticleSystemRenderer>().material = bloodMat;

            var boom = newEffect.transform.Find("Fire").GetComponent<ParticleSystem>().main;
            boom.startLifetimeMultiplier = 0.5f;
            boom = newEffect.transform.Find("Flash Lines, Fire").GetComponent<ParticleSystem>().main;
            boom.startLifetimeMultiplier = 0.3f;
            boom = newEffect.transform.GetChild(6).GetComponent<ParticleSystem>().main;
            boom.startLifetimeMultiplier = 0.4f;

            newEffect.transform.Find("Physics").GetComponent<ParticleSystemRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/Base/MagmaWorm/matFracturedGround.mat").WaitForCompletion();

            newEffect.transform.Find("Decal").GetComponent<Decal>().Material = decal;
            newEffect.transform.Find("Decal").GetComponent<AnimateShaderAlpha>().timeMax = 10f;

            newEffect.transform.Find("FoamSplash").gameObject.SetActive(false);
            newEffect.transform.Find("FoamBilllboard").gameObject.SetActive(false);
            newEffect.transform.Find("Dust").gameObject.SetActive(false);
            newEffect.transform.Find("Dust, Directional").gameObject.SetActive(false);

            newEffect.transform.localScale = Vector3.one * scale;

            newEffect.AddComponent<NetworkIdentity>();

            ParticleSystemColorFromEffectData PSCFED = newEffect.AddComponent<ParticleSystemColorFromEffectData>();
            PSCFED.particleSystems = new ParticleSystem[]
            {
                newEffect.transform.Find("Fire").GetComponent<ParticleSystem>(),
                newEffect.transform.Find("Flash Lines, Fire").GetComponent<ParticleSystem>(),
                newEffect.transform.GetChild(6).GetComponent<ParticleSystem>(),
                newEffect.transform.GetChild(3).GetComponent<ParticleSystem>()
            };
            PSCFED.effectComponent = newEffect.GetComponent<EffectComponent>();

            SpiritboundMod.Modules.Content.CreateAndAddEffectDef(newEffect);

            return newEffect;
        }
        public static Material CreateMaterial(string materialName, float emission, Color emissionColor, float normalStrength)
        {
            if (!commandoMat) commandoMat = Resources.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody").GetComponentInChildren<CharacterModel>().baseRendererInfos[0].defaultMaterial;

            Material mat = UnityEngine.Object.Instantiate<Material>(commandoMat);
            Material tempMat = mainAssetBundle.LoadAsset<Material>(materialName);

            if (!tempMat) return commandoMat;

            mat.name = materialName;
            mat.SetColor("_Color", tempMat.GetColor("_Color"));
            mat.SetTexture("_MainTex", tempMat.GetTexture("_MainTex"));
            mat.SetColor("_EmColor", emissionColor);
            mat.SetFloat("_EmPower", emission);
            mat.SetTexture("_EmTex", tempMat.GetTexture("_EmissionMap"));
            mat.SetFloat("_NormalStrength", normalStrength);

            return mat;
        }

        public static Material CreateMaterial(string materialName)
        {
            return CreateMaterial(materialName, 0f);
        }

        public static Material CreateMaterial(string materialName, float emission)
        {
            return CreateMaterial(materialName, emission, Color.black);
        }

        public static Material CreateMaterial(string materialName, float emission, Color emissionColor)
        {
            return CreateMaterial(materialName, emission, emissionColor, 0f);
        }

        public static GameObject CreateBlankPrefab(string name = "GameObject", bool network = false)
        {
            GameObject gameObject = new GameObject(name).InstantiateClone(name, registerNetwork: false);
            if (network)
            {
                gameObject.AddComponent<NetworkIdentity>();
                gameObject.RegisterNetworkPrefab();
            }

            return gameObject;
        }

        public static void CopyChildren(GameObject from, GameObject to, bool cloneFromThenDestroy = true)
        {
            string name = to.name;
            if (cloneFromThenDestroy)
            {
                from = from.InstantiateClone(from.name + "Copy", registerNetwork: false);
            }

            Transform parent = to.transform.parent;
            int childCount = from.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                from.transform.GetChild(0).SetParent(to.transform, worldPositionStays: false);
            }

            Component[] components = from.GetComponents<Component>();
            foreach (Component component in components)
            {
                Type type = component.GetType();
                Component component2 = to.GetComponent(type);
                if (type != typeof(Transform) && (Attribute.GetCustomAttribute(type, typeof(DisallowMultipleComponent)) == null || !component2))
                {
                    component2 = to.AddComponent(type);
                }

                bool flag = typeof(Animator).IsAssignableFrom(component.GetType());
                bool logWarnings = false;
                if (flag)
                {
                    Animator animator = (Animator)component;
                    Animator animator2 = (Animator)component2;
                    logWarnings = animator.logWarnings;
                    animator.logWarnings = false;
                    animator2.logWarnings = false;
                }

                BindingFlags bindingAttr = BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                PropertyInfo[] properties = type.GetProperties(bindingAttr);
                foreach (PropertyInfo propertyInfo in properties)
                {
                    if (propertyInfo.CanWrite)
                    {
                        try
                        {
                            propertyInfo.SetValue(component2, propertyInfo.GetValue(component));
                        }
                        catch
                        {
                        }
                    }
                }

                FieldInfo[] fields = type.GetFields(bindingAttr);
                foreach (FieldInfo fieldInfo in fields)
                {
                    fieldInfo.SetValue(component2, fieldInfo.GetValue(component));
                }

                if (flag)
                {
                    Animator animator3 = (Animator)component;
                    Animator animator4 = (Animator)component2;
                    animator3.logWarnings = logWarnings;
                    animator4.logWarnings = logWarnings;
                }
            }

            to.transform.SetParent(parent);
            to.name = name;
            to.layer = from.layer;
            if (cloneFromThenDestroy)
            {
                UnityEngine.Object.Destroy(from);
            }
        }

        #endregion
    }
}