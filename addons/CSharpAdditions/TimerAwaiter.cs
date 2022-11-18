using System;
using System.Runtime.CompilerServices;
using Godot;

namespace Additions
{
    public class TimeAwaiter : Godot.Object, IAwaiter, INotifyCompletion, IAwaitable
    {
        public Node node;
        public PauseMode pauseMode;

        public Action OnCompleted { get => userAction; set => userAction = value; }
        public bool IsCompleted => completed;
        public bool IsPaused { get; private set; }

        public float TimeLeft
        {
            get => timeLeft;
            set
            {
                timeLeft = value;
                if (value <= 0) Finish();
            }
        }

        private float timeLeft;
        private bool completed;
        private Action action, userAction;

        public TimeAwaiter(Node node, float time, PauseMode pauseMode = PauseMode.SyncWithNode, bool cancelOnNodeTreeExited = false, Action onCompleted = null)
        {
            if (!IsInstanceValid(node))
            {
                Cancel();
                throw new Exception("The instance of the given node is not valid.");
            }
            if (!node.IsInsideTree())
            {
                Cancel();
                throw new Exception("The given node is not inside the scene tree.");
            }

            userAction = onCompleted;
            this.node = node;
            this.TimeLeft = time;
            this.pauseMode = pauseMode;

            if (cancelOnNodeTreeExited) node.Connect("tree_exited", this, nameof(Cancel));
            node.GetTree().Connect("idle_frame", this, nameof(OnTreeIdleFrame));
        }

        public void Cancel()
        {
            CallDeferred("free");
        }

        public void Finish()
        {
            completed = true;

            action?.Invoke();
            userAction?.Invoke();

            CallDeferred("free");
        }

        public void Pause() => IsPaused = true;

        public void Continue() => IsPaused = false;

        private void OnTreeIdleFrame()
        {
            if (!IsInstanceValid(node))
            {
                Cancel();
                return;
            }

            if (!IsProcessing()) return;

            TimeLeft -= node.GetProcessDeltaTime();
        }

        public bool IsProcessing()
        {
            if (IsCompleted || IsPaused) return false;

            if (!node.GetTree().Paused) return true;

            switch (pauseMode)
            {
                case PauseMode.SyncWithNode:
                    return node.CanProcess();

                case PauseMode.Continue:
                    return true;

                default:
                    return false;
            }
        }

        public void GetResult() { }

        void INotifyCompletion.OnCompleted(Action action)
        {
            this.action = action;
        }

        public IAwaiter GetAwaiter() => (IAwaiter)this;

        public enum PauseMode
        {
            SyncWithNode,
            Stop,
            Continue
        }
    }
}