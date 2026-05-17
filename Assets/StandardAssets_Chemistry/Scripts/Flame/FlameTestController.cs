using System.Collections;
using UnityEngine;

namespace VirtualChemLab
{
    public class FlameTestController : MonoBehaviour
    {
        // ------------------------------------------------------------------
        //  Main Flame Particle System
        //  This single PS drives both Color over Lifetime (startColor tint)
        //  and the emission glow on its material.
        //  Chemically accurate peak colors come from FlameTestDatabase cations:
        //    Na⁺  →  #FF9400  (intense yellow-orange,  589 nm D-line)
        //    K⁺   →  #BF7FFF  (lilac / soft violet,    766 nm)
        //    Li⁺  →  #FF1A1A  (crimson red,            670 nm)
        //    Cu²⁺ →  #00E676  (blue-green,             510 nm)
        //    Ca²⁺ →  #FF6B35  (brick orange,           622 nm)
        //    Sr²⁺ →  #FF2222  (scarlet red,            460/674 nm)
        //    Ba²⁺ →  #AAFF44  (pale yellow-green,      524 nm)
        // ------------------------------------------------------------------
        [Header("Main Flame Particle System")]
        public ParticleSystem flameParticles;
        public Material flameMaterial;
        public string flameEmissionProperty = "_EmissionColor";

        [Header("Flame Light")]
        public Light flameLight;

        [Header("Stick Renderer")]
        public Renderer wireTipRenderer;
        public string tipColorProperty = "_Color";

        [Header("Particle Systems  (accents)")]
        public ParticleSystem sparkParticles;
        public ParticleSystem flameTipParticles;

        [Header("Audio")]
        public AudioClip acidDipSound;
        public AudioClip saltPickupSound;
        public AudioClip flameBurstSound;
        private AudioSource _audio;

        [Header("UI / Feedback")]
        public TMPro.TMP_Text logText;
        public float logDisplayTime = 6f;

        public FlameTestPhase CurrentPhase { get; private set; } = FlameTestPhase.Idle;

        private FlameTestSalt _loadedSalt;
        private FlameTestCation _activeCation;

        private Color _defaultStartColor;
        private Color _defaultEmission;
        private float _defaultLightIntensity;

        private Coroutine _activeRoutine;

        private void Awake()
        {
            _audio = GetComponent<AudioSource>();
            if (_audio == null) _audio = gameObject.AddComponent<AudioSource>();

            if (flameParticles != null)
            {
                var col = flameParticles.colorOverLifetime;
                _defaultStartColor = col.enabled
                    ? col.color.gradient.colorKeys[0].color
                    : Color.white;
            }

            if (flameMaterial != null && flameMaterial.HasProperty(flameEmissionProperty))
                _defaultEmission = flameMaterial.GetColor(flameEmissionProperty);
            else
                _defaultEmission = Color.black;

            if (flameLight != null)
                _defaultLightIntensity = flameLight.intensity;
        }


        public void CleanStick()
        {
            if (CurrentPhase == FlameTestPhase.InFlame) return;
            if (_activeRoutine != null) StopCoroutine(_activeRoutine);
            _activeRoutine = StartCoroutine(CleanRoutine());
        }

        public void LoadSalt(string saltId)
        {
            if (CurrentPhase != FlameTestPhase.AcidDipped)
            {
                Log("[FlameTest] Clean the wire in HCl first!");
                return;
            }
            if (!FlameTestDatabase.TryGetSalt(saltId, out var salt))
            {
                Debug.LogWarning($"[FlameTest] Unknown salt id: {saltId}");
                return;
            }
            if (_activeRoutine != null) StopCoroutine(_activeRoutine);
            _activeRoutine = StartCoroutine(LoadSaltRoutine(salt));
        }

        public void TestFlame()
        {
            if (CurrentPhase != FlameTestPhase.SaltLoaded)
            {
                Log("[FlameTest] Load a salt onto the wire first!");
                return;
            }
            if (_activeRoutine != null) StopCoroutine(_activeRoutine);
            _activeRoutine = StartCoroutine(FlameTestRoutine());
        }

