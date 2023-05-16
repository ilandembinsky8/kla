using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SwapManager : MonoBehaviour
{
    public GameManager gameManager;
    public List<Sprite> sprites_to_random;
    List<int> indices = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11};
    List<int> random_indices;
    public bool first_chip_shown;
    public bool second_chip_shown;
    public int first_chip_index;
    public int second_chip_index;
    bool first_frame_of_clicking = true;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("mouse:" + Input.mousePosition);
        if (Input.mousePosition.x > 363)
        {
            if (Input.GetMouseButton(0))
            {
                //if (gameManager.attempts_done > 4)
                //    return;
                ChipPos curr_chip = gameManager.G3ChipByPos(Input.mousePosition);
                //Debug.Log(curr_chip.x + "," + curr_chip.y);
                if (curr_chip.x > -1 && curr_chip.y > -1)
                {
                    if (first_frame_of_clicking)
                    {
                        first_frame_of_clicking = false;
                        if (!first_chip_shown)
                        {
                            first_chip_shown = true;
                            first_chip_index = (4 * (2 - curr_chip.y)) + curr_chip.x;
                            gameManager.g3_chip_cover_objects[first_chip_index].color = new Color(1, 1, 1, 0);
                        }
                        else if (!second_chip_shown)
                        {
                            int second_chip_index_calc = (4 * (2 - curr_chip.y)) + curr_chip.x;
                            if (second_chip_index_calc != first_chip_index)
                            {
                                second_chip_shown = true;
                                second_chip_index = (4 * (2 - curr_chip.y)) + curr_chip.x;
                                gameManager.g3_chip_cover_objects[second_chip_index].color = new Color(1, 1, 1, 0);

                                Invoke("SwapBackIfNeeded", 2f);
                                if (gameManager.g3_chip_objects[first_chip_index].sprite == gameManager.g3_chip_objects[second_chip_index].sprite)
                                {
                                    //gameManager.AttemptDone(true);
                                    gameManager.g3_big_vees[first_chip_index].color = Color.white;
                                    gameManager.g3_big_vees[second_chip_index].color = Color.white;
                                    gameManager.SuccessesDone();
                                }
                                //else
                                //{ gameManager.AttemptDone(false); }
                            }
                        }

                    }


                }
            }
            else
            {
                first_frame_of_clicking = true;
            }
        }
        
    }

    void SwapBackIfNeeded()
    {
        if (gameManager.g3_chip_objects[first_chip_index].sprite != gameManager.g3_chip_objects[second_chip_index].sprite)
        {
            gameManager.g3_chip_cover_objects[first_chip_index].color = Color.white;
            gameManager.g3_chip_cover_objects[second_chip_index].color = Color.white;
        }
        first_chip_shown = false;
        second_chip_shown = false;
    }

    public void InitSwap()
    {
        first_chip_shown = false;
        second_chip_shown = false;
        first_frame_of_clicking = true;

        sprites_to_random = new List<Sprite>();
        foreach (Sprite s in gameManager.g3_chip_sprites)
        {
            sprites_to_random.Add(s);
        }
        //random the indixes
        random_indices = new List<int>();
        List<int> indices_copy = new List<int>();
        foreach (int i in indices) { indices_copy.Add(i); }
        while (indices_copy.Count > 0)
        {
            int random_index = Random.Range(0, indices_copy.Count);
            random_indices.Add(indices_copy[random_index]);
            indices_copy.RemoveAt(random_index);
        }
        
        int counter = 0;
        //while (sprites_to_random.Count > 0)
        for (int sprite_counter = 0; sprite_counter < gameManager.g3_chip_sprites.Count; sprite_counter++)
        {
            //int index = Random.Range(0, sprites_to_random.Count);
            //{
                gameManager.g3_chip_objects[random_indices[counter]].sprite = gameManager.g3_chip_sprites[sprite_counter];
                counter++;
                gameManager.g3_chip_objects[random_indices[counter]].sprite = gameManager.g3_chip_sprites[sprite_counter];
                counter++;
            //}
        }
    }
}
