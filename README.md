# <Graphic_Shooter>  
IK애니메이션을 이용한 TPS게임입니다.

<게임 방법>
- WASD를 이용하여 캐릭터를 움직입니다.
- 마우스를 사용하여 조준을 하고 좌클릭을 눌러 총을 발사합니다.
- 좌측 상단 체력바가 다 떨어지면 게임오버가 됩니다.
- 체력바 밑 하얀색 게이지가 가득찬 뒤 G를 누르면 스킬을 사용할 수가 있고 시전중에는 적들이 멈춤니다.
- 적을 처치하면 점수가 50점씩 오르게 됩니다.

## 1. 타이틀 화면
https://user-images.githubusercontent.com/63942174/161748121-b4f0eb94-e665-4e2f-a31b-f1f2c67a9a43.mp4

    타이틀 화면입니다. 아무 키나 누른 후 START버튼을 누르면 게임이 시작되게 됩니다.
    맵씬과 플레이어씬을 분리해 놔서 UI를 적용하기 편하도록 만들었습니다.

<details>
    <summary>타이틀화면(LobbyMgr)</summary>
  
``` C#
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class LobbyMgr : MonoBehaviour
{
    [Header("오브젝트")]
    public GameObject m_Title_Root;
    public GameObject m_StartMenu;
    
    [Header("텍스트")]
    public Text m_Title_PressKey;
    
    [Header("버튼")]
    public Button m_Start_Btn;
    public Button m_Exit_Btn;
    
    private bool isKeyClick = false;
    
    
    void Start()
    {
    StartCoroutine(BlinkTextAlpha());
    
    if (m_Start_Btn != null)
            m_Start_Btn.onClick.AddListener(StartBtnClick);
        if (m_Exit_Btn != null)
            m_Exit_Btn.onClick.AddListener(() =>
            {
#if  UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            });
     }
    
    
    private void Update()
    {
        if (Input.anyKeyDown && !isKeyClick)
        {
            StopAllCoroutines();
            isKeyClick = true;
            m_Title_Root.SetActive(false);
            m_StartMenu.SetActive(true);
        }
    }
    
    
#region 글자 점멸 코루틴

public IEnumerator BlinkTextAlpha()
{ 
   m_Title_PressKey.color = new Color(m_Title_PressKey.color.r, m_Title_PressKey.color.g, m_Title_PressKey.color.b, 0);


    while (m_Title_PressKey.color.a < 1.0f)
    {
        m_Title_PressKey.color = new Color(m_Title_PressKey.color.r, m_Title_PressKey.color.g, m_Title_PressKey.color.b, m_Title_PressKey.color.a + (Time.deltaTime/2 ));
        yield return null;
    }

    StartCoroutine(BlinkTextAlpha2());
}
public IEnumerator BlinkTextAlpha2()
{
    m_Title_PressKey.color = new Color(m_Title_PressKey.color.r, m_Title_PressKey.color.g, m_Title_PressKey.color.b, 1);
    while (m_Title_PressKey.color.a > 0.0f)
    {
        m_Title_PressKey.color = new Color(m_Title_PressKey.color.r, m_Title_PressKey.color.g, m_Title_PressKey.color.b, m_Title_PressKey.color.a - (Time.deltaTime/2 ));
        yield return null;
    }
    StartCoroutine(BlinkTextAlpha());
}

#endregion
}
    
```
    
 </details>



    
## 2. 마우스 감도 설정
https://user-images.githubusercontent.com/63942174/161751139-322bc8fc-4928-4eb1-8f45-94a5d8649734.mp4
    
     인게임에서 ESC를 누르면 설정창이 열립니다. 설정창이 열린 동안엔 TimeScale을 0으로해서 
    게임이 일시정지 되도록 구현하였습니다.
     설정 화면에서는 마우스의 좌우, 수직 감도와 포스트프로세싱 강도를 조절할 수 있도록 하였습니다.
     나가기 버튼을 누르면 타이틀 화면으로 돌아갑니다.

<details>
    <summary>타이틀화면</summary>
  
``` C#
    
    using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraCtrl : MonoBehaviour
{
    
    public Slider SliderH;  // 마우스 좌우감도 슬라이더
    public Slider SliderV;  // 마우스 상하감도 슬라이더
    public Text ValueH;     // 마우스 좌우감도 값
    public Text ValueV;     
    
```



    
 </details>
    
    
    
## 1. 타이틀 화면
https://user-images.githubusercontent.com/63942174/161748121-b4f0eb94-e665-4e2f-a31b-f1f2c67a9a43.mp4

    타이틀 화면입니다. 아무 키나 누른 후 START버튼을 누르면 게임이 시작되게 됩니다.
    맵씬과 플레이어씬을 분리해 놔서 UI를 적용하기 편하도록 만들었습니다.

<details>
    <summary>타이틀화면</summary>
  
``` C#
    
    
```
    
 </details>
