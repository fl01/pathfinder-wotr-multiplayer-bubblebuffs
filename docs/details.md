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
