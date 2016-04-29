using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Input;
using OpenTK;
using System.Drawing;
using OpenTK.Graphics.OpenGL;

namespace Text_Blob_viseuliser
{
    class Camera
    {
        //States to check keyboard key presses
        static KeyboardState key;
        static KeyboardState lastKey;

        //States to check Mouse movement and button press states
        static MouseState mouse;
        static MouseState lastMouse;

        //Camera position, rotation, and scale
        public static Vector3 cameraPos = new Vector3(0, 0, 0);
        public static Vector3 rot = new Vector3(0, 0, 0);
        public static Vector3 scale = new Vector3(1, 1, 0);
        //Camera movement speed
        static int step = 5;
        //Sprint speed of the camera
        static int booster = 1;

        //Position of the cursor on the screen
        public static Vector2 cursorPos = new Vector2(0, 0);
        //Size of the cursor
        static float cursorSize = 15;
        //Sensitivity of the mouse
        static double senX = 0.9;
        static double senY = 0.9;

        //Draw debug information
        public static bool DEBUGDraw = true;

        //Position that was last clicked
        static Vector2 lastClickPosition = new Vector2(-1, -1);
        //Position that was clicked now
        static Vector2 clickedPosition = new Vector2(-1, -1);
        //Which mouse has been clicked 1 Left, 2 Right
        static int WhichButtonPressed = -1;
        //Which button was last pressed
        static int lastButtonPressed = -1;

        //Copy of the window for each check
        static GameWindow window;

        //
        static public Vector2 GetClickPos
        {
            get { return lastClickPosition; }
        }

        /// <summary>
        /// Run when the window is loaded
        /// </summary>
        /// <param name="gw">Game window of the current session</param>
        /// <param name="curSize">Size of the cursor in the program</param>
        public static void SetupCamera(GameWindow gw, float curSize)
        {
            window = gw;
            cursorSize = curSize;
        }

        static bool end = true;
        static float scaleMover = 0.03F;
        /// <summary>
        /// Check if any of the buttons or keys have been checked
        /// </summary>
        public static void CheckPresses()
        {
            //Get the newest state of the keyboard
            key = Keyboard.GetState();

            //Check if the key Q is pressed and close the program if it is
            if (key.IsKeyDown(Key.Q))
                window.Close();

            //Check if the camera needs speeding up
            if (key.IsKeyDown(Key.ShiftRight) && lastKey.IsKeyUp(Key.ShiftRight))
                booster = 2;
            if (key.IsKeyUp(Key.ShiftRight) && lastKey.IsKeyDown(Key.ShiftRight))
                booster = 1;

            //Release the mouse from the cameras grip
            if (key.IsKeyDown(Key.C) && lastKey.IsKeyUp(Key.C))
                end = !end;
            if (key.IsKeyDown(Key.O) && lastKey.IsKeyUp(Key.O))
            {
                cameraPos = Vector3.Zero;
                scale = Vector3.One;
                rot = Vector3.Zero; 
            }


            //Move the Camera if the corrisponding button is pressed
            if (key.IsKeyDown(Key.Keypad5))
                cameraPos.Y -= step * booster;
            else if (key.IsKeyDown(Key.Keypad8))
                cameraPos.Y += step * booster;
            if (key.IsKeyDown(Key.Keypad4))
                cameraPos.X += step * booster;
            else if (key.IsKeyDown(Key.Keypad6))
                cameraPos.X -= step * booster;

            //Rotate the screen by 1 degree around the Z axis (Out of screen)
            if (key.IsKeyDown(Key.Keypad7))
                rot.Z += MathHelper.DegreesToRadians(1);
            else if (key.IsKeyDown(Key.Keypad9))
                rot.Z -= MathHelper.DegreesToRadians(1);

            //"Move" the camera towards/Away from the screen
            if (key.IsKeyDown(Key.Keypad1))
            {
                scale.X += scaleMover;
                scale.Y += scaleMover;
            }
            else if (key.IsKeyDown(Key.Keypad3))
            {
                scale.X -= scaleMover;
                scale.Y -= scaleMover;
            }

            //Activate debug drawing
            if (key.IsKeyDown(Key.D) && lastKey.IsKeyUp(Key.D))
            {
                DEBUGDraw = !DEBUGDraw;
            }

            //Set the
            lastKey = key;

            //Move the cursor by the distance that the mouse has moved from the center position that it is set to
            cursorPos.X += (float)((mouse.X - window.Width / 2) / senX);
            cursorPos.X = Constraint(cursorPos.X, 0, window.Width);
            cursorPos.Y += (float)((mouse.Y - window.Height / 2) / senY);
            cursorPos.Y = Constraint(cursorPos.Y, 0, window.Height);

            //Check the mouse button presses and set the cursor positions
            #region Mouse Button presses
            if ((mouse.IsButtonDown(MouseButton.Left) && lastMouse.IsButtonUp(MouseButton.Left)))
            {
                clickedPosition = new Vector2(cursorPos.X - cameraPos.X, cursorPos.Y - cameraPos.Y);
                WhichButtonPressed = 1;
            }
            else if (mouse.IsButtonDown(MouseButton.Right) && lastMouse.IsButtonUp(MouseButton.Right))
            {
                clickedPosition = new Vector2(cursorPos.X - cameraPos.X, cursorPos.Y - cameraPos.Y);
                WhichButtonPressed = 2;
            }
            else if (mouse.IsButtonUp(MouseButton.Right) && lastMouse.IsButtonDown(MouseButton.Right))
            {
                lastClickPosition = clickedPosition;
                clickedPosition = new Vector2(-1, -1);
                lastButtonPressed = WhichButtonPressed;
                WhichButtonPressed = -1;
            }
            else if (mouse.IsButtonUp(MouseButton.Left) && lastMouse.IsButtonDown(MouseButton.Left))
            {
                lastClickPosition = clickedPosition;
                clickedPosition = new Vector2(-1, -1);
                lastButtonPressed = WhichButtonPressed;
                WhichButtonPressed = -1;
            }
            #endregion

        }

