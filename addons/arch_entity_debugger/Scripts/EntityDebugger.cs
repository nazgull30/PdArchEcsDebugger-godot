namespace ArchEntityDebugger;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using CompactJson;
using Godot;

[Tool]
public partial class EntityDebugger : Control
{
    private TabBar worldOptions;
    private Tree entityListTree;
    private Tree entityDetailsTree;

    [Export]
    private bool activateOnReady = true;
    [Export]
    private float refreshRate = 0.05f;
    [Export]
    private Panel debugWindow;

    private EntityData selectedEntity;

    private double timer;

    private WorldData[] _worlds;

    private int _totalEntities = -1;

    public WorldData ActiveWorld => worldOptions.CurrentTab != -1 && _worlds.Length > worldOptions.CurrentTab ? _worlds[worldOptions.CurrentTab] : null;
    public bool IsActive => debugWindow.Visible;

    /// <summary>
    /// Sets the window's visibility, and enables/disables processing of entities
    /// </summary>
    public void SetActive(bool active)
    {
        debugWindow.Visible = active;
    }

    /// <summary>
    /// Sets the active world to display entities from
    /// </summary>
    public void SetWorld(int index)
    {
        if (index > 0 && index < _worlds.Length)
        {
            worldOptions.CurrentTab = index;
            selectedEntity = null;
            entityDetailsTree.Clear();
            ClearEntityListTree();
        }
    }

    /// <summary>
    /// Selects an entity in the details tree
    /// </summary>
    /// <param name="entity"></param>
    public void SelectEntity(EntityData entity)
    {
        if (selectedEntity != entity)
        {
            selectedEntity = entity;
            entityDetailsTree.Clear();
            RefreshEntityDetailsTree();
        }
    }

    public override void _Ready()
    {
        // debugWindow.CloseRequested += () => SetActive(false);

        GetViewport().GuiEmbedSubwindows = false;
        SetActive(activateOnReady);
        // debugWindow.Title = "Arch Entity Debugger";
        // AddChild(debugWindow);
        debugWindow.Position = GetWindow().Position;
        debugWindow.AnchorLeft = 0;
        debugWindow.AnchorTop = 0;
        debugWindow.AnchorRight = 1;
        debugWindow.AnchorBottom = 1;
        debugWindow.SizeFlagsHorizontal = Control.SizeFlags.Expand | Control.SizeFlags.Fill;
        debugWindow.SizeFlagsVertical = Control.SizeFlags.Expand | Control.SizeFlags.Fill;

        worldOptions = debugWindow.GetNode<TabBar>("MarginContainer/WorldOptions");
        entityListTree = debugWindow.GetNode<Tree>("MarginContainer2/Container/EntitiesListTree");
        entityDetailsTree = debugWindow.GetNode<Tree>("MarginContainer2/Container/EntityDetailsTree");

        entityListTree.Connect("item_selected", Callable.From(OnEntitySelected));
        entityDetailsTree.Connect("button_clicked", Callable.From<TreeItem, int, int, int>(OnEntityComponentButtonClicked));

        worldOptions.Connect("tab_changed", Callable.From<int>(SetWorld));

        entityListTree.Columns = 2;
        entityListTree.SetColumnCustomMinimumWidth(1, 10);
        entityListTree.SetColumnExpandRatio(0, 100);
        entityListTree.SetColumnExpandRatio(1, 2);


        entityDetailsTree.Columns = 4;
        entityDetailsTree.SetColumnExpandRatio(0, 100);
        entityDetailsTree.SetColumnExpandRatio(1, 1);
        entityDetailsTree.SetColumnExpandRatio(2, 1);
        entityDetailsTree.SetColumnExpandRatio(3, 1);
    }

    public void Render(WorldData[] worlds)
    {
        if (!IsActive)
            return;
        _worlds = worlds;

        // var newTotalEntities = _worlds.Sum(w => w.TotalEntities);
        // if (newTotalEntities != _totalEntities)
        // {
        //     ClearEntityListTree();
        // }
        // _totalEntities = newTotalEntities;
        RefreshWorlds();
        if (ActiveWorld == null)
            return;
        RefreshEntityListTree();
        RefreshEntityDetailsTree();
        CheckForOverlap();
    }
    // public override void _Process(double delta)
    // {
    //     if (!IsActive)
    //         return;

