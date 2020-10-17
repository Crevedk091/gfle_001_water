using System.Net.NetworkInformation;
using System.Security;
using UnityEngine;

public class DynamicWater2D : MonoBehaviour
{
    [System.Serializable]
    public struct Bound
    {
        public float top;
        public float right;
        public float bottom;
        public float left;
    }

    [Header("Water Settings")]
    public Bound bound;
    public int quality;

    public Material waterMaterial;
    public GameObject splash;

    private Vector3[] vertices;
    private Mesh mesh;

    [Header("Phisics Settings")]
    public float sprigConstant = .02f;
    public float damping = .1f;
    public float spread = .1f;
    public float collisionVelocityFactor = .04f;

    float[] velocities;
    float[] accelerations;
    float[] leftDeltas;
    float[] rightDeltas;

    private float timer;

    private void GenerateMesh()
    {
        float range = (bound.right - bound.left) / (quality - 1);
        vertices = new Vector3[quality * 2];

        for (int i = 0; i < quality; i++)
        {
            vertices[i] = new Vector3(bound.left + (i * range), bound.top, 0);
        }

        for (int i = 0; i < quality; i++)
        {
            vertices[i + quality] = new Vector2(bound.left + (i * range), bound.bottom);
        }

        int[] template = new int[6];
        template[0] = quality;
        template[1] = 0;
        template[2] = quality + 1;
        template[3] = 0;
        template[4] = 1;
        template[5] = quality + 1;

        int marker = 0;
        int[] tris = new int[((quality - 1) * 2) * 3];
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

    private void Start()
    {
        InitializePhysics();
        GenerateMesh();
        SetBosCollider2D();
    }
    
    void InitializePhysics()
    {
        velocities = new float[quality];
        accelerations = new float[quality];
        leftDeltas = new float[quality];
        rightDeltas = new float[quality];
    }


    private void SetBosCollider2D()
    {
        BoxCollider2D col = gameObject.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
    }

    private void Update()
    {
        if (timer <= 0) return;
        timer -= Time.deltaTime;

        for (int i = 0; i < quality; i++)
        {
            float force = sprigConstant * (vertices[i].y - bound.top) + velocities[i] * damping;
            accelerations[i] = -force;
            vertices[i].y += velocities[i];
            velocities[i] += accelerations[i];
        }

        for (int i = 0; i < quality; i++)
        {
            if (i > 0)
            {
                leftDeltas[i] = spread * (vertices[i].y - vertices[i - 1].y);
                velocities[i - 1] += leftDeltas[i];
            }
            if (i < quality - 1)
            {
                rightDeltas[i] = spread * (vertices[i].y - vertices[i + 1].y);
                velocities[i + 1] += rightDeltas[i];
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
        Vector2 center = new Vector2(collision.bounds.center.x, bound.top);
        //Debug.Log("old -- center = " + center + " / radius = " + radius);

        //GameObject splashGO = Instantiate(splash, new Vector3(center.x, center.y, 0), Quaternion.Euler(0, 0, 60));
        //Destroy(splashGO, 2f);

        for (int i = 0; i < quality; i++)
        {
            //Debug.Log("old -- vertices " + i + " = " + vertices[i] + " / globalPosition = " + (vertices[i] + transform.position));
            if (PointInsideCircle(vertices[i], center, radius))
            {
                velocities[i] = force;
                Debug.Log("old -- boom");
            }
        }
    }

    bool PointInsideCircle(Vector2 point, Vector2 center, float radius)
    {
        return Vector2.Distance(point, center) < radius;
    }
}

