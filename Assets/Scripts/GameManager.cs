using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

[System.Serializable]
public class ChipPos
{
    public int x = -1;
    public int y = -1;
}

[System.Serializable]
public class StateResources
{
    public VideoClip StateVidTop;
    public VideoClip StateVidBottom;
    public GameObject GameImg;
    public GameObject GameSuccess;
    public GameObject PreGameTop;
    public GameObject FailureTop;
    public GameObject BottomPlayMenu;
    public VideoClip PostGameVidTop;
    public VideoClip PostGameBottom;
    public VideoClip CombinedVidTop;
    public VideoClip CombinedVidBottom;
    public float FirstVidEnd;
    public float SecondVidStart;
}

public class GameManager : MonoBehaviour
{
    public List<Sprite> MenuSprites;
    public Image Menu;
    public enum State { Opening, G1Vid, G1PreGame, G1Game, G1Success, G1Failure, G1PostGameVid, G2Vid, G2PreGame, G2Game, G2Success, G2Failure, G2PostGameVid, G3Vid, G3PreGame, G3Game, G3Success, G3Failure, G3PostGameVid, Ending}
    public State current_state = State.Opening;

    public List<StateResources> StateResourcesList;
    public RenderTexture RTTop;
    public RenderTexture RTBottom;
    public RenderTexture RTEmpty;
    public GameObject G1;
    public GameObject G2;
    public GameObject G3;
    public Image G1_Timer;
    public Image G2_Timer;
    public Image G3_Timer;
    public TMP_Text G1_TimerValue;
    public TMP_Text G2_TimerValue;
    public TMP_Text G3_TimerValue;
    public TMP_Text G1_AttemptsValue;
    public TMP_Text G2_AttemptsValue;
    public TMP_Text G3_AttemptsValue;
    List<List<int>> g1_chips_pos = new List<List<int>>();
    List<List<int>> g3_chips_pos = new List<List<int>>();
    public List<Sprite> g3_chip_sprites;
    public List<Image> g3_chip_objects;
    public List<Image> g3_chip_cover_objects;
    public List<Image> g3_big_vees;
    public int[] game1_faults;
    public ChipPos[] game2_faults;
    public List<Image> g1_attempts = new List<Image>();
    public List<Image> g2_attempts = new List<Image>();
    public List<Image> g3_attempts = new List<Image>();
    public List<Sprite> g1_post_attempts = new List<Sprite>();
    public List<Sprite> g2_post_attempts = new List<Sprite>();
    public List<Sprite> g3_post_attempts = new List<Sprite>();
    public Sprite G1_pre_attempts;
    public Sprite G2_pre_attempts;
    public Sprite G3_pre_attempts;
    public MaskManager G1_maskManager;
    public MaskManager G2_maskManager;
    public SwapManager swapManager;
    public Sprite G1_magnifier_sprite_with_finger;
    public Sprite G1_magnifier_sprite_without_finger;
    public Sprite G1_magnifier_sprite_without_nothing;
    public Sprite G2_flash_sprite_with_finger;
    public Sprite G2_flash_sprite_without_finger;
    public Sprite G2_flash_sprite_without_nothing;
    public bool G1_drag_on = false;
    public bool G2_drag_on = false;
    public Image G1_mag_glass_img;
    public Image G2_mag_glass_img;
    public GameObject success_indicator_parent;
    public GameObject success_indicator_prefab;
    public GameObject failure_indicator_parent;
    public GameObject failure_indicator_prefab;

    public int current_game = 0;
    const float TIMER_START_G1 = 30f;
    const float TIMER_START_G2 = 30f;
    const float TIMER_START_G3 = 60f;
    const float INSTRUCTIONS_TIMER_G1 = 7.1f;
    const float INSTRUCTIONS_TIMER_G2 = 2.5f;
    const float INSTRUCTIONS_TIMER_G3 = 2f;
    float curr_timer;
    float instructions_curr_timer;
    bool instructions;
    public int attempts_done = 0;
    public int successes_done = 0;

    public GameObject G1_hand;
    public GameObject G2_hand;

    public VideoPlayer TopVideoPlayer;
    public VideoPlayer BottomVideoPlayer;
    public GameObject TopVideoRawImage;
    public GameObject BottomVideoRawImage;

    public Animator Highlight1Anim;
    public Animator Highlight2Anim;
    public Animator Highlight3Anim;

    // Start is called before the first frame update
    void Start()
    {
        InitStation();
        Application.targetFrameRate = 60;
        TopVideoPlayer.loopPointReached += TopVideoEnded;
        BottomVideoPlayer.loopPointReached += BottomVideoEnded;
    }

