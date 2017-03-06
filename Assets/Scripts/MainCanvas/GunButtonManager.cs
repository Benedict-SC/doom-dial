using UnityEngine;
using UnityEngine.UI;

public class GunButtonManager : MonoBehaviour, EventHandler
{
    public static bool STATIC_GUNS = false;
    GunButton[] gbuttons = new GunButton[6];
    Gun[] baseGuns = new Gun[6];
    public void Start() {
        EventManager em = EventManager.Instance();
        em.RegisterForEventType("dial_locked", this);

        GameObject buttonHolder = GameObject.Find("GunButtons");
        for(int i = 1; i <= 6; i++) {
            GunButton gb = buttonHolder.transform.FindChild("Button" + i).GetComponent<GunButton>();
            gbuttons[i - 1] = gb;
            baseGuns[i - 1] = gb.gun;
        }
    }
    public void HandleEvent(GameEvent ge)
    {
        if (STATIC_GUNS) {
            return;
        }
        //if (ge.type.Equals("dial_locked")){
            float lockRotArg = (float)ge.args[0];
            int lockRot = ((int)lockRotArg) % 360;
            int offset = 6 + (lockRot / 60);
            for(int i = 0; i < 6; i++) {
                int idx = (i + offset) % 6;
                Debug.Log("idx " + i + ": " + idx);
                gbuttons[i].SetGun(baseGuns[idx]);
            }
        //}
    }
}