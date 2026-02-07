using UnityEngine;
using UnityEngine.AI; // NavMeshを使うために必要

public class EnemyAI : MonoBehaviour
{
    public Transform player;      // 追いかける対象（プレイヤー）
    public float detectRange = 10f; // プレイヤーを検知する距離
    
    private NavMeshAgent agent;

    void Start()
    {
        // 自分のNavMeshAgentを取得
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        // プレイヤーとの距離を計算
        float distance = Vector3.Distance(transform.position, player.position);

        // もし距離が設定値より近ければ
        if (distance <= detectRange)
        {
            // プレイヤーの位置を目的地に設定して追いかける
            agent.SetDestination(player.position);
        }
        else
        {
            // 範囲外なら立ち止まる（または元の場所に戻るなど）
            agent.ResetPath();
        }
    }
}