using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.IO;

namespace Text_Blob_viseuliser
{
    struct Vertex
    {
        public Vector2 position;
        public Vector2 texCoord;
        public Vector4 color;

        public Color Color
        {
            get
            {
                return Color.FromArgb((int)(255 * color.W), (int)(255 * color.X), (int)(255 * color.Y), (int)(255 * color.Z));
            }
            set
            {
                this.color = new Vector4(value.R / 255f, value.G / 255f, value.B / 255f, value.A / 255f);
            }

        }
        static public int SizeInBytes
        {
            get { return Vector2.SizeInBytes * 2 + Vector4.SizeInBytes; }
        }

        public Vertex(Vector2 position, Vector2 texCoord)
        {
            this.position = position;
            this.texCoord = texCoord;
            this.color = new Vector4(1, 1, 1, 1);
        }


    }

    struct Edge
    {
        public Vector3 e1;
        public Vector3 e2;
        public string se1;
        public string se2;

        public Edge(Vector3 edge1, Vector3 edge2, string sEdge1, string sEdge2)
        {
            e1 = edge1;
            e2 = edge2;
            se1 = sEdge1;
            se2 = sEdge2;
        }
    }
    class Game
    {
        public GameWindow window;
        Texture2D texture;
        Texture2D edgeTex;

        TextWriter textWriter;
        StreamReader sr = new StreamReader("Content/text.txt");
        Dictionary<string, Vector3> dict = new Dictionary<string, Vector3>();
        Dictionary<string, int> wordCounter = new Dictionary<string, int>();
        List<string> foundWords = new List<string>();
        List<Edge> edges = new List<Edge>();
        Random rand = new Random();

        //Start of the vertex buffer
        GraphicsBuffer blobBuffer = new GraphicsBuffer();
        GraphicsBuffer edgeBuffer = new GraphicsBuffer();
        GraphicsBuffer[] CursorBuf;

        //Size of the blobs Pre count
        int size = 20;

        //Number of lines counter
        int counterOfAll = 0;


        public Game(GameWindow windowInput)
        {
            window = windowInput;

            window.Load += Window_Load;
            window.RenderFrame += Window_RenderFrame;
            window.UpdateFrame += Window_UpdateFrame;
            window.Closing += Window_Closing;
            Camera.SetupCamera(window, 30);
            textWriter = new TextWriter("Alphabet/");

            window.CursorVisible = false;
        }


