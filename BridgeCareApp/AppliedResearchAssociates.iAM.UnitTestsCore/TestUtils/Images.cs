using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppliedResearchAssociates.iAM.UnitTestsCore.TestUtils
{
    public static class Images
    {
        /// <summary>Image implements IDisposable. Caller should
        /// dispose, i.e. using var image = whatever;</summary> 
        public static Image Image(int width, int height, Color color)
        {
            var bitmap = new Bitmap(width, height, PixelFormat.Format64bppArgb);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    bitmap.SetPixel(x, y, color);
                }
            }
            return bitmap;
        }
    }
}
