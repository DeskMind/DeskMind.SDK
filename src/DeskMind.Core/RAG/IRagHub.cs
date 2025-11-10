using System.Collections.Generic;

namespace DeskMind.Core.RAG
{
    public interface IRagHub
    {
        IReadOnlyCollection<string> Sources { get; }

        IRagSource Get(string name);

        bool TryGet(string name, out IRagSource? source);

        void Register(string name, IRagSource source);
    }
}