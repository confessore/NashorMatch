using Discord.WebSocket;

namespace NashorMatch.Models
{
    public class Player
    {
        public Player(SocketGuild guild, SocketRole tier, SocketRole lane)
        {
            Guild = guild;
            Tier = tier;
            Lane = lane;
        }

        public Player(bool add, SocketGuild guild, SocketRole tier, SocketRole lane)
        {
            Add = add;
            Guild = guild;
            Tier = tier;
            Lane = lane;
        }

        public bool Add { get; }
        public SocketGuild Guild { get; }
        public SocketRole Tier { get; }
        public SocketRole Lane { get; }
    }
}
