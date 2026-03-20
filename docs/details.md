## Load order

#### TL/DR: Multiplayer must load first

When you define new messages, you annotate them with attributes from the `multiplayer` assemblies. The `.NET` runtime (specifically the CLR) resolves attribute types during metadata loading - **before** your assembly is actually loaded. This means your code never gets a chance to run and hook into assembly resolution.

Because of this, the multiplayer assemblies must already be loaded. Otherwise, you will hit a `TypeLoadException`.

Unfortunately, UnityModManager doesn't offer explicit load order control, so the only reliable workaround is **folder naming**.

## Accessing WOTRMultiplayer instances
The simplest way to access multiplayer instances is via `ServiceProvider`. While this pattern is generally discouraged in typical application design, it works well here since it provides direct access without worrying about structure or encapsulation.

You can access it through:

```
WOTRMultiplayer.Main.ServiceProvider
```

Keep in mind:

* Only **singleton** services are safe to use this way
* Requesting non-singleton services will give you a new instance that isn't actually used by the mod

## Network messages
Most of the time, you will be working with `INetworkServer` or `INetworkClient`. These give you direct access to network messages, letting you either listen to existing messages or send your own custom ones.

When you register a handler, there is a simple priority system. You can choose to run your handler before or after other handlers - including the ones already set up by multiplayer. Use `High` priority only if you really need your handler to run before the default handlers.

#### Disclaimer: you are tinkering with low-level interfaces - they can change anytime, so don't count on backward compatibility