        private void Window_Load(object sender, EventArgs e)
        {
            texture = ContentPipe.LoadTexture("placeholder.png");
            edgeTex = ContentPipe.LoadTexture("edge.png");
            blobBuffer.vertBuffer = new Vertex[4]
            {
                new Vertex(new Vector2(0, 0), new Vector2(0, 0)),
                new Vertex(new Vector2(0, 1), new Vector2(0, 1)),
                new Vertex(new Vector2(1, 1), new Vector2(1, 1)),
                new Vertex(new Vector2(1, 0), new Vector2(1, 0))
            };
            edgeBuffer.VBO = GL.GenBuffer();
            edgeBuffer.IBO = GL.GenBuffer();

            blobBuffer.VBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, blobBuffer.VBO);
            GL.BufferData<Vertex>(BufferTarget.ArrayBuffer, (IntPtr)(Vertex.SizeInBytes * blobBuffer.vertBuffer.Length), blobBuffer.vertBuffer, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            blobBuffer.indexBuffer = new uint[4]
            {
                0,1,2,3
            };

            blobBuffer.IBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, blobBuffer.IBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(sizeof(uint) * (blobBuffer.indexBuffer.Length)), blobBuffer.indexBuffer, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        private void BufferFill(GraphicsBuffer buf)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, buf.VBO);
            GL.BufferData<Vertex>(BufferTarget.ArrayBuffer, (IntPtr)(Vertex.SizeInBytes * buf.vertBuffer.Length), buf.vertBuffer, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, buf.IBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(sizeof(uint) * (buf.indexBuffer.Length)), buf.indexBuffer, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        string text = "";
        int worldScale = 10;
        private void Window_UpdateFrame(object sender, FrameEventArgs e)
        {
            CursorBuf = Camera.CameraUpdate();
            foreach (GraphicsBuffer b in CursorBuf)
            {
                BufferFill(b);
            }

            text = sr.ReadLine();
            //Not end of the text
            if (text != null)
            {
                counterOfAll++;
                text = text.ToUpper();
                string[] words = text.Split();  //Get all the seperate words
                if (!words[0].Equals("") && !foundWords.Contains(words[0])) //Put the first one in the dictionary if it isn't already
                {
                    //Add the word to the word list and add the words corresponding blob pos to the dict
                    foundWords.Add(words[0]);
                    wordCounter.Add(words[0], 0);
                    dict.Add(words[0], new Vector3(rand.Next(0, window.Width * worldScale), rand.Next(0, window.Height * worldScale), 0));
                }
                else if (!words[0].Equals("") && foundWords.Contains(words[0]))
                {
                    wordCounter[words[0]]++;
                }
                //Same thing for the rest of the words
                for (int i = 1; i < words.Length; i++)
                {
                    if (!words[i].Equals("") && !foundWords.Contains(words[i]))
                    {
                        foundWords.Add(words[i]);
                        wordCounter.Add(words[i], 0);
                        dict.Add(words[i], new Vector3(rand.Next(0, window.Width * worldScale), rand.Next(0, window.Height * worldScale), 0));
                    }
                    else if (!words[i].Equals("") && foundWords.Contains(words[i]))
                    {
                        wordCounter[words[i]]++;
                    }
                    if (!words[i].Equals("") && !words[i - 1].Equals(""))
                    {
                        //The array stores the edge
                        Vector3[] edge = new Vector3[2];
                        //Get the current words pos and the word just before it's pos
                        dict.TryGetValue(words[i], out edge[0]);
                        edge[0].X += size / 2;
                        edge[0].Y += size / 2;
                        dict.TryGetValue(words[i - 1], out edge[1]);
                        edge[1].X += size / 2;
                        edge[1].Y += size / 2;
                        Edge edgeStruct = new Edge(edge[0], edge[1], words[i], words[i - 1]);
                        //Create an edge between these 2 blobs if it doesn't exist already
                        if (!edges.Contains(edgeStruct))
                        {
                            edges.Add(edgeStruct);
                        }
                    }
                }
                //Now I have all the blobs and all the edges.
                //Let's transform them into buffer objects
                blobBuffer.vertBuffer = new Vertex[foundWords.Count * 4];
                blobBuffer.indexBuffer = new uint[foundWords.Count * 4];

                edgeBuffer.vertBuffer = new Vertex[edges.Count * 4];
                edgeBuffer.indexBuffer = new uint[edges.Count * 4];

                for (int i = 0; i < foundWords.Count * 4; i += 4)
                {
                    Vector2 vec = dict[foundWords[i / 4]].Xy;
                    int wordCountValue = wordCounter[foundWords[i / 4]];
                    blobBuffer.vertBuffer[i] = new Vertex(vec, new Vector2(0, 0));
                    vec.Y += size + wordCountValue;
                    blobBuffer.vertBuffer[i + 1] = new Vertex(vec, new Vector2(0, 1));
                    vec.X += size + wordCountValue;
                    blobBuffer.vertBuffer[i + 2] = new Vertex(vec, new Vector2(1, 1));
                    vec.Y -= size + wordCountValue;
                    blobBuffer.vertBuffer[i + 3] = new Vertex(vec, new Vector2(1, 0));

                    blobBuffer.indexBuffer[i] = (uint)i;
                    blobBuffer.indexBuffer[i + 1] = (uint)i + 1;
                    blobBuffer.indexBuffer[i + 2] = (uint)i + 2;
                    blobBuffer.indexBuffer[i + 3] = (uint)i + 3;
                }
                for (int i = 0; i < edges.Count * 4; i += 4)
                {
                    Vector2 e1 = edges[i / 4].e1.Xy;
                    Vector2 e2 = edges[i / 4].e2.Xy;
                    Vector2 scaler1 = e2 - e1;
                    scaler1.Normalize();
                    scaler1 = scaler1.PerpendicularRight * size / 2;

                    Vector2 scaler2 = e2 - e1;
                    scaler2.Normalize();
                    scaler2 = scaler2.PerpendicularRight * size / 2;

                    e1.X += wordCounter[edges[i / 4].se1] / 2;
                    e1.Y += wordCounter[edges[i / 4].se1] / 2;

                    e2.X += wordCounter[edges[i / 4].se2] / 2;
                    e2.Y += wordCounter[edges[i / 4].se2] / 2;

                    edgeBuffer.vertBuffer[i] = new Vertex(e1, new Vector2(0, 0));
                    edgeBuffer.vertBuffer[i + 1] = new Vertex(e2, new Vector2(0, 1));
                    e2 += scaler2;
                    edgeBuffer.vertBuffer[i + 2] = new Vertex(e2, new Vector2(1, 1));
                    e1 += scaler1;
                    edgeBuffer.vertBuffer[i + 3] = new Vertex(e1, new Vector2(1, 0));

                    edgeBuffer.indexBuffer[i] = (uint)i;
                    edgeBuffer.indexBuffer[i + 1] = (uint)i + 1;
                    edgeBuffer.indexBuffer[i + 2] = (uint)i + 2;
                    edgeBuffer.indexBuffer[i + 3] = (uint)i + 3;
                }
                BufferFill(edgeBuffer);
                BufferFill(blobBuffer);
            }
            else
            {
                text = "Done";
            }
        }

        private void Window_RenderFrame(object sender, FrameEventArgs e)
        {
            Camera.MoveCamera();
            if (text != null)
            {
                //Clear screen color
                GL.ClearColor(Color.Black);
                GL.Clear(ClearBufferMask.ColorBufferBit);

                //Enable color blending, which allows transparency
                GL.Enable(EnableCap.Blend);
                GL.Enable(EnableCap.Texture2D);
                //Blending everything for transparency
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

                //Create the projection matrix for the scene
                //Matrix4 proj = Matrix4.CreateOrthographicOffCenter(0, window.Width, window.Height, 0, 0, 1);
                //GL.MatrixMode(MatrixMode.Projection);
                //GL.LoadMatrix(ref proj);



                //Enable all the different arrays
                GL.EnableClientState(ArrayCap.ColorArray);
                GL.EnableClientState(ArrayCap.VertexArray);
                GL.EnableClientState(ArrayCap.TextureCoordArray);

                textWriter.WriteToScreen(new Vector2(0, 30), counterOfAll.ToString(), window.Width, 20);
                textWriter.WriteToScreen(new Vector2(0, 60), text, window.Width, 20);

                //Bind the texture that will be used
                GL.BindTexture(TextureTarget.Texture2D, edgeTex.ID);



                GL.BindBuffer(BufferTarget.ArrayBuffer, edgeBuffer.VBO);
                GL.VertexPointer(2, VertexPointerType.Float, Vertex.SizeInBytes, (IntPtr)0);
                GL.TexCoordPointer(2, TexCoordPointerType.Float, Vertex.SizeInBytes, (IntPtr)(Vector2.SizeInBytes));
                GL.ColorPointer(4, ColorPointerType.Float, Vertex.SizeInBytes, (IntPtr)(Vector2.SizeInBytes * 2));
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, edgeBuffer.IBO);

                //Create a scale matrux
                Matrix4 mat = Matrix4.CreateTranslation(0, 0, 0);   //Create a translation matrix
                GL.MatrixMode(MatrixMode.Modelview);                //Load the modelview matrix, last in the chain of view matrices
                GL.LoadMatrix(ref mat);                             //Load the translation matrix into the modelView matrix
                mat = Matrix4.CreateScale(1, 1, 0);                 //Create a scale matrix
                GL.MultMatrix(ref mat);                              //Multiply the scale matrix with the modelview matrix
                GL.PushMatrix();
                GL.DrawElements(PrimitiveType.Quads, edgeBuffer.indexBuffer.Length, DrawElementsType.UnsignedInt, 0);
                GL.PopMatrix();

                GL.BindTexture(TextureTarget.Texture2D, texture.ID);
                //Load the vert and index buffers
                GL.BindBuffer(BufferTarget.ArrayBuffer, blobBuffer.VBO);
                GL.VertexPointer(2, VertexPointerType.Float, Vertex.SizeInBytes, (IntPtr)0);
                GL.TexCoordPointer(2, TexCoordPointerType.Float, Vertex.SizeInBytes, (IntPtr)(Vector2.SizeInBytes));
                GL.ColorPointer(4, ColorPointerType.Float, Vertex.SizeInBytes, (IntPtr)(Vector2.SizeInBytes * 2));
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, blobBuffer.IBO);

                //Create a scale matrux
                Matrix4 mat2 = Matrix4.CreateTranslation(0, 0, 0);   //Create a translation matrix
                GL.MatrixMode(MatrixMode.Modelview);                //Load the modelview matrix, last in the chain of view matrices
                GL.LoadMatrix(ref mat2);                             //Load the translation matrix into the modelView matrix
                mat2 = Matrix4.CreateScale(1, 1, 0);                 //Create a scale matrix
                GL.MultMatrix(ref mat2);                              //Multiply the scale matrix with the modelview matrix
                GL.PushMatrix();
                GL.DrawElements(PrimitiveType.Quads, blobBuffer.indexBuffer.Length, DrawElementsType.UnsignedInt, 0);
                GL.PopMatrix();

                for (int i = 0; i < foundWords.Count; i++)
                {
                    int wordCountValue = wordCounter[foundWords[i]];
                    Vector2 vec = dict[foundWords[i]].Xy;
                    vec.X += (size + wordCountValue - (size + wordCountValue) / 2) / 2;
                    vec.Y += (size + wordCountValue - 10) / 2;
                    textWriter.WriteToScreen(vec, foundWords[i], size + wordCountValue, wordCountValue > 5 ? wordCountValue / 5 : 5);
                }

                //Flush everything 
                GL.Flush();
                //Write the new buffer to the screen
                window.SwapBuffers();
            }
        }
    }
}
