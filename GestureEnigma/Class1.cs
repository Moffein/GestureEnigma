using BepInEx;
using MonoMod.Cil;
using R2API;
using RoR2;
using RoR2.Artifacts;
using System;

namespace GestureEnigma
{
    [BepInDependency("com.bepis.r2api")]
    [R2API.Utils.R2APISubmoduleDependency(nameof(LanguageAPI))]
    [BepInPlugin("com.Moffein.GestureEnigma", "GestureEnigma", "1.0.0")]
    public class Class1 : BaseUnityPlugin
    {
        public static bool autofireEnabled = false;

        public void Awake()
        {
            autofireEnabled = Config.Bind("Settings", "Enable Autofire (Client-Side)", false, "Gesture autofires equipment.").Value;

            if (!autofireEnabled)
            {
                IL.RoR2.EquipmentSlot.FixedUpdate += (il) =>
                {
                    ILCursor c = new ILCursor(il);
                    c.GotoNext(
                         x => x.MatchLdsfld(typeof(RoR2Content.Items), "AutoCastEquipment")
                        );

                    c.Index += 2;
                    c.EmitDelegate<Func<int, int>>(orig => 0);
                };
            }

            LanguageAPI.Add("GestureEnigma_Pickup", "Dramatically reduce Equipment cooldown... <color=#FF7F7F>BUT it is randomized after each activation.</color>");
            LanguageAPI.Add("GestureEnigma_Desc", "<style=cIsUtility>Reduce Equipment cooldown</style> by <style=cIsUtility>50%</style> <style=cStack>(+15% per stack)</style>. Forces your Equipment to change to a <style=cIsHealth>random Equipment</style> whenever it is <style=cIsUtility>activated</style>.");

            On.RoR2.ItemCatalog.Init += (orig) =>
            {
                orig();

                RoR2Content.Items.AutoCastEquipment.pickupToken = "GestureEnigma_Pickup";
                RoR2Content.Items.AutoCastEquipment.descriptionToken = "GestureEnigma_Desc";
            };

            EquipmentSlot.onServerEquipmentActivated += EquipmentSlot_onServerEquipmentActivated;
        }

        private static void EquipmentSlot_onServerEquipmentActivated(EquipmentSlot equipmentSlot, EquipmentIndex equipmentIndex)
        {
            if (equipmentSlot.characterBody.inventory.GetItemCount(RoR2Content.Items.AutoCastEquipment) > 0)
            {
                EnigmaArtifactManager.OnServerEquipmentActivated(equipmentSlot, equipmentIndex);
            }
        }
    }
}
