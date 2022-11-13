using System.Collections.Generic;
using System.Threading.Tasks;
using Additions;
using BetterInspector;
using Godot;

public partial class SceneManager : CanvasLayer
{
    public static SceneManager Instance { get; private set; }

    [NodeRef] private AnimationPlayer anim;

    [Export] private PackedScene menuScene;

    public override void _EnterTree() => Instance = this;
    public override void _ExitTree() => Instance = null;

    public static async Task StartTransition()
    {
        Instance.GetTree().Paused = true;
        Instance.anim.Play("Start");
        await Instance.ToSignal(Instance.anim, "animation_finished");
    }

    public static async Task EndTransition()
    {
        Instance.anim.Play("End");
        await Instance.ToSignal(Instance.anim, "animation_finished");
        Instance.GetTree().Paused = false;
    }

    public static async Task ReloadCurrentScene()
    {
        await StartTransition();
        Instance.GetTree().ReloadCurrentScene();
        await EndTransition();
    }

    public static async Task LoadScene(PackedScene scene)
    {
        await StartTransition();
        Instance.GetTree().ChangeSceneTo(scene);
        await EndTransition();
    }

    public static async Task LoadMenu()
    {
        await LoadScene(Instance.menuScene);
    }

    public static async Task ReloadCurrentSceneOrLevel()
    {
        if (Instance.GetTree().CurrentScene is GameManager game)
        {
            game.LoadLevel();
            return;
        }

        await StartTransition();
        Instance.GetTree().ReloadCurrentScene();
        await EndTransition();
    }
}
