using Mutagen.Bethesda;
using Mutagen.Bethesda.FormKeys.SkyrimSE;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using Noggog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NPCEnchantFix
{
    public class Program
    {
        static Lazy<PatcherSettings> Settings = null!;

        public static async Task<int> Main(string[] args)
        {
            return await SynthesisPipeline.Instance
                .AddPatch<ISkyrimMod, ISkyrimModGetter>(RunPatch)
                .SetTypicalOpen(GameRelease.SkyrimSE, "NPCEnchantFix.esp")
                .Run(args);
        }

        public class perkData
        {
            public IPerkGetter? Perk;
            public int Rank = 1;
        }

        public static void RunPatch(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            // Set data from Settings
            Dictionary<FormKey, perkData> perksList = new();
            Dictionary<string, int> perksEDIDRankList = new();
            foreach (var data in Settings.Value.PerksToAdd)
            {
                if (data.Perk != null && !data.Perk.FormKey.IsNull && data.Perk.TryResolve(state.LinkCache, out var perk))
                {
                    perksList.TryAdd(perk.FormKey, new perkData() { Perk = perk, Rank = data.Rank });
                }
                else if (!string.IsNullOrWhiteSpace(data.EDID))
                {
                    perksEDIDRankList.TryAdd(data.EDID, data.Rank);
                }
            }

            // search perks by EDID
            foreach (var perk in state.LoadOrder.PriorityOrder.Perk().WinningOverrides())
                if (perksEDIDRankList.ContainsKey(perk.EditorID + "")) perksList.TryAdd(perk.FormKey, new perkData() { Perk = perk, Rank = perksEDIDRankList[perk.EditorID!] });

            // Loop over all NPCs in the load order
            foreach (var npc in state.LoadOrder.PriorityOrder.Npc().WinningOverrides())
            {
                try
                {
                    // Skip NPC if it inherits spells from its template
                    if (npc.Configuration.TemplateFlags.HasFlag(NpcConfiguration.TemplateFlag.SpellList)) continue;

                    // Find if the NPC has perks
                    foreach (var perkPlacementGetter in npc.Perks.EmptyIfNull())
                    {
                        if (!perksList.ContainsKey(perkPlacementGetter.Perk.FormKey)) continue;

                        perksList.Remove(perkPlacementGetter.Perk.FormKey);
                    }

                    // If NPC have all, skip
                    if (perksList.Count == 0) continue;

                    // Otherwise, add the NPC to the patch
                    var modifiedNpc = state.PatchMod.Npcs.GetOrAddAsOverride(npc);

                    // Ensure perk list exists
                    modifiedNpc.Perks ??= new ExtendedList<PerkPlacement>();

                    // Add missing perks
                    foreach (var data in perksList.Values)
                    {
                        modifiedNpc.Perks.Add(new PerkPlacement()
                        {
                            Perk = data.Perk!.AsLink(),
                            Rank = (byte)data.Rank
                        });
                    }
                }
                catch (Exception ex)
                {
                    throw RecordException.Factory(ex, npc);
                }
            }
        }
    }
}
