using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;

namespace Text_Blob_viseuliser
{
    class TextWriter
    {
        string symbols = "!ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";//defghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ.,;:/\\-_()\"\'";
        Texture2D[] punc;
        Texture2D text;
        const int space = 0;
        const int point = 1;
        const int comma = 2;
        const int openP = 3;
        const int closedP = 4;
        const int dPoint = 5;

        GraphicsBuffer buffer = new GraphicsBuffer();
        GraphicsBuffer sel = new GraphicsBuffer();

        Texture2D[] alphabet;
        public TextWriter(string imageFileLocation)
        {
            text = ContentPipe.LoadTexture("Alphabet/selected.png");
            punc = new Texture2D[6];
            alphabet = new Texture2D[symbols.Length];
            for (int i = 0; i < symbols.Length; i++)
            {
                alphabet[i] = ContentPipe.LoadTexture(string.Concat(imageFileLocation, symbols[i] + ".png"));
            }

            punc[space] = ContentPipe.LoadTexture(imageFileLocation + "\\Space.png");
            punc[point] = ContentPipe.LoadTexture(imageFileLocation + "\\Point.png");
            punc[comma] = ContentPipe.LoadTexture(imageFileLocation + "\\Comma.png");
            punc[openP] = ContentPipe.LoadTexture(imageFileLocation + "\\OpenParanthese.png");
            punc[closedP] = ContentPipe.LoadTexture(imageFileLocation + "\\ClosedParanthese.png");
            punc[dPoint] = ContentPipe.LoadTexture(imageFileLocation + "\\doublePoint.png");

            buffer.indexBuffer = new uint[4];
            buffer.vertBuffer = new Vertex[4];
            buffer.IBO = GL.GenBuffer();
            buffer.VBO = GL.GenBuffer();
            for (int i = 0; i < buffer.indexBuffer.Length; i++)
            {
                buffer.indexBuffer[i] = (uint)i;
            }
            buffer.vertBuffer[0] = new Vertex(new Vector2(0, 0), new Vector2(0, 0));
            buffer.vertBuffer[1] = new Vertex(new Vector2(1, 0), new Vector2(1, 0));
            buffer.vertBuffer[2] = new Vertex(new Vector2(1, 1), new Vector2(1, 1));
            buffer.vertBuffer[3] = new Vertex(new Vector2(0, 1), new Vector2(0, 1));
            BufferFill(buffer);

            sel.indexBuffer = new uint[4];
            sel.vertBuffer = new Vertex[4];
            sel.IBO = GL.GenBuffer();
            sel.VBO = GL.GenBuffer();
            for (int i = 0; i < sel.indexBuffer.Length; i++)
            {
                sel.indexBuffer[i] = (uint)i;
            }
            sel.vertBuffer[0] = new Vertex(new Vector2(0, 0), new Vector2(0, 0)) { Color = Color.Orange };
            sel.vertBuffer[1] = new Vertex(new Vector2(1, 0), new Vector2(1, 0)) { Color = Color.Orange };
            sel.vertBuffer[2] = new Vertex(new Vector2(1, 1), new Vector2(1, 1)) { Color = Color.Orange };
            sel.vertBuffer[3] = new Vertex(new Vector2(0, 1), new Vector2(0, 1)) { Color = Color.Orange };
            BufferFill(sel);
        }

        public int GetSymbolTextureID(char a)
        {
            int index = -1;
            if (a == ' ')
                return punc[space].ID;
            else if (a == '.')
                return punc[point].ID;
            else if (a == ',')
                return punc[comma].ID;
            else if (a == '(')
                return punc[openP].ID;
            else if (a == ')')
                return punc[closedP].ID;
            else if (a == ':')
                return punc[dPoint].ID;
            for (int i = 0; i < symbols.Length; i++)
            {
                a = a.ToString().ToUpper().ToCharArray()[0];
                if (symbols[i].Equals(a))
                {
                    index = i;
                    return alphabet[index].ID;
                }
            }
            return -1;
        }

        private string[] CutUpString(string text, int length)
        {
            int step = 0;
            List<string> res = new List<string>();
            while (step + length < text.Length)
            {
                res.Add(text.Substring(step, length));
                step += length;
            }
            res.Add(text.Substring(step));
            return res.ToArray<string>();
        }

