
using Photon.Pun;

//using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody2D), typeof(PhotonView))]
public class LootSpawner : MonoBehaviourPunCallbacks
{
    [SerializeField] Loot[] loots;
    int new_loot_id;
    SpriteRenderer my_spriteRenderer;
    [SerializeField] float spawn_cooldown;
    [SerializeField] KeyCode interact_button;
    float timer = 0;
    bool check_timer = false;
    bool can_pick_up = false;
    IRobber thief;


    // Start is called before the first frame update
    void Start()
    {
        my_spriteRenderer = GetComponent<SpriteRenderer>();

        if (PhotonNetwork.LocalPlayer == PhotonNetwork.MasterClient)
        {
            new_loot_id = RollForNewLoot();
            photonView.RPC("SpawnNewLoot", RpcTarget.All, new_loot_id);
        }
        // SpawnNewLoot();

        //FindObjectOfType<StartManager>().ConfirmarValidacion("Loot");
    }

    private void Update()
    {
        if (check_timer && PhotonNetwork.LocalPlayer == PhotonNetwork.MasterClient)
        {
            timer += Time.deltaTime;
            if (timer > spawn_cooldown)
            {
                timer = 0;
                new_loot_id = RollForNewLoot();
                photonView.RPC("SpawnNewLoot", RpcTarget.All, new_loot_id);
                // SpawnNewLoot();
                check_timer = false;
            }
        }
        if (can_pick_up)
        {
            if (Input.GetKeyDown(interact_button))
            {
                thief.GetLoot(loots[new_loot_id].value);

                photonView.RPC("StealLoot", RpcTarget.All);
            }
        }
    }

    int RollForNewLoot()
    {
        int total_pool = 0;
        foreach (var loot in loots) { total_pool += loot.spawn_weight; }
        int roll = Random.Range(0, total_pool);
        int pool = 0;
        int idx = 0;
        foreach(var loot in loots) {  
            pool += loot.spawn_weight;
            if (pool >= roll) return idx;
            idx++;
        }

        return -1;
    }

    [PunRPC]
    void SpawnNewLoot(int loot_id)
    {
        my_spriteRenderer.sprite = loots[loot_id].my_sprite;
        my_spriteRenderer.enabled = true;
    }

    [PunRPC]
    void StealLoot()
    {
        my_spriteRenderer.enabled = false;
        check_timer = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IRobber new_thief = collision.gameObject.GetComponent<IRobber>();

        if (new_thief == null) { return; }
        if (collision.gameObject.GetComponent<PhotonView>().IsMine)
        {
            thief = new_thief;
            can_pick_up = true;
        }
        //Show button to interact
  
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        IRobber new_thief = collision.gameObject.GetComponent<IRobber>();

        if (new_thief == null) { return; }
        if (collision.gameObject.GetComponent<PhotonView>().IsMine)
        {
            thief = null;
            can_pick_up = false;
        }
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