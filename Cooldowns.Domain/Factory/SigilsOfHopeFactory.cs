using System;
using System.Collections.Generic;
using System.Drawing;
using Cooldowns.Domain.Status;
using Cooldowns.Domain.Timer;

namespace Cooldowns.Domain.Factory
{
    public class SigilsOfHopeFactory: ISigilsOfHopeFactory
    {
        private readonly IDispatcher dispatcher;
        private readonly IScreen screen;

        public SigilsOfHopeFactory(IDispatcher dispatcher, IScreen screen)
        {
            this.dispatcher = dispatcher;
            this.screen = screen;
        }

        public StatusChecker<SigilsOfHope> Create(ICooldownTimer timer, Action<SigilsOfHope> onChanged)
        {
            Fingerprint<SigilsOfHope> stateCheck = new(SigilsOfHope.None)
            {
                Points = new List<Point>
                {
                    new(1294, 1262),
                    new(1337, 1264),
                    new(1312, 1291),
                },
                Colors = new List<Color>
                {
                    Color.FromArgb(253, 249, 64),
                    Color.FromArgb(200, 210, 19),
                    Color.FromArgb(169, 166, 22),
                },
            };

            var stateValueChecks = new List<Fingerprint<SigilsOfHope>>
            {
                new(SigilsOfHope.One)
                {
                    Points = new List<Point>
                    {
                        new(1341, 1289),
                    },
                    Colors = new List<Color>
                    {
                        Color.FromArgb(57, 149, 45),
                    },
                },
                new(SigilsOfHope.Two)
                {
                    Points = new List<Point>
                    {
                        new(1341, 1289),
                    },
                    Colors = new List<Color>
                    {
                        Color.FromArgb(175, 175, 175),
                    },
                },
                new(SigilsOfHope.Three)
                {
                    Points = new List<Point>
                    {
                        new(1341, 1289),
                    },
                    Colors = new List<Color>
                    {
                        Color.FromArgb(194, 194, 194),
                    },
                },
                new(SigilsOfHope.Four)
                {
                    Points = new List<Point>
                    {
                        new(1341, 1289),
                    },
                    Colors = new List<Color>
                    {
                        Color.FromArgb(198, 198 ,198),
                    },

                    State = SigilsOfHope.Four,
                },
            };

            var statusCheckInfo = new StatusCheckInfo<SigilsOfHope>(SigilsOfHope.None, stateCheck, stateValueChecks);
            var checker = new StatusChecker<SigilsOfHope>(screen, dispatcher, timer, statusCheckInfo);

            checker.StatusChanged += (_, sigilState) => onChanged(sigilState);

            return checker;
        }
    }
}