        public void Reset()
        {
            if (_activeRoutine != null) StopCoroutine(_activeRoutine);
            StopAllCoroutines();

            _loadedSalt = null;
            _activeCation = null;
            CurrentPhase = FlameTestPhase.Idle;

            SetTipColor(Color.white);
            ApplyFlameColor(_defaultStartColor, _defaultEmission, _defaultLightIntensity);

            StopAccentParticles();
            Log("");
        }

        private IEnumerator CleanRoutine()
        {
            CurrentPhase = FlameTestPhase.AcidDipping;
            PlaySound(acidDipSound);
            Log("[FlameTest] Dipping wire into concentrated HCl to clean it…");

            Color startTip = wireTipRenderer != null
                ? wireTipRenderer.material.GetColor(tipColorProperty)
                : Color.white;

            yield return LerpTipColor(startTip, new Color(0.85f, 0.95f, 1f), 0.3f);
            yield return new WaitForSeconds(0.8f);
            yield return LerpTipColor(new Color(0.85f, 0.95f, 1f), Color.white, 0.3f);

            _loadedSalt = null;
            _activeCation = null;
            CurrentPhase = FlameTestPhase.AcidDipped;
            Log("[FlameTest] Wire cleaned. Now dip into the unknown salt.");
        }

        private IEnumerator LoadSaltRoutine(FlameTestSalt salt)
        {
            CurrentPhase = FlameTestPhase.SaltDipping;
            PlaySound(saltPickupSound);
            Log($"[FlameTest] Picking up {salt.displayName} ({salt.formula})…");

            yield return LerpTipColor(Color.white, salt.saltColor, 0.4f);
            yield return new WaitForSeconds(0.5f);

            _loadedSalt = salt;
            CurrentPhase = FlameTestPhase.SaltLoaded;
            Log($"[FlameTest]Wire loaded with {salt.formula}. Hold it in the flame.");
        }

        private IEnumerator FlameTestRoutine()
        {
            if (_loadedSalt == null) { CurrentPhase = FlameTestPhase.Idle; yield break; }

            _activeCation = FlameTestDatabase.GetCationForSalt(_loadedSalt.id);
            CurrentPhase = FlameTestPhase.InFlame;

            if (_activeCation == null)
            {
                Log($"[FlameTest] No characteristic flame colour detected for {_loadedSalt.formula}.");
                yield return new WaitForSeconds(2f);
                CurrentPhase = FlameTestPhase.CoolingDown;
                yield return CoolDownRoutine(_defaultStartColor, _defaultEmission, _defaultLightIntensity);
                yield break;
            }

            PlaySound(flameBurstSound);

            Color peakParticleColor = _activeCation.flameColor;
            Color peakEmission = _activeCation.flameColor * (_activeCation.flameIntensity * 2f);
            float peakLightI = _defaultLightIntensity + _activeCation.flameIntensity * 3f;

            Color startParticleColor = _defaultStartColor;
            Color startEmission = _defaultEmission;
            float startLightI = flameLight != null ? flameLight.intensity : 0f;

            float t = 0f, dur = _activeCation.colorRiseTime;
            while (t < dur)
            {
                t += Time.deltaTime;
                float p = Mathf.SmoothStep(0f, 1f, t / dur);
                ApplyFlameColor(
                    Color.Lerp(startParticleColor, peakParticleColor, p),
                    Color.Lerp(startEmission, peakEmission, p),
                    Mathf.Lerp(startLightI, peakLightI, p));
                yield return null;
            }
            ApplyFlameColor(peakParticleColor, peakEmission, peakLightI);

            if (_activeCation.produceSparks && sparkParticles != null)
            {
                var main = sparkParticles.main;
                main.startColor = _activeCation.sparkColor;
                sparkParticles.Play();
            }
            if (flameTipParticles != null)
            {
                var main2 = flameTipParticles.main;
                main2.startColor = _activeCation.flameColor;
                flameTipParticles.Play();
            }

            StartCoroutine(LerpTipColor(_loadedSalt.saltColor, _activeCation.flameColor, 0.3f));

            Log($"[FlameTest] Flame colour: <color=#{ColorUtility.ToHtmlStringRGB(_activeCation.flameColor)}" +
                $"><b>{_activeCation.flameColorName}</b></color>  →  " +
                $"Cation: {_activeCation.symbol} ({_activeCation.elementName})\n" +
                _activeCation.logMessage);

            // ----- Hold at peak -----------------------------------------------
            yield return new WaitForSeconds(_activeCation.sustainTime);

            StopAccentParticles();

            // ----- Cool down --------------------------------------------------
            CurrentPhase = FlameTestPhase.CoolingDown;
            yield return CoolDownRoutine(peakParticleColor, peakEmission, peakLightI);
        }

