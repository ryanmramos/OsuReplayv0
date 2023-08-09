using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace OsuReplayv0.ViewModel
{

    /* MMatrix (Math Matrix): Since Matrix class in C# is used moreso for visual geometrix transformations, making
     * own matrix class that will be able to do only what I need it to do. This includes:
     *  -   determinant
     *  -   retrieve minor of Matrix
     */
    internal class MMatrix
    {
        float[,] m;

        public MMatrix(float[,] _m)
        {
            m = _m;
        }


    }
}
