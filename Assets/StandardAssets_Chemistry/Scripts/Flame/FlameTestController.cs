using System.Collections;
using UnityEngine;

namespace VirtualChemLab
{
    public class FlameTestController : MonoBehaviour
    {

        [Header("Stick Renderer")]
        public Renderer wireTipRenderer;
        public string tipColorProperty = "_Color";
        public Material wireCleanMaterialTEST;

        [Header("Bunsen Flame")]
        public Renderer flameRenderer;
        public string flameEmissionProperty = "_Color";
        public Light flameLight;
        public Material flameCleanMaterialTEST;

        [Header("Particle Systems")]
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

        private Color _defaultFlameColor;
        private float _defaultFlameLightIntensity;

        private Coroutine _activeRoutine;

        private void Awake()
        {
            _audio = GetComponent<AudioSource>();
            if (_audio == null) _audio = gameObject.AddComponent<AudioSource>();

            if (flameRenderer != null)
                _defaultFlameColor = flameRenderer.material.GetColor(flameEmissionProperty);

            if (flameLight != null)
                _defaultFlameLightIntensity = flameLight.intensity;
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
                Log("Clean the wire in HCl first!");
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
                Log("Load a salt onto the wire first!");
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

            ApplyFlameColor(_defaultFlameColor, _defaultFlameLightIntensity);

            if (sparkParticles != null && sparkParticles.isPlaying) sparkParticles.Stop();
            if (flameTipParticles != null && flameTipParticles.isPlaying) flameTipParticles.Stop();

            Log("");
        }

        private IEnumerator CleanRoutine()
        {
            CurrentPhase = FlameTestPhase.AcidDipping;
            PlaySound(acidDipSound);
            Log("Dipping wire into concentrated HCl to clean it…");

            yield return LerpTipColor(wireTipRenderer != null
                ? wireTipRenderer.material.GetColor(tipColorProperty)
                : Color.white,
                new Color(0.85f, 0.95f, 1f), 0.3f);

            yield return new WaitForSeconds(0.8f);

            yield return LerpTipColor(new Color(0.85f, 0.95f, 1f), Color.white, 0.3f);

            _loadedSalt = null;
            _activeCation = null;
            CurrentPhase = FlameTestPhase.AcidDipped;
            Log("Wire cleaned. Now dip into the unknown salt.");
        }

        private IEnumerator LoadSaltRoutine(FlameTestSalt salt)
        {
            CurrentPhase = FlameTestPhase.SaltDipping;
            PlaySound(saltPickupSound);
            Log($"Picking up {salt.displayName} ({salt.formula})…");

            yield return LerpTipColor(Color.white, salt.saltColor, 0.4f);
            yield return new WaitForSeconds(0.5f);

            _loadedSalt = salt;
            CurrentPhase = FlameTestPhase.SaltLoaded;
            Log($"Wire loaded with {salt.formula}. Hold it in the flame.");
        }

        private IEnumerator FlameTestRoutine()
        {
            if (_loadedSalt == null) { CurrentPhase = FlameTestPhase.Idle; yield break; }

            _activeCation = FlameTestDatabase.GetCationForSalt(_loadedSalt.id);
            CurrentPhase = FlameTestPhase.InFlame;

            if (_activeCation == null)
            {
                Log($"No characteristic flame colour detected for {_loadedSalt.formula}.");
                yield return new WaitForSeconds(2f);
                CurrentPhase = FlameTestPhase.CoolingDown;
                yield return CoolDownRoutine(_defaultFlameColor);
                yield break;
            }

            PlaySound(flameBurstSound);

            float t = 0f;
            float dur = _activeCation.colorRiseTime;

            Color startFlame = _defaultFlameColor;
            Color peakFlame = _activeCation.flameColor *
                               (_activeCation.flameIntensity * 2f);

            float startLightI = flameLight != null ? flameLight.intensity : 0f;
            float peakLightI = _defaultFlameLightIntensity
                                + _activeCation.flameIntensity * 3f;

            while (t < dur)
            {
                t += Time.deltaTime;
                float p = Mathf.SmoothStep(0f, 1f, t / dur);
                ApplyFlameColor(Color.Lerp(startFlame, peakFlame, p),
                                Mathf.Lerp(startLightI, peakLightI, p));
                yield return null;
            }
            ApplyFlameColor(peakFlame, peakLightI);

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

            StartCoroutine(LerpTipColor(_loadedSalt.saltColor,
                                        _activeCation.flameColor, 0.3f));

            Log($"Flame colour: <color=#{ColorUtility.ToHtmlStringRGB(_activeCation.flameColor)}" +
                $"><b>{_activeCation.flameColorName}</b></color>  →  " +
                $"Cation: {_activeCation.symbol} ({_activeCation.elementName})\n" +
                _activeCation.logMessage);

            yield return new WaitForSeconds(_activeCation.sustainTime);

            if (sparkParticles != null && sparkParticles.isPlaying) sparkParticles.Stop();
            if (flameTipParticles != null && flameTipParticles.isPlaying) flameTipParticles.Stop();

            CurrentPhase = FlameTestPhase.CoolingDown;
            yield return CoolDownRoutine(peakFlame, peakLightI);
        }

        private IEnumerator CoolDownRoutine(Color fromColor, float fromLight = -1f)
        {
            if (fromLight < 0f)
                fromLight = flameLight != null ? flameLight.intensity : _defaultFlameLightIntensity;

            float t = 0f;
            float dur = _activeCation != null ? _activeCation.fadeTime : 0.8f;

            while (t < dur)
            {
                t += Time.deltaTime;
                float p = Mathf.SmoothStep(0f, 1f, t / dur);
                ApplyFlameColor(Color.Lerp(fromColor, _defaultFlameColor, p),
                                Mathf.Lerp(fromLight, _defaultFlameLightIntensity, p));
                yield return null;
            }
            ApplyFlameColor(_defaultFlameColor, _defaultFlameLightIntensity);
            SetTipColor(Color.white);
            CurrentPhase = FlameTestPhase.AcidDipped;
        }

        private void ApplyFlameColor(Color c, float lightIntensity = -1f)
        {
            if (flameRenderer != null)
                flameRenderer.material.SetColor(flameEmissionProperty, c);

            if (flameLight != null)
            {
                flameLight.color = c;
                if (lightIntensity >= 0f) flameLight.intensity = lightIntensity;
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