        private IEnumerator CoolDownRoutine(Color fromParticleColor, Color fromEmission, float fromLightI)
        {
            float t = 0f;
            float dur = _activeCation != null ? _activeCation.fadeTime : 0.8f;

            while (t < dur)
            {
                t += Time.deltaTime;
                float p = Mathf.SmoothStep(0f, 1f, t / dur);
                ApplyFlameColor(
                    Color.Lerp(fromParticleColor, _defaultStartColor, p),
                    Color.Lerp(fromEmission, _defaultEmission, p),
                    Mathf.Lerp(fromLightI, _defaultLightIntensity, p));
                yield return null;
            }

            ApplyFlameColor(_defaultStartColor, _defaultEmission, _defaultLightIntensity);
            SetTipColor(Color.white);
            CurrentPhase = FlameTestPhase.AcidDipped;
        }

        private void ApplyFlameColor(Color particleColor, Color emissionColor, float lightIntensity = -1f)
        {
            if (flameParticles != null)
            {
                var col = flameParticles.colorOverLifetime;
                if (col.enabled)
                {
                    Gradient grad = col.color.gradient;

                    GradientColorKey[] colorKeys = grad.colorKeys;
                    for (int i = 0; i < colorKeys.Length; i++)
                        colorKeys[i].color = particleColor;

                    grad.SetKeys(colorKeys, grad.alphaKeys);

                    var minMaxGrad = new ParticleSystem.MinMaxGradient(grad);
                    col.color = minMaxGrad;
                }
                else
                {
                    var main = flameParticles.main;
                    var sc = main.startColor;
                    particleColor.a = sc.color.a;
                    sc.color = particleColor;
                    main.startColor = sc;
                }
            }

            if (flameMaterial != null && flameMaterial.HasProperty(flameEmissionProperty))
                flameMaterial.SetColor(flameEmissionProperty, emissionColor);

            if (flameLight != null)
            {
                flameLight.color = particleColor;
                if (lightIntensity >= 0f)
                    flameLight.intensity = lightIntensity;
            }
        }

        private void SetTipColor(Color c)
        {
            if (wireTipRenderer != null)
                wireTipRenderer.material.SetColor(tipColorProperty, c);
        }

        private IEnumerator LerpTipColor(Color from, Color to, float duration)
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                SetTipColor(Color.Lerp(from, to, t / duration));
                yield return null;
            }
            SetTipColor(to);
        }

        private void StopAccentParticles()
        {
            if (sparkParticles != null && sparkParticles.isPlaying) sparkParticles.Stop();
            if (flameTipParticles != null && flameTipParticles.isPlaying) flameTipParticles.Stop();
        }

        private void PlaySound(AudioClip clip)
        {
            if (_audio != null && clip != null)
                _audio.PlayOneShot(clip);
        }

        private Coroutine _logClearRoutine;
        private void Log(string msg)
        {
            Debug.Log($"[FlameTest] {msg}");
            if (logText == null) return;
            logText.text = msg;
            if (_logClearRoutine != null) StopCoroutine(_logClearRoutine);
            if (!string.IsNullOrEmpty(msg))
                _logClearRoutine = StartCoroutine(ClearLogAfter(logDisplayTime));
        }

        private IEnumerator ClearLogAfter(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (logText != null) logText.text = "";
        }
    }
}