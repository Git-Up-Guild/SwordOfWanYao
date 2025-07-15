using System.Collections;
using UnityEngine;

public class AOEEffect : MonoBehaviour
{
    // 基础属性
    private SoldierModel m_attacker;
    private int m_damage;
    private float m_tickInterval;
    private float m_duration;
    private AOEShape m_shape;
    private Collider2D m_collider;

    // 风眼拉扯相关
    private bool m_canPull;
    private float m_pullStrength;
    private float m_moveSpeed;

    // 往返旋转相关
    private bool m_canRotate;
    private float m_rotateAngle;
    private float m_rotatingSpeed;
    private float m_rotatingInterval;
    private float m_baseRotation;
    private bool m_isAttached;

    public void Init(
        SoldierModel attacker,
        int damage,
        float tickInterval,
        float duration,
        AOEShape shape,
        bool canPull,
        float pullStrength,
        float moveSpeed,
        bool canRotate,
        float rotateAngle,
        float rotatingSpeed,
        float rotatingInterval,
        bool isAttached)
    {
        m_attacker = attacker;
        m_damage = damage;
        m_tickInterval = tickInterval;
        m_duration = duration;
        m_shape = shape;
        m_canPull = canPull;
        m_pullStrength = pullStrength;
        m_moveSpeed = moveSpeed;
        m_canRotate = canRotate;
        m_rotateAngle = rotateAngle;
        m_rotatingSpeed = rotatingSpeed;
        m_rotatingInterval = rotatingInterval;
        m_isAttached = isAttached;

        m_collider = GetComponent<Collider2D>();
        m_baseRotation = transform.eulerAngles.z;

        StartCoroutine(DamageLoop());

        if (m_canRotate && m_isAttached)
            StartCoroutine(RotateLoop());

        if (m_attacker.Type == SoldierType.LightMonk)
            AudioManager.Instance.PlayLightMonkSoundEffect();

        Destroy(gameObject, m_duration);
    }

    private IEnumerator DamageLoop()
    {
        float elapsed = 0;
        while (elapsed < m_duration)
        {
            DealDamage();

            yield return new WaitForSeconds(m_tickInterval);
            elapsed += m_tickInterval;
        }
    }

    private void Update()
    {
        if (m_canPull)
        {
            ApplyPullAndMove();

        }
        if (ShouldDestroy())
        {
            Destroy(gameObject);
        }
    }

    private void DealDamage()
    {
        Collider2D[] hits = null;

        switch (m_shape)
        {
            case AOEShape.Circle:
                var circle = m_collider as CircleCollider2D;
                if (circle != null)
                {
                    Vector2 checkPos = (Vector2)transform.position + circle.offset;
                    hits = Physics2D.OverlapCircleAll(checkPos, circle.radius);
                    DebugDrawCircle(checkPos, circle.radius, Color.yellow, 1f);
                    // Vector2 center = (Vector2)transform.position + circle.offset;
                    // hits = Physics2D.OverlapCircleAll(center, circle.radius);
                }
                break;

            case AOEShape.Rectangle:
                var box = m_collider as BoxCollider2D;
                if (box != null)
                {
                    // 计算矩形的世界尺寸
                    Vector2 size = Vector2.Scale(box.size, transform.lossyScale);

                    // 计算局部偏移：在原有碰撞体偏移的基础上，再向物体前方（X正方向）偏移半个矩形宽度（注意：这里用未缩放的box.size计算，但偏移量会在TransformPoint中自动缩放）
                    Vector2 localCenterOffset = box.offset;
                    Vector2 center = transform.TransformPoint(localCenterOffset);

                    // 旋转角度
                    float rotationAngle = transform.rotation.eulerAngles.z;

                    hits = Physics2D.OverlapBoxAll(center, size, rotationAngle);
                    DebugDrawRectangle(center, size, rotationAngle, Color.red, 10f);
                }
                break;
        }

        if (hits == null) return;

        foreach (var hit in hits)
        {
            if (hit.isTrigger) continue;
            SoldierModel model = hit.GetComponentInParent<SoldierModel>();
            if (model != null && model.Camp != m_attacker.Camp)
            {
                DamageApplyer.ApplyDamage(m_attacker, model, m_damage, model.transform.position);
            }
            else
            {
                var destructible = hit.GetComponent<IDestructible>();
                if (destructible != null)
                {
                    destructible.TakeDamage(m_damage, m_attacker);
                }
            }
        }
    }

    private Vector2 _currentMoveDir;
    private float _dirChangeTimer;
    public float _dirChangeInterval = 0f; // 每2秒换方向