    // Update is called once per frame
    void Update()
    {
        //print(Time.timeSinceLevelLoad);
        if (current_state == State.Opening && BottomVideoPlayer.isPlaying && BottomVideoPlayer.time >= 20)
        {
            HighlightMenu(1, true);
        }
        else if (current_state == State.G1PostGameVid && BottomVideoPlayer.isPlaying && BottomVideoPlayer.time >= 30)
        {
            HighlightMenu(2, true);
        }
        else if (current_state == State.G2PostGameVid && BottomVideoPlayer.isPlaying && BottomVideoPlayer.time >= 12)
        {
            HighlightMenu(3, true);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (TopVideoRawImage.activeSelf && BottomVideoRawImage.activeSelf)
            {
                TopVideoPlayer.time = BottomVideoPlayer.clip.length - 3;
                BottomVideoPlayer.time = BottomVideoPlayer.clip.length - 3;
            }
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            curr_timer = 5;
        }

        if (current_game == 1)
        {
            if (successes_done >= g1_attempts.Count)
            {
                G1_hand.SetActive(false);
                if (current_state == State.G1Game)
                {
                    MoveToState(State.G1Success);
                }
                return;
            }
            if (attempts_done >= g1_attempts.Count)
            {
                G1_hand.SetActive(false);
                if (current_state == State.G1Game)
                {
                    MoveToState(State.G1Failure);//G1PostGameVid);
                }
                return;
            }
        }
        if (current_game == 2)
        {
            if (successes_done >= g2_attempts.Count)
            {
                G2_hand.SetActive(false);
                if (current_state == State.G2Game)
                {
                    MoveToState(State.G2Success);
                }
                return;
            }
            if (attempts_done >= g2_attempts.Count)
            {
                G2_hand.SetActive(false);
                if (current_state == State.G2Game)
                {
                    MoveToState(State.G2Failure);//G2PostGameVid);
                }
                return;
            }
        }
        if (current_game == 3)
        {
            if (attempts_done >= g3_attempts.Count || successes_done >= 6)
            {
                if (current_state == State.G3Game)
                {
                    MoveToState(State.G3Success);
                }
                return;
            }
                
        }
        if (instructions_curr_timer > 0 && curr_timer <= 0)
        {
            instructions_curr_timer -= Time.deltaTime;
        }
        else if (instructions_curr_timer <= 0 && instructions)
        {
            instructions = false;
            if (current_game == 1)
                curr_timer = TIMER_START_G1;
            else if (current_game == 2)
                curr_timer = TIMER_START_G2;
            else if (current_game == 3)
                curr_timer = TIMER_START_G3;
        }
        if (curr_timer > 0 && !instructions)
        {
            curr_timer -= Time.deltaTime;
            //second label
            G1_Timer.fillAmount = (curr_timer / TIMER_START_G1);
            G2_Timer.fillAmount = (curr_timer / TIMER_START_G2);
            G3_Timer.fillAmount = (curr_timer / TIMER_START_G3);
            G1_TimerValue.text = (Mathf.Round(curr_timer).ToString());
            G2_TimerValue.text = (Mathf.Round(curr_timer).ToString());
            G3_TimerValue.text = (Mathf.Round(curr_timer).ToString());
        }
        else if (curr_timer <= 0 && !instructions)
        {
            if (current_game == 1 && current_state == State.G1Game)
            {
                G1_hand.SetActive(false);
                MoveToState(State.G1Failure);//G1PostGameVid);
            }
            if (current_game == 2 && current_state == State.G2Game)
            {
                G2_hand.SetActive(false);
                MoveToState(State.G2Failure);//G2PostGameVid);
            }
            if (current_game == 3 && current_state == State.G3Game)
            {
                MoveToState(State.G3Failure);//G3PostGameVid);
            }
        }
    }

    public void G2_showindication(bool successful, float x, float y)
    {
        if (successful)
        {
            GameObject indicator = Instantiate(success_indicator_prefab, success_indicator_parent.transform);
            indicator.transform.Translate(new Vector2(-1916 / 2, -1080 / 2));
            indicator.transform.Translate(new Vector2(x, y));
        }
        else
        {
            GameObject indicator = Instantiate(failure_indicator_prefab, failure_indicator_parent.transform);
            indicator.transform.Translate(new Vector2(-1916 / 2, -1080 / 2));
            indicator.transform.Translate(new Vector2(x, y));
        }
    }    

