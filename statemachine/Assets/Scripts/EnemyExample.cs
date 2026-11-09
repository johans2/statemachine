using UnityEngine;

public class EnemyExample : MonoBehaviour
{
    private class IdleState : State
    {
        private const float Duration = 2f;
        private readonly Renderer body;
        private float timer;

        public IdleState(Renderer body)
        {
            this.body = body;
        }

        public bool IsDone => timer >= Duration;

        public override void Enter() => body.material.color = Color.gray;

        public override void Tick(float deltaTime) => timer += deltaTime;
    }

    private class PatrolState : State
    {
        private readonly Transform self;
        private readonly Renderer body;
        private readonly Vector3[] waypoints;
        private readonly float speed;
        private int waypointIndex;

        public PatrolState(Transform self, Renderer body, Vector3[] waypoints, float speed)
        {
            this.self = self;
            this.body = body;
            this.waypoints = waypoints;
            this.speed = speed;
        }

        public bool ReachedWaypoint { get; private set; }

        public override void Enter()
        {
            body.material.color = Color.cyan;
            ReachedWaypoint = false;
        }

        public override void Tick(float deltaTime)
        {
            Vector3 target = waypoints[waypointIndex];
            self.position = Vector3.MoveTowards(self.position, target, speed * deltaTime);

            if (Vector3.Distance(self.position, target) > 0.1f)
            {
                return;
            }

            waypointIndex = (waypointIndex + 1) % waypoints.Length;
            ReachedWaypoint = true;
        }
    }

    private class ChaseState : State
    {
        private readonly Transform self;
        private readonly Renderer body;
        private readonly Transform target;
        private readonly float speed;

        public ChaseState(Transform self, Renderer body, Transform target, float speed)
        {
            this.self = self;
            this.body = body;
            this.target = target;
            this.speed = speed;
        }

        public override void Enter() => body.material.color = Color.red;

        public override void Tick(float deltaTime)
        {
            self.position = Vector3.MoveTowards(self.position, target.position, speed * deltaTime);
        }
    }

    private class DeadState : State
    {
        private readonly Transform self;
        private readonly Renderer body;

        public DeadState(Transform self, Renderer body)
        {
            this.self = self;
            this.body = body;
        }

        public override void Enter()
        {
            body.material.color = Color.black;
            self.localScale = new Vector3(1f, 0.2f, 1f);
        }
    }

    private const float PlayerSweepWidth = 14f;
    private const float PlayerSweepSpeed = 0.4f;
    private const float PlayerDistance = 6f;
    private const float PatrolRadius = 3f;

    [SerializeField] private float sightRange = 8f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private int health = 3;

    private readonly StateMachine machine = new();
    private Vector3 origin;
    private Renderer body;
    private Transform player;

    private bool IsDead => health <= 0;
    private bool CanSeePlayer => Vector3.Distance(transform.position, player.position) <= sightRange;

    private void Awake()
    {
        origin = transform.position;
        body = CreatePrimitive(PrimitiveType.Cube, "Body", transform).GetComponent<Renderer>();
        player = CreatePrimitive(PrimitiveType.Sphere, "Player", null).transform;

        Vector3[] waypoints =
        {
            origin + new Vector3(-PatrolRadius, 0f, -PatrolRadius),
            origin + new Vector3(PatrolRadius, 0f, -PatrolRadius),
            origin + new Vector3(PatrolRadius, 0f, PatrolRadius),
            origin + new Vector3(-PatrolRadius, 0f, PatrolRadius),
        };
        PatrolState patrol = new(transform, body, waypoints, moveSpeed);

        machine.AddAnyTransition<DeadState>(() => IsDead, () => new DeadState(transform, body));

        machine.AddTransition<IdleState, ChaseState>(_ => CanSeePlayer, CreateChase);
        machine.AddTransition<IdleState, PatrolState>(idle => idle.IsDone, () => patrol);
        machine.AddTransition<PatrolState, ChaseState>(_ => CanSeePlayer, CreateChase);
        machine.AddTransition<PatrolState, IdleState>(p => p.ReachedWaypoint, CreateIdle);
        machine.AddTransition<ChaseState, PatrolState>(_ => !CanSeePlayer, () => patrol);
    }

    private void Start() => machine.GoToState(CreateIdle());

    private void Update()
    {
        MovePlayer();
        machine.Tick(Time.deltaTime);
    }

    private void OnDestroy() => machine.TearDown();

    private IdleState CreateIdle() => new IdleState(body);

    private ChaseState CreateChase() => new ChaseState(transform, body, player, moveSpeed * 1.5f);

    private void MovePlayer()
    {
        float x = Mathf.Cos(Time.time * PlayerSweepSpeed) * PlayerSweepWidth;
        player.position = origin + new Vector3(x, 0f, PlayerDistance);
    }

    private static GameObject CreatePrimitive(PrimitiveType type, string name, Transform parent)
    {
        GameObject primitive = GameObject.CreatePrimitive(type);
        primitive.name = name;
        primitive.transform.SetParent(parent, false);
        return primitive;
    }

    [ContextMenu("Take Damage")]
    private void TakeDamage() => health--;
}
