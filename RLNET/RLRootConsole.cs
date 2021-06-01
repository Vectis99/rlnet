#region license
/* 
 * Released under the MIT License (MIT)
 * Copyright (c) 2014 Travis M. Clark
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to 
 * deal in the Software without restriction, including without limitation the 
 * rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
 * sell copies of the Software, and to permit persons to whom the Software is 
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in 
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
 */
#endregion

using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using System.IO;

namespace RLNET
{
    public class RLRootConsole : RLConsole
    {
        private const string VertexPath = "Shaders/vs.glsl";
        private const string FragmentPath = "Shaders/fs.glsl";
        private GameWindow window;
        private bool closed = false;

        #region Original GL
        private uint texId;
        private uint vboId; //position bo
        private uint iboId; //index bo
        private uint tcboId; //tex coord bo
        private uint foreColorId; //color3 bo
        private uint backColorId; //color3 bo
        private Vector2[] texVertices;
        private Vector3[] colorVertices;
        private Vector3[] backColorVertices;
        private float uRatio;
        private float vRatio;
        private int charsPerRow;

        private float scale;
        private Vector3 scale3;
        private int charWidth;
        private int charHeight;
        private RLResizeType resizeType;
        private int offsetX;
        private int offsetY;
        #endregion

        #region New GL
        /// <summary> The shader to draw tiles with. </summary>
        Shader shader;

        uint VertexArrayObject;
        #endregion

        public event UpdateEventHandler Render;
        public event UpdateEventHandler Update;
        public event EventHandler OnLoad;
        public event EventHandler<System.ComponentModel.CancelEventArgs> OnClosing;
        public event ResizeEventHandler OnResize;

        public RLKeyboard Keyboard { get; private set; }
        public RLMouse Mouse { get; private set; }

        /// <summary>
        /// Creates the root console.
        /// </summary>
        /// <param name="bitmapFile">Path to bitmap file.</param>
        /// <param name="width">Width in characters of the console.</param>
        /// <param name="height">Height in characters of the console.</param>
        /// <param name="charWidth">Width of the characters.</param>
        /// <param name="charHeight">Height of the characters.</param>
        /// <param name="scale">Scale the window by this value.</param>
        /// <param name="title">Title of the window.</param>
        public RLRootConsole(string bitmapFile, int width, int height, int charWidth, int charHeight, float scale = 1f, string title = "RLNET Console")
            : base(width, height)
        {
            RLSettings settings = new RLSettings();
            settings.BitmapFile = bitmapFile;
            settings.Width = width;
            settings.Height = height;
            settings.CharWidth = charWidth;
            settings.CharHeight = charHeight;
            settings.Scale = scale;
            settings.Title = title;

            Init(settings);
        }

        /// <summary>
        /// Creates the root console.
        /// </summary>
        /// <param name="settings">Settings for the RLRootConsole.</param>
        public RLRootConsole(RLSettings settings)
            : base(settings.Width, settings.Height)
        {
            Init(settings);
        }

