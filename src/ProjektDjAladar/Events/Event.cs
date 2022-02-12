using Microsoft.Extensions.Logging;

namespace ProjektDjAladar
{
    public abstract class Event
    {
        protected readonly EventId BotEventId = new(42, "ProjektDjAladar");
    }
}