    private void ApplyPullAndMove()
    {
        //拉扯范围内敌人向中心
        Collider2D[] hits = null;
        switch (m_shape)
        {
            case AOEShape.Circle:
                var circle = m_collider as CircleCollider2D;
                if (circle != null)
                {
                    Vector2 checkPos = (Vector2)transform.position + circle.offset;
                    hits = Physics2D.OverlapCircleAll(checkPos, circle.radius);
                    DebugDrawCircle(checkPos, circle.radius, Color.yellow, 1f);
                    // hits = Physics2D.OverlapCircleAll(transform.position, circle.radius);
                    // DebugDrawCircle(transform.position, circle.radius, Color.yellow, 1f);
                }
                break;
            case AOEShape.Rectangle:
                var box = m_collider as BoxCollider2D;
                if (box != null)
                {
                    Vector2 center = (Vector2)transform.position + box.offset;
                    Vector2 size = Vector2.Scale(box.size, transform.lossyScale);
                    hits = Physics2D.OverlapBoxAll(center, size, transform.eulerAngles.z);
                }
                break;
        }
        if (hits != null)
        {
            foreach (var hit in hits)
            {
                if (hit.isTrigger) continue;
                var model = hit.GetComponentInParent<SoldierModel>();
                if (model != null && model.Camp != m_attacker.Camp)
                {
                    Vector2 dir = ((Vector2)transform.position - (Vector2)hit.transform.position).normalized;
                    // 按 pullStrength 拉扯
                    model.transform.position += (Vector3)(dir * m_pullStrength * Time.deltaTime);
                }
            }
        }

        // 风眼自身随机方向移动
        _dirChangeTimer -= Time.deltaTime;
        if (_dirChangeTimer <= 0f)
        {
            _currentMoveDir = Random.insideUnitCircle.normalized;
            _dirChangeTimer = _dirChangeInterval;
        }
        transform.position += (Vector3)(_currentMoveDir * m_moveSpeed * Time.deltaTime);
    }

    private IEnumerator RotateLoop()
    {
        while (true)
        {
            // 发射者死亡则提前销毁
            if (ShouldDestroy())
            {
                Destroy(gameObject);
                yield break;
            }

            // —— 向一个方向旋转到 m_rotateAngle —— 
            float rotated = 0f;
            while (rotated < m_rotateAngle)
            {
                if (ShouldDestroy())
                {
                    Destroy(gameObject);
                    yield break;
                }

                float step = m_rotatingSpeed * Time.deltaTime;
                transform.RotateAround(m_attacker.transform.position, Vector3.forward, step);
                rotated += step;
                yield return null;
            }

            // 等待间隔
            yield return WaitWhileValid(m_rotatingInterval);

            // —— 再反向旋转回去 —— 
            float back = 0f;
            while (back < m_rotateAngle)
            {
                if (ShouldDestroy())
                {
                    Destroy(gameObject);
                    yield break;
                }

                float step = m_rotatingSpeed * Time.deltaTime;
                transform.RotateAround(m_attacker.transform.position, Vector3.forward, -step);
                back += step;
                yield return null;
            }

            // 等待间隔，准备下一轮
            yield return WaitWhileValid(m_rotatingInterval);
        }
    }

    private bool ShouldDestroy()
    {
        return m_attacker == null || m_attacker.IsDead || !m_attacker.IsAttacking;
    }

    private IEnumerator WaitWhileValid(float duration)
    {
        float timer = 0;
        while (timer < duration)
        {
            if (ShouldDestroy())
            {
                Destroy(gameObject);
                yield break;
            }
            timer += Time.deltaTime;
            yield return null;
        }
    }

    private void DebugDrawCircle(Vector2 center, float radius, Color color, float duration)
    {
        float segments = 36;
        float anglePerSeg = 360f / segments;

        for (int i = 0; i < segments; i++)
        {
            Vector2 start = (Vector3)center + Quaternion.Euler(0, 0, anglePerSeg * i) * Vector2.up * radius;
            Vector2 end = (Vector3)center + Quaternion.Euler(0, 0, anglePerSeg * (i + 1)) * Vector2.up * radius;
            Debug.DrawLine(start, end, color, duration);
        }
    }
    
    // 绘制矩形的调试方法
    private void DebugDrawRectangle(Vector2 center, Vector2 size, float angle, Color color, float duration)
    {
        // 计算半尺寸
        Vector2 halfSize = size * 0.5f;
        // 计算矩形的四个角点（未旋转前）
        Vector2[] corners = new Vector2[]
        {
            new Vector2(-halfSize.x, -halfSize.y),
            new Vector2(-halfSize.x, halfSize.y),
            new Vector2(halfSize.x, halfSize.y),
            new Vector2(halfSize.x, -halfSize.y)
        };

        // 创建旋转矩阵（绕Z轴旋转角度angle）
        float rad = angle * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        for (int i = 0; i < 4; i++)
        {
            // 旋转每个点
            Vector2 rotatedCorner = new Vector2(
                corners[i].x * cos - corners[i].y * sin,
                corners[i].x * sin + corners[i].y * cos
            );
            corners[i] = rotatedCorner + center;
        }

        // 绘制四条边
        Debug.DrawLine(corners[0], corners[1], color, duration);
        Debug.DrawLine(corners[1], corners[2], color, duration);
        Debug.DrawLine(corners[2], corners[3], color, duration);
        Debug.DrawLine(corners[3], corners[0], color, duration);
    }

}