        private void Init(RLSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException("settings");
            if (settings.BitmapFile == null)
                throw new ArgumentNullException("BitmapFile");
            if (settings.Title == null)
                throw new ArgumentNullException("Title");
            if (settings.Width <= 0)
                throw new ArgumentException("Width cannot be zero or less");
            if (settings.Height <= 0)
                throw new ArgumentException("Height cannot be zero or less");
            if (settings.CharWidth <= 0)
                throw new ArgumentException("CharWidth cannot be zero or less");
            if (settings.CharHeight <= 0)
                throw new ArgumentException("CharHeight cannot be zero or less");
            if (settings.Scale <= 0)
                throw new ArgumentException("Scale cannot be zero or less");
            if (System.IO.File.Exists(settings.BitmapFile) == false)
                throw new System.IO.FileNotFoundException("cannot find bitmapFile");



            this.scale = settings.Scale;
            this.scale3 = new Vector3(scale, scale, 1f);
            this.charWidth = settings.CharWidth;
            this.charHeight = settings.CharHeight;
            this.resizeType = settings.ResizeType;

            //window = new GameWindow((int)(settings.Width * charWidth * scale), (int)(settings.Height * charHeight * scale), GraphicsMode.Default);
            //following four lines are an adhoc replacement for the above
            GameWindowSettings gws = new GameWindowSettings();
            gws.RenderFrequency = 30d; // Seems to be implied by a later call.
            gws.UpdateFrequency = 30d;
            NativeWindowSettings nws = new NativeWindowSettings();
            nws.Size = new Vector2i((int)(settings.Width * charWidth * scale), (int)(settings.Height * charHeight * scale));
            window = new GameWindow(gws, nws);

            window.WindowBorder = (WindowBorder)settings.WindowBorder;
            if (settings.StartWindowState == RLWindowState.Fullscreen || settings.StartWindowState == RLWindowState.Maximized)
            {
                window.WindowState = (WindowState)settings.StartWindowState;
            }

            window.Title = settings.Title;
            window.RenderFrame += window_RenderFrame;
            window.UpdateFrame += window_UpdateFrame;
            window.Load += window_Load;
            window.Resize += window_Resize;
            window.Closed += window_Closed;
            window.Closing += window_Closing;
            Mouse = new RLMouse(window);
            Keyboard = new RLKeyboard(window);

            InitGL(settings);
        }

        /// <summary>
        /// Initalize the OpenGL context of the application.
        /// </summary>
        /// <param name="settings"></param>
        private void InitGL(RLSettings settings)
        {
            // 1. Load the shader.
            shader = new Shader(VertexPath, FragmentPath);

            
            // 2. Instantiate the Vertex Array Object (VAO)
            VertexArrayObject = GL.GenVertexArray();
            // 3. Load the glyph texture file.
            LoadTexture2d(settings.BitmapFile);
            vboId = GL.GenBuffer();
            iboId = GL.GenBuffer();
            tcboId = GL.GenBuffer();
            foreColorId = GL.GenBuffer();
            backColorId = GL.GenBuffer();
            // 6. Generate a window (performs additional GL initalization!)
            CalcWindow(true);
        }

        void window_Load()
        {
            EventArgs e = new EventArgs();
            window.VSync = VSyncMode.On;
            if (OnLoad != null) OnLoad(this, e);
        }

        void window_Closing(System.ComponentModel.CancelEventArgs e)
        {
            // Might be a better place to put this:
            shader.Dispose();
            if (OnClosing != null) OnClosing(this, e);
        }

        /// <summary>
        /// Resizes the window.
        /// </summary>
        /// <param name="width">The new width of the window, in pixels.</param>
        /// <param name="height">The new height of the window, in pixels.</param>
        public void ResizeWindow(int width, int height)
        {
            window.Size = new Vector2i(width, height);
        }

        public void SetWindowState(RLWindowState windowState)
        {
            window.WindowState = (WindowState)windowState;
        }

        public void LoadBitmap(string bitmapFile, int charWidth, int charHeight)
        {
            if (bitmapFile == null)
                throw new ArgumentNullException("bitmapFile");
            if (charWidth <= 0)
                throw new ArgumentException("charWidth cannot be zero or less");
            if (charHeight <= 0)
                throw new ArgumentException("charHeight cannot be zero or less");

            if (this.charWidth != charWidth || this.charHeight != charHeight)
            {
                this.charWidth = charWidth;
                this.charHeight = charHeight;

                if (resizeType == RLResizeType.ResizeCells || window.WindowState == WindowState.Fullscreen || window.WindowState == WindowState.Maximized)
                {
                    CalcWindow(false);
                }
                else
                {
                    int viewWidth = (int)(Width * charWidth * scale);
                    int viewHeight = (int)(Height * charHeight * scale);

                    if (viewWidth != window.Size.X || viewHeight != window.Size.Y)
                    {
                        ResizeWindow(viewWidth, viewHeight);
                    }
                    else
                    {
                        CalcWindow(false);
                    }
                }
            }
            LoadTexture2d(bitmapFile);
        }

