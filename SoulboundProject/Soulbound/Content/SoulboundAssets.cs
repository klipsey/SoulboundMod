using RoR2;
using UnityEngine;
using SoulboundMod.Modules;
using RoR2.Projectile;
using RoR2.UI;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using R2API;
using UnityEngine.Rendering.PostProcessing;
using ThreeEyedGames;
using SoulboundMod.Soulbound.Components;

namespace SoulboundMod.Soulbound.Content
{
    public static class SoulboundAssets
    {
        //AssetBundle
        internal static AssetBundle mainAssetBundle;

        //Materials
        internal static Material commandoMat;
        internal static Material fireMat;

        //Shader
        internal static Shader hotpoo = Resources.Load<Shader>("Shaders/Deferred/HGStandard");

        //Effects
        internal static GameObject bloodSplatterEffect;
        internal static GameObject bloodExplosionEffect;
        internal static GameObject bloodSpurtEffect;

        internal static GameObject batSwingEffect;
        internal static GameObject swordSwingEffect;
        internal static GameObject batHitEffect;

        internal static GameObject batHitEffectRed;

        internal static GameObject dashEffect;

        internal static GameObject interrogatorGuilty;
        internal static GameObject interrogatorConvicted;
        internal static GameObject interrogatorConvictedConsume;

        //Models

        //Projectiles
        internal static GameObject arrowPrefab;
        //Sounds
        internal static NetworkSoundEventDef batImpactSoundEvent;
        internal static NetworkSoundEventDef swordImpactSoundEvent;

        //Colors
        internal static Color interrogatorColor = new Color(255f / 255f, 191f / 255f, 102f / 255f);
        internal static Color interrogatorSecondaryColor = new Color(70f / 255f, 63f / 255f, 94f / 255f);

