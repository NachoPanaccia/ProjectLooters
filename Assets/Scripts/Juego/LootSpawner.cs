using ExitGames.Client.Photon;
using Photon.Pun;

//using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody2D), typeof(PhotonView))]
public class LootSpawner : MonoBehaviourPunCallbacks
{
    [SerializeField] Loot[] loots;
    Loot actual_loot;
    SpriteRenderer my_spriteRenderer;
    [SerializeField] float spawn_cooldown;
    [SerializeField] KeyCode interact_button;
    float timer = 0;
    bool check_timer = false;

    // Start is called before the first frame update
    void Start()
    {
        my_spriteRenderer = GetComponent<SpriteRenderer>();

        SpawnNewLoot();

        //FindObjectOfType<StartManager>().ConfirmarValidacion("Loot");
    }

    private void Update()
    {
        if (check_timer)
        {
            timer += Time.deltaTime;
            if (timer > spawn_cooldown)
            {
                timer = 0;
                SpawnNewLoot();
                check_timer = false;
            }
        }
    }

    Loot RollForNewLoot()
    {
        int total_pool = 0;
        foreach (var loot in loots) { total_pool += loot.spawn_weight; }
        Debug.Log("TOTAL: " + total_pool);
        int roll = Random.Range(0, total_pool);
        Debug.Log("ROLL: " + roll);
        int pool = 0;
        foreach(var loot in loots) {  
            pool += loot.spawn_weight;
            Debug.Log("PULL: " + pool);
            if (pool >= roll) return loot;
        }

        return new Loot();
    }
    void SpawnNewLoot()
    {
        actual_loot = RollForNewLoot();
        my_spriteRenderer.sprite = actual_loot.my_sprite;
        my_spriteRenderer.enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IRobber thief = collision.gameObject.GetComponent<IRobber>();

        if (thief == null) { return; }

        //Show button to interact
  
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        IRobber thief = collision.gameObject.GetComponent<IRobber>();

        if (thief == null) { return; }
        if (Input.GetKeyDown(interact_button))
        {
            thief.GetLoot(actual_loot.value);
            my_spriteRenderer.enabled = false;
            check_timer = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        IRobber thief = collision.gameObject.GetComponent<IRobber>();

        if (thief == null) { return; }

        //Hide button to interact

    }
}

[System.Serializable]
public struct Loot
{
    public Sprite my_sprite;
    public int value;
    public int spawn_weight;
}