        public void Close()
        {
            window.Close();
        }

        void window_Resize(OpenTK.Windowing.Common.ResizeEventArgs e)
        {
            CalcWindow(false);
        }

        private void CalcWindow(bool startup)
        {
            if (resizeType == RLResizeType.None)
            {
                int viewWidth = (int)(Width * charWidth * scale);
                int viewHeight = (int)(Height * charHeight * scale);
                int newOffsetX = (window.Size.X - viewWidth) / 2;
                int newOffsetY = (window.Size.Y - viewHeight) / 2;

                if (startup || offsetX != newOffsetX || offsetY != newOffsetY)
                {
                    offsetX = newOffsetX;
                    offsetY = newOffsetY;
                    Mouse.Calibrate(charWidth, charHeight, offsetX, offsetY, scale);
                    GL.Viewport(offsetX, offsetY, viewWidth, viewHeight);
                }

                if (startup)
                {
                    CreateBuffers(Width, Height);
                }
            }
            else if (resizeType == RLResizeType.ResizeCells)
            {
                int width = window.Size.X / charWidth;
                int height = window.Size.Y / charHeight;

                if (startup || width != Width || height != Height)
                {
                    Resize(width, height);
                    CreateBuffers(width, height);
                    GL.Viewport(0, 0, (int)(Width * charWidth * scale), (int)(Height * charHeight * scale));
                    Mouse.Calibrate(charWidth, charHeight, offsetX, offsetY, scale);
                    if (OnResize != null) OnResize(this, new ResizeEventArgs(width, height));
                }
            }
            else if (resizeType == RLResizeType.ResizeScale)
            {
                float newScale = Math.Min(window.Size.X / (charWidth * Width), window.Size.Y / (charHeight * Height));
                int viewWidth = (int)(Width * charWidth * newScale);
                int viewHeight = (int)(Height * charHeight * newScale);
                int newOffsetX = (window.Size.X - viewWidth) / 2;
                int newOffsetY = (window.Size.Y - viewHeight) / 2;

                if (startup || newScale != scale || offsetX != newOffsetX || offsetY != newOffsetY)
                {
                    offsetX = newOffsetX;
                    offsetY = newOffsetY;
                    scale = newScale;
                    scale3 = new Vector3(scale, scale, 1);
                    Mouse.Calibrate(charWidth, charHeight, offsetX, offsetY, scale);
                    GL.Viewport(offsetX, offsetY, viewWidth, viewHeight);
                }

                if (startup)
                {
                    CreateBuffers(Width, Height);
                }
            }
        }

        ~RLRootConsole()
        {
            if (window != null)
                window.Dispose();

            if (!closed)
            {
                // TODO: Might not be the right way to delete buffers according to: https://opentk.net/learn/chapter1/2-hello-triangle.html
                GL.DeleteBuffer(vboId);
                GL.DeleteBuffer(iboId);
                GL.DeleteBuffer(tcboId);
                GL.DeleteBuffer(foreColorId);
                GL.DeleteBuffer(backColorId);
                GL.DeleteTexture(texId);
            }
        }