        //Crosshair
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
                Object.DestroyImmediate(startingTrans.GetChild(num).gameObject);
            }
        }

        private static void CreateMaterials()
        {
            fireMat = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/GreaterWisp/matGreaterWispFire.mat").WaitForCompletion());
            fireMat.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture>("RoR2/Base/Common/ColorRamps/texRampWispSoul.png").WaitForCompletion());
        }

        private static void CreateModels()
        {
        }
        #region effects
        private static void CreateEffects()
        {
            bloodExplosionEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ImpBoss/ImpBossBlink.prefab").WaitForCompletion().InstantiateClone("DriverBloodExplosion", false);

            Material bloodMat = Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matBloodHumanLarge.mat").WaitForCompletion();
            Material bloodMat2 = Addressables.LoadAssetAsync<Material>("RoR2/Base/moon2/matBloodSiphon.mat").WaitForCompletion();

            bloodExplosionEffect.transform.Find("Particles/LongLifeNoiseTrails").GetComponent<ParticleSystemRenderer>().material = bloodMat;
            bloodExplosionEffect.transform.Find("Particles/LongLifeNoiseTrails, Bright").GetComponent<ParticleSystemRenderer>().material = bloodMat;
            bloodExplosionEffect.transform.Find("Particles/Dash").GetComponent<ParticleSystemRenderer>().material = bloodMat;
            bloodExplosionEffect.transform.Find("Particles/Dash, Bright").GetComponent<ParticleSystemRenderer>().material = bloodMat;
            bloodExplosionEffect.transform.Find("Particles/DashRings").GetComponent<ParticleSystemRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/Base/moon2/matBloodSiphon.mat").WaitForCompletion();
            bloodExplosionEffect.GetComponentInChildren<Light>().gameObject.SetActive(false);

            bloodExplosionEffect.GetComponentInChildren<PostProcessVolume>().sharedProfile = Addressables.LoadAssetAsync <PostProcessProfile>("RoR2/Base/title/ppLocalGold.asset").WaitForCompletion();

            Modules.Content.CreateAndAddEffectDef(bloodExplosionEffect);

            bloodSpurtEffect = mainAssetBundle.LoadAsset<GameObject>("BloodSpurtEffect");

            bloodSpurtEffect.transform.Find("Blood").GetComponent<ParticleSystemRenderer>().material = bloodMat2;
            bloodSpurtEffect.transform.Find("Trails").GetComponent<ParticleSystemRenderer>().trailMaterial = bloodMat2;

            dashEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherDashEffect.prefab").WaitForCompletion().InstantiateClone("SoulboundDashEffect");
            dashEffect.AddComponent<NetworkIdentity>();
            Object.Destroy(dashEffect.transform.Find("Point light").gameObject);
            Object.Destroy(dashEffect.transform.Find("Flash, White").gameObject);
            Object.Destroy(dashEffect.transform.Find("NoiseTrails").gameObject);
            dashEffect.transform.Find("Donut").localScale *= 0.5f;
            dashEffect.transform.Find("Donut, Distortion").localScale *= 0.5f;
            dashEffect.transform.Find("Dash").GetComponent<ParticleSystemRenderer>().material.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture>("RoR2/Base/Common/ColorRamps/texRampDefault.png").WaitForCompletion());
            dashEffect.transform.Find("Dash").GetComponent<ParticleSystemRenderer>().material.SetColor("_TintColor", interrogatorColor);
            Modules.Content.CreateAndAddEffectDef(dashEffect);

            batHitEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandit2/HitsparkBandit.prefab").WaitForCompletion().InstantiateClone("InterreogatorBatHitEffect");
            batHitEffect.AddComponent<NetworkIdentity>();
            Modules.Content.CreateAndAddEffectDef(batHitEffect);

            batHitEffectRed = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandit2/HitsparkBandit.prefab").WaitForCompletion().InstantiateClone("InterreogatorBatRedHitEffect");
            batHitEffectRed.AddComponent<NetworkIdentity>();
            batHitEffectRed.transform.Find("Particles").Find("TriangleSparksLarge").gameObject.GetComponent<ParticleSystemRenderer>().material.SetColor("_TintColor", Color.red);
            batHitEffectRed.transform.Find("Particles").Find("TriangleSparks").gameObject.GetComponent<ParticleSystemRenderer>().material.SetColor("_TintColor", Color.red);
            Modules.Content.CreateAndAddEffectDef(batHitEffectRed);

            batSwingEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Merc/MercSwordSlash.prefab").WaitForCompletion().InstantiateClone("SoulboundBatSwing", false);
            batSwingEffect.transform.GetChild(0).GetComponent<ParticleSystemRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/Base/Huntress/matHuntressSwingTrail.mat").WaitForCompletion();
            var swing = batSwingEffect.transform.GetChild(0).GetComponent<ParticleSystem>().main;
            swing.startLifetimeMultiplier *= 2f;

            swordSwingEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Merc/MercSwordSlash.prefab").WaitForCompletion().InstantiateClone("SoulboundswordSwing", false);
            swordSwingEffect.transform.GetChild(0).localScale *= 1.5f;
            swordSwingEffect.transform.GetChild(0).GetComponent<ParticleSystemRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matGenericSwingTrail.mat").WaitForCompletion();
            swordSwingEffect.transform.GetChild(0).GetComponent<ParticleSystemRenderer>().material.SetColor("_TintColor", interrogatorColor);
            swing = swordSwingEffect.transform.GetChild(0).GetComponent<ParticleSystem>().main;
            swing.startLifetimeMultiplier *= 2f;

            bloodSplatterEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherSlamImpact.prefab").WaitForCompletion().InstantiateClone("SoulboundSplat", true);
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
            SoulboundMod.Modules.Content.CreateAndAddEffectDef(bloodSplatterEffect);

            interrogatorConvicted = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/SlowOnHit/SlowDownTime.prefab").WaitForCompletion().InstantiateClone("Convicted", true);
            interrogatorConvicted.AddComponent<NetworkIdentity>();
            interrogatorConvicted.transform.Find("Visual").GetChild(0).gameObject.GetComponent<MeshRenderer>().materials[0].SetColor("_TintColor", new Color(166f / 255f, 159f / 255f, 20f / 255f));
            interrogatorConvicted.transform.Find("Visual").GetChild(1).gameObject.GetComponent<MeshRenderer>().materials[0].SetColor("_TintColor", new Color(166f / 255f, 159f / 255f, 20f / 255f));

            Material fakeMerc = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Merc/matMercExposed.mat").WaitForCompletion());
            fakeMerc.SetTexture("_MainTex", mainAssetBundle.LoadAsset<Texture>("texGuilty"));
            fakeMerc.SetColor("_TintColor", interrogatorColor);
            interrogatorGuilty = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Merc/MercExposeEffect.prefab").WaitForCompletion().InstantiateClone("Guilty", true);
            interrogatorGuilty.AddComponent<NetworkIdentity>();
            interrogatorGuilty.transform.Find("Visual, On").Find("PulseEffect, Ring").gameObject.GetComponent<ParticleSystemRenderer>().material = fakeMerc;

            fakeMerc = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Merc/matMercExposed.mat").WaitForCompletion());
            fakeMerc.SetTexture("_MainTex", mainAssetBundle.LoadAsset<Texture>("texGuilty"));
            fakeMerc.SetColor("_TintColor", Color.red);
            interrogatorConvictedConsume = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Merc/MercExposeConsumeEffect.prefab").WaitForCompletion().InstantiateClone("ConvictMarked", true);
            interrogatorConvictedConsume.AddComponent<NetworkIdentity>();
            interrogatorConvictedConsume.transform.Find("Visual, Consumed").Find("PulseEffect, Ring (1)").gameObject.GetComponent<ParticleSystemRenderer>().material = fakeMerc;
            interrogatorConvictedConsume.gameObject.GetComponent<EffectComponent>().soundName = "sfx_interrogator_point";
            Object.Destroy(interrogatorConvictedConsume.transform.Find("Visual, Consumed").Find("PulseEffect, Slash").gameObject);

            Modules.Content.CreateAndAddEffectDef(interrogatorConvictedConsume);
        }

        #endregion

        #region projectiles
        private static void CreateProjectiles()
        {
            arrowPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandit2/Bandit2ShivProjectile.prefab").WaitForCompletion().InstantiateClone("SoulboundCleaver");
            arrowPrefab.AddComponent<NetworkIdentity>();
            arrowPrefab.GetComponent<ProjectileSingleTargetImpact>().hitSoundString = "sfx_scout_cleaver_miss";
            arrowPrefab.GetComponent<ProjectileSingleTargetImpact>().enemyHitSoundString = "sfx_scout_cleaver_hit";
            arrowPrefab.GetComponent<SphereCollider>().radius = 0.5f;

            arrowPrefab.GetComponent<ProjectileController>().allowPrediction = true;

            arrowPrefab.GetComponent<ProjectileDamage>().damageType = DamageType.BonusToLowHealth;
            DamageAPI.ModdedDamageTypeHolderComponent moddedDamage = arrowPrefab.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>();
            moddedDamage.Add(DamageTypes.SoulboundStack);

            arrowPrefab.GetComponent<ProjectileController>().ghostPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandit2/Bandit2ShivGhostAlt.prefab").WaitForCompletion().InstantiateClone("SoulboundCleaverGhost");
            arrowPrefab.GetComponent<ProjectileController>().ghostPrefab.AddComponent<NetworkIdentity>();
            arrowPrefab.GetComponent<ProjectileSimple>().desiredForwardSpeed = 120f;
            TrailRenderer trail = arrowPrefab.AddComponent<TrailRenderer>();
            trail.startWidth = 0.5f;
            trail.endWidth = 0.1f;
            trail.time = 0.5f;
            trail.emitting = true;
            trail.numCornerVertices = 0;
            trail.numCapVertices = 0;
            trail.material = Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matSmokeTrail.mat").WaitForCompletion();
            trail.startColor = Color.white;
            trail.endColor = Color.gray;
            trail.alignment = LineAlignment.TransformZ;

            //arrowPrefab.GetComponent<ProjectileController>().ghostPrefab.transform.GetChild(0).gameObject.GetComponent<MeshFilter>().mesh = mainAssetBundle.LoadAsset<GameObject>("interrogatorCleaver").GetComponent<MeshFilter>().mesh;
            //arrowPrefab.GetComponent<ProjectileController>().ghostPrefab.transform.GetChild(0).localRotation = new Quaternion(90f, 0f, 90f, Quaternion.identity.w);
            //arrowPrefab.GetComponent<ProjectileController>().ghostPrefab.transform.GetChild(0).localScale = Vector3.one * 0.015f;
            //arrowPrefab.GetComponent<ProjectileController>().ghostPrefab.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().material = batMat;


            Modules.Content.AddProjectilePrefab(arrowPrefab);
        }
        #endregion

        #region sounds
        private static void CreateSounds()
        {
            batImpactSoundEvent = Modules.Content.CreateAndAddNetworkSoundEventDef("sfx_interrogator_self_damage");
            swordImpactSoundEvent = Modules.Content.CreateAndAddNetworkSoundEventDef("Play_merc_sword_impact");
        }
        #endregion

        private static void CreateUI()
        {
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

            SoulboundMod.Modules.Content.CreateAndAddEffectDef(newEffect);

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
        #endregion
    }
}