# Swift Timer
A lightweight and flexible callback-driven timer system for Unity.  

This package provides a centralized way to schedule and manage timers without relying on individual MonoBehaviour instances, coroutines, or manually maintained update loops.  

I designed this system with performance and flexibility in mind, it also provides you with simple APIs for creating and managing timers. It is also burst-compiled and job supported, meaning it can handle many timers at once and not bog down Unity's mainthread. However, it does not mean its free performance. The synchronization and the job itself still has a cost because of their overhead. unless your game uses alot of individual timers (256+ timers just to barely break even). This timer system would more likely to be a liability than a benefit, so be mindful.

There's also the callbacks. Despite the timers being multithreaded, their callbacks are still singlethreaded. This is important because Unity's PlayerLoop is executed in the mainthread which also allow callbacks to safely interact with its ecosystem. This also means that if alot of timers were completed in the same frame, all their callbacks would be invoked at the same time, causing a lag spike (it also depends of how much work each callback does).

## Installation

1. Open you Unity project
2. Select `Window` at your top corner screen
3. Then go to `Package Management`
4. Select `Package Manager`
5. Select `+` at the top-left corner of the package manager window
6. Select `Install package from git URL`
7. Swift Timer requires these packages first before it can work
   - `Unity Job System` - Exists in the Unity Registry. But its usually already built-in in URP projects
   - `Unity Collections` - Exists in the Unity Registry. But its usually already built-in in URP projects
   - `Command And Callback` - My own command library package. Paste this git URL `https://github.com/Castforn/CommandAndCallback.git`
8. Paste this git URL `https://github.com/Castforn/SwiftTimer.git`
9. Your done!

## Features

- Handle-Based Management – Timers can be controlled using lightweight `TimerHandle` structs.
- Callback Driven - When a timer completes, it will invoke the callback you supplied with.
- Reusable Blueprints – `TimerBlueprint` can be reused to create independent timers.
- Flexible Timing – You can choose when callbacks are invoked using `TimerTiming`.
- Multiple Clocks – Supports up to 4 time sources, which are scaled, unscaled, frame-based, and fixed frame-based timing.
- Manipulable States – The state of a timer can freely be changed as long as its still active.
- Timer Dependencies – Timers can be scheduled to wait until another timer completes. Basically, its the same principle as Unity's `JobHandle` in the job system.
- 8 Generic Parameters Support - Callbacks can support up to 8 generic parameters.

## API

These are most of the APIs you can use.  
I say 'most' because the rest of the remaining API are all abstracted by `TimerHandle` which im pretty sure you'll generally be using that handle the entire time.

### TimerBlueprint

A reusable configuration that can create multiple independent timers with.

```csharp
using SwiftTimer;
using UnityEngine;

public class Test : MonoBehaviour
{

    // You can create a blueprint like this
    static TimerBlueprint blueprint1 = TimerBlueprint.Create()
        .SetDuration(3)
        .SetClock(TimerClock.Scaled)
        .SetTiming(TimerTiming.PreUpdate)
        .SetEnvironment(TimerEnvironment.Runtime);


    // Or this
    static TimerBlueprint blueprint2 = TimerBlueprint.Create(3, 0, false, TimerClock.Unscaled, TimerTiming.PostFixedUpdate, TimerEnvironment.Editor);


    // But i would not recommend creating a blueprint on a non-static field
    TimerBlueprint timerblueprint = TimerBlueprint.Create()
        .Set(5.5f, 0, true, TimerClock.Frame, TimerTiming.EndFrame, TimerEnvironment.Both);


    // This is because blueprints are meant to be used as a configuration for your timers
    // Having one blueprint per instance would just be a waste of memory

}
```

### TimerHandle

A lightweight struct that provides access to a scheduled timer. It also allows you to inspect, modify, and control the timer without directly accessing the `TimerManager`.

