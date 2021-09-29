using System;
using System.Collections.Generic;
using System.Drawing;
using Cooldowns.Domain;
using Cooldowns.Domain.Buttons;
using Cooldowns.Domain.Status;
using Cooldowns.Domain.Timer;

namespace Cooldowns.Factory
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
            StatusCheckInfo<SigilsOfHope> statusCheckInfo = new(SigilsOfHope.None, new List<Fingerprint<SigilsOfHope>>
            {
                new(SigilsOfHope.One)
                {
                    Points = new List<Point>
                    {
                        new(1294, 1262),
                        new(1335, 1255),
                        new(1324, 1296),

                        new(1341, 1290),
                        // new(1335, 1304),
                        // new(1342, 1304),
                    },
                    Colors = new List<Color>
                    {
                        Color.FromArgb(253, 249, 64),
                        Color.FromArgb(206, 209, 32),
                        Color.FromArgb(222, 231, 47),

                        Color.FromArgb(54, 141, 42),
                        //Color.FromArgb(96, 65, 49),
                        //Color.FromArgb(98, 63, 45),
                    },
                },
                new(SigilsOfHope.Two)
                {
                    Points = new List<Point>
                    {
                        new(1294, 1262),
                        new(1335, 1255),
                        new(1324, 1296),

                        new(1341, 1290),
                        // new(1335, 1304),
                        // new(1342, 1304),
                    },
                    Colors = new List<Color>
                    {
                        Color.FromArgb(253, 249, 64),
                        Color.FromArgb(206, 209, 32),
                        Color.FromArgb(222, 231, 47),

                        Color.FromArgb(210, 210, 210),
                        //Color.FromArgb(210, 210, 210),
                        //Color.FromArgb(214, 214, 214),
                    },
                },
                new(SigilsOfHope.Three)
                {
                    Points = new List<Point>
                    {
                        new(1294, 1262),
                        new(1335, 1255),
                        new(1324, 1296),

                        new(1341, 1290),
                        // new(1335, 1304),
                        // new(1342, 1304),
                    },
                    Colors = new List<Color>
                    {
                        Color.FromArgb(253, 249, 64),
                        Color.FromArgb(206, 209, 32),
                        Color.FromArgb(222, 231, 47),

                        Color.FromArgb(202, 202, 202),
                        //Color.FromArgb(210, 210, 210),
                        //Color.FromArgb(214, 214, 214),
                    },
                },
                new(SigilsOfHope.Four)
                {
                    Points = new List<Point>
                    {
                        new(1294, 1262),
                        new(1335, 1255),
                        new(1324, 1296),

                        new(1341, 1290),
                        // new(1335, 1304),
                        // new(1342, 1304),
                    },
                    Colors = new List<Color>
                    {
                        Color.FromArgb(253, 249, 64),
                        Color.FromArgb(206, 209, 32),
                        Color.FromArgb(222, 231, 47),

                        Color.FromArgb(200, 200, 200),
                        //Color.FromArgb(210, 210, 210),
                        //Color.FromArgb(214, 214, 214),
                    },

                    State = SigilsOfHope.Four,
                },
                // new()
                // {
                //     Points = new List<Point>
                //     {
                //         new(1294, 1262),
                //         new(1335, 1255),
                //         new(1324, 1296),
                //     },
                //     Colors = new List<Color>
                //     {
                //         Color.FromArgb(253, 249, 64),
                //         Color.FromArgb(206, 209, 32),
                //         Color.FromArgb(222, 231, 47),
                //     },
                //
                //     State = SigilsOfHope.None,
                // },
            });

            StatusChecker<SigilsOfHope> checker = new(screen, dispatcher, timer, statusCheckInfo);
            checker.StatusChanged += (_, sigilState) => onChanged(sigilState);

            return checker;
        }
    }
}