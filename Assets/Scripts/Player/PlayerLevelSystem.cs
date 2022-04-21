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
    //ÿһ��������ܾ���ֵ
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
                UIController.instance.textInfo.text = "Զ��ʱ��\nì\nʯͷ(δ����)\n��(δ����)";
                break;
            case 1:
                UIController.instance.textInfo.text = "Զ��ʱ��\nì\nʯͷ\n��(δ����)";
                break;
            case 2:
                UIController.instance.textInfo.text = "Զ��ʱ��\nì\nʯͷ\n��";
                break;
            case 3:
                UIController.instance.textInfo.text = "�ŵ�ʱ����������\n��\n��(δ����)\nս��(δ����)";
                break;
            case 4:
                UIController.instance.textInfo.text = "�ŵ�ʱ����������\n��\n��\nս��(δ����)";
                break;
            case 5:
                UIController.instance.textInfo.text = "�ŵ�ʱ����������\n��\n��\nս��";
                break;
            case 6:
                UIController.instance.textInfo.text = "���ո���ʱ��\nըҩ\n���(δ����)\n��ʯ��(δ����)";
                break;
            case 7:
                UIController.instance.textInfo.text = "���ո���ʱ��\nըҩ\n���\n��ʯ��(δ����)";
                break;
            case 8:
                UIController.instance.textInfo.text = "���ո���ʱ��\nըҩ\n���\n��ʯ��";
                break;
            case 9:
                UIController.instance.textInfo.text = "��ҵʱ��\n��ǹ\n�����(δ����)\n̹��(δ����)";
                break;
            case 10:
                UIController.instance.textInfo.text = "��ҵʱ��\n��ǹ\n�����\n̹��(δ����)";
                break;
            case 11:
                UIController.instance.textInfo.text = "��ҵʱ��\n��ǹ\n�����\n̹��";
                break;
            case 12:
                UIController.instance.textInfo.text = "��Ϣʱ��\n����\n����(δ����)\n��Ϣ�����г�(δ����)";
                break;
            case 13:
                UIController.instance.textInfo.text = "��Ϣʱ��\n����\n����\n��Ϣ�����г�(δ����)";
                break;
            case 14:
                UIController.instance.textInfo.text = "��Ϣʱ��\n����\n����\n��Ϣ�����г�";
                break;
            case 15:
                UIController.instance.textInfo.text = "δ��ʱ��\n�⽣\n����ǹ(δ����)\n���ٷɴ�(δ����)";
                break;
            case 16:
                UIController.instance.textInfo.text = "δ��ʱ��\n�⽣\n����ǹ\n���ٷɴ�(δ����)";
                break;
            case 17:
                UIController.instance.textInfo.text = "δ��ʱ��\n�⽣\n����ǹ\n���ٷɴ�";
                break;
        }
    }

}
