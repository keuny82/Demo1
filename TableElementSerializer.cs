using System.Collections.Generic;

public class BitPacker
{
    List<uint> data = new List<uint>();

    public byte this[int index]
    {
        get
        {
            int arrayIndex = index >> 5; // remove lower 5 bits
            if (data.Count <= arrayIndex)
                return 0;

            uint val = data[arrayIndex];

            int bitIndex = index & 0x1F;
            uint bitMask = 1u << bitIndex;

            return (bitMask & val) == 0 ? (byte)0 : (byte)1;
        }
        set
        {
            int arrayIndex = index >> 5; // remove lower 5 bits

            while (data.Count <= arrayIndex)
            {
                data.Add(0);
            }

            uint val = data[arrayIndex];

            int bitIndex = index & 0x1F;
            uint bitMask = 1u << bitIndex;

            if (value != 0)
            {
                data[arrayIndex] = val | bitMask;
            }
            else
            {
                data[arrayIndex] = val & ~bitMask;
            }
        }
    }

    public void ReadFrom(System.IO.BinaryReader reader)
    {
        int count = reader.ReadByte();
        data.Clear();
        for (int i = 0; i < count; ++i)
        {
            uint val = reader.ReadUInt32();
            data.Add(val);
        }
    }

    public void WriteTo(System.IO.BinaryWriter writer)
    {
        writer.Write((byte)data.Count);
        for (int i = 0; i < data.Count; ++i)
        {
            writer.Write(data[i]);
        }
    }
}
public struct UIntPackerInfo
{
    public short index;
    public short offset;
    public ushort mask;
}
public class UIntPacker
{
    List<uint> data = new List<uint>();

    public const int MAX_3 = 2;	/// 2 bits
	public const int MAX_7 = 3;	/// 3 bits
	public const int MAX_15 = 4;	/// 4 bits
	public const int MAX_31 = 5;	/// 5 bits
	public const int MAX_63 = 6;	/// 6 bits
	public const int MAX_127 = 7;   /// 7 bits

    public static UIntPackerInfo Pack(int numBits)
    {
        UIntPackerInfo info = new UIntPackerInfo();
        info.index = 0;
        info.offset = 0;
        info.mask = (ushort)((1 << numBits) - 1);
        return info;
    }

    public static UIntPackerInfo Pack(int numBits, UIntPackerInfo lastInfo)
    {
        int temp = lastInfo.mask;
        int lastInfoBitCount = 0;
        while (temp != 0)
        {
            lastInfoBitCount++;
            temp = temp >> 1;
        }

        UIntPackerInfo info = new UIntPackerInfo();
        info.mask = (ushort)((1 << numBits) - 1);
        if (lastInfo.offset + lastInfoBitCount + numBits <= 32)
        {
            info.index = lastInfo.index;
            info.offset = (short)(lastInfo.offset + lastInfoBitCount);
        }
        else
        {
            info.index = (short)(lastInfo.index + 1);
            info.offset = 0;
        }
        return info;
    }

    public uint this[UIntPackerInfo info]
    {
        get
        {
            if (data.Count <= info.index)
                return 0;

            uint val = data[info.index];
            uint shiftval = val >> info.offset;
            uint maskval = shiftval & info.mask;
            return maskval;
        }
        set
        {
            while (data.Count <= info.index)
            {
                data.Add(0);
            }

            uint prevval = data[info.index];
            uint otherval = prevval & ~(((uint)info.mask) << info.offset);
            uint newval = (value & info.mask) << info.offset;
            data[info.index] = otherval | newval;
        }
    }

    public void ReadFrom(System.IO.BinaryReader reader)
    {
        int count = reader.ReadByte();
        data.Clear();
        for (int i = 0; i < count; ++i)
        {
            uint val = reader.ReadUInt32();
            data.Add(val);
        }
    }

    public void WriteTo(System.IO.BinaryWriter writer)
    {
        writer.Write((byte)data.Count);
        for (int i = 0; i < data.Count; ++i)
        {
            writer.Write(data[i]);
        }
    }
}

public struct FloatPackerInfo
{
    public short index;
    public short digit;
}

public class FloatPacker
{
    List<ushort> data = new List<ushort>();

    const int BITFLAG_SIGN = (1 << 8);

    public const int MAX_16384_4 = (16 - 14);   /// up to 16383.99
	public const int MAX_1023_64 = (16 - 10);	  /// up to 1023.999999 (2^10)
	public const int MAX_511_128 = (16 - 9);	  /// up to 511.99999999
	public const int MAX_255_256 = (16 - 8);      /// up to 255.999999999

