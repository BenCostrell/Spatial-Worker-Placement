using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hex {
    public readonly int[] coords;
    public int q { get { return coords[0]; } }
    public int r { get { return coords[1]; } }
    public int s { get { return coords[2]; } }

    public Hex (int q_, int r_, int s_)
    {
        Debug.Assert(q_ + r_ + s_ == 0);
        coords = new int[3] { q_, r_, s_ };
    }

    public override bool Equals(object obj)
    {
        Hex hex = obj as Hex;
        if (hex == null) return false;
        return (q == hex.q) && (r == hex.r) && (s == hex.s);
    }

    public bool Equals(Hex hex)
    {
        if (hex == null) return false;
        return (q == hex.q) && (r == hex.r) && (s == hex.s);
    }

    public override int GetHashCode()
    {
        return q ^ r ^ s;
    }

    public static bool operator ==(Hex a, Hex b)
    {
        if (ReferenceEquals(a, b)) return true;
        if ((object)a == null || (object)b == null) return false;
        return (a.q == b.q) && (a.r == b.r) && (a.s == b.s);
    }

    public static bool operator !=(Hex a, Hex b)
    {
        return !(a == b);
    }

    public Hex Add (Hex b)
    {
        return new Hex(q + b.q, r + b.r, s + b.s);
    }

    public Hex Subtract (Hex b)
    {
        return new Hex(q - b.q, r - b.r, s - b.s);
    }

    public Hex Multiply(int k)
    {
        return new Hex(k * q, k * r, k * s);
    }

    public int Length()
    {
        return ((Mathf.Abs(q) + Mathf.Abs(r) + Mathf.Abs(s)) / 2);
    }

    public int Distance(Hex b)
    {
        return Subtract(b).Length();
    }

    readonly Hex[] directions = new Hex[6]
    {
        new Hex(1, 0, -1), new Hex(1, -1, 0), new Hex(0, -1, 1),
        new Hex(-1, 0, 1), new Hex(-1, 1, 0), new Hex(0, 1, -1)
    };

    public Hex Direction(int direction)
    {
        return directions[direction % 6];
    }

    public Hex Neighbor(int direction)
    {
        return Add(Direction(direction));
    }
}