    public void SuccessesDone()
    {
        successes_done++;
    }
    public void AttemptDone(bool successfully)
    {
        attempts_done++;
        if (current_game == 1 && (attempts_done < g1_attempts.Count + 1))
        {
            G1_AttemptsValue.text = g1_attempts.Count - attempts_done + " attempts";
            if (successfully)
            {
                g1_attempts[attempts_done - 1].sprite = g1_post_attempts[0];
            }
            else
            {
                g1_attempts[attempts_done - 1].sprite = g1_post_attempts[1];
            }

        }
        else if (current_game == 2 && (attempts_done < g2_attempts.Count + 1))
        {
            G2_AttemptsValue.text = g2_attempts.Count - attempts_done + " attempts";
            if (successfully)
            {
                g2_attempts[attempts_done - 1].sprite = g2_post_attempts[0];
            }
            else
            {
                g2_attempts[attempts_done - 1].sprite = g2_post_attempts[1];
            }
        }
        else if (current_game == 3 && (attempts_done < g3_attempts.Count + 1))
        {
            G3_AttemptsValue.text = g3_attempts.Count - attempts_done + " attempts";
            if (successfully)
            {
                g3_attempts[attempts_done - 1].sprite = g3_post_attempts[0];
            }
            else
            {
                g3_attempts[attempts_done - 1].sprite = g3_post_attempts[1];
            }
        }
    }
    public ChipPos ChipByPos(Vector3 pos)
    {
        ChipPos result = new ChipPos();
        for (int i = 0; i < g1_chips_pos[0].Count/2; i++)
        {
            for (int j = 0; j < g1_chips_pos[1].Count / 2; j++)
            {
                if ((pos.x >= g1_chips_pos[0][i*2] && pos.x <= g1_chips_pos[0][(i*2)+1]) && (pos.y >= g1_chips_pos[1][j*2] && pos.y <= g1_chips_pos[1][(j*2)+1]))
                {
                    result.x = i;
                    result.y = j;
                }
            }
        }
        return result;
    }

    public ChipPos G3ChipByPos(Vector3 pos)
    {
        ChipPos result = new ChipPos();
        for (int i = 0; i < g3_chips_pos[0].Count / 2; i++)
        {
            for (int j = 0; j < g3_chips_pos[1].Count / 2; j++)
            {
                if ((pos.x >= g3_chips_pos[0][i * 2] && pos.x <= g3_chips_pos[0][(i * 2) + 1]) && (pos.y >= g3_chips_pos[1][j * 2] && pos.y <= g3_chips_pos[1][(j * 2) + 1]))
                {
                    result.x = i;
                    result.y = j;
                    break;
                }
            }
        }
        return result;
    }

    public void DragModeToggleClicked(int game)
    {
        if (game == 1 && current_game == 1)
        {
            G1_drag_on = !G1_drag_on;
            if (G1_drag_on)
            {
                G1_mag_glass_img.sprite = G1_magnifier_sprite_without_finger;
                G1_maskManager.mark_mode_on = false;
                G1_hand.SetActive(false);
            }
            else
            {
                G1_mag_glass_img.sprite = G1_magnifier_sprite_without_nothing; //finger;
                G1_maskManager.mark_mode_on = true;
                G1_hand.SetActive(true);
            }
        }
        else if (game == 2 && current_game == 2)
        {
            G2_drag_on = !G2_drag_on;
            if (G2_drag_on)
            {
                G2_mag_glass_img.sprite = G2_flash_sprite_without_finger;
                G2_maskManager.mark_mode_on = false;
                G2_hand.SetActive(false);
            }
            else
            {
                G2_mag_glass_img.sprite = G2_flash_sprite_without_nothing; //_finger;
                G2_maskManager.mark_mode_on = true;
                G2_hand.SetActive(true);
            }
        }
    }
    void InitStation()
    {
        //InitG1();
        g1_chips_pos.Add(new List<int> { 510, 708, 800, 994, 1078, 1288, 1348, 1546});
        g1_chips_pos.Add(new List<int> { 166, 380, 427, 641, 688, 900});
        g3_chips_pos.Add(new List<int> { 540, 675, 817, 957, 1104, 1237, 1387, 1516 });
        g3_chips_pos.Add(new List<int> { 212, 352, 468, 604, 719, 862 });

        G1_hand.SetActive(false);
        G2_hand.SetActive(false);

        StateResourcesList[1].GameImg.SetActive(false);
        StateResourcesList[1].GameSuccess.SetActive(false);
        StateResourcesList[1].PreGameTop.SetActive(false);
        StateResourcesList[1].FailureTop.SetActive(false);
        StateResourcesList[1].BottomPlayMenu.SetActive(false);
        StateResourcesList[2].GameImg.SetActive(false);
        StateResourcesList[2].GameSuccess.SetActive(false);
        StateResourcesList[2].PreGameTop.SetActive(false);
        StateResourcesList[2].FailureTop.SetActive(false);
        StateResourcesList[2].BottomPlayMenu.SetActive(false);
        StateResourcesList[3].GameImg.SetActive(false);
        StateResourcesList[3].GameSuccess.SetActive(false);
        StateResourcesList[3].PreGameTop.SetActive(false);
        StateResourcesList[3].FailureTop.SetActive(false);
        StateResourcesList[3].BottomPlayMenu.SetActive(false);
        //###
    }

