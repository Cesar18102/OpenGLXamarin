using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

/*using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.ES30;
using OpenTK.Platform;*/

using Android.Opengl;
using Android.Views;
using Android.Content;
using Android.Util;
using J = Java.Nio;

namespace OpenTKTest
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        int rotateSide = 1;

        private Random Rand = new Random();

        private int program = 0;
        private float scale = 1;
        private float red = 1;

        private int[] VAO = new int[1];
        private int[] VBO = new int[1];
        private int[] IBO = new int[1];

        private static float[] vertices = {
            0f, 0f, 0f,
            0.95f, 0f, 0f,
            -0.95f, 0f, 0f,
            -0.95f, 0.95f, 0f,
            -0.95f, -0.95f, 0f,
            0.95f, 0.95f, 0f,
            0.95f, -0.95f, 0f
        };

        private static byte[] inds = {
            3, 4, 5,
            4, 5, 6
        };

        private static float[] mat = {
            1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1
        };

        private static Matrix world = Matrix.FromArray<float>(mat) as Matrix;

        private J.Buffer buffer = J.ByteBuffer.AllocateDirect(vertices.Length * sizeof(float)).
                                               Order(J.ByteOrder.NativeOrder()).AsFloatBuffer().
                                               Put(vertices).Position(0);

        private J.Buffer ibuffer = J.ByteBuffer.AllocateDirect(inds.Length * sizeof(int)).
                                                Order(J.ByteOrder.NativeOrder()).
                                                Put(inds).Position(0);


        public MainPage()
        {
            InitializeComponent();
            this.Appearing += MainPage_Appearing;
        }

        private void MainPage_Appearing(object sender, EventArgs e)
        {
            GLView.OnDisplay = Render;
        }

        private void Render(Rectangle R)
        {
            if (program == 0)
            {
                program = PreloadShaders();

                GLES32.GlEnable(GLES32.GlVertexArray);

                GLES32.GlGenVertexArrays(1, VAO, 0);
                GLES32.GlBindVertexArray(VAO[0]);

                GLES32.GlGenBuffers(1, VBO, 0);
                GLES32.GlBindBuffer(GLES32.GlArrayBuffer, VBO[0]);
                GLES32.GlBufferData(GLES32.GlArrayBuffer, vertices.Length * sizeof(float), buffer, GLES32.GlStaticDraw);

                int posloc = GLES32.GlGetAttribLocation(program, "Position");
                GLES32.GlVertexAttribPointer(posloc, 3, GLES32.GlFloat, false, 0, 0);
                GLES32.GlEnableVertexAttribArray(posloc);

                GLES32.GlGenBuffers(1, IBO, 0);
                GLES32.GlBindBuffer(GLES32.GlElementArrayBuffer, IBO[0]);
                GLES32.GlBufferData(GLES32.GlElementArrayBuffer, inds.Length * sizeof(byte), ibuffer, GLES32.GlStaticDraw);

                GLES32.GlBindBuffer(GLES32.GlElementArrayBuffer, 0);
                GLES32.GlBindBuffer(GLES32.GlArrayBuffer, 0);
                GLES32.GlBindVertexArray(0);

                GLES32.GlUseProgram(program);
            }

            GLES32.GlClearColor(0, 0, 0, 1);
            GLES32.GlClear(GLES32.GlColorBufferBit | GLES32.GlDepthBufferBit);

            GLES32.GlViewport(0, 0, (int)(GLView.Width * 3), (int)(GLView.Height * 3));

            GLES32.GlLineWidth(50);

            int scaleLocation = GLES32.GlGetUniformLocation(program, "gScale");
            //scale = (scale + 0.005f) % 1;
            GLES32.GlUniform1f(scaleLocation, scale);

            /*int colorLocation = GLES32.GlGetUniformLocation(program, "color");
            red = (float)((red + 0.005f) % (2.0 * Math.PI));
            GLES32.GlUniform4f(colorLocation, (float)Math.Abs(Math.Sin(red)), 0, 0, 1);*/

            int worldLocation = GLES32.GlGetUniformLocation(program, "world");

            //Matrix.TranslateM(mat, 0, 0f, 0f, 0f);
            Matrix.RotateM(mat, 0, 1 * rotateSide, 0, 0, 1);
            //Matrix.TranslateM(mat, 0, 0f, 0f, 0f);

            GLES32.GlUniformMatrix4fv(worldLocation, 1, true, mat, 0);

            GLES32.GlBindVertexArray(VAO[0]);
            GLES32.GlBindBuffer(GLES32.GlElementArrayBuffer, IBO[0]);
            GLES32.GlDrawElements(GLES32.GlTriangles, 6, GLES32.GlUnsignedByte, 0);

            //GLES32.GlBindBuffer(GLES32.GlArrayBuffer, VBO[0]);
            //GLES32.GlDrawArrays(GLES32.GlTriangleFan, 0, 7);

            GLES32.GlFinish();
        }

        private int LoadShader(int type, string shaderCode)
        {
            int shader = GLES32.GlCreateShader(type);

            GLES32.GlShaderSource(shader, shaderCode);
            GLES32.GlCompileShader(shader);

            return shader;
        }

        private int PreloadShaders()
        {
            string vertexShaderSrc = "precision highp float;\n" +
                                     "precision highp int;\n" +

                                     "varying vec4 Color;\n" +
                                     "uniform float gScale;\n" +
                                     "uniform mat4 world;\n" +

                                     "attribute vec4 Position;\n" +

                                     "void main() { gl_Position = vec4(gScale * Position.x, gScale * Position.y, Position.z, 1.0) * world; \n" +
                                                   "Color = clamp(Position, 0.2, 0.8); }";

            string fragmentShaderSrc = "precision mediump float;\n" +
                                       "precision mediump int;\n" +

                                       "varying vec4 Color;\n" +
                                       //"uniform vec4 color;\n" +

                                       "void main() { gl_FragColor = Color; }";

            int vertexShader = LoadShader(GLES32.GlVertexShader, vertexShaderSrc);
            int fragmentShader = LoadShader(GLES32.GlFragmentShader, fragmentShaderSrc);

            int program = GLES32.GlCreateProgram();

            GLES32.GlAttachShader(program, vertexShader);
            GLES32.GlAttachShader(program, fragmentShader);

            GLES32.GlBindAttribLocation(program, 0, "Position");
            GLES32.GlLinkProgram(program);

            return program;
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            rotateSide = width > height ? 1 : -1;
        }
    }
}
