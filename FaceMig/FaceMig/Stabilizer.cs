using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceMig
{
    public class Stabilizer : List<float>
    {
        public Stabilizer(int num) : base(num) { }

        public new void Add(float num)
        {
            if (base.Count == base.Capacity)
            {
                base.RemoveAt(0);
            }

            base.Add(num);
        }

        public float Average()
        {
            return this.Sum() / base.Count;
        }

        public float MovingAverage()
        {
            var num = 0F;
            var denom = 0F;
            for (var i = 0; i < this.Count; i++)
            {
                num += this[i] * (i + 1);
                denom += (i + 1);
            }
            return num / denom;
        }

        public float Many()
        {

            return (this.Sum() < (this.Count >> 1)) ? 0 : 1;
        }

        float stabilized = 0;
        public float Stabilize()
        {
            var result = 0;
            if (this.Count == 1)
            {
                return this[0];
            }
            else
            {
                if (Math.Abs(this[this.Count - 1] - stabilized) < 0.05)
                {
                    return this[this.Count - 1];
                }
                else
                {
                    return stabilized;
                }
            }

            return result;
        }
    }
}
