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
            dp.SetHealth(0);
            // Avoid spawning blood.
            dp.currentDamageType = Damageable.DamageType.Drown;
            dp.handleNoHealth(playerPos, new(0, 0, -1), srcPos);
            dp.fallOver(srcPos, 1, true);
            nextDeathIsInstant = true;
        }
    }

    private static bool nextDeathIsInstant = false;

    // DeathText objects get reused sometimes, so Start does not get
    // reliably called every time. OnEnable would work, but DeathText
    // doesn't have that method so we can't hook it.
    [HL.HarmonyPatch(typeof(DeathText), nameof(DeathText.FixedUpdate))]
    [HL.HarmonyPostfix]
    private static void ISaidInstantly(DeathText __instance)
    {
        if (nextDeathIsInstant)
        {
            nextDeathIsInstant = false;
            __instance.timer = __instance.delayDeathTime;
            __instance.blackWaitTimer = 0;
        }
    }
}
