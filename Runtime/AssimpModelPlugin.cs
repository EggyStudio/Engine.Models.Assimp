namespace Engine;

/// <summary>
/// Plugin that brings up the Open Asset Import Library backend for the model-import
/// system. Registers <see cref="AssimpModelLoader"/> with the <see cref="AssetServer"/>
/// and the matching <see cref="AssimpModelReader"/> with the
/// <see cref="SceneReaderRegistry"/>.
/// </summary>
/// <remarks>
/// <para>
/// <b>Coverage:</b> Assimp parses ~40 long-tail formats - FBX, OBJ, COLLADA (.dae),
/// 3DS, BLEND, PLY, STL, X, MD2/MD3/MD5, IFC, LWO, glTF (handled by
/// <see cref="GltfModelPlugin"/> at higher fidelity instead), and more. The full
/// up-to-date list comes from the native library at runtime via
/// <see cref="Assimp.AssimpContext.GetSupportedImportFormats"/>; this plugin registers
/// the loader for every extension it advertises (less the USD family which is owned by
/// <see cref="UsdScenesPlugin"/>).
/// </para>
/// <para>
/// <b>Order:</b> add <i>after</i> <see cref="ScenesPlugin"/> and <see cref="AssetPlugin"/>;
/// <see cref="ModelsPlugin"/> wires this up first, then <see cref="GltfModelPlugin"/> so
/// the glTF-specific path wins for <c>.gltf</c> / <c>.glb</c> via last-registration
/// semantics in <see cref="AssetServer.RegisterLoader{T}"/>.
/// </para>
/// </remarks>
/// <seealso cref="ModelsPlugin"/>
/// <seealso cref="AssimpModelReader"/>
public sealed class AssimpModelPlugin : IPlugin
{
    private static readonly ILogger Logger = Log.Category("Engine.Models.Assimp");

    /// <inheritdoc />
    public void Build(App app)
    {
        Logger.Info("AssimpModelPlugin: Initialising Assimp backend...");

        var reader = new AssimpModelReader();
        var loader = new AssimpModelLoader(reader);

        if (!app.World.TryGetResource<SceneReaderRegistry>(out var registry))
        {
            registry = new SceneReaderRegistry();
            app.World.InsertResource(registry);
            Logger.Warn("AssimpModelPlugin: SceneReaderRegistry was missing - did you forget to add ScenesPlugin? Created one implicitly.");
        }
        registry.RegisterReader(reader);

        if (app.World.TryGetResource<AssetServer>(out var server))
        {
            server.RegisterLoader(loader);
            Logger.Debug($"AssimpModelPlugin: AssimpModelLoader registered with AssetServer for {loader.Extensions.Length} extensions.");
        }
        else
        {
            Logger.Warn("AssimpModelPlugin: AssetServer not found - AssimpModelLoader was NOT registered. Add AssetPlugin first.");
        }

        Logger.Info("AssimpModelPlugin: Assimp backend ready.");
    }
}