        /// <summary>
        /// Limit val between 2 values high and low
        /// </summary>
        /// <param name="val">Value to limit</param>
        /// <param name="low">Lowest posible value</param>
        /// <param name="high">Highest posible value</param>
        /// <returns>The limited value if val \< low return low, </returns>
        private static float Constraint(float val, float low, float high)
        {
            return val > high ? high : val < low ? low : val;
        }


        /// <summary>
        /// Draw the cursor to the screen
        /// </summary>
        /// <param name="buf">Buffer holding the current graphic information</param>
        /// <param name="size">Size of the mouse cursor</param>
        /// <param name="outBuf">What the results need to be placed in</param>
        private static void DrawCursorPosition(GraphicsBuffer buf, float size, out GraphicsBuffer outBuf)
        {
            List<Vertex> vert = new List<Vertex>();
            vert.AddRange(buf.vertBuffer);
            List<uint> index = new List<uint>();
            index.AddRange(buf.indexBuffer);

            Vector2 vec = new Vector2(cursorPos.X - cameraPos.X, cursorPos.Y - cameraPos.Y);

            vert.Add(new Vertex(vec, new Vector2(0, 0)));
            vec.X += size;
            vert.Add(new Vertex(vec, new Vector2(1, 0)));
            vec.Y += size;
            vert.Add(new Vertex(vec, new Vector2(1, 1)));
            vec.X -= size;
            vert.Add(new Vertex(vec, new Vector2(0, 1)));

            for (int i = 4; i > 0; i--)
            {
                index.Add((uint)(vert.Count - i));
            }

            outBuf = new GraphicsBuffer();
            outBuf = buf;
            outBuf.vertBuffer = vert.ToArray<Vertex>();
            outBuf.indexBuffer = index.ToArray<uint>();
        }

        /// <summary>
        /// Draw where the mouse clicked 
        /// </summary>
        /// <param name="buf">Buffer holding the current graphic information</param>
        /// 
        /// <param name="outBuf">What the results need to be placed in</param>
        private static void DrawClickPositions(GraphicsBuffer buf, out GraphicsBuffer outBuf)
        {

            List<Vertex> vert = new List<Vertex>();
            vert.AddRange(buf.vertBuffer);
            List<uint> index = new List<uint>();
            index.AddRange(buf.indexBuffer);
            Vector2 v = clickedPosition;

            vert.Add(new Vertex(v, new Vector2(0, 0)));
            Vector2 vec = v;
            vec.X += 10;
            vert.Add(new Vertex(vec, new Vector2(0, 1)));
            vec.Y += 10;
            vert.Add(new Vertex(vec, new Vector2(1, 1)));
            vec.X -= 10;
            vert.Add(new Vertex(vec, new Vector2(1, 0)));

            for (int i = 4; i > 0; i--)
            {
                index.Add((uint)(vert.Count - i));
            }

            outBuf = new GraphicsBuffer();
            outBuf.vertBuffer = vert.ToArray<Vertex>();
            outBuf.indexBuffer = index.ToArray<uint>();
        }

        /// <summary>
        /// Move the camera by doing all the matrix math
        /// </summary>
        public static void MoveCamera()
        {
            Matrix4 proj = Matrix4.CreateOrthographicOffCenter(0, window.Width, window.Height, 0, 0, 1);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref proj);
            Matrix4 mat = Matrix4.CreateTranslation(cameraPos);
            GL.MultMatrix(ref mat);                 //Load the translation matrix into the modelView matrix
            mat = Matrix4.CreateScale(scale);
            GL.MultMatrix(ref mat);
            mat = Matrix4.CreateRotationX(rot.X);
            GL.MultMatrix(ref mat);
            mat = Matrix4.CreateRotationY(rot.Y);
            GL.MultMatrix(ref mat);
            mat = Matrix4.CreateRotationZ(rot.Z);
            GL.MultMatrix(ref mat);
        }

        public static bool HasClicked(MouseButton ms)
        {
            if (mouse.IsButtonDown(ms) && lastMouse.IsButtonUp(ms))
            {
                lastClickPosition = new Vector2(cursorPos.X, cursorPos.Y);
                return true;
            }
            else if (mouse.IsButtonUp(ms) && lastMouse.IsButtonUp(ms))
            {
                lastClickPosition = new Vector2(-1, -1);
            }
            return false;
        }

        private static void UpdateMouseState()
        {
            lastMouse = mouse;
            mouse = Mouse.GetCursorState();
            if (end)
                Mouse.SetPosition(window.Width / 2, window.Height / 2);
        }
        private static void UpdateKeyboardState()
        {
            lastKey = key;
            key = Keyboard.GetState();
        }

        public static GraphicsBuffer[] CameraUpdate()
        {
            GraphicsBuffer[] buffer = new GraphicsBuffer[2];
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i].vertBuffer = new Vertex[0];
                buffer[i].indexBuffer = new uint[0];
                buffer[i].IBO = GL.GenBuffer();
                buffer[i].VBO = GL.GenBuffer();
            }
            UpdateMouseState();
            UpdateKeyboardState();
            CheckPresses();
            DrawCursorPosition(buffer[0], cursorSize, out buffer[0]);
            DrawClickPositions(buffer[1], out buffer[1]);
            //MoveCamera();
            return buffer;
        }
    }
}
