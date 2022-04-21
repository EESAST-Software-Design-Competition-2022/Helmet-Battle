using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerLevelSystem : NetworkBehaviour
{
    public TextMesh LevelText;
    public ProgressBar progress;
    PlayerController playerController;
    [HideInInspector]
    public int level;
    int computeLevel;
    //每一级所需的总经验值
    public static float[] exps = {3,5,10,20,30,40,50,60,70,80,90,100,110,120,130,145,170,230};
    
    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        playerController.playerLevelSystem = this;
        level = -1;
    }
    private void Update()
    {
#if UNITY_EDITOR
        if (isLocalPlayer)
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (level< exps.Length)
                    playerController.score = exps[level] + 1;
            }
        }
#endif
    }
    private void FixedUpdate()
    {
        int computeLevel;
        computeLevel = ComputeLevel(playerController.score);
        if(level < computeLevel && level >= 0)
        {
            OnLevelUp();
        }
        level = computeLevel;
        if (isLocalPlayer)
        {
            LevelUpdate(level);
        }
        LevelText.text = "Lv: " + level.ToString();
        var percent = level > 0 ? (playerController.score - exps[level - 1]) / (exps[level] - exps[level - 1]) : playerController.score / exps[level];
        progress.SetValue(percent);
    }
    public void OnLevelUp()
    {
        SoundManager.instance.UpgradeAudio();
    }
    int ComputeLevel(float score)
    {
        int i = 0;
        for (i = 0; i <17 && score > exps[i]; i++) ;
        return i;
    }
    void LevelUpdate(int level)
    {
        switch (level)
        {
            case 0:
                UIController.instance.textInfo.text = "远古时代\n矛\n石头(未解锁)\n马(未解锁)";
                break;
            case 1:
                UIController.instance.textInfo.text = "远古时代\n矛\n石头\n马(未解锁)";
                break;
            case 2:
                UIController.instance.textInfo.text = "远古时代\n矛\n石头\n马";
                break;
            case 3:
                UIController.instance.textInfo.text = "古典时代和中世纪\n剑\n弓(未解锁)\n战马(未解锁)";
                break;
            case 4:
                UIController.instance.textInfo.text = "古典时代和中世纪\n剑\n弓\n战马(未解锁)";
                break;
            case 5:
                UIController.instance.textInfo.text = "古典时代和中世纪\n剑\n弓\n战马";
                break;
            case 6:
                UIController.instance.textInfo.text = "文艺复兴时期\n炸药\n火铳(未解锁)\n射石炮(未解锁)";
                break;
            case 7:
                UIController.instance.textInfo.text = "文艺复兴时期\n炸药\n火铳\n射石炮(未解锁)";
                break;
            case 8:
                UIController.instance.textInfo.text = "文艺复兴时期\n炸药\n火铳\n射石炮";
                break;
            case 9:
                UIController.instance.textInfo.text = "工业时代\n手枪\n火箭炮(未解锁)\n坦克(未解锁)";
                break;
            case 10:
                UIController.instance.textInfo.text = "工业时代\n手枪\n火箭炮\n坦克(未解锁)";
                break;
            case 11:
                UIController.instance.textInfo.text = "工业时代\n手枪\n火箭炮\n坦克";
                break;
            case 12:
                UIController.instance.textInfo.text = "信息时代\n手榴弹\n导弹(未解锁)\n信息化自行车(未解锁)";
                break;
            case 13:
                UIController.instance.textInfo.text = "信息时代\n手榴弹\n导弹\n信息化自行车(未解锁)";
                break;
            case 14:
                UIController.instance.textInfo.text = "信息时代\n手榴弹\n导弹\n信息化自行车";
                break;
            case 15:
                UIController.instance.textInfo.text = "未来时代\n光剑\n镭射枪(未解锁)\n光速飞船(未解锁)";
                break;
            case 16:
                UIController.instance.textInfo.text = "未来时代\n光剑\n镭射枪\n光速飞船(未解锁)";
                break;
            case 17:
                UIController.instance.textInfo.text = "未来时代\n光剑\n镭射枪\n光速飞船";
                break;
        }
    }

}
