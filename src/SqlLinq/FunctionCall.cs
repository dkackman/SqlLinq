namespace SqlLinq
{
    public sealed class FunctionCall
    {
        internal FunctionCall(int start, int end, string name, string args)
        {
            Start = start;
            End = end;
            Name = name;
            Args = args;
        }

        public int Start { get; }

        public int End { get; }

        public string Name { get; }

        public string Args { get; }
    }
}
