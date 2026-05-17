using UnityEngine;
using System.Collections.Generic;

namespace VirtualChemLab
{
    public enum FlameTestPhase
    {
        Idle,           // Stick is clean / in hand
        AcidDipping,    // Stick entering acid beaker
        AcidDipped,     // Stick is coated with HCl (cleaned)
        SaltDipping,    // Stick entering salt dish
        SaltLoaded,     // Stick carries salt sample
        InFlame,        // Stick inside Bunsen flame
        CoolingDown     // Flame test done, colour fading
    }

    [System.Serializable]
    public class FlameTestCation
    {
        [Header("Identity")]
        public string id;               // "Na", "K", "Li", …
        public string symbol;           // "Na⁺"
        public string elementName;      // "Sodium"

        [Header("Flame Colour")]
        public Color flameColor;        // Primary glow colour
        public Color flameColorOuter;   // Outer / secondary halo colour
        public float flameIntensity = 1f;   // Multiplier for emission / bloom

        [Header("Timing")]
        public float colorRiseTime = 0.4f; // s – how fast colour appears
        public float sustainTime = 3.5f; // s – how long full colour lasts
        public float fadeTime = 1.2f; // s – how fast it fades back

        [Header("Visual Extras")]
        public bool produceSparks = false;
        public Color sparkColor = Color.white;
        public float sparkIntensity = 0.5f;

        [Header("Info")]
        public string flameColorName;   // "Golden yellow"
        public string logMessage;
    }

    [System.Serializable]
    public class FlameTestSalt
    {
        [Header("Identity")]
        public string id;               // "NaCl"
        public string displayName;      // "Sodium chloride"
        public string formula;          // "NaCl"

        [Header("Visual (in dish)")]
        public Color saltColor = Color.white;
        public float grainSize = 0.5f;         // 0-1, cosmetic

        [Header("Flame Test")]
        public string cationId;         // → FlameTestDatabase.Cations key
    }

    public static class FlameTestDatabase
    {
        private static Dictionary<string, FlameTestCation> _cations;
        private static Dictionary<string, FlameTestSalt> _salts;

        public static Dictionary<string, FlameTestCation> Cations
            => _cations ?? BuildCations();
        public static Dictionary<string, FlameTestSalt> Salts
            => _salts ?? BuildSalts();