```csharp
using SwiftTimer;
using UnityEngine;

public class Test : MonoBehaviour
{

    static TimerBlueprint blueprint = TimerBlueprint.Create()
        .SetDuration(3)
        .SetClock(TimerClock.Scaled)
        .SetTiming(TimerTiming.PreUpdate)
        .SetEnvironment(TimerEnvironment.Runtime);


    void Start()
    {
        // When you call Schedule(), the blueprint will output a TimerHandle which you can use to track or manage its state
        TimerHandle timer1 = blueprint.Schedule(MyMethod1);


        // You can also add dependencies to the timer
        // Timers with dependencies will not start until the timer they depend on has been terminated
        // It basically has the same principles as JobHandle in the Unity Job System


        // This timer will only run once timer1 has finished
        TimerHandle timer2 = blueprint.Schedule(MyMethod2, timer1);


        // You can also change the configurations of a blueprint before you schedule a timer
        blueprint.Duration = 9.1f;
        blueprint.SetClock(TimerClock.FixedFrame);

        TimerHandle timer3 = blueprint.Schedule(MyMethod1, timer2);
    }


    void MyMethod1()
    {
        Debug.Log("My method 1");
    }

    void MyMethod2()
    {
        Debug.Log("My method 2");
    }

}
```

## Timer Fields

#### These are all of the necessary fields of a timer which can be manipulated in `TimerHandle`.  

`Duration` - Duration of the timer.  
`Time` - Elapsed time. The timer will be marked for completion once (`Time` >= `Duration`) is true.  
`Pause` - Whether the timer should be running or not.  
`TimerClock` - Time source being used (scaled time, unscaled time, frame-based, fixed frame-based).  
`TimerTiming` - When should the callback be invoked (begin frame, pre-fixed update, post-fixed update, pre-update, post-update, pre-last update, post-last update, end frame).  
`TimerEnvironment` - What environment should the timer be allowed to exist (Runtime, Editor, Both). A timer not in its correct environment is automatically stopped without calling its callback.  

#### These are not necessary fields but are still useful.  

`TimeRemaining` - Remaining time.  
`TotalDuration` - Total duration which includes all of its dependencies.  
`TotalTime` - Total elapsed time which includes all of its dependencies.  
`TotalTimeRemaining` - Total remaining time which includes all of its dependencies.  
`IsActive` - Whether the timer still exists.  
`IsPending` - Whether the timer is active and currently waiting for its dependency to complete.  
`IsRunning` - Whether the elapsed time of the timer is running.  
`HasCompleted` - Whether the timer has been marked for completion.  
`HasStopped` - Whether the timer has been marked for stopping.  
`HasTerminated` - Whether the timer has either been completed or stopped.  
`ChangeState()` - Changes multiple necessary fields at once. Is more efficient at manipulating multiple fields.  
`ChangeCallback()` - Changes the callback tied to the timer.  
`Complete()` - Marks the timer for completion.  
`Stop()` - Marks the timer for stopping. Does not invoke its callback.  
`CompleteAll()` - Marks the timer and all of its dependencies for completion.  
`StopAll()` - Marks the timer and all of its dependencies for stopping.  

#### The rest are not really that helpful for you but here they are.  

`Version` - Version of the underlying `TimerToken` that the `TimerHandle` manages.  
`Index` - Index of the underlying `TimerToken` that the `TimerHandle` manages.  
`IsInitialized` - Whether the underlying `TimerToken` was initialized by the `TimerManager`. This does not mean that the timer is active. That's what `IsActive` is for.  
`GetState()` - Gets the `TimerState` state of the timer.  
`SetState()` - Sets the `TimerState` state of the timer.  
`TryGetState()` - Attempts to get the `TimerState` state of the timer.  
`GetCallback()` - Gets the `BaseActionCommand` callback of the timer.  
`SetCallback()` - Sets the `BaseActionCommand` callback of the timer.  
`TryGetCallback()` - Attempts to get the `BaseActionCommand` callback of the timer.  
`GetDependency()` - Gets the `TimerToken` dependency of the timer.  
`TryGetDependency()` - Attempts to get the `TimerToken` dependency of the timer.  

## Thanks For Scrolling

Thats all i have for this package, well not really everything, there are still some APIs left in `TimerManager` but im pretty sure you dont need it because `TimerHandle` already gives you a more convenient API to work with. Lastly, there are also APIs for `PlayerLoopRunners` and `PlayerLoopUtility` but their not really the stuff you want to use in this package right? So yeah, enjoy.
