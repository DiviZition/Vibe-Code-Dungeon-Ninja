using System.Collections.Generic;
using Zenject;

namespace Core
{
    public class SimulationTicker : ITickable
    {
        private readonly List<ITickable> _tickables = new();
        private readonly List<ITickable> _toAdd = new();
        private readonly List<ITickable> _toRemove = new();

        public void Tick()
        {
            if (_toAdd.Count > 0)
            {
                _tickables.AddRange(_toAdd);
                _toAdd.Clear();
            }

            if (_toRemove.Count > 0)
            {
                foreach (var tickable in _toRemove)
                    _tickables.Remove(tickable);
                _toRemove.Clear();
            }

            for (int i = 0; i < _tickables.Count; i++)
                _tickables[i].Tick();
        }

        public void Register(ITickable tickable) => _toAdd.Add(tickable);
        public void Unregister(ITickable tickable) => _toRemove.Add(tickable);
    }
}