using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Text_Blob_viseuliser
{
    class Program
    {
        static void Main(string[] args)
        {
            GameWindow window = new GameWindow(1388, 768, OpenTK.Graphics.GraphicsMode.Default, "This is the title", GameWindowFlags.Fullscreen);
            Game game = new Game(window);

            window.Run();
        }
    }
}
