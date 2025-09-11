using PurrNet.Modules;

namespace PurrNet.Packing
{
    public static class NormalizedFloatPacking
    {
        [UsedByIL]
        private static void WriteHalf(BitPacker packer, NormalizedFloat value)
        {
            PackingIntegers.WritePrefixed(packer, value.value, NormalizedFloat.BIT_RESOLUTION);
        }

        [UsedByIL]
        private static void ReadHalf(BitPacker packer, ref NormalizedFloat value)
        {
            PackingIntegers.ReadPrefixed(packer, ref value.value, NormalizedFloat.BIT_RESOLUTION);
        }

        [UsedByIL]
        private static bool WriteAngle(BitPacker packer, NormalizedFloat oldvalue, NormalizedFloat newvalue)
        {
            PackedLong delta = newvalue.value - oldvalue.value;

            if (delta == 0)
            {
                Packer<bool>.Write(packer, false);
                return false;
            }

            Packer<bool>.Write(packer, true);
            Packer<PackedLong>.Write(packer, delta);
            return true;
        }

        [UsedByIL]
        private static void ReadAngle(BitPacker packer, NormalizedFloat oldvalue, ref NormalizedFloat value)
        {
            bool hasChanged = default;
            Packer<bool>.Read(packer, ref hasChanged);

            if (!hasChanged)
            {
                value = oldvalue;
                return;
            }

            PackedLong delta = default;
            Packer<PackedLong>.Read(packer, ref delta);
            value.value = oldvalue.value + delta.value;
        }
    }
}