    void InitG1()
    {
        //Menu.sprite = MenuSprites[1];
        //curr_timer = TIMER_START_G1;
        instructions_curr_timer = INSTRUCTIONS_TIMER_G1;
        instructions = true;
        curr_timer = 0;
        G1.SetActive(true);
        ////StateResourcesList[1].PreGameTop.SetActive(true);
        G2.SetActive(false);
        G3.SetActive(false);
        StateResourcesList[1].FailureTop.SetActive(false);
        G1_Timer.SetNativeSize();
        G1_TimerValue.text = TIMER_START_G1.ToString();
        InitCommon();
        current_game = 1;
        foreach (Image i in g1_attempts) { i.sprite = G1_pre_attempts; }
        G1_drag_on = false;
        G1_mag_glass_img.sprite = G1_magnifier_sprite_without_nothing;//_finger;
        G1_maskManager.mark_mode_on = true;

        G1_maskManager.Init();
        G1_hand.SetActive(true);
        G2_hand.SetActive(false);
    }
    void InitG2()
    {
        //Menu.sprite = MenuSprites[2];
        //curr_timer = TIMER_START_G2;
        instructions_curr_timer = INSTRUCTIONS_TIMER_G2;
        instructions = true;
        curr_timer = 0;
        G1.SetActive(false);
        G2.SetActive(true);
        ////StateResourcesList[1].PreGameTop.SetActive(true);
        G3.SetActive(false);
        StateResourcesList[2].FailureTop.SetActive(false);
        G2_Timer.SetNativeSize();
        G2_TimerValue.text = TIMER_START_G2.ToString();
        InitCommon();
        current_game = 2;
        foreach (Image i in g2_attempts) { i.sprite = G2_pre_attempts; }
        foreach (Transform child in success_indicator_parent.transform) { GameObject.Destroy(child.gameObject); }
        foreach (Transform child in failure_indicator_parent.transform) { GameObject.Destroy(child.gameObject); }
        G2_drag_on = false;
        G2_mag_glass_img.sprite = G2_flash_sprite_without_nothing;//_finger;
        
        G2_maskManager.Init();
        G1_hand.SetActive(false);
        G2_hand.SetActive(true);
    }
    void InitG3()
    {
        //Menu.sprite = MenuSprites[3];
        //curr_timer = TIMER_START_G3;
        instructions_curr_timer = INSTRUCTIONS_TIMER_G3;
        instructions = true;
        curr_timer = 0;
        G1.SetActive(false);
        G2.SetActive(false);
        G3.SetActive(true);
        ////StateResourcesList[1].PreGameTop.SetActive(true);
        StateResourcesList[3].FailureTop.SetActive(false);
        G3_Timer.SetNativeSize();
        G3_TimerValue.text = TIMER_START_G3.ToString();
        InitCommon();
        current_game = 3;
        foreach (Image i in g3_attempts) { i.sprite = G3_pre_attempts; }
        foreach (Image i in g3_chip_cover_objects) { i.color = Color.white; }
        foreach (Image i in g3_big_vees) { i.color = new Color(1,1,1,0); }
        swapManager.InitSwap();
        G1_hand.SetActive(false);
        G2_hand.SetActive(false);
    }

    void InitCommon()
    {
        //curr_timer = TIMER_START; //moved to individual init since games now have different times
        G1_Timer.fillAmount = 1;
        G2_Timer.fillAmount = 1;
        G3_Timer.fillAmount = 1;
        attempts_done = 0;
        successes_done = 0;
    }