    //     timer += delta;
    //     if (timer > refreshRate)
    //     {
    //         timer = 0;
    //         RefreshWorlds();
    //         if (ActiveWorld == null)
    //             return;
    //         RefreshEntityListTree();
    //         RefreshEntityDetailsTree();
    //         CheckForOverlap();
    //     }
    // }

    private void CheckForOverlap()
    {
        Rect2 debugRect = new(debugWindow.Position, debugWindow.Size);
        Rect2 mainRect = new(GetWindow().Position, GetWindow().Size);

        if (debugRect.Intersects(mainRect))
        {
            debugWindow.GetViewport().TransparentBg = true;
        }
        else
        {
            debugWindow.GetViewport().TransparentBg = false;
        }
    }

    private void RefreshWorlds()
    {
        if (worldOptions.TabCount != _worlds.Length)
        {
            worldOptions.ClearTabs();

            for (int i = 0; i < _worlds.Length; i++)
            {
                worldOptions.AddTab($"{i}");
            }
        }

        if (worldOptions.CurrentTab == -1 && !(_worlds.Length == 0))
        {
            worldOptions.CurrentTab = 0;
        }
    }

    private readonly Dictionary<string, TreeItem> archetypeItems = new();
    private readonly Dictionary<string, TreeItem> entityItems = new();
    private readonly Dictionary<string, TreeItem> categoryItems = new();
    private readonly StringBuilder stringBuilder = new();
    private readonly HashSet<string> currentEntities = new();
    private readonly List<string> toRemove = new();

    public void ClearEntityListTree()
    {
        entityListTree.Clear();
        archetypeItems.Clear();
        entityItems.Clear();
        categoryItems.Clear();
    }

