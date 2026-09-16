using System;
using System.Linq;
using BepInEx;
using UnityEngine;

namespace PrayerClarityResearch
{
    [BepInPlugin("prayerclarity.research.visualaudition", "PrayerClarity Visual Audition Probe", "0.1.0")]
    public sealed class PrayerClarityVisualAuditionProbe : BaseUnityPlugin
    {
        private const string PlayerName = "Player(Clone)";
        private const string HeroPath = "content/character/char_hero";
        private const string ShardPath = "content/character/char_hero/shard_charge_fx";

        private Transform _hero;
        private GameObject _shardSource;
        private GameObject _prayTrackSource;
        private GameObject _prayBurstSource;
        private GameObject _goldAura;
        private GameObject _prayAura;
        private float _nextInitAttempt;
        private bool _ready;

        private void Awake()
        {
            Logger.LogInfo("Visual audition probe loaded. F2=gold aura, F3=pray-track aura, F4=combined, F5=one-shot blessing burst, F6=off.");
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
                ShowGoldAura();
            }
            else if (Input.GetKeyDown(KeyCode.F3))
            {
                ShowPrayAura();
            }
            else if (Input.GetKeyDown(KeyCode.F4))
            {
                ShowCombinedAura();
            }
            else if (Input.GetKeyDown(KeyCode.F5))
            {
                PlayBlessingBurst();
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
            if (_hero == null || _shardSource == null)
            {
                return false;
            }

            // Research-only one-time loaded-object inventory. Sources are inactive stock church FX,
            // so GameObject.Find cannot resolve them. No recurring scan is performed after success.
            var particles = Resources.FindObjectsOfTypeAll<ParticleSystem>();
            var prayTrack = particles.FirstOrDefault(p =>
                p != null &&
                p.name == "pray_track_fx" &&
                GetPath(p.transform).IndexOf("church_pulpit", StringComparison.OrdinalIgnoreCase) >= 0);

            var rays = particles.FirstOrDefault(p =>
                p != null &&
                p.name == "rays_fx" &&
                GetPath(p.transform).IndexOf("church_pulpit", StringComparison.OrdinalIgnoreCase) >= 0);

            _prayTrackSource = prayTrack?.gameObject;
            _prayBurstSource = rays?.transform.parent?.gameObject;

            if (_prayTrackSource == null || _prayBurstSource == null)
            {
                Logger.LogWarning("Player FX resolved, but church PrayFX sources are not loaded yet; retrying.");
                return false;
            }

            Logger.LogInfo("Visual audition ready. F2=gold aura, F3=pray-track aura, F4=combined, F5=blessing burst, F6=off.");
            return true;
        }

        private void ShowGoldAura()
        {
            DisablePersistentEffects();
            _goldAura = CloneUnderHero(_shardSource, "PrayerClarity Audition - Gold Aura");
            ConfigureGold(_goldAura);
            ActivateParticles(_goldAura);
            Logger.LogInfo("Visual audition: GOLD AURA active.");
        }

        private void ShowPrayAura()
        {
            DisablePersistentEffects();
            _prayAura = CloneUnderHero(_prayTrackSource, "PrayerClarity Audition - Pray Track Aura");
            ActivateParticles(_prayAura);
            Logger.LogInfo("Visual audition: PRAY-TRACK AURA active.");
        }

        private void ShowCombinedAura()
        {
            DisablePersistentEffects();
            _goldAura = CloneUnderHero(_shardSource, "PrayerClarity Audition - Gold Aura");
            ConfigureGold(_goldAura);
            ActivateParticles(_goldAura);
            _prayAura = CloneUnderHero(_prayTrackSource, "PrayerClarity Audition - Pray Track Aura");
            ActivateParticles(_prayAura);
            Logger.LogInfo("Visual audition: COMBINED AURA active.");
        }

        private void PlayBlessingBurst()
        {
            var burst = CloneUnderHero(_prayBurstSource, "PrayerClarity Audition - Blessing Burst");
            burst.SetActive(true);
            var systems = burst.GetComponentsInChildren<ParticleSystem>(true);
            foreach (var ps in systems)
            {
                var main = ps.main;
                main.loop = false;
                ps.gameObject.SetActive(true);
                ps.Clear(true);
                ps.Play(true);
            }

            Destroy(burst, 2.5f);
            Logger.LogInfo("Visual audition: BLESSING BURST played.");
        }

        private GameObject CloneUnderHero(GameObject source, string name)
        {
            var clone = Instantiate(source, _hero, false);
            clone.name = name;
            clone.transform.localPosition = Vector3.zero;
            clone.transform.localRotation = Quaternion.identity;
            clone.SetActive(true);
            return clone;
        }

        private static void ConfigureGold(GameObject root)
        {
            if (root == null)
            {
                return;
            }

            var gold = new Color(1f, 0.78f, 0.22f, 0.22f);
            foreach (var ps in root.GetComponentsInChildren<ParticleSystem>(true))
            {
                var main = ps.main;
                main.startColor = gold;
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
                ps.Play(true);
            }
        }

        private void DisablePersistentEffects()
        {
            if (_goldAura != null)
            {
                Destroy(_goldAura);
                _goldAura = null;
            }

            if (_prayAura != null)
            {
                Destroy(_prayAura);
                _prayAura = null;
            }

            Logger.LogInfo("Visual audition: persistent effects OFF.");
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
