using System;
using System.Linq;
using BepInEx;
using UnityEngine;

namespace PrayerClarityResearch
{
    [BepInPlugin("prayerclarity.research.visualaudition", "PrayerClarity Visual Audition Probe", "0.1.1")]
    public sealed class PrayerClarityVisualAuditionProbe : BaseUnityPlugin
    {
        private const string PlayerName = "Player(Clone)";
        private const string HeroPath = "content/character/char_hero";
        private const string ShardPath = "content/character/char_hero/shard_charge_fx";
        private const string ToolFlamePath = "content/character/char_hero/tool/fire/fire (1)";
        private const string ToolSpritePath = "content/character/char_hero/tool/tool sprite (front)";

        private Transform _hero;
        private Transform _toolSprite;
        private GameObject _shardSource;
        private GameObject _prayTrackSource;
        private GameObject _toolFlameSource;
        private GameObject _goldAura;
        private GameObject _prayAura;
        private GameObject _weaponFlame;
        private float _nextInitAttempt;
        private bool _ready;

        private static readonly Color SoftGold = new Color(1f, 0.78f, 0.22f, 0.24f);
        private static readonly Color PrayerGold = new Color(1f, 0.88f, 0.48f, 0.42f);
        private static readonly Color FlameGold = new Color(1f, 0.72f, 0.18f, 0.72f);

        private void Awake()
        {
            Logger.LogInfo("Visual audition 0.1.1 loaded. F2=scaled gold aura, F3=player-bound pray aura, F4=combined, F5=gold tool/weapon flame, F6=off.");
        }

        private void Update()
        {
            if (!_ready)
            {
                if (Time.unscaledTime < _nextInitAttempt)
                {
                    return;
                }

                _nextInitAttempt = Time.unscaledTime + 2f;
                _ready = TryResolveSources();
                return;
            }

            if (Input.GetKeyDown(KeyCode.F2))
            {
                ShowScaledGoldAura();
            }
            else if (Input.GetKeyDown(KeyCode.F3))
            {
                ShowPlayerBoundPrayAura();
            }
            else if (Input.GetKeyDown(KeyCode.F4))
            {
                ShowCombinedAura();
            }
            else if (Input.GetKeyDown(KeyCode.F5))
            {
                ShowWeaponFlame();
            }
            else if (Input.GetKeyDown(KeyCode.F6))
            {
                DisablePersistentEffects();
            }
        }

        private bool TryResolveSources()
        {
            var player = GameObject.Find(PlayerName);
            if (player == null)
            {
                return false;
            }

            _hero = player.transform.Find(HeroPath);
            _shardSource = player.transform.Find(ShardPath)?.gameObject;
            _toolFlameSource = player.transform.Find(ToolFlamePath)?.gameObject;
            _toolSprite = player.transform.Find(ToolSpritePath);
            if (_hero == null || _shardSource == null || _toolFlameSource == null || _toolSprite == null)
            {
                return false;
            }

            // Research-only one-time loaded-object inventory. The church source is inactive stock PrayFX,
            // so GameObject.Find cannot resolve it. No recurring scan is performed after success.
            var particles = Resources.FindObjectsOfTypeAll<ParticleSystem>();
            var prayTrack = particles.FirstOrDefault(p =>
                p != null &&
                p.name == "pray_track_fx" &&
                GetPath(p.transform).IndexOf("church_pulpit", StringComparison.OrdinalIgnoreCase) >= 0);

            _prayTrackSource = prayTrack?.gameObject;
            if (_prayTrackSource == null)
            {
                Logger.LogWarning("Player FX resolved, but church pray_track_fx is not loaded yet; retrying.");
                return false;
            }

            Logger.LogInfo("Visual audition 0.1.1 ready. F2=scaled gold aura, F3=local pray aura, F4=combined, F5=gold tool/weapon flame, F6=off.");
            return true;
        }

