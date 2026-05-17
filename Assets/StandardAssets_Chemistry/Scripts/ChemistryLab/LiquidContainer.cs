using UnityEngine;
using System.Collections;
using LiquidVolumeFX;
using UnityEngine.UI;

namespace VirtualChemLab
{
    public class LiquidContainer : MonoBehaviour
    {
        [Header("Chemical")]
        public string chemicalId = "HCl";

        [Header("Fill")]
        [Range(0f, 1f)] public float fillLevel = 0.6f;
        public bool isBeaker = false;

        [Header("Pour Settings")]
        public Transform pourPoint;
        public ParticleSystem pourParticles;

        [Header("Labels")]
        public Text labelName;
        public Text labelFormula;

        private LiquidVolume _lv;
        private Chemical _chemical;
        private bool _reactionActive;

        public float CurrentAmount { get; private set; }
        public string ChemicalId => chemicalId;
        public bool IsEmpty => CurrentAmount <= 0.01f;

        void Awake()
        {
            _lv = GetComponentInChildren<LiquidVolume>();
        }

        void Start()
        {
            CurrentAmount = fillLevel;
            ApplyChemical(chemicalId);
        }

        public void ApplyChemical(string id)
        {
            chemicalId = id;
            _chemical = ChemicalDatabase.GetChemical(id);

            if (_chemical == null)
            {
                Debug.LogWarning($"[LiquidContainer] Chemical '{id}' not found in database.");
                return;
            }

            ApplyVisuals(_chemical);
            UpdateLabels();
        }

        private void ApplyVisuals(Chemical c)
        {
            if (_lv == null) return;

            _lv.liquidColor1 = c.liquidColor;
            _lv.liquidColor2 = c.liquidColor2;
            _lv.murkiness = c.murkiness;
            _lv.turbulence1 = c.turbulence1;
            _lv.turbulence2 = c.turbulence2;
            _lv.level = CurrentAmount;

            _lv.foamVisibleFromBottom = c.hasFoam;
            if (c.hasFoam)
            {
                _lv.foamColor = c.foamColor;
                _lv.foamThickness = c.foamThickness;
                _lv.foamDensity = c.foamDensity;
            }

            _lv.smokeEnabled = c.hasSmoke;
            if (c.hasSmoke)
            {
                _lv.smokeColor = c.smokeColor;
                _lv.smokeSpeed = c.smokeSpeed;
            }
        }

        private void UpdateLabels()
        {
            if (_chemical == null) return;
            if (labelName) labelName.text = _chemical.displayName;
            if (labelFormula) labelFormula.text = _chemical.formula;
        }
        public void PlayReaction(ReactionResult result, float addedVolume)
        {
            if (_reactionActive) return;
            StartCoroutine(ReactionCoroutine(result, addedVolume));
        }

        private IEnumerator ReactionCoroutine(ReactionResult result, float addedVolume)
        {
            _reactionActive = true;

            float targetLevel = Mathf.Clamp01(CurrentAmount + addedVolume);

            Color startC1 = _lv.liquidColor1;
            Color startC2 = _lv.liquidColor2;
            float startMurk = _lv.murkiness;
            float elapsed = 0f;
            float duration = result.colorTransitionSpeed;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

                _lv.liquidColor1 = Color.Lerp(startC1, result.resultColor, t);
                _lv.liquidColor2 = Color.Lerp(startC2, result.resultColor2, t);
                _lv.murkiness = Mathf.Lerp(startMurk, result.resultMurkiness, t);
                _lv.level = Mathf.Lerp(CurrentAmount, targetLevel, t);

                yield return null;
            }

            CurrentAmount = targetLevel;
            _lv.level = CurrentAmount;

            if (result.produceFoam)
            {
                _lv.foamVisibleFromBottom = true;
                _lv.foamColor = result.foamColor;
                _lv.foamThickness = result.foamBurst;
                _lv.foamDensity = 0.9f;
                StartCoroutine(FadeOutFoam(result.foamBurst, result.foamDuration));
            }