        private string[] CutUpInterString(string text, int length)
        {
            string[] split = text.Split();
            bool finished = false;
            List<string> res = new List<string>();
            string inter = "";
            int counter = 0;
            while (!finished)
            {
                if (inter.Length + split[counter].Length <= length)
                {
                    inter += split[counter];
                    inter += " ";
                    counter++;
                }
                else
                {
                    if (inter.Equals(""))
                    {
                        inter = split[counter];
                        counter++;
                    }
                    res.Add(inter);
                    inter = "";
                }
                if (counter >= split.Length)
                {
                    res.Add(inter);
                    inter = "";
                    finished = true;
                }
            }
            return res.ToArray<string>();
        }
        public void WriteToScreen(Vector2 pos, string word, int windowWidth, int fontSize, bool selected = false)
        {
            float stepDown = fontSize * 1.5F;
            int length = windowWidth / fontSize;
            string[] splitWords = CutUpInterString(word, length);
            if(splitWords == null)
            {
                splitWords = new string[1];
                splitWords[0] = word;
            }


            if (selected)
            {

                //Load the vert and index buffers
                GL.BindBuffer(BufferTarget.ArrayBuffer, sel.VBO);
                GL.VertexPointer(2, VertexPointerType.Float, Vertex.SizeInBytes, (IntPtr)0);
                GL.TexCoordPointer(2, TexCoordPointerType.Float, Vertex.SizeInBytes, (IntPtr)(Vector2.SizeInBytes));
                GL.ColorPointer(4, ColorPointerType.Float, Vertex.SizeInBytes, (IntPtr)(Vector2.SizeInBytes * 2));
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, sel.IBO);

                GL.BindTexture(TextureTarget.Texture2D, text.ID);

                Matrix4 mat = Matrix4.CreateTranslation(pos.X, pos.Y, 0);  //Create a translation matrix
                GL.MatrixMode(MatrixMode.Modelview);    //Load the modelview matrix, last in the chain of view matrices
                GL.LoadMatrix(ref mat);                 //Load the translation matrix into the modelView matrix
                mat = Matrix4.CreateScale(fontSize * splitWords[0].Length, fontSize * splitWords.Length, 0);
                GL.MultMatrix(ref mat);
                GL.DrawElements(PrimitiveType.Quads, sel.indexBuffer.Length, DrawElementsType.UnsignedInt, 0);
            }
            //Load the vert and index buffers
            GL.BindBuffer(BufferTarget.ArrayBuffer, buffer.VBO);

            GL.VertexPointer(2, VertexPointerType.Float, Vertex.SizeInBytes, (IntPtr)0);
            GL.TexCoordPointer(2, TexCoordPointerType.Float, Vertex.SizeInBytes, (IntPtr)(Vector2.SizeInBytes));
            GL.ColorPointer(4, ColorPointerType.Float, Vertex.SizeInBytes, (IntPtr)(Vector2.SizeInBytes * 2));

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, buffer.IBO);


            for (int i = 0; i < splitWords.Length; i++)
            {
                for (int j = 0; j < splitWords[i].Length; j++)
                {
                    GL.BindTexture(TextureTarget.Texture2D, GetSymbolTextureID(splitWords[i][j]));
                    //Multiply the scale matrix with the modelview matrix
                    Matrix4 mat = Matrix4.CreateTranslation(pos.X + (j) * fontSize, pos.Y + i * stepDown, 0);  //Create a translation matrix
                    GL.MatrixMode(MatrixMode.Modelview);    //Load the modelview matrix, last in the chain of view matrices
                    GL.LoadMatrix(ref mat);                 //Load the translation matrix into the modelView matrix
                    mat = Matrix4.CreateScale(fontSize, fontSize, 0);
                    GL.MultMatrix(ref mat);
                    GL.DrawElements(PrimitiveType.Quads, buffer.indexBuffer.Length, DrawElementsType.UnsignedInt, 0);

                }
            }
        }


        private void BufferFill(GraphicsBuffer buf)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, buf.VBO);
            GL.BufferData<Vertex>(BufferTarget.ArrayBuffer, (IntPtr)(Vertex.SizeInBytes * buf.vertBuffer.Length), buf.vertBuffer, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, buf.IBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(sizeof(uint) * (buf.indexBuffer.Length)), buf.indexBuffer, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

        }
    }
}
