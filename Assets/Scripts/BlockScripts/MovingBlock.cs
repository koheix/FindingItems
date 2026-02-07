using UnityEngine;

public class MovingBlock : MonoBehaviour
{
    [Header("ブロックの移動設定")]
    public float speed = 2.0f; // 移動速度
    public float amplitude = 0.5f; // 移動の振幅
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        // ブロックを上下に移動させる
        Vector3 position = transform.position;
        position.y += Mathf.Sin(Time.time * speed) * amplitude * Time.fixedDeltaTime;
        transform.position = position;

    }
}
