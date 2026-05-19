using UnityEngine;
using System.Collections.Generic;

namespace VirtualChemLab
{
    public enum ReactionType
    {
        None,
        AcidBase, // Acid + Base TO Effervescence + Color Change
        Redox, // Oxidation/Reduction TO Color Change
        GasProducing, // Produces gas/smoke
        Precipitation, // Precipitation TO Solid
        Exothermic, // Exothermic Reaction
        Neutral, // Simple Neutrality
        Substitution
    }

    [System.Serializable]
    public class Chemical
    {
        [Header("Identity")]
        public string id;               //"HCl"
        public string displayName;
        public string formula;

        [Header("Visual")]
        public Color liquidColor = Color.blue;
        public Color liquidColor2 = Color.cyan;
        public float murkiness = 0.3f;
        public float turbulence1 = 0.3f;
        public float turbulence2 = 0.1f;

        [Header("Foam")]
        public bool hasFoam = false;
        public Color foamColor = Color.white;
        public float foamThickness = 0.04f;
        public float foamDensity = 0.5f;

        [Header("Smoke")]
        public bool hasSmoke = false;
        public Color smokeColor = new Color(0.8f, 0.8f, 0.8f, 0.5f);
        public float smokeSpeed = 3f;

        [Header("Properties")]
        public float ph = 7f;   // pH (0-14)
        public bool isAcid => ph < 6.5f;
        public bool isBase => ph > 7.5f;
    }

    [System.Serializable]
    public class ReactionResult
    {
        [Header("Identity")]
        public string productName;          //Output name

        [Header("Visual Transition")]
        public Color resultColor = Color.green;
        public Color resultColor2 = Color.yellow;
        public float resultMurkiness = 0.5f;
        public float colorTransitionSpeed = 1.5f;   //S

        [Header("Foam Effect")]
        public bool produceFoam = false;
        public Color foamColor = Color.white;
        public float foamBurst = 0.08f;
        public float foamDuration = 4f;

        [Header("Smoke Effect")]
        public bool produceSmoke = false;
        public Color smokeColor = new Color(0.9f, 0.9f, 0.9f, 0.6f);
        public float smokeDuration = 5f;

        [Header("Particle Burst")]
        public bool produceBubbles = false;
        public float bubbleIntensity = 1f;

        [Header("Sound & Feedback")]
        public ReactionType reactionType = ReactionType.Neutral;
        public float heatGlow = 0f;       // 0-1  For repulsive reactions
        public string logMessage = "";       //Explanatory message
    }

    public static class ChemicalDatabase
    {
        private static Dictionary<string, Chemical> _chemicals;
        private static Dictionary<string, ReactionResult> _reactions;

        public static Dictionary<string, Chemical> Chemicals => _chemicals ?? BuildChemicals();
        public static Dictionary<string, ReactionResult> Reactions => _reactions ?? BuildReactions();

