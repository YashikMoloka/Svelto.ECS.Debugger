# Svelto.ECS.Debugger
## Alarm!
WIP. This project have bugs, Bugs and BUGS.
### Install

- Copy Svelto.ECS.Debugger to your project folder.
- Make sure that Svelto.ECS.Debugger have references to Svelto.ECS and Svelto.Common
- (I delete this later) Attach Debugger.cs script to your Root GameObject.
- Attach Debugger with `_enginesRoot.AttachDebugger();` in your MainCompositionRoot.cs
- (Optional) Rename `new EnginesRoot(_scheduler)` to `new EnginesRootNamed(_scheduler, "ThisAwesomeRoot")`
- (Optional) Rename `new ExclusiveGroup()` to `new ExclusiveGroupNamed("ThisAwesomeGroup")`

### Using
Just open debugger in top menu Window/Analysis/Svelto.ECS Debugger
