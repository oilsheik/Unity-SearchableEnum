# Unity Searchable Enum

A custom attribute and property drawer to make enums easily searchable in the Unity Inspector. 
Ideal for enums with a large number of options.


## Features
- Instant Search: Filter long enum lists just by typing.
- Keyboard Support: Navigate with arrow keys, select with Enter, and cancel with Escape.


## Setup

This utility requires two script files in specific folders.

1.  Place `SearchableEnumDrawer.cs` in a folder named `Editor`. (It's an editor script so it needs to be in an editor folder!)
    Assets/Editor/SearchableEnumDrawer.cs

2.  Place `SearchableEnumAttribute.cs` in any other folder inside Assets. For example `Assets/Scripts/` or directly in Assets.
    Assets/SearchableEnumAttribute.cs


## How to Use

Simply add the `[SearchableEnum]` attribute above any public enum field in your scripts.


Example script:
```csharp
using UnityEngine;

public enum ItemType
{
    None,
    Shortsword,
    Longsword,
    Broadsword,
    Greatsword,
};

public class PlayerInventory : MonoBehaviour
{
    [SearchableEnum]
    public ItemType primaryWeapon;


    //other code
}

