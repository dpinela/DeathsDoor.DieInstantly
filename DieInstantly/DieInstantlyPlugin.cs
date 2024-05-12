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
        new HL.Harmony("deathsdoor.dieinstantly").PatchAll();
    }

    public static void Log(string s)
    {
        Instance!.Logger.LogInfo(s);
    }
}

[HL.HarmonyPatch(typeof(UIMenuOptions), nameof(UIMenuOptions.Start))]
internal static class OptionsMenuPatch
{
    private static void Postfix(UIMenuOptions __instance)
    {
        UE.GameObject? quitButton = null;
        foreach (var btn in __instance.grid)
        {
            DieInstantlyPlugin.Log($"found button: {btn.buttonText.text} @ ({btn.gameObject.transform.position.x}, {btn.gameObject.transform.position.y})");
            if (btn is UIAction a)
            {
                DieInstantlyPlugin.Log($"found action: {a}");
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
            DieInstantlyPlugin.Log("DIE!!!!");
            dp.currentDamageType = Damageable.DamageType.Drown;
            dp.SetHealth(0);
            dp.handleNoHealth(
                dp.gameObject.transform.position,
                new(0, 0, 0),
                dp.gameObject.transform.position
            );
        }
    }
}
