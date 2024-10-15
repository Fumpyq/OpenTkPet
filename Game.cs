using ConsoleApp1_Pet.Render;
using Dear_ImGui_Sample;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1_Pet
{
    public class Game
    {
        private static Game _instance;
        public static Game instance { get => _instance ?? (_instance= new Game()); }

        public Renderer renderer;
        public ImGuiController _controller;
        public void OnGameWindowLoaded()
        {

        }

        public void OnRenderFrame()
        {

        }
    }

}
