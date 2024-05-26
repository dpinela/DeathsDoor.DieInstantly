namespace DDoor.DieInstantly;

internal class DelegateButton : UIButton
{
    private System.Action action = () => {};

    // Adding this component to a button causes a NullReferenceException
    // to be thrown because Init has not been called yet to fill in its
    // fields. This is not an issue in practice, but is worth noting for
    // anyone who notices it in the logs.

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