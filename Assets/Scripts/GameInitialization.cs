using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInitialization : MonoBehaviour {
    public Dialog RestartDialog;
    public GameObject RedLine;
    public GameObject LeftBorder;
    public GameObject RightBorder;
    public GameObject Background;
    public GameObject Background2;
    public GameObject Ground;
    // Start is called before the first frame update
    void Start() {
        GameMain.Instance.HalfScreenWidth = Camera.main.orthographicSize * ((float)Screen.width / Screen.height);
        float widthScale = GameMain.Instance.HalfScreenWidth * 2.5f;
        float heightScale = Camera.main.orthographicSize;
        LeftBorder.transform.position = new Vector3(-GameMain.Instance.HalfScreenWidth, 0, 0);
        RightBorder.transform.position = new Vector3(GameMain.Instance.HalfScreenWidth, 0, 0);
        RedLine.transform.localScale = new Vector3(widthScale, RedLine.transform.localScale.y, 1);
        Background.transform.localScale = new Vector3(widthScale, Background.transform.localScale.y, 1);
        Background2.transform.localScale = new Vector3(widthScale, Background2.transform.localScale.y, 1);
        Ground.transform.localScale = new Vector3(widthScale, Ground.transform.localScale.y, 1);
        Screen.SetResolution(Screen.width, Screen.height, true);
        Screen.autorotateToLandscapeLeft = false;
        Screen.autorotateToLandscapeRight = false;
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;

        // ����9��16����Ļ������׼�����Token�ߴ����ų˱������г˱��ı������ᳬ����׼�߶ȵ�1/5�����߱�׼�߶ȵ�1/4
        float standardPortraitRatio = 9.0f / 16.0f;
        float standardLandscapeRatio = 1.0f / 1.0f;
        // �����Ļ��Ϊ���򣨼����С�ڸ߶ȣ��������߶�����������ӵ��˱���
        if (Screen.width / Screen.height < standardLandscapeRatio) {
            float standardHeight = (1 / standardPortraitRatio) * Screen.width;
            float tokenRadiusMultiplier = 1 + (Screen.height - standardHeight) / standardHeight;
            GameMain.Instance.TokenRadiusMultiplier = tokenRadiusMultiplier;
        }
        // ������������ȱȣ��ӵ�������
        else {
            float standardWidth = standardLandscapeRatio * Screen.height;
            float tokenRadiusMultiplier = 1 + (Screen.width - standardWidth) / standardWidth;
            GameMain.Instance.TokenRadiusMultiplier = tokenRadiusMultiplier;
        }

        Application.targetFrameRate = 60;

        GameMain.Instance.StartNewGame();
        GameMain.GameCompleted.AddListener((game) => {
            RestartDialog.Show();
        });


    }
}
