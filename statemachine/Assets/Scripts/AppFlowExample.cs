using UnityEngine;

public class AppFlowExample : MonoBehaviour
{
    private class AppRequests
    {
        public bool Play;
        public bool Quit;

        public void Clear()
        {
            Play = false;
            Quit = false;
        }
    }

    private class LoadingState : State
    {
        private const float Duration = 2f;
        private GameObject screen;
        private float timer;

        public bool IsDone => timer >= Duration;

        public override void Enter() => screen = new GameObject("Loading Screen");

        public override void Tick(float deltaTime) => timer += deltaTime;

        public override void Exit() => Object.Destroy(screen);
    }

    private class MainMenuState : State
    {
        private readonly AppRequests requests;
        private GameObject screen;

        public MainMenuState(AppRequests requests)
        {
            this.requests = requests;
        }

        public override void Enter()
        {
            screen = new GameObject("Main Menu");
            requests.Clear();
        }

        public override void Exit() => Object.Destroy(screen);
    }

    private class PlayingState : State
    {
        private const float RoundDuration = 10f;
        private GameObject level;
        private float timer;

        public bool IsOver => timer >= RoundDuration;

        public override void Enter() => level = new GameObject("Level");

        public override void Tick(float deltaTime) => timer += deltaTime;

        public override void Exit() => Object.Destroy(level);
    }

    private class PostGameState : State
    {
        private const float Duration = 3f;
        private GameObject screen;
        private float timer;

        public bool IsDone => timer >= Duration;

        public override void Enter() => screen = new GameObject("Results Screen");

        public override void Tick(float deltaTime) => timer += deltaTime;

        public override void Exit() => Object.Destroy(screen);
    }

    private readonly AppRequests requests = new();
    private readonly StateMachine machine = new();

    private void Awake()
    {
        machine.AddAnyTransition<MainMenuState>(() => requests.Quit, CreateMainMenu);

        machine.AddTransition<LoadingState, MainMenuState>(loading => loading.IsDone, CreateMainMenu);
        machine.AddTransition<MainMenuState, PlayingState>(_ => requests.Play, () => new PlayingState());
        machine.AddTransition<PlayingState, PostGameState>(playing => playing.IsOver, () => new PostGameState());
        machine.AddTransition<PostGameState, MainMenuState>(postGame => postGame.IsDone, CreateMainMenu);
    }

    private void Start() => machine.GoToState(new LoadingState());

    private void Update() => machine.Tick(Time.deltaTime);

    private void OnDestroy() => machine.TearDown();

    private MainMenuState CreateMainMenu() => new MainMenuState(requests);

    [ContextMenu("Play")]
    private void Play() => requests.Play = true;

    [ContextMenu("Quit To Menu")]
    private void QuitToMenu() => requests.Quit = true;
}
