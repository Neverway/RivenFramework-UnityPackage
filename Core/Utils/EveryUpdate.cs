using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace ErryLib.MonoTasks
{
    public class For : INotifyCompletion
    {
        private event Action onComplete;
        public bool IsCompleted { get; set; }
        public void OnCompleted(Action continuation) => onComplete += continuation;
        public void GetResult() { }
        public For GetAwaiter() => this;
        protected void CompleteTask()
        {
            IsCompleted = true;
            AfterCompleted();
            onComplete?.Invoke();
        }
        protected virtual void AfterCompleted() { }

        public static For Seconds(float seconds) => new ForSeconds(seconds);
        public static For NextFrame => new ForNextUpdate();
        public static For NextFixedUpdate => new ForNextFixedUpdate();
        public static For NextUpdate => new ForNextUpdate();
        public static For NextLateUpdate => new ForNextLateUpdate();
        public static For AfterRender => new ForNextOnPostRender();
    }

    public abstract class EveryUpdate : For
    {
        public EveryUpdate()
        {
            AwaitForMonoRunner.onUpdate += OnMonoUpdate;
            OnCompleted(() => AwaitForMonoRunner.onUpdate -= OnMonoUpdate);
        }
        private void OnMonoUpdate()
        {
            if (IsCompleteOnUpdate())
                CompleteTask();
        }
        protected abstract bool IsCompleteOnUpdate();
    }
    public class ForCoroutine : For
    {
        public ForCoroutine(YieldInstruction yieldInfo)
        {
            AwaitForMonoRunner.RegisterCoroutine(CompleteTaskAfterYieldInstruction(yieldInfo));
        }
        private IEnumerator CompleteTaskAfterYieldInstruction(YieldInstruction yieldInfo)
        {
            yield return yieldInfo;
            CompleteTask();
        }
        public ForCoroutine(IEnumerator enumerator) =>
            AwaitForMonoRunner.RegisterCoroutine(CompleteTaskAfterCoroutine(enumerator));
        private IEnumerator CompleteTaskAfterCoroutine(IEnumerator enumerator)
        {
            yield return enumerator;
            CompleteTask();
        }
    }

    public class ForSeconds : ForCoroutine { public ForSeconds(float seconds) : base(new WaitForSeconds(seconds)) { } }
    public class ForNextFixedUpdate : For
    {
        public ForNextFixedUpdate() => AwaitForMonoRunner.onFixedUpdate += CompleteTask;
        protected override void AfterCompleted() => AwaitForMonoRunner.onFixedUpdate -= CompleteTask;
    }
    public class ForNextUpdate : For
    {
        public ForNextUpdate() => AwaitForMonoRunner.onUpdate += CompleteTask;
        protected override void AfterCompleted() => AwaitForMonoRunner.onUpdate -= CompleteTask;
    }
    public class ForNextLateUpdate : For
    {
        public ForNextLateUpdate() => AwaitForMonoRunner.onLateUpdate += CompleteTask;
        protected override void AfterCompleted() => AwaitForMonoRunner.onLateUpdate -= CompleteTask;
    }
    public class ForNextOnPostRender: For
    {
        public ForNextOnPostRender() => AwaitForMonoRunner.onPostRender += CompleteTask;
        protected override void AfterCompleted() => AwaitForMonoRunner.onPostRender -= CompleteTask;
    }


    internal class AwaitForMonoRunner : MonoBehaviour
    {
        private static AwaitForMonoRunner Instance;

        public static void EnsureInstanceExists()
        {
            if (Instance == null) 
                CreateMonoRunner();
        }
        [RuntimeInitializeOnLoadMethod]
        private static void CreateMonoRunner()
        {
            GameObject obj = new GameObject(nameof(AwaitForMonoRunner));
            DontDestroyOnLoad(obj);
            Instance = obj.AddComponent<AwaitForMonoRunner>();

            onFixedUpdate = null;
            onUpdate = null;
            onLateUpdate = null;
            onUpdate = null;
        }

        public static Coroutine RegisterCoroutine(IEnumerator coroutine) => Instance.StartCoroutine(coroutine);
        public static void UnregisterCoroutine(Coroutine coroutine) => Instance.StopCoroutine(coroutine);

        public void FixedUpdate() => onFixedUpdate?.Invoke();
        public static event Action onFixedUpdate;

        public void Update() => onUpdate?.Invoke();
        public static event Action onUpdate;

        public void LateUpdate() => onLateUpdate?.Invoke();
        public static event Action onLateUpdate;

        public void OnPostRender() => onPostRender?.Invoke();
        public static event Action onPostRender;
    }
}