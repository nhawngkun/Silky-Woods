using UnityEngine;

public class uihowtoplay_SilkyWoods : UICanvas_SilkyWoods
{
    public void back()
    {
        UIManager_SilkyWoods.Instance.EnableHowToPlay(false);
        UIManager_SilkyWoods.Instance.EnableHome(true);
        SoundManager_SilkyWoods.Instance.PlayVFXSound(1);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
