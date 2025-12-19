
namespace KS.Benchmark.SmartFox2X
{
    /// <summary>
    /// SFSObject field names. These are all 1 character to minimize bandwidth.
    /// </summary>
    public static class sfVars
    {
        public const string POS_X = "x";
        public const string POS_Y = "y";
        public const string POS_Z = "z";
        public const string Q0 = "0";
        public const string Q1 = "1";
        public const string Q2 = "2";
        public const string Q3 = "3";
        public const string SCALE_X = "a";
        public const string SCALE_Y = "b";
        public const string SCALE_Z = "c";
        public const string PREFAB = "p";
    }

    /// <summary>SFSObject room variable id prefixes. These are all 1 character to minimize bandwidth.</summary>
    public static class sfIdPrefix
    {
        // Prefix for room variable SFSObjects containing immutable spawn data.
        public const string SPAWN = "s";
        // Prefix for room variable SFSObjects containing variable update data.
        public const string UPDATE = "u";
    }
}