        private void ShowScaledGoldAura()
        {
            DisablePersistentEffects();
            _goldAura = CloneUnder(_shardSource, _hero, "PrayerClarity Audition - Scaled Gold Aura");
            _goldAura.transform.localScale = Vector3.one * 3f;
            ConfigureParticles(_goldAura, SoftGold, 1.15f, true);
            ActivateParticles(_goldAura);
            Logger.LogInfo("Visual audition: SCALED GOLD AURA active.");
        }

        private void ShowPlayerBoundPrayAura()
        {
            DisablePersistentEffects();
            _prayAura = CloneUnder(_prayTrackSource, _hero, "PrayerClarity Audition - Local Pray Aura");
            ConfigureParticles(_prayAura, PrayerGold, 1f, true);
            ActivateParticles(_prayAura);
            Logger.LogInfo("Visual audition: PLAYER-BOUND PRAY AURA active.");
        }

        private void ShowCombinedAura()
        {
            DisablePersistentEffects();

            _goldAura = CloneUnder(_shardSource, _hero, "PrayerClarity Audition - Scaled Gold Aura");
            _goldAura.transform.localScale = Vector3.one * 3f;
            ConfigureParticles(_goldAura, SoftGold, 1.15f, true);
            ActivateParticles(_goldAura);

            _prayAura = CloneUnder(_prayTrackSource, _hero, "PrayerClarity Audition - Local Pray Aura");
            ConfigureParticles(_prayAura, PrayerGold, 1f, true);
            ActivateParticles(_prayAura);

            Logger.LogInfo("Visual audition: COMBINED LOCAL AURA active.");
        }

        private void ShowWeaponFlame()
        {
            DisablePersistentEffects();
            _weaponFlame = CloneUnder(_toolFlameSource, _toolSprite, "PrayerClarity Audition - Gold Weapon Flame");
            _weaponFlame.transform.localScale = Vector3.one * 0.55f;
            ConfigureParticles(_weaponFlame, FlameGold, 0.4f, true);
            ActivateParticles(_weaponFlame);
            Logger.LogInfo("Visual audition: GOLD TOOL/WEAPON FLAME active.");
        }

        private static GameObject CloneUnder(GameObject source, Transform parent, string name)
        {
            var clone = Instantiate(source, parent, false);
            clone.name = name;
            clone.transform.localPosition = Vector3.zero;
            clone.transform.localRotation = Quaternion.identity;
            clone.SetActive(true);
            return clone;
        }

        private static void ConfigureParticles(GameObject root, Color color, float sizeMultiplier, bool localSimulation)
        {
            if (root == null)
            {
                return;
            }

            foreach (var ps in root.GetComponentsInChildren<ParticleSystem>(true))
            {
                var main = ps.main;
                main.startColor = color;
                main.startSizeMultiplier *= sizeMultiplier;
                main.scalingMode = ParticleSystemScalingMode.Hierarchy;
                if (localSimulation)
                {
                    main.simulationSpace = ParticleSystemSimulationSpace.Local;
                }

                var emission = ps.emission;
                emission.enabled = true;
            }
        }

        private static void ActivateParticles(GameObject root)
        {
            if (root == null)
            {
                return;
            }

            root.SetActive(true);
            foreach (var ps in root.GetComponentsInChildren<ParticleSystem>(true))
            {
                ps.gameObject.SetActive(true);
                var emission = ps.emission;
                emission.enabled = true;
                ps.Clear(true);
                ps.Play(true);
            }
        }

        private void DisablePersistentEffects()
        {
            DestroyAndClear(ref _goldAura);
            DestroyAndClear(ref _prayAura);
            DestroyAndClear(ref _weaponFlame);
            Logger.LogInfo("Visual audition: persistent effects OFF.");
        }

        private static void DestroyAndClear(ref GameObject go)
        {
            if (go == null)
            {
                return;
            }

            Destroy(go);
            go = null;
        }

        private static string GetPath(Transform transform)
        {
            if (transform == null)
            {
                return string.Empty;
            }

            var path = transform.name;
            var current = transform.parent;
            while (current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }

            return path;
        }

        private void OnDestroy()
        {
            DisablePersistentEffects();
        }
    }
}
