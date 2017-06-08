using UnityEngine;
using UnityEngine.UI;

public class KillCountBox : MonoBehaviour, EventHandler {

    Text box;
    Button button;
    Text buttonText;

    int alpha255 = 0;
    bool dismissed = false;
    Timer runTimer;
    readonly float WARNTIME = 3f;

    public void Start() {
        box = gameObject.GetComponent<Text>();
        button = transform.Find("Button").GetComponent<Button>();
        buttonText = button.transform.Find("Text").GetComponent<Text>();
        EventManager.Instance().RegisterForEventType("kill_display", this);
        runTimer = new Timer();
    }
    public void Update() {
        if (!dismissed) {
            //button.enabled = true;
            alpha255 += 16;
            if (alpha255 > 255) {
                alpha255 = 255;
            }
        }else {
            //button.enabled = false;
            alpha255 -= 16;
            if (alpha255 < 0) {
                alpha255 = 0;
            }
        }
        SetAlpha(alpha255);
        if(runTimer.TimeElapsedSecs() > WARNTIME) {
            dismissed = true;
        }
    }
    public void PrematurelyDismiss() {
        Debug.Log("dismissed");
        dismissed = true;
    }
    public void HandleEvent(GameEvent ge) {
        //only event is a kill display
        string message = (string)ge.args[0];
        dismissed = false;
        runTimer.Restart();
        box.text = message;
    }
    void SetAlpha(int eightbit) {
        float alpha = ((float)eightbit) / 255f;
        box.color = new Color(box.color.r, box.color.g, box.color.b, alpha);
        button.image.color = new Color(button.image.color.r, button.image.color.g, button.image.color.b, alpha);
        buttonText.color = new Color(buttonText.color.r, buttonText.color.g, buttonText.color.b, alpha);
    }

    public static void KillDisplay(int killedSoFar,int maxEnemies, bool first) {
        GameEvent ge = new GameEvent("kill_display");
        string message = killedSoFar + " out of " + maxEnemies + " defeated!";
        if (first) {
            message = "Scanners found " + maxEnemies + " enemies!\n" + message;
        }
        ge.addArgument(message);
        EventManager.Instance().RaiseEvent(ge);
    }
}
