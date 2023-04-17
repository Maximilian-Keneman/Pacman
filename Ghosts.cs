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
        public Blinky(GameTable owner, Point escapeGoal, (int timer, Behaviour newBehaviour)[] events)
            : base(owner, escapeGoal, Behaviour.GetOut, events)
        {
            Animations = new()
            {
                {
                    Direction.None, new Image[1]
                    {
                        Images.GetFragment(Properties.Resources.Ghosts, new(new(5, 269), new(45, 49)), Bounds.Size.ToSize())
                    }
                },
                {
                    Direction.Up, new Image[2]
                    {
                        Images.GetFragment(Properties.Resources.Ghosts, new(new(5, 269), new(45, 49)), Bounds.Size.ToSize()),
                        Images.GetFragment(Properties.Resources.Ghosts, new(new(5, 313), new(45, 49)), Bounds.Size.ToSize())
                    }
                },
                {
                    Direction.Right, new Image[2]
                    {
                        Images.GetFragment(Properties.Resources.Ghosts, new(new(5, 5), new(45, 49)), Bounds.Size.ToSize()),
                        Images.GetFragment(Properties.Resources.Ghosts, new(new(5, 49), new(45, 49)), Bounds.Size.ToSize())
                    }
                },
                {
                    Direction.Down, new Image[2]
                    {
                        Images.GetFragment(Properties.Resources.Ghosts, new(new(5, 93), new(45, 49)), Bounds.Size.ToSize()),
                        Images.GetFragment(Properties.Resources.Ghosts, new(new(5, 137), new(45, 49)), Bounds.Size.ToSize())
                    }
                },
                {
                    Direction.Left, new Image[2]
                    {
                        Images.GetFragment(Properties.Resources.Ghosts, new(new(5, 181), new(45, 49)), Bounds.Size.ToSize()),
                        Images.GetFragment(Properties.Resources.Ghosts, new(new(5, 225), new(45, 49)), Bounds.Size.ToSize())
                    }
                }
            };
            Texture = Animations[Direction.None].Last();
            CurrentBehaviour = Behaviour.GetOut;
        }

        public override Point SetGoal((Point position, Direction direction) player)
        {
            return player.position;
        }
    }
    public class Pinky : Ghost
    {
        public Pinky(GameTable owner, Point escapeGoal, (int timer, Behaviour newBehaviour)[] events)
            : base(owner, escapeGoal, Behaviour.Wander, events)
        {
            Animations = new()
            {
                {
                    Direction.None, new Image[1]
                    {
                        Images.GetFragment(Properties.Resources.Ghosts, new(new(49, 269), new(45, 49)), Bounds.Size.ToSize())
                    }
                },
                {
                    Direction.Up, new Image[2]
                    {
                        Images.GetFragment(Properties.Resources.Ghosts, new(new(49, 269), new(45, 49)), Bounds.Size.ToSize()),
                        Images.GetFragment(Properties.Resources.Ghosts, new(new(49, 313), new(45, 49)), Bounds.Size.ToSize())
                    }
                },
                {
                    Direction.Right, new Image[2]
                    {
                        Images.GetFragment(Properties.Resources.Ghosts, new(new(49, 5), new(45, 49)), Bounds.Size.ToSize()),
                        Images.GetFragment(Properties.Resources.Ghosts, new(new(49, 49), new(45, 49)), Bounds.Size.ToSize())
                    }
                },
                {
                    Direction.Down, new Image[2]
                    {
                        Images.GetFragment(Properties.Resources.Ghosts, new(new(49, 93), new(45, 49)), Bounds.Size.ToSize()),
                        Images.GetFragment(Properties.Resources.Ghosts, new(new(49, 137), new(45, 49)), Bounds.Size.ToSize())
                    }
                },
                {
                    Direction.Left, new Image[2]
                    {
                        Images.GetFragment(Properties.Resources.Ghosts, new(new(49, 181), new(45, 49)), Bounds.Size.ToSize()),
                        Images.GetFragment(Properties.Resources.Ghosts, new(new(49, 225), new(45, 49)), Bounds.Size.ToSize())
                    }
                }
            };
            Texture = Animations[Direction.None].Last();
        }

        public override Point SetGoal((Point position, Direction direction) player)
        {
            return player.position + player.direction.ToSizeOrEmpty().Multiple(2);
        }
    }
    public class Inky : Ghost
    {
        private Blinky Blinky;
        public Inky(GameTable owner, Point escapeGoal, Blinky blinky, (int timer, Behaviour newBehaviour)[] events)
            : base(owner, escapeGoal, Behaviour.Wander, events)
        {
            Animations = new()
            {
                {
                    Direction.None, new Image[1]
                    {
                        Images.GetFragment(Properties.Resources.Ghosts, new(new(93, 269), new(45, 49)), Bounds.Size.ToSize())
                    }
                },
                {
                    Direction.Up, new Image[2]
                    {
                        Images.GetFragment(Properties.Resources.Ghosts, new(new(93, 269), new(45, 49)), Bounds.Size.ToSize()),
                        Images.GetFragment(Properties.Resources.Ghosts, new(new(93, 313), new(45, 49)), Bounds.Size.ToSize())
                    }
                },
                {
                    Direction.Right, new Image[2]
                    {
                        Images.GetFragment(Properties.Resources.Ghosts, new(new(93, 5), new(45, 49)), Bounds.Size.ToSize()),
                        Images.GetFragment(Properties.Resources.Ghosts, new(new(93, 49), new(45, 49)), Bounds.Size.ToSize())
                    }
                },
                {
                    Direction.Down, new Image[2]
                    {
                        Images.GetFragment(Properties.Resources.Ghosts, new(new(93, 93), new(45, 49)), Bounds.Size.ToSize()),
                        Images.GetFragment(Properties.Resources.Ghosts, new(new(93, 137), new(45, 49)), Bounds.Size.ToSize())
                    }
                },
                {
                    Direction.Left, new Image[2]
                    {
                        Images.GetFragment(Properties.Resources.Ghosts, new(new(93, 181), new(45, 49)), Bounds.Size.ToSize()),
                        Images.GetFragment(Properties.Resources.Ghosts, new(new(93, 225), new(45, 49)), Bounds.Size.ToSize())
                    }
                }
            };
            Texture = Animations[Direction.None].Last();
            Blinky = blinky;
        }

        public override Point SetGoal((Point position, Direction direction) player)
        {
            return Owner.GetPositionSector(Blinky.Center).position + ((Size)(player.position + player.direction.ToSizeOrEmpty() - (Size)Owner.GetPositionSector(Blinky.Center).position)).Multiple(2);
        }
    }
    public class Clyde : Ghost
    {
        public Clyde(GameTable owner, Point escapeGoal, (int timer, Behaviour newBehaviour)[] events)
            : base(owner, escapeGoal, Behaviour.Wander, events)
        {
            Animations = new()
            {
                {
                    Direction.None, new Image[1]
                    {
                        Images.GetFragment(Properties.Resources.Ghosts, new(new(137, 269), new(45, 49)), Bounds.Size.ToSize())
                    }
                },
                {
                    Direction.Up, new Image[2]
                    {
                        Images.GetFragment(Properties.Resources.Ghosts, new(new(137, 269), new(45, 49)), Bounds.Size.ToSize()),
                        Images.GetFragment(Properties.Resources.Ghosts, new(new(137, 313), new(45, 49)), Bounds.Size.ToSize())
                    }
                },
                {
                    Direction.Right, new Image[2]
                    {
                        Images.GetFragment(Properties.Resources.Ghosts, new(new(137, 5), new(45, 49)), Bounds.Size.ToSize()),
                        Images.GetFragment(Properties.Resources.Ghosts, new(new(137, 49), new(45, 49)), Bounds.Size.ToSize())
                    }
                },
                {
                    Direction.Down, new Image[2]
                    {
                        Images.GetFragment(Properties.Resources.Ghosts, new(new(137, 93), new(45, 49)), Bounds.Size.ToSize()),
                        Images.GetFragment(Properties.Resources.Ghosts, new(new(137, 137), new(45, 49)), Bounds.Size.ToSize())
                    }
                },
                {
                    Direction.Left, new Image[2]
                    {
                        Images.GetFragment(Properties.Resources.Ghosts, new(new(137, 181), new(45, 49)), Bounds.Size.ToSize()),
                        Images.GetFragment(Properties.Resources.Ghosts, new(new(137, 225), new(45, 49)), Bounds.Size.ToSize())
                    }
                }
            };
            Texture = Animations[Direction.None].Last();
        }

        public override Point SetGoal((Point position, Direction direction) player)
        {
            return ((Size)(player.position - (Size)Owner.GetPositionSector(Center).position)).Length() > 4 ?
                player.position : EscapeGoal;
        }
    }
}