        /// <summary>
        /// Populate all OpenGL buffers with dummy data.
        /// </summary>
        /// <param name="width">The width of the canvas.</param>
        /// <param name="height">The height of the canvas.</param>
        private void CreateBuffers(int width, int height)
        {
            /*The following buffer-related functions had to be updated to the new method*/
            Vector2[] vertices = CreateVertices(width, height, charWidth, charHeight);
            GL.BindBuffer(BufferTargetARB.ArrayBuffer, vboId);
            GL.BufferData<Vector2>(BufferTargetARB.ArrayBuffer, vertices, BufferUsageARB.StaticDraw);

            texVertices = new Vector2[width * height * 4];
            GL.BindBuffer(BufferTargetARB.ArrayBuffer, tcboId);
            GL.BufferData(BufferTargetARB.ArrayBuffer, texVertices, BufferUsageARB.DynamicDraw);

            colorVertices = new Vector3[width * height * 4];
            GL.BindBuffer(BufferTargetARB.ArrayBuffer, foreColorId);
            GL.BufferData(BufferTargetARB.ArrayBuffer, colorVertices, BufferUsageARB.DynamicDraw);

            backColorVertices = new Vector3[width * height * 4];
            GL.BindBuffer(BufferTargetARB.ArrayBuffer, backColorId);
            GL.BufferData(BufferTargetARB.ArrayBuffer, backColorVertices, BufferUsageARB.DynamicDraw);

            uint[] indices = CreateIndices(width * height);
            GL.BindBuffer(BufferTargetARB.ElementArrayBuffer, iboId);
            GL.BufferData(BufferTargetARB.ElementArrayBuffer, indices, BufferUsageARB.StaticDraw);

            // Unbinding:
            GL.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
            GL.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
        }

        /// <summary>
        /// Opens the window and begins the game loop.
        /// </summary>
        public void Run()
        {
            window.Run();
        }