    public void PlayButtonPressed(bool play)
    {
        StateResourcesList[1].BottomPlayMenu.SetActive(false);
        StateResourcesList[2].BottomPlayMenu.SetActive(false);
        StateResourcesList[3].BottomPlayMenu.SetActive(false);
        StateResourcesList[1].FailureTop.SetActive(false);
        StateResourcesList[2].FailureTop.SetActive(false);
        StateResourcesList[3].FailureTop.SetActive(false);
        StateResourcesList[1].PreGameTop.SetActive(false);
        StateResourcesList[2].PreGameTop.SetActive(false);
        StateResourcesList[3].PreGameTop.SetActive(false);
        //if (before)
        {
            int game = 0;
            if (current_state == State.G1Failure || current_state == State.G1Game || current_state == State.G1PostGameVid || current_state == State.G1PreGame || current_state == State.G1Success || current_state == State.G1Vid)
                game = 1;
            else if (current_state == State.G2Failure || current_state == State.G2Game || current_state == State.G2PostGameVid || current_state == State.G2PreGame || current_state == State.G2Success || current_state == State.G2Vid)
                game = 2;
            else if (current_state == State.G3Failure || current_state == State.G3Game || current_state == State.G3PostGameVid || current_state == State.G3PreGame || current_state == State.G3Success || current_state == State.G3Vid)
                game = 3;
            if (play)
            {
                switch (game)
                {
                    case 1:
                        MoveToState(State.G1Game);
                        break;
                    case 2:
                        MoveToState(State.G2Game);
                        break;
                    case 3:
                        MoveToState(State.G3Game);
                        break;
                    default:
                        break;
                }
            }
            else //skip game
            {
                switch (game)
                {
                    case 1:
                        MoveToState(State.G1PostGameVid);
                        break;
                    case 2:
                        MoveToState(State.G2PostGameVid);
                        break;
                    case 3:
                        MoveToState(State.G3PostGameVid);
                        break;
                    default:
                        break;
                }
            }

        }
    }

    public void G1_Btn_Clicked()
    {
        //if (current_state == State.Opening)
        {
            MoveToState(State.G1Vid);
        }
        //InitG1();
    }

    public void G2_Btn_Clicked()
    {
        //if (current_state == State.G1PostGameVid)
        {
            MoveToState(State.G2Vid);
        }
        //InitG2();
    }
    public void G3_Btn_Clicked()
    {
        //if (current_state == State.G2PostGameVid)
        {
            MoveToState(State.G3Vid);
        }
        //InitG3();
    }

    private void HightlighsOff()
    {
        HighlightMenu(1, false);
        HighlightMenu(2, false);
        HighlightMenu(3, false);
    }
    private void HighlightMenu(int index, bool show)
    {
        switch (index)
        {
            case 1:
                if (show)
                    Highlight1Anim.SetInteger("flash", 1);
                else
                    Highlight1Anim.SetInteger("flash", 0);
                break;
            case 2:
                if (show)
                    Highlight2Anim.SetInteger("flash", 1);
                else
                    Highlight2Anim.SetInteger("flash", 0);
                break;
            case 3:
                if (show)
                    Highlight3Anim.SetInteger("flash", 1);
                else
                    Highlight3Anim.SetInteger("flash", 0);
                break;
            default:
                break;
        }
    }

    private void BottomVideoEnded(VideoPlayer source)
    {
        HightlighsOff();
        if (current_state == State.Opening)
        {
            TopVideoPlayer.Stop();
            BottomVideoPlayer.Stop();
            TopVideoPlayer.time = 0;
            BottomVideoPlayer.time = 0;
            TopVideoPlayer.Play();
            BottomVideoPlayer.Play();
        }
        if (current_state == State.G1Vid)
        {
            MoveToState(State.G1PreGame);//G1Game);
        }
        else if (current_state == State.G2Vid)
        {
            MoveToState(State.G2PreGame);//.G2Game);
        }
        else if (current_state == State.G3Vid)
        {
            MoveToState(State.G3PreGame);//G3Game);
        }
        else if (current_state == State.G1PostGameVid)
        {
            MoveToState(State.Ending);
        }
        else if (current_state == State.G2PostGameVid)
        {
            MoveToState(State.Ending);
        }
        else if (current_state == State.G3PostGameVid)
        {
            MoveToState(State.Ending);
        }
        else if (current_state == State.Ending)
        {
            SceneManager.LoadScene(0);
        }
    }

    private void TopVideoEnded(VideoPlayer source)
    {
        //do nothing
    }

