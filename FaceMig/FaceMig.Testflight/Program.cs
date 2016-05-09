using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FaceMig;

namespace FaceMig.Testflight
{
    class Program
    {
        static void Main(string[] args)
        {
            var facemig = new FaceMig();
            facemig.Initialize();
            facemig.Enabled();
            NativeBridge.ReOpenDevice(0);

            Console.WriteLine(@"press any key to next step");
            Console.ReadKey(false);

            for (var i = 0; i < 500; i++)
            {
                NativeBridge.Track();
                var status = NativeBridge.GetStatus();
                facemig.eyeL.Add(status.EyeL);
                facemig.eyeR.Add(status.EyeR);
                var eyeL = facemig.eyeL.Many();
                var eyeR = facemig.eyeR.Many();
                //var mouth = facemig.Limit(status.Mouth);
                //var lean = status.Lean;

                //Console.Clear();
                //Console.WriteLine(eyeL + "\t" + eyeR + "\t" + mouth + "\t" + lean);
                
                Thread.Sleep(50);
            }

            Console.WriteLine(@"press any key to exit");
            Console.ReadKey(false);

            facemig.Disabled();
            facemig.Dispose();
        }
    }
}