        private static Dictionary<string, FlameTestCation> BuildCations()
        {
            _cations = new Dictionary<string, FlameTestCation>
            {
                // Lithium  – crimson red
                ["Li"] = new FlameTestCation
                {
                    id = "Li",
                    symbol = "Li⁺",
                    elementName = "Lithium",
                    flameColor = new Color(0.85f, 0.05f, 0.05f, 1f),
                    flameColorOuter = new Color(1.00f, 0.25f, 0.10f, 1f),
                    flameIntensity = 1.1f,
                    colorRiseTime = 0.35f,
                    sustainTime = 3.5f,
                    fadeTime = 1.2f,
                    produceSparks = false,
                    flameColorName = "Crimson",
                    logMessage = "Li⁺ emits crimson red light (670 nm) due to electron excitation"
                },

                // Sodium – golden yellow (very strong, can mask others)
                ["Na"] = new FlameTestCation
                {
                    id = "Na",
                    symbol = "Na⁺",
                    elementName = "Sodium",
                    flameColor = new Color(1.00f, 0.78f, 0.00f, 1f),
                    flameColorOuter = new Color(1.00f, 0.95f, 0.30f, 1f),
                    flameIntensity = 1.4f,     // Sodium is notably bright
                    colorRiseTime = 0.25f,
                    sustainTime = 4.0f,
                    fadeTime = 1.5f,
                    produceSparks = true,
                    sparkColor = new Color(1f, 0.9f, 0.4f, 1f),
                    sparkIntensity = 0.4f,
                    flameColorName = "Golden yellow",
                    logMessage = "Na⁺ emits intense golden-yellow light (589 nm) — D-line emission"
                },

                // Potassium – light violet (lilac); use cobalt-blue glass to see it through Na contamination
                ["K"] = new FlameTestCation
                {
                    id = "K",
                    symbol = "K⁺",
                    elementName = "Potassium",
                    flameColor = new Color(0.65f, 0.45f, 0.95f, 1f),
                    flameColorOuter = new Color(0.80f, 0.60f, 1.00f, 1f),
                    flameIntensity = 0.85f,
                    colorRiseTime = 0.45f,
                    sustainTime = 3.0f,
                    fadeTime = 1.0f,
                    produceSparks = false,
                    flameColorName = "Light violet",
                    logMessage = "K⁺ emits light violet / lilac light (766–769 nm)"
                },

                // Rubidium – red violet
                ["Rb"] = new FlameTestCation
                {
                    id = "Rb",
                    symbol = "Rb⁺",
                    elementName = "Rubidium",
                    flameColor = new Color(0.75f, 0.10f, 0.55f, 1f),
                    flameColorOuter = new Color(0.90f, 0.25f, 0.65f, 1f),
                    flameIntensity = 0.9f,
                    colorRiseTime = 0.40f,
                    sustainTime = 3.2f,
                    fadeTime = 1.1f,
                    produceSparks = false,
                    flameColorName = "Red violet",
                    logMessage = "Rb⁺ emits red-violet light (780 nm)"
                },

                // Cesium – blue violet
                ["Cs"] = new FlameTestCation
                {
                    id = "Cs",
                    symbol = "Cs⁺",
                    elementName = "Cesium",
                    flameColor = new Color(0.30f, 0.10f, 0.90f, 1f),
                    flameColorOuter = new Color(0.55f, 0.30f, 1.00f, 1f),
                    flameIntensity = 0.95f,
                    colorRiseTime = 0.40f,
                    sustainTime = 3.0f,
                    fadeTime = 1.0f,
                    produceSparks = false,
                    flameColorName = "Blue violet",
                    logMessage = "Cs⁺ emits blue-violet light (455 nm)"
                },

                // ── Bonus cations (common in labs) ────────────────────────────

                // Calcium – brick red / orange-red
                ["Ca"] = new FlameTestCation
                {
                    id = "Ca",
                    symbol = "Ca²⁺",
                    elementName = "Calcium",
                    flameColor = new Color(0.90f, 0.30f, 0.05f, 1f),
                    flameColorOuter = new Color(1.00f, 0.50f, 0.15f, 1f),
                    flameIntensity = 1.0f,
                    colorRiseTime = 0.40f,
                    sustainTime = 3.5f,
                    fadeTime = 1.3f,
                    produceSparks = false,
                    flameColorName = "Brick red / orange",
                    logMessage = "Ca²⁺ emits brick-red / orange light (616–622 nm)"
                },

                // Barium – pale green
                ["Ba"] = new FlameTestCation
                {
                    id = "Ba",
                    symbol = "Ba²⁺",
                    elementName = "Barium",
                    flameColor = new Color(0.30f, 0.90f, 0.20f, 1f),
                    flameColorOuter = new Color(0.55f, 1.00f, 0.40f, 1f),
                    flameIntensity = 0.9f,
                    colorRiseTime = 0.45f,
                    sustainTime = 3.2f,
                    fadeTime = 1.2f,
                    produceSparks = false,
                    flameColorName = "Pale / apple green",
                    logMessage = "Ba²⁺ emits pale green / apple-green light (524 nm)"
                },

                // Copper – blue-green
                ["Cu"] = new FlameTestCation
                {
                    id = "Cu",
                    symbol = "Cu²⁺",
                    elementName = "Copper",
                    flameColor = new Color(0.00f, 0.75f, 0.65f, 1f),
                    flameColorOuter = new Color(0.20f, 0.95f, 0.75f, 1f),
                    flameIntensity = 1.0f,
                    colorRiseTime = 0.35f,
                    sustainTime = 3.8f,
                    fadeTime = 1.4f,
                    produceSparks = true,
                    sparkColor = new Color(0.0f, 1.0f, 0.8f, 1f),
                    sparkIntensity = 0.6f,
                    flameColorName = "Blue-green",
                    logMessage = "Cu²⁺ emits blue-green light (515 nm); chloride gives vivid azure"
                },
            };
            return _cations;
        }