    private void HideGames()
    {
        StateResourcesList[1].GameImg.SetActive(false);
        StateResourcesList[1].GameSuccess.SetActive(false);
        StateResourcesList[2].GameImg.SetActive(false);
        StateResourcesList[2].GameSuccess.SetActive(false);
        StateResourcesList[3].GameImg.SetActive(false);
        StateResourcesList[3].GameSuccess.SetActive(false);
    }
    private void BottomVideoPrepared(VideoPlayer source)
    {
        TopVideoRawImage.GetComponent<RawImage>().texture = RTTop;
        BottomVideoRawImage.GetComponent<RawImage>().texture = RTBottom;
        TopVideoPlayer.Play();
        BottomVideoPlayer.Play();
        Invoke("HideGames", 0.5f);

        if (current_state == State.G1Vid)
        {
            
        }
        else if (current_state == State.G1PostGameVid)
        {

        }
        else if (current_state == State.G2Vid)
        {
            
        }
        else if (current_state == State.G2PostGameVid)
        {

        }
        else if (current_state == State.G3Vid)
        {
            
        }
        else if (current_state == State.G3PostGameVid)
        {
            
        }
    }

    private void TopVideoPrepared(VideoPlayer source)
    {
        //do nothing
    }

    private void ShowSuccess(GameObject game, GameObject gameSuccess)
    {
        game.SetActive(false);
        gameSuccess.SetActive(true);
        switch (current_game)
        {
            case 1:
                Invoke("MoveToStateG1PostGameVid", 3f);
                break;
            case 2:
                Invoke("MoveToStateG2PostGameVid", 3f);
                break;
            case 3:
                Invoke("MoveToStateG3PostGameVid", 3f);
                break;
        }
    }

    private void ShowPlayButtons(GameObject topToShow, GameObject bottomToShow, GameObject game)
    {
        game.SetActive(false);
        topToShow.SetActive(true);
        bottomToShow.SetActive(true);
        //$$$TopVideoRawImage.SetActive(false);
        //$$$BottomVideoRawImage.SetActive(false);
        //switch (current_game)
        //{
        //    case 1:
        //        Invoke("MoveToStateG1PostGameVid", 3f);
        //        break;
        //    case 2:
        //        Invoke("MoveToStateG2PostGameVid", 3f);
        //        break;
        //    case 3:
        //        Invoke("MoveToStateG3PostGameVid", 3f);
        //        break;
        //}
    }