            if (result.produceSmoke)
            {
                _lv.smokeEnabled = true;
                _lv.smokeColor = result.smokeColor;
                _lv.smokeSpeed = 5f;

                Invoke(nameof(StopSmoke), result.smokeDuration);
            }

            if (result.produceBubbles)
            {
                StartCoroutine(BubbleEffect(result.bubbleIntensity));
            }

            if (result.heatGlow > 0.01f)
            {
                StartCoroutine(HeatGlow(result.heatGlow));
            }

            chemicalId = result.productName;
            if (labelName) labelName.text = result.productName;
            if (labelFormula) labelFormula.text = "";

            Debug.Log($"[Reaction] {result.logMessage}");

            _reactionActive = false;
        }

        private IEnumerator FadeOutFoam(float maxThickness, float duration)
        {
            yield return new WaitForSeconds(duration * 0.4f);

            float elapsed = 0f;
            float fadeDur = duration * 0.6f;

            while (elapsed < fadeDur)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeDur;
                _lv.foamThickness = Mathf.Lerp(maxThickness, 0.001f, t);
                _lv.foamDensity = Mathf.Lerp(0.9f, 0.0f, t);
                yield return null;
            }

            _lv.foamVisibleFromBottom = false;
        }

        private IEnumerator BubbleEffect(float intensity)
        {
            float origT1 = _lv.turbulence1;
            float origT2 = _lv.turbulence2;

            _lv.turbulence1 = Mathf.Min(origT1 + intensity * 0.4f, 1f);
            _lv.turbulence2 = Mathf.Min(origT2 + intensity * 0.3f, 1f);

            yield return new WaitForSeconds(3f);

            float elapsed = 0f;
            while (elapsed < 2f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / 2f;
                _lv.turbulence1 = Mathf.Lerp(_lv.turbulence1, origT1, t * 0.1f);
                _lv.turbulence2 = Mathf.Lerp(_lv.turbulence2, origT2, t * 0.1f);
                yield return null;
            }

            _lv.turbulence1 = origT1;
            _lv.turbulence2 = origT2;
        }

        private IEnumerator HeatGlow(float intensity)
        {
            Color heatColor = new Color(1f, 0.4f, 0.1f, 1f);
            float origBright = _lv.emissionBrightness;

            _lv.emissionColor = heatColor;
            _lv.emissionBrightness = intensity;

            yield return new WaitForSeconds(2f);

            float elapsed = 0f;
            while (elapsed < 3f)
            {
                elapsed += Time.deltaTime;
                _lv.emissionBrightness = Mathf.Lerp(intensity, 0f, elapsed / 3f);
                yield return null;
            }

            _lv.emissionBrightness = origBright;
        }

        private void StopSmoke()
        {
            _lv.smokeEnabled = false;
        }

        public float Pour(float amount)
        {
            float poured = Mathf.Min(amount, CurrentAmount);
            CurrentAmount = Mathf.Clamp01(CurrentAmount - poured);
            _lv.level = CurrentAmount;
            return poured;
        }

        public void StartPourParticles()
        {
            if (pourParticles == null) return;

            var main = pourParticles.main;
            main.startColor = new ParticleSystem.MinMaxGradient(
                _chemical?.liquidColor ?? Color.blue
            );

            pourParticles.Play();
        }

        public void StopPourParticles()
        {
            if (pourParticles == null) return;
            pourParticles.Stop();
        }

        public void FillWithoutReaction(string newChemicalId, float addedVolume)
        {
            if (IsEmpty || string.IsNullOrEmpty(chemicalId) || chemicalId == "Empty")
            {
                ApplyChemical(newChemicalId);
            }

            CurrentAmount = Mathf.Clamp01(CurrentAmount + addedVolume);

            if (_lv != null)
            {
                _lv.level = CurrentAmount;
            }
        }
    }
}