    public const int MAX_SIGN_1023_32 = ((16 - 11) | BITFLAG_SIGN);	/// up to 1023.99 (2^10, 1 sign bit)
	public const int MAX_SIGN_511_64 = ((16 - 10) | BITFLAG_SIGN);  /// up to 511.99 (2^10, 1 sign bit)

    public const int MAX_DEFAULT = MAX_511_128;

    public static FloatPackerInfo Pack(int index)
    {
        return Pack(index, MAX_DEFAULT);
    }

    public static FloatPackerInfo Pack(int index, int precision)
    {
        FloatPackerInfo info = new FloatPackerInfo();
        info.index = (short)index;
        info.digit = (short)precision;
        return info;
    }

    public float this[FloatPackerInfo info]
    {
        get
        {
            if (data.Count <= info.index)
                return 0;

            int to_shift = (~BITFLAG_SIGN & info.digit);
            int div = 1 << to_shift;
            float f;

            if ((info.digit & BITFLAG_SIGN) != 0)
            {
                short val = unchecked((short)data[info.index]);
                f = val / ((float)div);
            }
            else
            {
                ushort val = data[info.index];
                f = val / ((float)div);
            }

            //UnityEngine.MonoBehaviour.print("short " + val + ", div " + div + " -> float " + f);

            return f;
        }
        set
        {
            while (data.Count <= info.index)
            {
                data.Add(0);
            }

            int to_shift = (~BITFLAG_SIGN & info.digit);
            int div = 1 << to_shift;

            if ((info.digit & BITFLAG_SIGN) != 0)
            {
                short s = (short)(value * div);
                unchecked
                {
                    data[info.index] = (ushort)s;
                }
            }
            else
            {
                ushort s = (ushort)(value * div);
                data[info.index] = s;
            }

			float saved = this[info];
			float diff = System.Math.Abs(saved - value);
			if (diff > 1 / 30.0f) // min precision : 1/32
			{
				string digitstr;
				switch (info.digit)
				{
					case FloatPacker.MAX_1023_64:
						digitstr = "MAX_1023_64";
						break;
					case FloatPacker.MAX_511_128:
						digitstr = "MAX_511_128";
						break;
					case FloatPacker.MAX_255_256:
						digitstr = "MAX_255_256";
						break;
					case FloatPacker.MAX_SIGN_1023_32:
						digitstr = "MAX_SIGN_1023_32";
						break;
					case FloatPacker.MAX_SIGN_511_64:
						digitstr = "MAX_SIGN_511_64";
						break;
					default:
						digitstr = "Unknown";
						break;
				}
				if (info.digit == FloatPacker.MAX_DEFAULT)
					digitstr += " (MAX_DEFAULT)";

				// UnityEngine.Debug.LogError("ERROR : FloatPacker value is out of range. value " + value + ", index " + info.index + ", precision " + digitstr + ".");
			}

            //UnityEngine.MonoBehaviour.print("short " + s + " <- div " + div + ", float " + value);
        }
    }

    public void ReadFrom(System.IO.BinaryReader reader)
    {
        int count = reader.ReadByte();
        data.Clear();
        for (int i = 0; i < count; ++i)
        {
            ushort val = reader.ReadUInt16();
            data.Add(val);
        }
    }

    public void WriteTo(System.IO.BinaryWriter writer)
    {
        writer.Write((byte)data.Count);
        for (int i = 0; i < data.Count; ++i)
        {
            writer.Write(data[i]);
        }
    }
}


public interface TableElementSerializer
{
    void Bind(ref string outval, string columnName);
    void Bind(ref short outval, string columnName);
    void Bind(ref ushort outval, string columnName);
    void Bind(ref int outval, string columnName);
    void Bind(ref uint outval, string columnName);
    void Bind(ref long outval, string columnName);
    void Bind(ref float outval, string columnName);
    void Bind(ref byte outval, string columnName);
    void Bind(ref bool outval, string columnName);

    //void Bind(ref ByteCompactor outval);
    //void Bind(ref ByteCompactor outval, int index, string columnName);
    //void Bind(ref UIntCompactor outval, int index, string columnName);
    //void Bind(ref ByteCompactor outval, int index, string columnName);
    void Bind(ref BitPacker outval);
    void Bind(ref UIntPacker outval);
    void Bind(ref FloatPacker outval);

    void Bind(ref BitPacker outval, int index, string columnName);
    void Bind(ref UIntPacker outval, UIntPackerInfo info, string columnName);
    void Bind(ref FloatPacker outval, FloatPackerInfo info, string columnName);

    bool IsBinaryLoading();
}

public interface TableElement
{
    void Serialize(TableElementSerializer helper);
}