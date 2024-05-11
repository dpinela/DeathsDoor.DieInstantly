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
            var dupeButton = UE.Object.Instantiate(quitButton, quitButton.transform.parent);
            var newGrid = new UIButton[__instance.grid.Length + 1];
            __instance.grid.CopyTo(newGrid, 0);
            newGrid[__instance.grid.Length] = dupeButton.GetComponent<UIButton>();
            __instance.grid = newGrid;
        }
    }
}
