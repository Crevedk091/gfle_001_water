using UnityEngine;

public class DynamicWater2Df : MonoBehaviour
{
    [SerializeField]
    Vector2 bound = Vector2.one;
    [SerializeField, Min(10)]
    int resolution = 50;
    [SerializeField]
    Material waterMaterial;
    [SerializeField]
    float
        springConstant = .02f,
        damping = .1f,
        spread = .1f,
        collisionVelocityFactor = .04f;

    Vector3[] vertices;
    Mesh mesh;
    float[] velocities, accelerations;
    float timer;

    private void Start()
    {
        InitializePhysics();
        GenerateMesh();
        SetBosCollider2D();
    }

    void GenerateMesh()
    {
        float range = bound.x / (resolution - 1);
        vertices = new Vector3[resolution * 2];

        for(int i = 0; i < resolution; i++)
        {
            vertices[i] = new Vector3(bound.x + (i * range), bound.y, 0f);
        }

        for(int i = 0; i < resolution; i++)
        {
            vertices[i + resolution] = new Vector2(bound.x + (i * range), 0f);
        }

        int[] template = new int[6];
        template[0] = resolution;
        template[1] = 0;
        template[2] = resolution + 1;
        template[3] = 0;
        template[4] = 1;
        template[5] = resolution + 1;

        int marker = 0;
        int[] tris = new int[((resolution - 1) * 2) * 3];
        for (int i = 0; i < tris.Length; i++)
        {
            tris[i] = template[marker++]++;
            if (marker >= 6) marker = 0;
        }

        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        if (waterMaterial) meshRenderer.sharedMaterial = waterMaterial;

        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();

        mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.mesh = mesh;
    }

    void InitializePhysics()
    {
        velocities = new float[resolution];
        accelerations = new float[resolution];
    }

    private void SetBosCollider2D()
    {
        BoxCollider2D collision = gameObject.AddComponent<BoxCollider2D>();
        collision.isTrigger = true;
    }

    private void Update()
    {
        if (timer <= 0) return;
        timer -= Time.unscaledDeltaTime;

        for (int i = 0; i < resolution; i++)
        {
            float force = springConstant * (vertices[i].y - bound.y) + velocities[i] * damping;
            accelerations[i] = -force;
            vertices[i].y += velocities[i];
            velocities[i] += accelerations[i];
        }

        for (int i = 0; i < resolution; i++)
        {
            if (i > 0)
            {
                float l = spread * (vertices[i].y - vertices[i - 1].y);
                velocities[i - 1] += l;
            }

            if (i < resolution - 1)
            {
                float r = spread * (vertices[i].y - vertices[i + 1].y);
                velocities[i + 1] += r;
            }
        }

        mesh.vertices = vertices;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
        Splash(collision, rb.velocity.y * collisionVelocityFactor);
    }

    private void Splash(Collider2D collision, float force)
    {
        timer = 3f;
        float radius = collision.bounds.max.x - collision.bounds.min.x;
        Vector2 center = new Vector2(collision.bounds.center.x, transform.TransformPoint(bound).y);

        //GameObject splashGO = Instantiate(splash, new Vector3(center.x, center.y, 0), Quaternion.Euler(0, 0, 60));
        //Destroy(splashGO, 2f);

        for (int i = 0; i < resolution; i++)
        {
            if (PointInsideCircle(transform.TransformPoint(vertices[i]), center, radius))
            {
                velocities[i] = force;
                Debug.Log("boom");
            }
        }
    }

    bool PointInsideCircle(Vector2 point, Vector2 center, float radius)
    {
        return Vector2.Distance(point, center) < radius;
    }
}
