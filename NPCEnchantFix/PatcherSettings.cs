using Mutagen.Bethesda;
using Mutagen.Bethesda.FormKeys.SkyrimSE;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis.Settings;
using System.Collections.Generic;

namespace NPCEnchantFix
{
    public class PatcherSettings
    {
        [SynthesisOrder]
        [SynthesisTooltip("List of perks to add")]
        public HashSet<PerkData> PerksToAdd = new()
        {
            // Skyrim
            new PerkData() { Perk=Skyrim.Perk.AlchemySkillBoosts },
            new PerkData() { Perk=Skyrim.Perk.PerkSkillBoosts },
            // Choose your Armor mod
            new PerkData() { EDID="aleRandomizeDamage" },
            new PerkData() { EDID="aleArmorPropertiesPerk" },
            new PerkData() { EDID="aleLightArmorAtributes" },
            new PerkData() { EDID="aleShieldAtributes" },
            new PerkData() { EDID="aleRangedAtributes" },
            new PerkData() { EDID="aleRangedAtributes" },
            new PerkData() { EDID="aleWeaponAtributesPerk" },
            // Worlds Dawn mod
            new PerkData() { EDID="XTDPerk_Cumulative" },
        };
    }

    public class PerkData
    {
        [SynthesisOrder]
        [SynthesisTooltip("Specific selected perk")]
        public FormLink<IPerkGetter>? Perk=null;
        [SynthesisOrder]
        [SynthesisTooltip("Optional. Using when perk is not set. Perk EDID can be set here")]
        public string EDID="";
        [SynthesisOrder]
        [SynthesisTooltip("Rank of the perk")]
        public int Rank = 1;
    }
}
