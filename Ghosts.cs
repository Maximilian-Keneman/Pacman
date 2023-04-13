using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pacman
{
    public class Blinky : Ghost
    {
        public Blinky(GameTable owner, Point escapeGoal) : base(owner, escapeGoal, Color.Red, Behaviour.GetOut)
        {
            CurrentBehaviour = Behaviour.GetOut;
        }

        public override Point SetGoal((Point position, Direction direction) player)
        {
            return player.position;
        }
    }
    public class Pinky : Ghost
    {
        public Pinky(GameTable owner, Point escapeGoal) : base(owner, escapeGoal, Color.Pink, Behaviour.Wander)
        { }

        public override Point SetGoal((Point position, Direction direction) player)
        {
            return player.position + player.direction.ToSizeOrEmpty().Multiple(2);
        }
    }
    public class Inky : Ghost
    {
        private Blinky Blinky;
        public Inky(GameTable owner, Point escapeGoal, Blinky blinky) : base(owner, escapeGoal, Color.Blue, Behaviour.Wander)
        {
            Blinky = blinky;
        }

        public override Point SetGoal((Point position, Direction direction) player)
        {
            return Owner.GetPositionSector(Blinky.Center).position + ((Size)(player.position + player.direction.ToSizeOrEmpty() - (Size)Owner.GetPositionSector(Blinky.Center).position)).Multiple(2);
        }
    }
    public class Clyde : Ghost
    {
        public Clyde(GameTable owner, Point escapeGoal) : base(owner, escapeGoal, Color.Orange, Behaviour.Wander)
        { }

        public override Point SetGoal((Point position, Direction direction) player)
        {
            return ((Size)(player.position - (Size)Owner.GetPositionSector(Center).position)).Length() > 4 ?
                player.position : EscapeGoal;
        }
    }
}
