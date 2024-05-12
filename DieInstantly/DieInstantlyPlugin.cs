using Bep = BepInEx;
using HL = HarmonyLib;
using UE = UnityEngine;

namespace DDoor.DieInstantly;

[Bep.BepInPlugin("deathsdoor.dieinstantly", "DieInstantly", "1.0.0.0")]
internal class DieInstantlyPlugin : Bep.BaseUnityPlugin
{
    internal static DieInstantlyPlugin? Instance;

    public void Start()
    {
        Instance = this;
        new HL.Harmony("deathsdoor.dieinstantly").PatchAll(typeof(DieInstantlyPlugin));
    }

    [HL.HarmonyPatch(typeof(UIMenuOptions), nameof(UIMenuOptions.Start))]
    [HL.HarmonyPostfix]
    private static void AddButtonToMenu(UIMenuOptions __instance)
    {
        UE.GameObject? quitButton = null;
        foreach (var btn in __instance.grid)
        {
            if (btn is UIAction a)
            {
                if (a.actionId == "ExitSession")
                {
                    quitButton = a.gameObject;
                }
            }
        }
        if (quitButton != null)
        {
            const string locKey = "die_instantly";

            var dupeButton = UE.Object.Instantiate(quitButton, quitButton.transform.parent);
            var origComponent = dupeButton.GetComponent<UIButton>();
            var newComponent = dupeButton.AddComponent<DelegateButton>();
            newComponent.Init(origComponent, KillPlayer);
            var xlat = newComponent.buttonText.GetComponent<LocTextTMP>();
            xlat.locId = locKey;
            UE.Object.Destroy(origComponent);

            var newGrid = new UIButton[__instance.grid.Length + 1];
            __instance.grid.CopyTo(newGrid, 0);
            newGrid[__instance.grid.Length] = newComponent;
            __instance.grid = newGrid;

            var newCtxt = new string[__instance.ctxt.Length + 1];
            __instance.ctxt.CopyTo(newCtxt, 0);
            newCtxt[__instance.ctxt.Length] = "";
            __instance.ctxt = newCtxt;

            var data = new DialogueManager.SpeechData();
            var blk = new DialogueManager.SpeechData.SpeechBlock();
            blk.lines = new string[]{ "DIE INSTANTLY" };
            for (var i = 0; i < data.block.Length; i++)
            {
                data.block[i] = blk;
            }
            DialogueManager.instance.speechChains[locKey] = data;
        }
    }

    private static void KillPlayer()
    {
        UIMenuPauseController.instance.UnPause();
        var dp = PlayerGlobal.instance.GetComponent<DamageablePlayer>();
        
        if (dp != null)
        {
            var playerPos = dp.gameObject.transform.position;
            var srcPos = new UE.Vector3(playerPos.x, playerPos.y, playerPos.z + 1);
            dp.SetInvul(0);
            dp.SetHealth(1);
            // This method ignores its damage parameters and always deals
            // 1 damage anyway.
            // Use the Drown damage type to disable spawning blood.
            dp.ReceiveDamage(-666, 0, srcPos, playerPos, Damageable.DamageType.Drown);
            // ReceiveDamage doesn't call this for the Drown damage type.
            dp.fallOver(srcPos, 1, true);
            nextDeathIsInstant = true;
        }
    }

    private static bool nextDeathIsInstant = false;

    [HL.HarmonyPatch(typeof(DeathText), nameof(DeathText.Start))]
    [HL.HarmonyPostfix]
    private static void ISaidInstantly(DeathText __instance)
    {
        if (nextDeathIsInstant)
        {
            nextDeathIsInstant = false;
            __instance.delayDeathTime = 0;
            __instance.blackWaitTimer = 0;
        }
    }
}
