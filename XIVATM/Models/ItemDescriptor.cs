using ECommons.ExcelServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace XIVATM.Models;

public struct ItemDescriptor : IEquatable<ItemDescriptor>
{
    public int Id;
    public bool HQ;

    public ItemDescriptor(int id, bool hQ)
    {
        this.Id = id;
        this.HQ = hQ;
    }

    public ItemDescriptor(uint id, bool hQ)
    {
        this.Id = (int)id;
        this.HQ = hQ;
    }

    public override bool Equals(object obj) => obj is ItemDescriptor other && this.Equals(other);

    public bool Equals(ItemDescriptor other) => this.Id == other.Id && this.HQ == other.HQ;

    public override int GetHashCode() => HashCode.Combine<int, bool>(this.Id, this.HQ);

    public override readonly string ToString()
    {
        DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 2);
        interpolatedStringHandler.AppendLiteral("[");
        interpolatedStringHandler.AppendFormatted(ExcelItemHelper.GetName((uint)this.Id, true));
        interpolatedStringHandler.AppendLiteral(",");
        interpolatedStringHandler.AppendFormatted<bool>(this.HQ);
        interpolatedStringHandler.AppendLiteral("]");
        return interpolatedStringHandler.ToStringAndClear();
    }

    public static bool operator ==(ItemDescriptor left, ItemDescriptor right) => left.Equals(right);

    public static bool operator !=(ItemDescriptor left, ItemDescriptor right) => !(left == right);
}
