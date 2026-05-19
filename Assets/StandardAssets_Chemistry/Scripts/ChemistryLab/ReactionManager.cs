using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;


namespace VirtualChemLab
{
    public class ReactionManager : MonoBehaviour
    {
        public static ReactionManager Instance { get; private set; }

        [Header("UI Log")]
        public TMPro.TextMeshProUGUI reactionLogText;
        public int maxLogLines = 8;

        [Header("Minimum Pour to Trigger (0-1)")]
        [Range(0.01f, 0.2f)]
        public float minimumPourToReact = 0.05f;

        [Header("Events")]
        public UnityEvent<string> onReactionTriggered;
        public UnityEvent onNoReaction;

        private Dictionary<string, float> _pourProgress
            = new Dictionary<string, float>();

        private List<string> _logLines = new List<string>();

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void OnLiquidPoured(
                    LiquidContainer source,
                    LiquidContainer target,
                    float amount)
        {
            if (source == null || target == null) return;
            if (source.IsEmpty) return;

            if (target.IsEmpty || target.ChemicalId == source.ChemicalId || string.IsNullOrEmpty(target.ChemicalId) || target.ChemicalId == "Empty")
            {
                target.FillWithoutReaction(source.ChemicalId, amount);
                return;
            }

            string progressKey = $"{source.GetInstanceID()}→{target.GetInstanceID()}";

            if (!_pourProgress.ContainsKey(progressKey))
                _pourProgress[progressKey] = 0f;

            _pourProgress[progressKey] += amount;

            if (_pourProgress[progressKey] < minimumPourToReact)
            {
                target.FillWithoutReaction(target.ChemicalId, amount);
                return;
            }

            float volumeToReact = _pourProgress[progressKey];
            _pourProgress[progressKey] = 0f;

            TriggerReaction(source, target, volumeToReact);
        }

        public void OnThermalReaction(ThermalReactionResult result, LiquidContainer container)
        {
            LogReaction(result);
            onReactionTriggered?.Invoke(result.productName);
            Debug.Log($"[ReactionManager] Thermal: {container.ChemicalId} → {result.productName}");
        }

        private void TriggerReaction(
            LiquidContainer source,
            LiquidContainer target,
            float addedVolume)
        {
            string idA = source.ChemicalId;
            string idB = target.ChemicalId;

            if (ChemicalDatabase.TryGetReaction(idA, idB, out ReactionResult result))
            {
                target.PlayReaction(result, addedVolume);

                LogReaction(result);

                onReactionTriggered?.Invoke(result.productName);

                Debug.Log($"[ReactionManager] {idA} + {idB} → {result.productName}");
            }
            else
            {
                MixColors(source, target, addedVolume);

                onNoReaction?.Invoke();
                Debug.Log($"[ReactionManager] No reaction: {idA} + {idB}");
            }
        }

        private void MixColors(
            LiquidContainer source,
            LiquidContainer target,
            float addedVolume)
        {
            float totalVol = target.CurrentAmount + addedVolume;
            if (totalVol < 0.001f) return;

            float ratioSrc = addedVolume / totalVol;
            float ratioTgt = target.CurrentAmount / totalVol;

            Chemical srcChem = ChemicalDatabase.GetChemical(source.ChemicalId);
            Chemical tgtChem = ChemicalDatabase.GetChemical(target.ChemicalId);

            if (srcChem == null || tgtChem == null) return;

            Color mixC1 = Color.Lerp(tgtChem.liquidColor, srcChem.liquidColor, ratioSrc);
            Color mixC2 = Color.Lerp(tgtChem.liquidColor2, srcChem.liquidColor2, ratioSrc);

            var mixResult = new ReactionResult
            {
                productName = $"mixture ({tgtChem.displayName} + {srcChem.displayName})",
                resultColor = mixC1,
                resultColor2 = mixC2,
                resultMurkiness = Mathf.Lerp(tgtChem.murkiness, srcChem.murkiness, ratioSrc),
                colorTransitionSpeed = 1.0f,
                produceFoam = false,
                produceSmoke = false,
                reactionType = ReactionType.None,
                logMessage = $"A simple mix: {tgtChem.displayName} + {srcChem.displayName}"
            };

            target.PlayReaction(mixResult, addedVolume);
            LogReaction(mixResult, isSimpleMix: true);
        }

        private void LogReaction(ReactionResult result, bool isSimpleMix = false)
        {
            if (reactionLogText == null) return;

            string prefix = isSimpleMix ? "⚪" : GetReactionEmoji(result.reactionType);
            string line = $"{result.logMessage}";

            _logLines.Add(line);
            if (_logLines.Count > maxLogLines)
                _logLines.RemoveAt(0);

            reactionLogText.text = string.Join("\n", _logLines);
        }

        private string GetReactionEmoji(ReactionType type)
        {
            return type switch
            {
                ReactionType.AcidBase => "🧪",
                ReactionType.Redox => "⚡",
                ReactionType.GasProducing => "💨",
                ReactionType.Precipitation => "🔵",
                ReactionType.Exothermic => "🔥",
                ReactionType.Neutral => "🟢",
                _ => "⚪"
            };
        }

        public void ResetLab()
        {
            _pourProgress.Clear();
            _logLines.Clear();
            if (reactionLogText) reactionLogText.text = "";
        }
    }
}