    private void MoveToStateG1PostGameVid() { MoveToState(State.G1PostGameVid); }
    private void MoveToStateG2PostGameVid() { MoveToState(State.G2PostGameVid); }
    private void MoveToStateG3PostGameVid() { MoveToState(State.G3PostGameVid); }
    private void HideVidShowImg(GameObject img, int new_current_game)
    {
        StateResourcesList[1].PreGameTop.SetActive(false);
        StateResourcesList[2].PreGameTop.SetActive(false);
        StateResourcesList[3].PreGameTop.SetActive(false);
        TopVideoPlayer.Stop();
        BottomVideoPlayer.Stop();
        TopVideoRawImage.GetComponent<RawImage>().texture = RTEmpty;
        BottomVideoRawImage.GetComponent<RawImage>().texture = RTEmpty;
        //$$$TopVideoRawImage.SetActive(false);
        //$$$BottomVideoRawImage.SetActive(false);
        StateResourcesList[new_current_game].GameImg.SetActive(true);

        switch (new_current_game)
        {
            case 1:
                current_game = 1;
                InitG1();
                break;
            case 2:
                current_game = 2;
                InitG2();
                break;
            case 3:
                current_game = 3;
                InitG3();
                break;
            default:
                break;
        }
    }
    private void PlayNewVid(VideoClip top, VideoClip bottom)
    {
        //StateResourcesList[1].GameImg.SetActive(false);
        //StateResourcesList[1].GameSuccess.SetActive(false);
        //StateResourcesList[2].GameImg.SetActive(false);
        //StateResourcesList[2].GameSuccess.SetActive(false);
        //StateResourcesList[3].GameImg.SetActive(false);
        //StateResourcesList[3].GameSuccess.SetActive(false);
        //$$$TopVideoRawImage.SetActive(true);
        //$$$BottomVideoRawImage.SetActive(true);
        StateResourcesList[1].FailureTop.SetActive(false);
        StateResourcesList[1].GameImg.SetActive(false);
        StateResourcesList[1].GameSuccess.SetActive(false);
        StateResourcesList[1].BottomPlayMenu.SetActive(false);

        StateResourcesList[2].FailureTop.SetActive(false);
        StateResourcesList[2].GameImg.SetActive(false);
        StateResourcesList[2].GameSuccess.SetActive(false);
        StateResourcesList[2].BottomPlayMenu.SetActive(false);

        StateResourcesList[3].FailureTop.SetActive(false);
        StateResourcesList[3].GameImg.SetActive(false);
        StateResourcesList[3].GameSuccess.SetActive(false);
        StateResourcesList[3].BottomPlayMenu.SetActive(false);

        TopVideoPlayer.Stop();
        BottomVideoPlayer.Stop();
        TopVideoRawImage.GetComponent<RawImage>().texture = RTTop;
        BottomVideoRawImage.GetComponent<RawImage>().texture = RTBottom;
        TopVideoPlayer.isLooping = false;
        BottomVideoPlayer.isLooping = false;
        TopVideoPlayer.clip = top;
        BottomVideoPlayer.clip = bottom;
        TopVideoPlayer.Prepare();
        BottomVideoPlayer.Prepare();

        //TopVideoPlayer.Play();
        //BottomVideoPlayer.Play();
    }
    private void MoveToState(State newState)
    {
        switch (newState)       
        {
            case State.Opening:
                HightlighsOff();
                break;
            case State.G1Vid:
                HightlighsOff();
                StateResourcesList[1].FailureTop.SetActive(false);
                StateResourcesList[1].GameImg.SetActive(false);
                StateResourcesList[1].GameSuccess.SetActive(false);
                StateResourcesList[1].BottomPlayMenu.SetActive(false);

                StateResourcesList[2].FailureTop.SetActive(false);
                StateResourcesList[2].GameImg.SetActive(false);
                StateResourcesList[2].GameSuccess.SetActive(false);
                StateResourcesList[2].BottomPlayMenu.SetActive(false);

                StateResourcesList[3].FailureTop.SetActive(false);
                StateResourcesList[3].GameImg.SetActive(false);
                StateResourcesList[3].GameSuccess.SetActive(false);
                StateResourcesList[3].BottomPlayMenu.SetActive(false);



                TopVideoPlayer.prepareCompleted += TopVideoPrepared;
                BottomVideoPlayer.prepareCompleted += BottomVideoPrepared;
                Menu.sprite = MenuSprites[1];
                PlayNewVid(StateResourcesList[1].StateVidTop, StateResourcesList[1].StateVidBottom);
                current_state = State.G1Vid;
                break;
            case State.G1Game:
                HightlighsOff();
                HideVidShowImg(StateResourcesList[1].GameImg, 1);
                current_state = State.G1Game;
                break;
            case State.G1Success:
                HightlighsOff();
                ShowSuccess(StateResourcesList[1].GameImg, StateResourcesList[1].GameSuccess);
                current_state = State.G1Success;
                break;
            case State.G1PreGame:
                HightlighsOff();
                ShowPlayButtons(StateResourcesList[1].PreGameTop, StateResourcesList[1].BottomPlayMenu, StateResourcesList[1].GameImg);
                PreparePostGameVid(1);
                current_state = State.G1Failure;
                break;
            case State.G1Failure:
                HightlighsOff();
                ShowPlayButtons(StateResourcesList[1].FailureTop, StateResourcesList[1].BottomPlayMenu, StateResourcesList[1].GameImg);
                current_state = State.G1Failure;
                break;
            case State.G1PostGameVid:
                HightlighsOff();
                PlayNewVid(StateResourcesList[1].PostGameVidTop, StateResourcesList[1].PostGameBottom);
                current_state = State.G1PostGameVid;
                break;
            case State.G2Vid:
                HightlighsOff();
                StateResourcesList[1].FailureTop.SetActive(false);
                StateResourcesList[1].GameImg.SetActive(false);
                StateResourcesList[1].GameSuccess.SetActive(false);
                StateResourcesList[1].BottomPlayMenu.SetActive(false);

                StateResourcesList[2].FailureTop.SetActive(false);
                StateResourcesList[2].GameImg.SetActive(false);
                StateResourcesList[2].GameSuccess.SetActive(false);
                StateResourcesList[2].BottomPlayMenu.SetActive(false);

                StateResourcesList[3].FailureTop.SetActive(false);
                StateResourcesList[3].GameImg.SetActive(false);
                StateResourcesList[3].GameSuccess.SetActive(false);
                StateResourcesList[3].BottomPlayMenu.SetActive(false);
                Menu.sprite = MenuSprites[2];
                PlayNewVid(StateResourcesList[2].StateVidTop, StateResourcesList[2].StateVidBottom);
                current_state = State.G2Vid;
                break;
            case State.G2Game:
                HightlighsOff();
                HideVidShowImg(StateResourcesList[2].GameImg, 2);
                current_state = State.G2Game;
                break;
            case State.G2Success:
                HightlighsOff();
                ShowSuccess(StateResourcesList[2].GameImg, StateResourcesList[2].GameSuccess);
                current_state = State.G2Success;
                break;
            case State.G2PreGame:
                HightlighsOff();
                ShowPlayButtons(StateResourcesList[2].PreGameTop, StateResourcesList[2].BottomPlayMenu, StateResourcesList[2].GameImg);
                PreparePostGameVid(2);
                current_state = State.G2PreGame;
                break;
            case State.G2Failure:
                HightlighsOff();
                ShowPlayButtons(StateResourcesList[2].FailureTop, StateResourcesList[2].BottomPlayMenu, StateResourcesList[2].GameImg);
                current_state = State.G2Failure;
                break;
            case State.G2PostGameVid:
                HightlighsOff();
                PlayNewVid(StateResourcesList[2].PostGameVidTop, StateResourcesList[2].PostGameBottom);
                current_state = State.G2PostGameVid;
                break;
            case State.G3Vid:
                HightlighsOff();
                StateResourcesList[1].FailureTop.SetActive(false);
                StateResourcesList[1].GameImg.SetActive(false);
                StateResourcesList[1].GameSuccess.SetActive(false);
                StateResourcesList[1].BottomPlayMenu.SetActive(false);

                StateResourcesList[2].FailureTop.SetActive(false);
                StateResourcesList[2].GameImg.SetActive(false);
                StateResourcesList[2].GameSuccess.SetActive(false);
                StateResourcesList[2].BottomPlayMenu.SetActive(false);

                StateResourcesList[3].FailureTop.SetActive(false);
                StateResourcesList[3].GameImg.SetActive(false);
                StateResourcesList[3].GameSuccess.SetActive(false);
                StateResourcesList[3].BottomPlayMenu.SetActive(false);
                Menu.sprite = MenuSprites[3];
                PlayNewVid(StateResourcesList[3].StateVidTop, StateResourcesList[3].StateVidBottom);
                current_state = State.G3Vid;
                break;
            case State.G3Game:
                HightlighsOff();
                HideVidShowImg(StateResourcesList[3].GameImg, 3);
                current_state = State.G3Game;
                break;
            case State.G3Success:
                HightlighsOff();
                ShowSuccess(StateResourcesList[3].GameImg, StateResourcesList[3].GameSuccess);
                current_state = State.G3Success;
                break;
            case State.G3PreGame:
                HightlighsOff();
                ShowPlayButtons(StateResourcesList[3].PreGameTop, StateResourcesList[3].BottomPlayMenu, StateResourcesList[3].GameImg);
                PreparePostGameVid(3);
                current_state = State.G3PreGame;
                break;
            case State.G3Failure:
                HightlighsOff();
                ShowPlayButtons(StateResourcesList[3].FailureTop, StateResourcesList[3].BottomPlayMenu, StateResourcesList[3].GameImg);
                current_state = State.G3Failure;
                break;
            case State.G3PostGameVid:
                HightlighsOff();
                PlayNewVid(StateResourcesList[3].PostGameVidTop, StateResourcesList[3].PostGameBottom);
                current_state = State.G3PostGameVid;
                break;
            case State.Ending:
                HightlighsOff();
                Menu.sprite = MenuSprites[0];
                PlayNewVid(StateResourcesList[4].StateVidTop, StateResourcesList[4].StateVidBottom);
                current_state = State.Ending;
                break;
            default:
                break;
        }
    }

    private void PreparePostGameVid(int v)
    {
        TopVideoPlayer.Stop();
        BottomVideoPlayer.Stop();
        TopVideoPlayer.clip = StateResourcesList[v].PostGameVidTop;
        BottomVideoPlayer.clip = StateResourcesList[v].PostGameBottom;
        TopVideoPlayer.Play();
        BottomVideoPlayer.Play();
        Invoke("ResetVideo", 0.5f);
    }

    private void ResetVideo()
    {
        TopVideoPlayer.Play();
        BottomVideoPlayer.Play();
        TopVideoPlayer.Stop();
        BottomVideoPlayer.Stop();
        TopVideoPlayer.time = 0;
        BottomVideoPlayer.time = 0;
    }
}

