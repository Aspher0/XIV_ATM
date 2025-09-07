using FFXIVClientStructs.FFXIV.Client.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace XIVATM.Models;

public record struct QueueEntry
{
    public InventoryType Type;
    public int SlotID;
    public int Quantity;

    public QueueEntry(InventoryType type, int slotID, int quantity)
    {
        this.Type = type;
        this.SlotID = slotID;
        this.Quantity = quantity;
    }

    [CompilerGenerated]
    public override readonly int GetHashCode()
    {
        return (EqualityComparer<InventoryType>.Default.GetHashCode(this.Type) * -1521134295 + EqualityComparer<int>.Default.GetHashCode(this.SlotID)) * -1521134295 + EqualityComparer<int>.Default.GetHashCode(this.Quantity);
    }

    [CompilerGenerated]
    public readonly bool Equals(QueueEntry other)
    {
        return EqualityComparer<InventoryType>.Default.Equals(this.Type, other.Type) && EqualityComparer<int>.Default.Equals(this.SlotID, other.SlotID) && EqualityComparer<int>.Default.Equals(this.Quantity, other.Quantity);
    }
}