        private static Dictionary<string, Chemical> BuildChemicals()
        {
            _chemicals = new Dictionary<string, Chemical>
            {
                ["HCl"] = new Chemical
                {
                    id = "HCl",
                    displayName = "HCl",
                    formula = "HCl",
                    liquidColor = new Color(0.85f, 0.95f, 1f, 0.85f),
                    liquidColor2 = new Color(0.7f, 0.9f, 1f, 0.85f),
                    murkiness = 0.1f,
                    ph = 1f,
                    hasFoam = false,
                    hasSmoke = false
                },
                ["H2SO4"] = new Chemical
                {
                    id = "H2SO4",
                    displayName = "Sulfuric acid",
                    formula = "H₂SO₄",
                    liquidColor = new Color(0.9f, 0.95f, 0.6f, 0.9f),
                    liquidColor2 = new Color(1f, 1f, 0.4f, 0.9f),
                    murkiness = 0.2f,
                    ph = 0.5f,
                    hasFoam = false,
                    hasSmoke = true,
                    smokeColor = new Color(0.95f, 0.95f, 0.8f, 0.3f),
                    smokeSpeed = 2f
                },
                ["CH3COOH"] = new Chemical
                {
                    id = "CH3COOH",
                    displayName = "Acetic acid",
                    formula = "CH₃COOH",
                    liquidColor = new Color(0.95f, 1f, 0.95f, 0.8f),
                    liquidColor2 = new Color(0.85f, 1f, 0.85f, 0.8f),
                    murkiness = 0.15f,
                    ph = 3f,
                    hasFoam = false,
                    hasSmoke = false
                },

                ["NaOH"] = new Chemical
                {
                    id = "NaOH",
                    displayName = "NaOH",
                    formula = "NaOH",
                    liquidColor = new Color(0.6f, 0.8f, 1f, 0.85f),
                    liquidColor2 = new Color(0.4f, 0.6f, 1f, 0.85f),
                    murkiness = 0.1f,
                    ph = 13f,
                    hasFoam = false,
                    hasSmoke = false
                },
                ["NH3"] = new Chemical
                {
                    id = "NH3",
                    displayName = "NH3",
                    formula = "NH₃",
                    liquidColor = new Color(0.9f, 1f, 0.7f, 0.8f),
                    liquidColor2 = new Color(0.8f, 1f, 0.5f, 0.8f),
                    murkiness = 0.2f,
                    ph = 11f,
                    hasFoam = false,
                    hasSmoke = true,
                    smokeColor = new Color(0.8f, 0.95f, 0.8f, 0.4f),
                    smokeSpeed = 4f
                },

                ["H2O2"] = new Chemical
                {
                    id = "H2O2",
                    displayName = "Hydrogen peroxide",
                    formula = "H₂O₂",
                    liquidColor = new Color(0.95f, 0.95f, 1f, 0.75f),
                    liquidColor2 = new Color(0.85f, 0.85f, 1f, 0.75f),
                    murkiness = 0.05f,
                    ph = 4.5f,
                    hasFoam = true,
                    foamColor = Color.white,
                    foamThickness = 0.02f,
                    hasSmoke = false
                },
                ["KMnO4"] = new Chemical
                {
                    id = "KMnO4",
                    displayName = "Potassium permanganate",
                    formula = "KMnO₄",
                    liquidColor = new Color(0.5f, 0.0f, 0.5f, 0.95f),
                    liquidColor2 = new Color(0.7f, 0.0f, 0.7f, 0.95f),
                    murkiness = 0.6f,
                    ph = 7f,
                    hasFoam = false,
                    hasSmoke = false,
                    turbulence1 = 0.4f,
                    turbulence2 = 0.15f
                },
                ["NaHCO3"] = new Chemical
                {
                    id = "NaHCO3",
                    displayName = "NaHCO3",
                    formula = "NaHCO₃",
                    liquidColor = new Color(0.7f, 0.85f, 1f, 0.8f),
                    liquidColor2 = new Color(0.6f, 0.75f, 1f, 0.8f),
                    murkiness = 0.25f,
                    ph = 8.3f,
                    hasFoam = false,
                    hasSmoke = false
                },
                ["CuSO4"] = new Chemical
                {
                    id = "CuSO4",
                    displayName = "CuSO4",
                    formula = "CuSO₄",
                    liquidColor = new Color(0.1f, 0.4f, 0.9f, 0.95f),
                    liquidColor2 = new Color(0.0f, 0.3f, 0.8f, 0.95f),
                    murkiness = 0.45f,
                    ph = 4f,
                    hasFoam = false,
                    hasSmoke = false
                },
                ["Na2S2O3"] = new Chemical
                {
                    id = "Na2S2O3",
                    displayName = "Na2S2O3",
                    formula = "Na2S2O3",
                    liquidColor = new Color(0.95f, 0.95f, 1f, 0.8f),
                    liquidColor2 = new Color(0.85f, 0.9f, 1f, 0.8f),
                    murkiness = 0.05f,
                    ph = 8.5f,
                    hasFoam = false,
                    hasSmoke = false
                },

                ["FeCl3"] = new Chemical
                {
                    id = "FeCl3",
                    displayName = "FeCl3",
                    formula = "FeCl3",
                    liquidColor = new Color(1f, 0.92f, 0.55f, 0.85f),
                    liquidColor2 = new Color(0.95f, 0.85f, 0.45f, 0.85f),
                    murkiness = 0.2f,
                    ph = 2.5f,
                    hasFoam = false,
                    hasSmoke = false
                },

                ["NH4SCN"] = new Chemical
                {
                    id = "NH4SCN",
                    displayName = "NH4SCN",
                    formula = "NH4SCN",
                    liquidColor = new Color(0.98f, 0.98f, 1f, 0.75f),
                    liquidColor2 = new Color(0.9f, 0.95f, 1f, 0.75f),
                    murkiness = 0.03f,
                    ph = 6.5f,
                    hasFoam = false,
                    hasSmoke = false
                },
            };
            return _chemicals;
        }

