using UnityEngine;
using System.Collections.Generic;

namespace VirtualChemLab
{
    [System.Serializable]
    public class ThermalReactionResult : ReactionResult
    {
        [Header("Thermal Properties")]
        public float activationTempC = 100f;    // °C needed to start
        public float reactionDurationSec = 30f; // Real-world scaled duration
        public float peakTempC = 150f;          // Temperature at peak
        public bool isEndothermic = false;       // Absorbs vs releases heat
        public Color heatingColor = new Color(1f, 0.6f, 0.1f, 0.9f);
        public float heatingGlowIntensity = 0.6f;
    }

    public static class ThermalReactionDatabase
    {
        private static Dictionary<string, ThermalReactionResult> _reactions;
        public static Dictionary<string, ThermalReactionResult> Reactions
            => _reactions ?? Build();

        private static Dictionary<string, ThermalReactionResult> Build()
        {
            _reactions = new Dictionary<string, ThermalReactionResult>();

            // Cu(OH)2 heated → CuO (black) + H2O
            // Forms after CuSO4 + NaOH reaction; heating the blue precipitate
            _reactions["Cu(OH)2"] = new ThermalReactionResult
            {
                productName = "Copper(II) oxide (black) + water",
                activationTempC = 80f,
                reactionDurationSec = 25f,
                peakTempC = 200f,
                isEndothermic = true,
                resultColor = new Color(0.08f, 0.05f, 0.05f, 0.95f),
                resultColor2 = new Color(0.12f, 0.08f, 0.06f, 0.95f),
                resultMurkiness = 0.9f,
                colorTransitionSpeed = 8f,
                produceFoam = false,
                produceSmoke = true,
                smokeColor = new Color(0.7f, 0.7f, 0.7f, 0.5f),
                smokeDuration = 10f,
                produceBubbles = false,
                reactionType = ReactionType.Redox,
                heatGlow = 0.5f,
                heatingColor = new Color(0.6f, 0.3f, 0.1f, 1f),
                heatingGlowIntensity = 0.5f,
                logMessage = "Cu(OH)₂ → CuO + H₂O  |  Thermal decomposition at ~80°C, black copper oxide forms"
            };

            // NaHCO3 heated → Na2CO3 + CO2 + H2O  (baking soda decomposition)
            _reactions["NaHCO3"] = new ThermalReactionResult
            {
                productName = "Sodium carbonate + CO₂ + water",
                activationTempC = 50f,
                reactionDurationSec = 20f,
                peakTempC = 120f,
                isEndothermic = true,
                resultColor = new Color(0.75f, 0.85f, 1f, 0.75f),
                resultColor2 = new Color(0.65f, 0.78f, 1f, 0.75f),
                resultMurkiness = 0.15f,
                colorTransitionSpeed = 5f,
                produceFoam = false,
                produceSmoke = true,
                smokeColor = new Color(0.9f, 0.9f, 0.9f, 0.45f),
                smokeDuration = 12f,
                produceBubbles = true,
                bubbleIntensity = 1.5f,
                reactionType = ReactionType.GasProducing,
                heatGlow = 0.2f,
                heatingColor = new Color(1f, 0.7f, 0.2f, 1f),
                heatingGlowIntensity = 0.35f,
                logMessage = "2NaHCO₃ → Na₂CO₃ + CO₂↑ + H₂O  |  Thermal decomposition at ~50°C"
            };

            // H2O2 heated → H2O + O2  (accelerated decomposition)
            _reactions["H2O2"] = new ThermalReactionResult
            {
                productName = "Water + oxygen gas",
                activationTempC = 40f,
                reactionDurationSec = 15f,
                peakTempC = 80f,
                isEndothermic = false,
                resultColor = new Color(0.9f, 0.95f, 1f, 0.65f),
                resultColor2 = new Color(0.85f, 0.92f, 1f, 0.65f),
                resultMurkiness = 0.03f,
                colorTransitionSpeed = 4f,
                produceFoam = true,
                foamColor = Color.white,
                foamBurst = 0.1f,
                foamDuration = 8f,
                produceSmoke = false,
                produceBubbles = true,
                bubbleIntensity = 2.5f,
                reactionType = ReactionType.GasProducing,
                heatGlow = 0.25f,
                heatingColor = new Color(1f, 0.8f, 0.3f, 1f),
                heatingGlowIntensity = 0.4f,
                logMessage = "2H₂O₂ → 2H₂O + O₂↑  |  Thermal decomposition, vigorous O₂ bubbling above 40°C"
            };

            // KMnO4 heated → K2MnO4 + MnO2 + O2
            _reactions["KMnO4"] = new ThermalReactionResult
            {
                productName = "Potassium manganate + MnO₂ + oxygen",
                activationTempC = 150f,
                reactionDurationSec = 35f,
                peakTempC = 240f,
                isEndothermic = false,
                resultColor = new Color(0.25f, 0.1f, 0.1f, 0.95f),
                resultColor2 = new Color(0.15f, 0.05f, 0.05f, 0.95f),
                resultMurkiness = 0.95f,
                colorTransitionSpeed = 10f,
                produceFoam = false,
                produceSmoke = true,
                smokeColor = new Color(0.4f, 0.0f, 0.4f, 0.6f),
                smokeDuration = 15f,
                produceBubbles = true,
                bubbleIntensity = 2f,
                reactionType = ReactionType.Redox,
                heatGlow = 0.7f,
                heatingColor = new Color(0.8f, 0.2f, 0.8f, 1f),
                heatingGlowIntensity = 0.8f,
                logMessage = "2KMnO₄ → K₂MnO₄ + MnO₂ + O₂↑  |  Thermal decomposition above 150°C"
            };

            // CH3COOH (acetic acid) + ethanol would need ethanol, so just evaporation
            _reactions["CH3COOH"] = new ThermalReactionResult
            {
                productName = "Acetic acid vapour (evaporating)",
                activationTempC = 60f,
                reactionDurationSec = 18f,
                peakTempC = 118f,
                isEndothermic = true,
                resultColor = new Color(0.95f, 1f, 0.95f, 0.4f),
                resultColor2 = new Color(0.9f, 1f, 0.9f, 0.4f),
                resultMurkiness = 0.05f,
                colorTransitionSpeed = 6f,
                produceFoam = false,
                produceSmoke = true,
                smokeColor = new Color(0.85f, 0.95f, 0.85f, 0.55f),
                smokeDuration = 14f,
                produceBubbles = false,
                reactionType = ReactionType.GasProducing,
                heatGlow = 0.15f,
                heatingColor = new Color(1f, 0.75f, 0.2f, 1f),
                heatingGlowIntensity = 0.3f,
                logMessage = "CH₃COOH(l) → CH₃COOH(g)  |  Evaporation/boiling above 60°C, pungent vapour"
            };

            return _reactions;
        }

        public static bool TryGetThermalReaction(string chemicalId, out ThermalReactionResult result)
        {
            return Reactions.TryGetValue(chemicalId, out result);
        }
    }
}