    private void RefreshEntityListTree()
    {
        if (entityListTree.GetRoot() == null)
        {
            TreeItem root = entityListTree.CreateItem();
            root.SetText(0, "Entities List");
        }
        TreeItem rootItem = entityListTree.GetRoot();

        foreach (ArchetypeData archetype in ActiveWorld)
        {
            // if (!ArchetypeManager.TryGetArchetypeDisplayName(archetype.Types, out string archetypeKey))
            // {

            // }
            if (archetype.Types.Count == 0)
                continue;

            stringBuilder.Clear();

            foreach (var type in archetype.Types)
            {
                stringBuilder.Append(type).Append(", ");
            }
            if (stringBuilder.Length > 0)
                stringBuilder.Length -= 2;  // Remove trailing ", "

            var archetypeKey = stringBuilder.ToString();

            if (archetypeItems.ContainsKey(archetypeKey))
            {
                archetypeItems[archetypeKey].SetText(1, "0");
            }

            string[] categories = archetypeKey.Split('/');
            if (categories.Length == 1)
            {
                categories = categories.Prepend("Misc").ToArray();
            }
            TreeItem parentItem = rootItem;

            string builtPath = "";
            foreach (string category in categories)
            {
                builtPath = string.IsNullOrEmpty(builtPath) ? category : $"{builtPath}/{category}";
                if (!categoryItems.ContainsKey(builtPath))
                {
                    TreeItem newCategoryItem = parentItem.CreateChildAlphabetically(category);
                    newCategoryItem.SetSelectable(0, false);
                    newCategoryItem.SetSelectable(1, false);
                    newCategoryItem.SetCustomColor(1, Colors.LightYellow);
                    categoryItems[builtPath] = newCategoryItem;
                }
                parentItem = categoryItems[builtPath];
            }

            if (!archetypeItems.ContainsKey(archetypeKey))
            {
                archetypeItems[archetypeKey] = parentItem;
                parentItem.SetTooltipText(0, archetypeKey);
                parentItem.SetCustomBgColor(0, new Color(1, 1, 1, 0.1f));
                parentItem.SetCustomBgColor(1, new Color(1, 1, 1, 0.1f));
                parentItem.Collapsed = true;
            }

            TreeItem archetypeItem = parentItem;
            archetypeItem.SetText(1, archetype.EntityCount.ToString());

            if (archetype.EntityCount > 0)
            {
                foreach (ChunkData chunk in archetype)
                {
                    foreach (var entity in chunk)
                    {
                        string entityIdKey = $"{entity.Id} | {entity.Version}";
                        // GD.Print($"entityIdKey: {entityIdKey}");

                        currentEntities.Add(entityIdKey);
                        if (!entityItems.ContainsKey(entityIdKey) || archetypeItem.GetChildCount() != archetype.EntityCount)
                        {
                            TreeItem newEntityItem = archetypeItem.CreateChild(0);
                            newEntityItem.SetMetadata(0, entity.Id);
                            newEntityItem.SetText(0, entityIdKey);
                            entityItems[entityIdKey] = newEntityItem;
                            currentEntities.Add(entityIdKey);
                        }
                        TreeItem entityItem = entityItems[entityIdKey];
                        if (entity.Id == selectedEntity?.Id)
                        {
                            if (entityListTree.IsConnected("item_selected", Callable.From(OnEntitySelected)))
                            {
                                entityListTree.Disconnect("item_selected", Callable.From(OnEntitySelected));
                            }
                            entityItem.Select(0);
                            entityListTree.Connect("item_selected", Callable.From(OnEntitySelected));
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < archetypeItem.GetChildCount(); i++)
                {
                    archetypeItem.GetChild(i).Free();
                }
            }
        }

        toRemove.AddRange(entityItems.Keys.Except(currentEntities));
        foreach (string key in toRemove)
        {
            TreeItem parentArchetypeItem = entityItems[key].GetParent();
            entityItems[key].Free();
            entityItems.Remove(key);
        }

        currentEntities.Clear();
        toRemove.Clear();
    }

    private void OnEntitySelected()
    {
        TreeItem selected = entityListTree.GetSelected();
        if (selected == null)
            return;

        if (entityDetailsTree.GetRoot() == null)
        {
            entityDetailsTree.CreateItem();
        }
        Variant selectedMetadata = selected.GetMetadata(0);
        var entity = FindEntityById((int)selectedMetadata);
        SelectEntity(entity);
    }

    private void OnEntityComponentButtonClicked(TreeItem item, int buttonId, int column, int mouseButtonIndex)
    {
        if (item.HasMeta("ENTITY_REF"))
        {
            int entityId = (int)item.GetMeta("ENTITY_REF");

            if (buttonId == 0)
            {
                var entity = FindEntityById(entityId);
                SelectEntity(entity);
            }
            if (buttonId == 1)
            {
                bool isExpanded = item.HasMeta("EXPAND_ENTITY_REF") && (bool)item.GetMeta("EXPAND_ENTITY_REF");

                item.SetMeta("EXPAND_ENTITY_REF", !isExpanded);
            }
        }
    }

    private EntityData? FindEntityById(int id)
    {
        foreach (ArchetypeData archetype in ActiveWorld)
        {
            foreach (ChunkData chunk in archetype)
            {
                foreach (var entity in chunk)
                {
                    if (entity.Id == id)
                        return entity;
                }
            }
        }
        return null;
    }

    private void RefreshEntityDetailsTree()
    {
        if (selectedEntity == null)
        {
            entityDetailsTree.Clear();
            return;
        }

        var entity = selectedEntity;

        TreeItem entityRoot = entityDetailsTree.GetRoot();

        if (entityRoot == null)
        {
            entityRoot = entityDetailsTree.CreateItem();
            // if (ArchetypeManager.TryGetArchetypeDisplayName(entity.GetComponentTypes(), out string archetypeName))
            //     archetypeName = archetypeName.Split("/").LastOrDefault();
            // else
            var archetypeName = "Entity";

            entityRoot.SetText(0, $"{archetypeName} | ID: {entity.Id} | Version: {entity.Version}");
        }

        var components = entity.Types;
        for (int i = 0; i < components.Length; i++)
        {
            EntityTreeRendering.Render(entityRoot, components[i], i, "", true);
        }
    }
}