        private static Dictionary<string, ReactionResult> BuildReactions()
        {
            _reactions = new Dictionary<string, ReactionResult>();

            void Add(string a, string b, ReactionResult r)
            {
                string key = MakeKey(a, b);
                _reactions[key] = r;
            }

            Add("HCl", "NaOH", new ReactionResult
            {
                productName = "Sodium chloride (salt) + water",
                resultColor = new Color(0.85f, 0.95f, 1f, 0.75f),
                resultColor2 = new Color(0.75f, 0.9f, 1f, 0.75f),
                resultMurkiness = 0.05f,
                colorTransitionSpeed = 1.2f,
                produceFoam = true,
                foamColor = Color.white,
                foamBurst = 0.06f,
                foamDuration = 3f,
                produceSmoke = false,
                produceBubbles = false,
                reactionType = ReactionType.AcidBase,
                logMessage = "HCl + NaOH → NaCl + H₂O  | Neutralization reaction — produces heat"
            });

            Add("H2SO4", "NaOH", new ReactionResult
            {
                productName = "Sodium sulfate + water",
                resultColor = new Color(0.9f, 0.95f, 0.8f, 0.7f),
                resultColor2 = new Color(0.85f, 0.9f, 0.7f, 0.7f),
                resultMurkiness = 0.1f,
                colorTransitionSpeed = 1.0f,
                produceFoam = true,
                foamColor = new Color(0.95f, 0.95f, 0.85f, 1f),
                foamBurst = 0.07f,
                foamDuration = 4f,
                produceSmoke = true,
                smokeColor = new Color(0.9f, 0.9f, 0.7f, 0.3f),
                smokeDuration = 4f,
                reactionType = ReactionType.AcidBase,
                heatGlow = 0.4f,
                logMessage = "H₂SO₄ + 2NaOH → Na₂SO₄ + 2H₂O  |  hyperthermic equilibrium reaction"
            });

            Add("HCl", "NaHCO3", new ReactionResult
            {
                productName = "NaCl + CO2 + H2O",
                resultColor = new Color(0.85f, 0.95f, 1f, 0.7f),
                resultColor2 = new Color(0.8f, 0.9f, 1f, 0.7f),
                resultMurkiness = 0.05f,
                colorTransitionSpeed = 0.8f,
                produceFoam = true,
                foamColor = Color.white,
                foamBurst = 0.15f,
                foamDuration = 6f,
                produceSmoke = true,
                smokeColor = new Color(0.95f, 0.95f, 0.95f, 0.5f),
                smokeDuration = 5f,
                produceBubbles = true,
                bubbleIntensity = 2f,
                reactionType = ReactionType.GasProducing,
                logMessage = "HCl + NaHCO2 → NaCl + CO2↑ + H2O  |  Intense effervescence of CO2 gas"
            });

            Add("CH3COOH", "NaHCO3", new ReactionResult
            {
                productName = "Sodium acetate + CO₂ + water",
                resultColor = new Color(0.9f, 1f, 0.9f, 0.75f),
                resultColor2 = new Color(0.85f, 1f, 0.85f, 0.75f),
                resultMurkiness = 0.1f,
                colorTransitionSpeed = 1.0f,
                produceFoam = true,
                foamColor = Color.white,
                foamBurst = 0.09f,
                foamDuration = 5f,
                produceSmoke = false,
                produceBubbles = true,
                bubbleIntensity = 1.2f,
                reactionType = ReactionType.GasProducing,
                logMessage = "CH₃COOH + NaHCO₃ → CH₃COONa + CO₂↑ + H₂O"
            });

            Add("H2O2", "KMnO4", new ReactionResult
            {
                productName = "Manganese dioxide + oxygen + water",
                resultColor = new Color(0.6f, 0.0f, 0.15f, 0.9f),
                resultColor2 = new Color(0.4f, 0.0f, 0.1f, 0.9f),
                resultMurkiness = 0.7f,
                colorTransitionSpeed = 2.0f,
                produceFoam = true,
                foamColor = new Color(0.7f, 0.3f, 0.7f, 1f),
                foamBurst = 0.12f,
                foamDuration = 8f,
                produceSmoke = true,
                smokeColor = new Color(0.5f, 0.0f, 0.5f, 0.4f),
                smokeDuration = 6f,
                produceBubbles = true,
                bubbleIntensity = 3f,
                reactionType = ReactionType.Redox,
                heatGlow = 0.3f,
                logMessage = "2KMnO₄ + 5H₂O₂ + 3H₂SO₄ → 2MnSO₄ + 5O₂↑ + 8H₂O  |  Oxidation and reduction"
            });

            Add("HCl", "NH3", new ReactionResult
            {
                productName = "NH4Cl",
                resultColor = new Color(0.9f, 0.95f, 0.9f, 0.6f),
                resultColor2 = new Color(0.85f, 0.9f, 0.85f, 0.6f),
                resultMurkiness = 0.8f,
                colorTransitionSpeed = 1.5f,
                produceFoam = false,
                produceSmoke = true,
                smokeColor = new Color(0.95f, 0.95f, 0.95f, 0.8f),
                smokeDuration = 8f,
                produceBubbles = false,
                reactionType = ReactionType.GasProducing,
                logMessage = "HCl(g) + NH3(g) → NH4Cl  |  Dense white smoke"
            });

            Add("CuSO4", "NaOH", new ReactionResult
            {
                productName = "Cu(OH)2↓ + Na2SO4",
                resultColor = new Color(0.05f, 0.25f, 0.7f, 0.95f),
                resultColor2 = new Color(0.0f, 0.15f, 0.5f, 0.95f),
                resultMurkiness = 0.85f,
                colorTransitionSpeed = 1.8f,
                produceFoam = false,
                produceSmoke = false,
                produceBubbles = false,
                reactionType = ReactionType.Precipitation,
                logMessage = "CuSO4 + 2NaOH → Cu(OH)2↓ + Na2SO4  |  blue precipitation"
            });

            Add("H2SO4", "NaHCO3", new ReactionResult
            {
                productName = "Sodium sulfate + CO₂ + water",
                resultColor = new Color(0.9f, 0.95f, 0.75f, 0.7f),
                resultColor2 = new Color(0.85f, 0.9f, 0.65f, 0.7f),
                resultMurkiness = 0.1f,
                colorTransitionSpeed = 0.7f,
                produceFoam = true,
                foamColor = Color.white,
                foamBurst = 0.18f,
                foamDuration = 7f,
                produceSmoke = true,
                smokeColor = new Color(0.95f, 0.95f, 0.8f, 0.5f),
                smokeDuration = 5f,
                produceBubbles = true,
                bubbleIntensity = 2.5f,
                reactionType = ReactionType.GasProducing,
                heatGlow = 0.5f,
                logMessage = "H₂SO₄ + 2NaHCO₃ → Na₂SO₄ + 2CO₂↑ + 2H₂O"
            });

            Add("Na2S2O3", "HCl", new ReactionResult
            {
                productName = "2NaCl + H2O + SO2↑ + S↓",

                resultColor = new Color(1f, 0.95f, 0.5f, 0.85f),
                resultColor2 = new Color(0.9f, 0.85f, 0.3f, 0.85f),

                resultMurkiness = 0.75f,
                colorTransitionSpeed = 0.8f,

                produceSmoke = true,
                smokeColor = new Color(0.9f, 0.9f, 0.8f, 0.45f),
                smokeDuration = 5f,

                produceBubbles = true,
                bubbleIntensity = 1.2f,

                reactionType = ReactionType.GasProducing,

                logMessage = "Na2S2O3 + 2HCl → 2NaCl + H2O + SO2↑ + S↓"
            });

            Add("FeCl3", "NH4SCN", new ReactionResult
            {
                productName = "Fe(SCN)₃ + 3NH₄Cl",

                resultColor = new Color(0.65f, 0f, 0f, 0.95f),
                resultColor2 = new Color(0.85f, 0.05f, 0.05f, 0.95f),

                resultMurkiness = 0.15f,
                colorTransitionSpeed = 1.2f,

                reactionType = ReactionType.Substitution,

                heatGlow = 0.1f,

                logMessage = "FeCl3 + 3NH4SCN → Fe(SCN)3 + 3NH4Cl"
            });
            return _reactions;
        }

        public static string MakeKey(string idA, string idB)
        {
            string a = idA.Trim();
            string b = idB.Trim();
            return string.Compare(a, b, System.StringComparison.Ordinal) <= 0
                   ? $"{a}+{b}"
                   : $"{b}+{a}";
        }

        public static bool TryGetReaction(string idA, string idB, out ReactionResult result)
        {
            string key = MakeKey(idA, idB);
            return Reactions.TryGetValue(key, out result);
        }

        public static Chemical GetChemical(string id)
        {
            Chemicals.TryGetValue(id, out Chemical c);
            return c;
        }
    }
}