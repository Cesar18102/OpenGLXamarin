using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.ES30;
using OpenTK.Platform;

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
        private int program = 0;
        private float dx = 0;

        private int[] buffers = new int[1];
        private static float[] vertices = {
            0f, 0f, 0f,
            0.95f, 0f, 0f,
            -0.95f, 0f, 0f,
            -0.95f, 0.95f, 0f,
            -0.95f, -0.95f, 0f,
            0.95f, 0.95f, 0f,
            0.95f, -0.95f, 0f
        };

        private J.Buffer buffer = J.ByteBuffer.AllocateDirect(vertices.Length * sizeof(float)).
                                               Order(J.ByteOrder.NativeOrder()).AsFloatBuffer().
                                               Put(vertices).Position(0);

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

                GLES32.GlGenBuffers(1, buffers, 0);
                GLES32.GlBindBuffer(GLES32.GlArrayBuffer, buffers[0]);
                GLES32.GlBufferData(GLES32.GlArrayBuffer, vertices.Length * sizeof(float), buffer, GLES32.GlStaticDraw);
                GLES32.GlBindBuffer(GLES32.GlArrayBuffer, 0);
            }

            GLES32.GlClearColor(0, 0, 0, 1);
            GLES32.GlClear(GLES32.GlColorBufferBit | GLES32.GlDepthBufferBit);

            GLES32.GlViewport(0, 0, (int)(GLView.Width * 3), (int)(GLView.Height * 3));
            GLES32.GlUseProgram(program);

            GLES32.GlEnableVertexAttribArray(0);
            GLES32.GlVertexAttribPointer(0, 3, GLES32.GlFloat, false, 0, buffer);
            GLES32.GlDrawArrays(GLES32.GlLineStrip, 0, 7);

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
            string vertexShaderSrc = "attribute vec4 vPosition;\n" +
                                     "void main() { gl_Position = vPosition; }";

            string fragmentShaderSrc = "precision mediump float;\n" +
                                       "void main() { gl_FragColor = vec4 (1.0, 1.0, 1.0, 1.0); }";

            int vertexShader = LoadShader(GLES32.GlVertexShader, vertexShaderSrc);
            int fragmentShader = LoadShader(GLES32.GlFragmentShader, fragmentShaderSrc);

            int program = GLES32.GlCreateProgram();

            GLES32.GlAttachShader(program, vertexShader);
            GLES32.GlAttachShader(program, fragmentShader);

            GLES32.GlBindAttribLocation(program, 0, "vPosition");
            GLES32.GlLinkProgram(program);

            return program;
        }
    }
}