        /// <summary>
        /// Gets or sets the title of the window.
        /// </summary>
        public string Title
        {
            set
            {
                window.Title = value;
            }
            get
            {
                return window.Title;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the mouse cursor is visible.
        /// </summary>
        public bool CursorVisible
        {
            set
            {
                window.CursorVisible = value;
            }
            get
            {
                return window.CursorVisible;
            }
        }

        /// <summary>
        /// Returns whether the window has been closed.
        /// </summary>
        /// <returns></returns>
        public bool IsWindowClosed()
        {
            return closed;
        }

        private void window_UpdateFrame(FrameEventArgs e)
        {
            if (Update != null)
                Update(this, new UpdateEventArgs(e.Time));
        }


        private void window_RenderFrame(FrameEventArgs e)
        {
            if (Render != null)
                Render(this, new UpdateEventArgs(e.Time));
        }

        private void window_Closed()
        {
            closed = true;

            // TODO: Might not be the right way to delete buffers according to: https://opentk.net/learn/chapter1/2-hello-triangle.html
            // Either that, or I'm redundant here with elsewhere, which seems more likely.
            /* GL.DeleteBuffer(vboId);
            GL.DeleteBuffer(iboId);
            GL.DeleteBuffer(tcboId);
            GL.DeleteBuffer(foreColorId);
            GL.DeleteBuffer(backColorId);
            GL.DeleteTexture(texId);*/
        }

        /// <summary>
        /// Draws the root console to the window.
        /// </summary>
        /// <remarks>
        /// This method had to be heavily revisited in the conversion to the .NET 5.0 version of OpenTK.
        /// This is largely due to a revisiting of new openGL principles. <see href="http://neokabuto.blogspot.com/2013/03/opentk-tutorial-2-drawing-triangle.html"/>.
        /// While working on this, I transitioned to a later tutorial which uses the latest specification: <see href="https://opentk.net/learn/chapter1/2-hello-triangle.html"/>.
        /// Handy boilerplate code reference: https://opentk.net/learn/chapter1/4-shaders.html
        /// </remarks>
        public void Draw()
        {
            CellsToVertices();

            //Clear
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            // This GL.ClearColor overload doesn't exist in OpenTK 5.0 pre-5: GL.ClearColor(Color.Black);
            GL.ClearColor(Color.Red.R, Color.Red.G, Color.Red.B, Color.Black.A); // This only needs to be called once; it sets the values referred to by "GL.Clear" for every subsequent call.

            #region Static Pipeline Projection Setting
            //Set Projection
            // The old static method (bad):
            /*
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, Width * charWidth * scale, Height * charHeight * scale, 0, -1, 1);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            */
            // ^ However, I would like to verify that I have an appropriate substitute for the above. Seems I don't...
            #endregion

            // Orthographic Projection:
            Matrix4 model = Matrix4.CreateOrthographicOffCenter(0, Width * charWidth * scale, Height * charHeight * scale, 0, -1, 1);
            shader.SetMatrix4("model", model);
            shader.SetMatrix4("view", Matrix4.Identity);
            shader.SetMatrix4("projection", Matrix4.Identity);


            //Setup States
            GL.Enable(EnableCap.VertexArray);
            GL.Enable(EnableCap.IndexArray);
            GL.Enable(EnableCap.ColorArray);
            GL.Enable(EnableCap.Blend);
            GL.Disable(EnableCap.Lighting);
            GL.Disable(EnableCap.DepthTest);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.BindTexture(TextureTarget.Texture2d, texId);

            #region Half-broken Static Pipeline
            /*
            // The following four lines aren't a part of the latest OpenGL.
            //GL.EnableClientState(ArrayCap.VertexArray);
            //GL.EnableClientState(ArrayCap.IndexArray);
            //GL.EnableClientState(ArrayCap.ColorArray);
            //GL.Scale(scale3);

            // In the following section, I believe the reason we don't often have to call GL.BufferData
            // is because this is handled in the method CellsToVertices();

            // GL.BufferData() is called in CreateBuffers(). This might not be right.
            //VBO (Vertex Buffer Object)
            //Vertex Buffer
            GL.BindVertexArray(VertexArrayObject);
            GL.BindBuffer(BufferTargetARB.ArrayBuffer, vboId);
            // Originally: GL.VertexPointer(2, VertexPointerType.Float, 2 * sizeof(float), 0);
            // uint index refers to vs.glsl's contents (layout(location = 0)).
            // To be determined: Attributes "size" and "stride".
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0); // TODO: Should this be 2 * sizeof(float) or 3 * sizeof(float)?
            GL.EnableVertexAttribArray(0);
            shader.Use();
            
            //Index Buffer
            GL.BindBuffer(BufferTargetARB.ElementArrayBuffer, iboId);

            //Back Color Draw
            //Color Buffer
            GL.BindBuffer(BufferTargetARB.ArrayBuffer, backColorId);
            // TODO: Can't find a replacement for GL.ColorPointer(3, ColorPointerType.Float, 3 * sizeof(float), 0);
            GL.BufferData(BufferTargetARB.ArrayBuffer, backColorVertices, BufferUsageARB.DynamicDraw);
            //Draw Back Color
            GL.DrawElements(PrimitiveType.Triangles, Width * Height * 6, DrawElementsType.UnsignedInt, 0); // why *6?

            //Fore Color / Texture Draw
            //Texture Coord Buffer
            GL.Enable(EnableCap.Texture2d);
            GL.BindBuffer(BufferTargetARB.ArrayBuffer, tcboId);
            // TODO: Can't find a replacement for: GL.TexCoordPointer(2, TexCoordPointerType.Float, 2 * sizeof(float), 0);
            GL.BufferData(BufferTargetARB.ArrayBuffer, texVertices, BufferUsageARB.DynamicDraw);
            //Color Buffer
            GL.BindBuffer(BufferTargetARB.ArrayBuffer, foreColorId);
            // TODO: Can't find a replacement for: GL.ColorPointer(3, ColorPointerType.Float, 3 * sizeof(float), 0);
            GL.BufferData(BufferTargetARB.ArrayBuffer, colorVertices, BufferUsageARB.DynamicDraw);
            //Draw
            GL.DrawElements(PrimitiveType.Triangles, Width * Height * 6, DrawElementsType.UnsignedInt, 0);

            // Draw???
            // The following doesn't seem to do anything:
            // GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

            //Clean Up
            GL.Disable(EnableCap.Texture2d);
            // This is obsolete in this OpenGL version, however it may be safe to remove because
            // it is tied to something else commented out:
            // GL.DisableClientState(ArrayCap.VertexArray);
            // GL.DisableClientState(ArrayCap.TextureCoordArray);
            // GL.DisableClientState(ArrayCap.IndexArray);
            // GL.DisableClientState(ArrayCap.ColorArray);
            GL.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
            GL.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);*/
            #endregion

            GL.BindVertexArray(VertexArrayObject);
            GL.BindBuffer(BufferTargetARB.ArrayBuffer, vboId);
            #region Triangle 
            // Sloppy test code.
            /*Vector2[] vertices = {
                new Vector2(-0.5f, -0.5f), //Bottom-left vertex
                new Vector2(0.5f, -0.5f), //Bottom-right vertex
                new Vector2(0.0f,  0.5f) //Top vertex
            };
            GL.BufferData(BufferTargetARB.ArrayBuffer, vertices, BufferUsageARB.StaticDraw);*/
            /*Vector2[] bigTriangle = {
                new Vector2(20, 20), //Bottom-left vertex
                new Vector2(40, 20), //Bottom-right vertex
                new Vector2(30, 40) //Top vertex
            };
            GL.BufferData(BufferTargetARB.ArrayBuffer, bigTriangle, BufferUsageARB.StaticDraw);*/
            float[] texturedRectancle ={
                128, 128, 1, 1,
                128, 16, 1, 0,
                16, 16, 0, 0,
                16, 128, 0, 1
            };
            GL.BufferData(BufferTargetARB.ArrayBuffer, texturedRectancle, BufferUsageARB.StaticDraw);

            uint[] texturedRectangleIndices =
            {
                0, 1, 3,
                1, 2, 3
            };
            GL.BindBuffer(BufferTargetARB.ElementArrayBuffer, iboId);
            GL.BufferData(BufferTargetARB.ElementArrayBuffer, texturedRectangleIndices, BufferUsageARB.StaticDraw);

            #endregion
            // Array management 

            int vertexShaderStride = 4 * sizeof(float);

            // TODO: Get attribute location should be called once instead of every render frame.
            uint vertexCoordinateAttributeLocation = shader.GetAttribLocation("aPosition");
            GL.EnableVertexAttribArray(vertexCoordinateAttributeLocation);
            GL.VertexAttribPointer(vertexCoordinateAttributeLocation, 2, VertexAttribPointerType.Float, false, vertexShaderStride, 0);

            uint textureCoordinateAttributeLocation = shader.GetAttribLocation("aTexCoord");
            GL.EnableVertexAttribArray(textureCoordinateAttributeLocation);
            GL.VertexAttribPointer(textureCoordinateAttributeLocation, 2, VertexAttribPointerType.Float, false, vertexShaderStride, 2 * sizeof(float));

            GL.BindVertexArray(VertexArrayObject);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2d, texId);
            shader.Use();
            // Old: GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
            // New?: GL.DrawElements(PrimitiveType.Triangles, texturedRectangleIndices.Length, DrawElementsType.UnsignedInt, 0);
            GL.DrawElements(PrimitiveType.Triangles, texturedRectangleIndices.Length, DrawElementsType.UnsignedInt, 0);

            window.SwapBuffers();
        }

        private void CellsToVertices()
        {
            float charU = (float)charWidth * uRatio;
            float charV = (float)charHeight * vRatio;

            //Convert cells to vertices
            for (int iy = 0; iy < Height; iy++)
            {
                for (int ix = 0; ix < Width; ix++)
                {
                    //1 -- 0
                    //2 -- 3
                    int i = (ix + (iy * Width)) * 4;

                    int character = cells[ix, iy].character;

                    float charX = (float)(character % charsPerRow) * charU;
                    float charY = (float)(character / charsPerRow) * charV;

                    texVertices[i].X = charX + charU;
                    texVertices[i].Y = charY;
                    texVertices[i + 1].X = charX;
                    texVertices[i + 1].Y = charY;
                    texVertices[i + 2].X = charX;
                    texVertices[i + 2].Y = charY + charV;
                    texVertices[i + 3].X = charX + charU;
                    texVertices[i + 3].Y = charY + charV;

                    Vector3 backColor = cells[ix, iy].backColor.ToVector3();
                    backColorVertices[i] = backColor;
                    backColorVertices[i + 1] = backColor;
                    backColorVertices[i + 2] = backColor;
                    backColorVertices[i + 3] = backColor;

                    Vector3 color = cells[ix, iy].color.ToVector3();
                    colorVertices[i] = color;
                    colorVertices[i + 1] = color;
                    colorVertices[i + 2] = color;
                    colorVertices[i + 3] = color;
                }
            }
        }

        private uint[] CreateIndices(int cellCount)
        {
            uint[] indices = new uint[cellCount * 6];

            for (uint i = 0; i < cellCount; i++)
            {
                uint iv = (uint)(i * 4);
                uint ii = (uint)(i * 6);
                indices[ii] = iv;
                indices[ii + 1] = (uint)(iv + 1);
                indices[ii + 2] = (uint)(iv + 2);
                indices[ii + 3] = (uint)(iv + 2);
                indices[ii + 4] = (uint)(iv + 3);
                indices[ii + 5] = iv;
            }

            return indices;
        }

        private Vector2[] CreateVertices(int width, int height, int charWidth, int charHeight)
        {
            Vector2[] vertices = new Vector2[width * height * 4];
            for (int iy = 0; iy < height; iy++)
            {
                for (int ix = 0; ix < width; ix++)
                {
                    int i = (ix + (iy * width)) * 4;

                    //1 -- 0
                    //2 -- 3

                    int x = ix * charWidth;
                    int y = iy * charHeight;

                    vertices[i].X = x + charWidth;
                    vertices[i].Y = y;

                    vertices[i + 1].X = x;
                    vertices[i + 1].Y = y;

                    vertices[i + 2].X = x;
                    vertices[i + 2].Y = y + charHeight;

                    vertices[i + 3].X = x + charWidth;
                    vertices[i + 3].Y = y + charHeight;
                }
            }
            return vertices;
        }

        /// <summary>
        /// Loads a texture file with alpha = 0 for one pallete color and alpha = 1 for all others.
        /// </summary>
        /// <param name="filename"></param>
        private void LoadTexture2d(string filename)
        {
            if (texId != 0) GL.DeleteTexture(texId);
            texId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2d, texId);

            // In order to use Bitmap in .NET 5.0, you need System.Drawing.Common
            Bitmap bmp = new Bitmap(filename);
            BitmapData bmpData = bmp.LockBits(
                new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            charsPerRow = bmp.Width / charWidth;
            uRatio = 1f / (float)bmp.Width;
            vRatio = 1f / (float)bmp.Height;

            //Set Alpha
            IntPtr ptr = bmpData.Scan0;
            int bytes = Math.Abs(bmpData.Stride) * bmp.Height;
            byte[] rgbValues = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);
            int r = rgbValues[0];
            int b = rgbValues[1];
            int g = rgbValues[2];
            for (int i = 0; i < rgbValues.Length; i += 4)
            {
                if (rgbValues[i] == r && rgbValues[i + 1] == b && rgbValues[i + 2] == g)
                    rgbValues[i + 3] = 0;
                // rgbValues[i] = (byte)((i + 25) % 255);
                // rgbValues[i + 1] = (byte)((i + 100) % 255);
                // rgbValues[i + 2] = (byte)((i + 10) % 255);
                // rgbValues[i + 3] = 1;
            }

            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);

            //Create Texture
            GL.TexImage2D(TextureTarget.Texture2d, 0, (int)InternalFormat.Rgba, bmpData.Width, bmpData.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmpData.Scan0);
            bmp.UnlockBits(bmpData);

            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        }
    }
}
