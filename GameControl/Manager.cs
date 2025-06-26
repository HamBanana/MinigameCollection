using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class Manager : MonoBehaviour
{
    [Header("Manager Base Settings")]
    public bool enableDebugLogs = true;
    public int initializationPriority = 0; // Lower numbers initialize first
    
    // Static manager registry
    private static Dictionary<Type, Manager> managerRegistry = new Dictionary<Type, Manager>();
    private static List<Manager> allManagers = new List<Manager>();
    private static bool isQuitting = false;
    
    // Manager lifecycle events
    public static event EventHandler<ManagerEventArgs> OnManagerRegistered;
    public static event EventHandler<ManagerEventArgs> OnManagerUnregistered;
    public static event EventHandler OnAllManagersInitialized;
    
    // Instance properties
    public bool IsInitialized { get; private set; } = false;
    public virtual int Priority => this.initializationPriority;
    
    // Controller registration (for your existing system)
    public event EventHandler<ControllerActionEventArgs> onControllerAction;
    private List<Controller> registeredControllers = new List<Controller>();
    
    #region Unity Lifecycle
    
    protected virtual void Awake()
    {
        if (isQuitting) return;
        
        // Register this manager
        RegisterManager(this);
        
        this.DebugLog($"Manager {this.GetType().Name} awakened");
    }
    
    protected virtual void Start()
    {
        if (isQuitting) return;
        
        // Mark as initialized
        this.IsInitialized = true;
        
        this.DebugLog($"Manager {this.GetType().Name} started and initialized");
        
        // Check if all managers are initialized
        CheckAllManagersInitialized();
    }
    
    protected virtual void OnDestroy()
    {
        if (!isQuitting)
        {
            UnregisterManager(this);
        }
        
        // Cleanup controllers
        foreach (var controller in this.registeredControllers)
        {
            if (controller != null)
            {
                this.UnregisterController(controller);
            }
        }
        this.registeredControllers.Clear();
    }
    
    protected virtual void OnApplicationQuit()
    {
        isQuitting = true;
    }

    public virtual void Initialize()
    {
        Debug.Log("Manager: Initialized...");
    }
    
    #endregion

    #region Manager Registry System

    private static void RegisterManager(Manager manager)
    {
        Type managerType = manager.GetType();

        // Check for duplicates
        if (managerRegistry.ContainsKey(managerType))
        {
            Debug.LogWarning($"Manager of type {managerType.Name} already registered. Replacing with new instance.");
            var oldManager = managerRegistry[managerType];
            allManagers.Remove(oldManager);
        }

        managerRegistry[managerType] = manager;
        allManagers.Add(manager);

        // Sort by priority
        allManagers.Sort((a, b) => a.Priority.CompareTo(b.Priority));

        Debug.Log($"Manager registered: {managerType.Name} (Priority: {manager.Priority})");

        // Emit event
        OnManagerRegistered?.Invoke(null, new ManagerEventArgs { Manager = manager });
    }
    
    private static void UnregisterManager(Manager manager)
    {
        Type managerType = manager.GetType();
        
        if (managerRegistry.ContainsKey(managerType) && managerRegistry[managerType] == manager)
        {
            managerRegistry.Remove(managerType);
            allManagers.Remove(manager);
            
            Debug.Log($"Manager unregistered: {managerType.Name}");
            
            // Emit event
            OnManagerUnregistered?.Invoke(null, new ManagerEventArgs { Manager = manager });
        }
    }
    
    private static void CheckAllManagersInitialized()
    {
        foreach (var manager in allManagers)
        {
            if (!manager.IsInitialized)
            {
                return; // Not all managers are initialized yet
            }
        }
        
        // All managers are initialized
        Debug.Log("All managers initialized!");
        OnAllManagersInitialized?.Invoke(null, EventArgs.Empty);
    }
    
    #endregion
    
    #region Static Manager Access
    
    /// <summary>
    /// Get a manager of the specified type
    /// </summary>
    public static T Get<T>() where T : Manager
    {
        Type managerType = typeof(T);
        
        if (managerRegistry.TryGetValue(managerType, out Manager manager))
        {
            return manager as T;
        }
        
        Debug.LogWarning($"Manager of type {managerType.Name} not found!");
        return null;
    }
    
    /// <summary>
    /// Check if a manager of the specified type exists
    /// </summary>
    public static bool Exists<T>() where T : Manager
    {
        return managerRegistry.ContainsKey(typeof(T));
    }
    
    /// <summary>
    /// Get all registered managers
    /// </summary>
    public static List<Manager> GetAllManagers()
    {
        return new List<Manager>(allManagers);
    }
    
    /// <summary>
    /// Get all managers of a specific base type
    /// </summary>
    public static List<T> GetManagersOfType<T>() where T : Manager
    {
        List<T> result = new List<T>();
        foreach (var manager in allManagers)
        {
            if (manager is T typedManager)
            {
                result.Add(typedManager);
            }
        }
        return result;
    }
    
    #endregion
    
    #region Controller Management (Your Existing System)
    
    /// <summary>
    /// Register a controller with this manager
    /// </summary>
    public virtual void RegisterController(Controller controller)
    {
        if (controller == null)
        {
            this.DebugLog("Cannot register null controller");
            return;
        }
        
        if (this.registeredControllers.Contains(controller))
        {
            this.DebugLog($"Controller {controller.gameObject.name} already registered");
            return;
        }
        
        this.registeredControllers.Add(controller);
        
        // Subscribe to controller events
        controller.OnAction += this.HandleControllerAction;
        
        this.DebugLog($"Controller registered: {controller.gameObject.name}");
        
        // Virtual method for derived classes
        this.OnControllerRegistered(controller);
    }
    
    /// <summary>
    /// Unregister a controller from this manager
    /// </summary>
    public virtual void UnregisterController(Controller controller)
    {
        if (controller == null || !this.registeredControllers.Contains(controller))
        {
            return;
        }
        
        this.registeredControllers.Remove(controller);
        
        // Unsubscribe from controller events
        controller.OnAction -= this.HandleControllerAction;
        
        this.DebugLog($"Controller unregistered: {controller.gameObject.name}");
        
        // Virtual method for derived classes
        this.OnControllerUnregistered(controller);
    }
    
    /// <summary>
    /// Handle controller actions
    /// </summary>
    private void HandleControllerAction(object sender, ControllerActionEventArgs e)
    {
        this.DebugLog($"Controller action received from: {e.ControllerGameObject.name}");
        
        // Forward to manager-specific handling
        this.ProcessControllerAction(e);
        
        // Emit general event
        this.onControllerAction?.Invoke(this, e);
    }
    
    /// <summary>
    /// Override this to handle controller actions in derived managers
    /// </summary>
    protected virtual void ProcessControllerAction(ControllerActionEventArgs e)
    {
        // Default implementation - do nothing
    }
    
    /// <summary>
    /// Called when a controller is registered - override in derived classes
    /// </summary>
    protected virtual void OnControllerRegistered(Controller controller)
    {
        // Default implementation - do nothing
    }
    
    /// <summary>
    /// Called when a controller is unregistered - override in derived classes
    /// </summary>
    protected virtual void OnControllerUnregistered(Controller controller)
    {
        // Default implementation - do nothing
    }
    
    #endregion
    
    #region Utility Methods
    
    /// <summary>
    /// Debug logging with manager name prefix
    /// </summary>
    protected void DebugLog(string message)
    {
        if (this.enableDebugLogs)
        {
            Debug.Log($"[{this.GetType().Name}] {message}");
        }
    }
    
    /// <summary>
    /// Debug warning with manager name prefix
    /// </summary>
    protected void DebugWarning(string message)
    {
        if (this.enableDebugLogs)
        {
            Debug.LogWarning($"[{this.GetType().Name}] {message}");
        }
    }
    
    /// <summary>
    /// Debug error with manager name prefix
    /// </summary>
    protected void DebugError(string message)
    {
        Debug.LogError($"[{this.GetType().Name}] {message}");
    }
    
    /// <summary>
    /// Get other manager - convenience method
    /// </summary>
    protected T GetManager<T>() where T : Manager
    {
        return Get<T>();
    }
    
    /// <summary>
    /// Check if another manager exists - convenience method
    /// </summary>
    protected bool HasManager<T>() where T : Manager
    {
        return Exists<T>();
    }
    
    #endregion
    
    #region Manager Communication
    
    /// <summary>
    /// Send a message to all managers
    /// </summary>
    public static void BroadcastToAllManagers<T>(string methodName, T data)
    {
        foreach (var manager in allManagers)
        {
            try
            {
                var method = manager.GetType().GetMethod(methodName);
                if (method != null)
                {
                    method.Invoke(manager, new object[] { data });
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error broadcasting to {manager.GetType().Name}: {e.Message}");
            }
        }
    }
    
    /// <summary>
    /// Send a message to specific manager type
    /// </summary>
    public static void SendToManager<TManager, TData>(string methodName, TData data) where TManager : Manager
    {
        var manager = Get<TManager>();
        if (manager != null)
        {
            try
            {
                var method = manager.GetType().GetMethod(methodName);
                if (method != null)
                {
                    method.Invoke(manager, new object[] { data });
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error sending to {typeof(TManager).Name}: {e.Message}");
            }
        }
    }
    
    #endregion
    
    #region Virtual Methods for Derived Classes
    
    /// <summary>
    /// Called after all managers are initialized - override for post-init setup
    /// </summary>
    public virtual void OnAllManagersReady()
    {
        // Override in derived classes for setup that requires all managers
    }
    
    /// <summary>
    /// Called when the game is paused - override to handle pause logic
    /// </summary>
    public virtual void OnGamePaused(bool isPaused)
    {
        // Override in derived classes
    }
    
    /// <summary>
    /// Called when transitioning between scenes - override for cleanup/setup
    /// </summary>
    public virtual void OnSceneTransition(string fromScene, string toScene)
    {
        // Override in derived classes
    }
    
    #endregion
    
    #region Static Initialization
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeManagerSystem()
    {
        // Clear registry on scene reload
        managerRegistry.Clear();
        allManagers.Clear();
        isQuitting = false;
        
        // Subscribe to all managers ready event
        OnAllManagersInitialized += NotifyManagersReady;
        
        Debug.Log("Manager system initialized");
    }
    
    private static void NotifyManagersReady(object sender, EventArgs e)
    {
        // Notify all managers that everyone is ready
        foreach (var manager in allManagers)
        {
            try
            {
                manager.OnAllManagersReady();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error in {manager.GetType().Name}.OnAllManagersReady(): {ex.Message}");
            }
        }
    }
    
    #endregion
}

// Event Args
public class ManagerEventArgs : EventArgs
{
    public Manager Manager { get; set; }
}

// Assuming you have these from your existing system
public class ControllerActionEventArgs : EventArgs
{
    public GameObject ControllerGameObject { get; set; }
    // Add other properties as needed
}

// Placeholder for your Controller base class
public abstract class Controller : MonoBehaviour
{
    public event EventHandler<ControllerActionEventArgs> OnAction;
    
    protected void EmitAction(GameObject controllerGameObject)
    {
        this.OnAction?.Invoke(this, new ControllerActionEventArgs 
        { 
            ControllerGameObject = controllerGameObject 
        });
    }
}