        // ── Salts ─────────────────────────────────────────────────────────────
        private static Dictionary<string, FlameTestSalt> BuildSalts()
        {
            _salts = new Dictionary<string, FlameTestSalt>
            {
                // Sodium salts
                ["NaCl"] = new FlameTestSalt
                {
                    id = "NaCl",
                    displayName = "Sodium chloride",
                    formula = "NaCl",
                    saltColor = Color.white,
                    cationId = "Na"
                },
                ["Na2SO4"] = new FlameTestSalt
                {
                    id = "Na2SO4",
                    displayName = "Sodium sulfate",
                    formula = "Na₂SO₄",
                    saltColor = new Color(0.95f, 0.95f, 0.9f),
                    cationId = "Na"
                },
                ["NaHCO3"] = new FlameTestSalt
                {
                    id = "NaHCO3",
                    displayName = "Sodium bicarbonate",
                    formula = "NaHCO₃",
                    saltColor = Color.white,
                    cationId = "Na"
                },
                ["NaNO3"] = new FlameTestSalt
                {
                    id = "NaNO3",
                    displayName = "Sodium nitrate",
                    formula = "NaNO₃",
                    saltColor = Color.white,
                    cationId = "Na"
                },

                // Potassium salts
                ["KCl"] = new FlameTestSalt
                {
                    id = "KCl",
                    displayName = "Potassium chloride",
                    formula = "KCl",
                    saltColor = Color.white,
                    cationId = "K"
                },
                ["KNO3"] = new FlameTestSalt
                {
                    id = "KNO3",
                    displayName = "Potassium nitrate",
                    formula = "KNO₃",
                    saltColor = Color.white,
                    cationId = "K"
                },
                ["K2SO4"] = new FlameTestSalt
                {
                    id = "K2SO4",
                    displayName = "Potassium sulfate",
                    formula = "K₂SO₄",
                    saltColor = Color.white,
                    cationId = "K"
                },

                // Lithium salts
                ["LiCl"] = new FlameTestSalt
                {
                    id = "LiCl",
                    displayName = "Lithium chloride",
                    formula = "LiCl",
                    saltColor = Color.white,
                    cationId = "Li"
                },
                ["Li2CO3"] = new FlameTestSalt
                {
                    id = "Li2CO3",
                    displayName = "Lithium carbonate",
                    formula = "Li₂CO₃",
                    saltColor = Color.white,
                    cationId = "Li"
                },

                // Rubidium salts
                ["RbCl"] = new FlameTestSalt
                {
                    id = "RbCl",
                    displayName = "Rubidium chloride",
                    formula = "RbCl",
                    saltColor = Color.white,
                    cationId = "Rb"
                },

                // Cesium salts
                ["CsCl"] = new FlameTestSalt
                {
                    id = "CsCl",
                    displayName = "Cesium chloride",
                    formula = "CsCl",
                    saltColor = Color.white,
                    cationId = "Cs"
                },

                // Calcium salts
                ["CaCl2"] = new FlameTestSalt
                {
                    id = "CaCl2",
                    displayName = "Calcium chloride",
                    formula = "CaCl₂",
                    saltColor = Color.white,
                    cationId = "Ca"
                },
                ["CaCO3"] = new FlameTestSalt
                {
                    id = "CaCO3",
                    displayName = "Calcium carbonate",
                    formula = "CaCO₃",
                    saltColor = Color.white,
                    cationId = "Ca"
                },

                // Barium salts
                ["BaCl2"] = new FlameTestSalt
                {
                    id = "BaCl2",
                    displayName = "Barium chloride",
                    formula = "BaCl₂",
                    saltColor = Color.white,
                    cationId = "Ba"
                },

                // Copper salts
                ["CuCl2"] = new FlameTestSalt
                {
                    id = "CuCl2",
                    displayName = "Copper(II) chloride",
                    formula = "CuCl₂",
                    saltColor = new Color(0.6f, 0.9f, 0.85f),
                    cationId = "Cu"
                },
                ["CuSO4"] = new FlameTestSalt
                {
                    id = "CuSO4",
                    displayName = "Copper(II) sulfate",
                    formula = "CuSO₄",
                    saltColor = new Color(0.4f, 0.65f, 0.95f),
                    cationId = "Cu"
                },

                // Unknown salt (used for quiz / discovery mode)
                ["UNKNOWN"] = new FlameTestSalt
                {
                    id = "UNKNOWN",
                    displayName = "Unknown salt",
                    formula = "???",
                    saltColor = new Color(0.9f, 0.9f, 0.85f),
                    cationId = ""
                },
            };
            return _salts;
        }

        // ── Helpers ──────────────────────────────────────────────────────────
        public static bool TryGetCation(string id, out FlameTestCation cation)
            => Cations.TryGetValue(id, out cation);

        public static bool TryGetSalt(string id, out FlameTestSalt salt)
            => Salts.TryGetValue(id, out salt);

        public static FlameTestCation GetCationForSalt(string saltId)
        {
            if (!TryGetSalt(saltId, out var salt)) return null;
            if (string.IsNullOrEmpty(salt.cationId)) return null;
            TryGetCation(salt.cationId, out var cation);
            return cation;
        }
    }
}