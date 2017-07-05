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

    public static Hex Add(Hex a, Hex b)
    {
        return a.Add(b);
    }

    public Hex Add (Hex b)
    {
        return new Hex(q + b.q, r + b.r, s + b.s);
    }

    public static Hex Subtract (Hex a, Hex b)
    {
        return a.Subtract(b);
    }

    public Hex Subtract (Hex b)
    {
        return new Hex(q - b.q, r - b.r, s - b.s);
    }

    public static Hex Multiply(int k, Hex a)
    {
        return a.Multiply(k);
    }

    public Hex Multiply(int k)
    {
        return new Hex(k * q, k * r, k * s);
    }

    public static int Length(Hex a)
    {
        return a.Length();
    }

    public int Length()
    {
        return ((Mathf.Abs(q) + Mathf.Abs(r) + Mathf.Abs(s)) / 2);
    }

    public static int Distance(Hex a, Hex b)
    {
        return a.Distance(b);
    }

    public int Distance(Hex b)
    {
        return Subtract(b).Length();
    }

    static readonly Hex[] directions = new Hex[6]
    {
        new Hex(1, 0, -1), new Hex(1, -1, 0), new Hex(0, -1, 1),
        new Hex(-1, 0, 1), new Hex(-1, 1, 0), new Hex(0, 1, -1)
    };

    public static Hex Direction(int direction)
    {
        return directions[direction % 6];
    }

    public static Hex Neighbor(Hex a, int direction)
    {
        return a.Neighbor(direction);
    }

    public Hex Neighbor(int direction)
    {
        return Add(Direction(direction));
    }

    public static Vector2 ScreenPos(Layout layout, Hex hex)
    {
        return hex.ScreenPos(layout);
    }

    public Vector2 ScreenPos(Layout layout)
    {
        Orientation orientation = layout.orientation;
        float x = (orientation.f0 * q + orientation.f1 * r) * layout.size.x;
        float y = (orientation.f2 * q + orientation.f3 * r) * layout.size.y;
        return new Vector2(x + layout.origin.x, y + layout.origin.y);
    }
}

public class Orientation {
    public readonly float f0, f1, f2, f3;
    public readonly float b0, b1, b2, b3;
    public readonly float startAngle;

    public Orientation(float f0_, float f1_, float f2_, float f3_, 
        float b0_, float b1_, float b2_, float b3_, 
        float startAngle_) 
    {
        f0 = f0_;
        f1 = f1_;
        f2 = f2_;
        f3 = f3_;
        b0 = b0_;
        b1 = b1_;
        b2 = b2_;
        b3 = b3_;
        startAngle = startAngle_;
    }

    public static Orientation pointy = new Orientation(
        Mathf.Sqrt(3f), Mathf.Sqrt(3f) / 2f, 0f, 3f / 2f,
        Mathf.Sqrt(3f) / 3f, -1f / 3f, 0f, 2f / 3f,
        0.5f);

    public static Orientation flat = new Orientation(
        3f / 2f, 0f, Mathf.Sqrt(3f) / 2f, Mathf.Sqrt(3f),
        2f / 3f, 0f, -1f / 3f, Mathf.Sqrt(3f) / 3f,
        0f);
}

public class Layout
{
    public readonly Orientation orientation;
    public Vector2 size;
    public Vector2 origin;

    public Layout (Orientation orientation_, Vector2 size_, Vector2 origin_)
    {
        orientation = orientation_;
        size = size_;
        origin = origin_;
    }
}

