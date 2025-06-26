using UnityEngine;
using Ham.GameControl.Player;
using System;

public class Ownable : MonoBehaviour
{
    public event EventHandler<OwnerChangedEventArgs> OnOwnerChanged;

    private PlayerController owner;
    public PlayerController Owner
    {
        get => owner;
        set
        {
            if (owner != value)
            {
                PlayerController oldOwner = owner;
                owner = value;
                OnOwnerChanged?.Invoke(this, new OwnerChangedEventArgs
                {
                    Target = this,
                    NewOwner = owner,
                    PreviousOwner = oldOwner
                });
            }
        }
    }
}

public class OwnerChangedEventArgs : EventArgs
{
    public Ownable Target;
    public PlayerController NewOwner;
    public PlayerController PreviousOwner;
}
