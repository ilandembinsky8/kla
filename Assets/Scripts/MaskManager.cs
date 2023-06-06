using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MaskManager : MonoBehaviour
{
    bool drag_on = false;
    public bool mark_mode_on = true;
    public Image chips;
    public GameManager gameManager;
    Vector3 mouse_delta;
    Vector3 mouse_prev;
    Vector3 mouse_origin;
    Vector3 object_delta;
    bool first_mousedown_frame = true;
    //bool chip_x_shown = false;
    //bool chip_x_to_hide = false;
    public GameObject[] chip_x;
    public Sprite X_sprite;
    public Sprite flash_sprite_with_x;
    public Sprite flash_sprite_without_x;
    const float G2_RADIUS = 50;
    bool keep_mag_on_place = true;
    public Image mag_to_drag;
    public GameObject button_mag_x;

    public Sprite G1_magnify_true1;
    public Sprite G1_magnify_true2;
    public Sprite G1_magnify_false1;
    public Sprite G1_magnify_false2;

    //public int interpolationFramesCountG1=20; // Number of frames to completely interpolate between the 2 positions
    //public int interpolationFramesCountG2=1; // Number of frames to completely interpolate between the 2 positions
    int elapsedFrames = 0;
    bool animatedHome = false;
    Vector3 anim_start;
    Vector3 anim_end;
    Vector3 orig_transform;
    Vector3 chip_orig_transform;
    Sprite G2_chip_sprite;

    bool first_time_second_game = true;

    //public GameObject G1_hand;
    //public GameObject G2_hand;

    public Sprite background;

    // Start is called before the first frame update
    void Start()
    {
        Init();
        orig_transform = transform.position;
        //chip_orig_transform = chips.transform.position;
        G2_chip_sprite = chips.sprite;
    }

    public void Init()
    {
        foreach (Image i in GetComponentsInChildren<Image>()) { i.enabled = false; }
        foreach (GameObject g in chip_x) { g.SetActive(false); }
        mark_mode_on = true;
        keep_mag_on_place = false;
        //G1_hand.SetActive(false);
        //G2_hand.SetActive(false);
        if (gameManager.current_game == 1)
        {
            button_mag_x.SetActive(false);
            chips.transform.position = new Vector3(650, 220, 0);
            //G1_hand.SetActive(true);
            //G2_hand.SetActive(false);
        }
        if (gameManager.current_game == 2)
        {
            mag_to_drag.sprite = flash_sprite_without_x;
            //G1_hand.SetActive(false);
            //G2_hand.SetActive(true);
        }
    }

    public void G1_hide_mag_btn_clicked()
    {
        transform.position = orig_transform;
        chips.transform.position = chip_orig_transform;
        button_mag_x.SetActive(false);
        mark_mode_on = true;
        keep_mag_on_place = false;
        foreach (Image i in GetComponentsInChildren<Image>()) { i.enabled = false; }
        gameManager.DragModeToggleClicked(1);

    }

    public void G2_hide_flash_btn_clicked()
    {
        transform.position = orig_transform;
        chips.sprite = G2_chip_sprite;
        //chips.transform.position = Vector3.zero;//chip_orig_transform;
        //mark_mode_on = true;
        //hide flash
        //Init();
        button_mag_x.SetActive(false);
        mark_mode_on = true;
        keep_mag_on_place = false;
        foreach (Image i in GetComponentsInChildren<Image>()) { i.enabled = false; }
        gameManager.DragModeToggleClicked(2);
    }

    void G1HandleMark()
    {
        ChipPos curr_chip_pos = gameManager.ChipByPos(Input.mousePosition);
        int chip_pos_index = (4 * curr_chip_pos.y) + curr_chip_pos.x;
        
        if (curr_chip_pos.x >= 0 && curr_chip_pos.y >= 0)
        {
            if (System.Array.IndexOf(gameManager.game1_faults, chip_pos_index) > -1)
            {
                chip_x[(4 * curr_chip_pos.y) + curr_chip_pos.x].SetActive(true);
                //chip_x_shown = true;
                //gameManager.AttemptDone(true);
                gameManager.SuccessesDone();
            }
            else
            {
                chip_x[(4 * curr_chip_pos.y) + curr_chip_pos.x].SetActive(true);
                chip_x[(4 * curr_chip_pos.y) + curr_chip_pos.x].GetComponent<Image>().sprite = X_sprite;
                gameManager.AttemptDone(false);
            }
        }
    }

    void G2HandleMark()
    {
        if (gameManager.attempts_done > 2)
            return;
        bool success = false;
        foreach (ChipPos chipPos in gameManager.game2_faults)
        {
            Vector2 curr_fault = new Vector2(chipPos.x, chipPos.y);
            if (Vector2.Distance(curr_fault, Input.mousePosition) < G2_RADIUS)
            {
                success = true;
                //show green indication
                gameManager.G2_showindication(true, chipPos.x, chipPos.y);
                gameManager.SuccessesDone();
            }
        }
        if (!success)
        {
            //show red indication
            gameManager.G2_showindication(false, Input.mousePosition.x, Input.mousePosition.y);;
            gameManager.AttemptDone(false);
        }
        //gameManager.AttemptDone(success);
    }

    // Update is called once per frame
    void Update()
    {
        if (animatedHome)
        {
            int interpolationFramesCount = 20;
            if (gameManager.current_game == 2)
            {
                interpolationFramesCount = 1;
            }
            float interpolationRatio = (float)elapsedFrames / interpolationFramesCount;
            Vector3 interpolatedPosition = Vector3.Lerp(anim_start, anim_end, interpolationRatio);
            transform.position = interpolatedPosition;
            elapsedFrames = (elapsedFrames + 1) % (interpolationFramesCount + 1);  // reset elapsedFrames to zero after it reached (interpolationFramesCount + 1)
            if (elapsedFrames == 0)
            {
                animatedHome = false;
            }
        }
        if (Input.GetMouseButton(0)) //before first drag, and after releasing the drag.
        {
            if (mark_mode_on && Input.mousePosition.x > 1625 && Input.mousePosition.y < 411)
            {
                Debug.Log("clicked on magnifier!");
            
                if (gameManager.current_game == 1)
                {
                    gameManager.DragModeToggleClicked(1);
                    gameManager.G1_drag_on = true;
                    
                }
                else if (gameManager.current_game == 2)
                {
                    gameManager.DragModeToggleClicked(2);
                    gameManager.G2_drag_on = true;
                    
                }
            }
            if (mark_mode_on)
            {
                if (gameManager.current_game == 1)
                {
                    if (first_mousedown_frame)
                    {
                        G1HandleMark();
                        first_mousedown_frame = false;
                    }
                }
                else if (gameManager.current_game == 2)
                {
                    
                    if (first_mousedown_frame)
                    {
                        G2HandleMark();
                        first_mousedown_frame = false;
                    }
                }
            }
        }
        else
        {
            if (mark_mode_on)
            {
                if (gameManager.current_game == 1)
                {
                    first_mousedown_frame = true;
                }
                else if (gameManager.current_game == 2)
                {
                    first_mousedown_frame = true;
                }
            }
        }
        if ((gameManager.current_game == 1 && !gameManager.G1_drag_on)||(gameManager.current_game == 2 && !gameManager.G2_drag_on))
            return;
        //1625,411
        if (/*(!chip_x_shown) && (!chip_x_to_hide) &&*/ (Input.mousePosition.x > 363 ))
        {
            Vector3 gap = Input.mousePosition - transform.position;
            //Debug.Log("gap:" + gap + " ,origin:" + mouse_origin + " , mouse:" + Input.mousePosition + " , pos:" + transform.position);
            //Debug.Log(gameManager.ChipByPos(Input.mousePosition).x + "," + gameManager.ChipByPos(Input.mousePosition).y);
            if (Input.GetMouseButton(0))
            {
                if (!keep_mag_on_place)//### && (Input.mousePosition.x < 1625))// && Input.mousePosition.y > 411))
                { 
                    //foreach (GameObject g in chip_x) { g.SetActive(false); }
                    drag_on = true;
                    if (first_mousedown_frame)
                    {
                        foreach (Image i in GetComponentsInChildren<Image>()) { i.enabled = true; }
                        if (gameManager.current_game == 1)
                        {
                            ChipPos curr_chip_pos = gameManager.ChipByPos(Input.mousePosition);
                            if (curr_chip_pos.x > -1 && curr_chip_pos.y > -1)
                            {
                                int chip_pos_index = (4 * curr_chip_pos.y) + curr_chip_pos.x;
                                if (chip_pos_index == 0)
                                    chips.sprite = G1_magnify_false1;
                                if (chip_pos_index == 1)
                                    chips.sprite = G1_magnify_true1;
                                if (chip_pos_index == 2)
                                    chips.sprite = G1_magnify_true1;
                                if (chip_pos_index == 3)
                                    chips.sprite = G1_magnify_true2;
                                if (chip_pos_index == 4)
                                    chips.sprite = G1_magnify_true2;
                                if (chip_pos_index == 5)
                                    chips.sprite = G1_magnify_true1;
                                if (chip_pos_index == 6)
                                    chips.sprite = G1_magnify_false1;
                                if (chip_pos_index == 7)
                                    chips.sprite = G1_magnify_false2;
                                if (chip_pos_index == 8)
                                    chips.sprite = G1_magnify_true2;
                                if (chip_pos_index == 9)
                                    chips.sprite = G1_magnify_true2;
                                if (chip_pos_index == 10)
                                    chips.sprite = G1_magnify_true1;
                                if (chip_pos_index == 11)
                                    chips.sprite = G1_magnify_true2;
                            }
                            else
                            {
                                chips.sprite = null;//###
                                //Debug.LogError("3333333");
                            }
                            chips.transform.Translate(new Vector3((800 - 510) * curr_chip_pos.x, (817 - 540) * curr_chip_pos.y, 0));
                            
                        }
                        else if (gameManager.current_game == 2)
                        {
                            if (first_time_second_game)
                            {
                                chip_orig_transform = chips.transform.position;
                                first_time_second_game = false;
                            }
                            chips.transform.position = chip_orig_transform;
                            Debug.Log("dddddddddd");
                        }
                        mouse_origin = Input.mousePosition;
                        mouse_prev = Input.mousePosition - mouse_origin;
                        first_mousedown_frame = false;
                        transform.Translate(gap);
                        chips.transform.Translate(-gap);
                        anim_end = transform.position;//###
                        
                    }
                }
            }
            else
            {
                if (drag_on)
                {
                    
                    {
                        Debug.Log("d - animate home");
                        //if (gameManager.current_game == 1)
                        {
                            drag_on = false;
                            keep_mag_on_place = true;
                            first_mousedown_frame = true;

                            anim_start = transform.position;
                            animatedHome = true;
                            //chips.sprite = null;//###
                            chips.sprite = background;//null;
                            chips.SetNativeSize();
                            chips.transform.position = new Vector3(1000, 540, 0);
                            //Debug.LogError("1111111");
                            //mark_mode_on = true;
                            if (gameManager.current_game == 1)
                            {
                                button_mag_x.SetActive(true);
                                //to return x//button_mag_x.GetComponent<Image>().enabled = true;
                                Invoke("G1_hide_mag_btn_clicked", (20 / 60.0f));

                                //foreach (Image i in GetComponentsInChildren<Image>()) { i.enabled = false; }
                                //foreach (GameObject g in chip_x) { g.SetActive(false); }
                                //button_mag_x.SetActive(true);
                            }
                            else if (gameManager.current_game == 2)
                            {
                                //to return x//mag_to_drag.sprite = flash_sprite_with_x;
                                button_mag_x.SetActive(true);
                                //to return x//button_mag_x.GetComponent<Image>().enabled = true;

                                Invoke("G2_hide_flash_btn_clicked", (1 / 60.0f));
                                //button_mag_x.SetActive(true);
                            }
                        }
                        
                    }

                    if (gameManager.current_game == 1 && Input.mousePosition.x > 363)
                    {
                        Debug.Log("c1");
                        //ChipPos curr_chip_pos = gameManager.ChipByPos(Input.mousePosition);
                        //if (curr_chip_pos.x >= 0 && curr_chip_pos.y >= 0)
                        //{
                        //    chip_x[(4 * curr_chip_pos.y) + curr_chip_pos.x].SetActive(true);
                        //    chip_x_shown = true;
                        //}
                    }
                    else if (gameManager.current_game == 2 && Input.mousePosition.x > 363)
                    {
                        Debug.Log("c2");

                    }
                }

            }

            if (drag_on)
            {
                Debug.Log("a");
                if (gameManager.current_game == 1)
                {
                    
                    ChipPos curr_chip_pos = gameManager.ChipByPos(Input.mousePosition);
                    //int chip_pos_index = (4 * curr_chip_pos.y) + curr_chip_pos.x;
                    if (curr_chip_pos.x > -1 && curr_chip_pos.y > -1)
                    {
                        int chip_pos_index = (4 * curr_chip_pos.y) + curr_chip_pos.x;
                        if (chip_pos_index == 0)
                            chips.sprite = G1_magnify_false1;
                        if (chip_pos_index == 1)
                            chips.sprite = G1_magnify_true1;
                        if (chip_pos_index == 2)
                            chips.sprite = G1_magnify_true1;
                        if (chip_pos_index == 3)
                            chips.sprite = G1_magnify_true2;
                        if (chip_pos_index == 4)
                            chips.sprite = G1_magnify_true2;
                        if (chip_pos_index == 5)
                            chips.sprite = G1_magnify_true1;
                        if (chip_pos_index == 6)
                            chips.sprite = G1_magnify_false1;
                        if (chip_pos_index == 7)
                            chips.sprite = G1_magnify_false2;
                        if (chip_pos_index == 8)
                            chips.sprite = G1_magnify_true2;
                        if (chip_pos_index == 9)
                            chips.sprite = G1_magnify_true2;
                        if (chip_pos_index == 10)
                            chips.sprite = G1_magnify_true1;
                        if (chip_pos_index == 11)
                            chips.sprite = G1_magnify_true2;
                    }
                    else
                    {
                        chips.sprite = background;//null;
                        chips.SetNativeSize();
                        chips.transform.position = new Vector3(1000, 540, 0);
                        //Debug.LogError("222222");
                        //first_frame_in_chip = true;
                    }
                    //if (first_frame_in_chip)
                    //{
                    if (chips.sprite != background)
                    {
                        chips.rectTransform.sizeDelta = new Vector2(900, 900);
                        chips.transform.position = new Vector3(650, 220, 0);
                        chips.transform.Translate(new Vector3((800 - 510) * curr_chip_pos.x, (817 - 540) * curr_chip_pos.y, 0));
                    }
                        //first_frame_in_chip = false;
                    //}
                }
                Vector3 mouse_pos = Input.mousePosition - mouse_origin;
                mouse_delta = mouse_pos - mouse_prev;
                transform.Translate(mouse_delta);// +mouse_offset);
                chips.transform.Translate(-mouse_delta);// -mouse_offset);
                mouse_prev = mouse_pos;
            }
        }
        else
        {
            Debug.Log("b");

            //if (chip_x_to_hide && !Input.GetMouseButton(0))
            //{
                //Debug.Log("b1");
                //chip_x_to_hide = false;
                //foreach (GameObject g in chip_x) { g.SetActive(false); }
                //ChipPos curr_chip_pos = gameManager.ChipByPos(Input.mousePosition);
                //if (curr_chip_pos.x >= 0 && curr_chip_pos.y >= 0)
                //{
                //    int chip_pos_index = (4 * curr_chip_pos.y) + curr_chip_pos.x;
                //    if (System.Array.IndexOf(gameManager.game1_faults, chip_pos_index) > -1)
                //    { gameManager.AttemptDone(true); }
                //    else
                //    { gameManager.AttemptDone(false); }
                //}
                //else
                //{
                //    gameManager.AttemptDone(false);
                //}
            //}

            //if (Input.GetMouseButton(0) && chip_x_shown)
            //{
            //    chip_x_shown = false;
            //    chip_x_to_hide = true;
            //    
            //}
        }
    }

    //public void clicked()
    //{
    //    Debug.Log("mouse clicked");
    //    drag_on = true;
    //}

    //private void OnMouseDown()
    //{
    //    Debug.Log("on mouse down");
    //}


    //public void OnPointerDown(PointerEventData eventData)
    //{
    //    Debug.Log("mouse");
    //    drag_on = true;
    //}

    public void PointerDown()
    {
        Debug.Log("mouse");
        drag_on = true;
    }

    //OnPointerUp()
    //{
    //    drag_on = false;
    //}
}
