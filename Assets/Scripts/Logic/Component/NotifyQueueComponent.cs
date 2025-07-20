

using System.Collections.Generic;
using OVFL.ECS;

namespace Minomino
{
    public class NotifyQueueComponent : IComponent
    {
        public Queue<Notify> Notifies = new Queue<Notify>();
    }

    public struct Notify
    {
        public NotifyType Type;
        public object PayLoad;
    }

    public enum NotifyType : ushort
    {
        None = 0,
        MinoStateChanged,
    }
}