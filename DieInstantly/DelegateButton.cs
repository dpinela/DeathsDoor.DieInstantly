namespace DDoor.DieInstantly;

internal class DelegateButton : UIButton
{
    private System.Action action = () => {};

    public void Init(UIButton btn, System.Action action)
    {
        highlighter = btn.highlighter;
        canClickAction = btn.canClickAction;
        highlightSound = btn.highlightSound;
        buttonText = btn.buttonText;
        canFlash = btn.canFlash;
        playSound = btn.playSound;
        ignoreHighlight = btn.ignoreHighlight;
        useBoxWidth = btn.useBoxWidth;

        this.action = action;
    }

    public override void Action()
    {
        action();
    }
}