using ConsoleApp1_Pet.Scripts;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace ConsoleApp1_Pet
{

    public class ConsoleWindow:IUpdatable,IScript
    {
        public string windowName;
        public List<ConsoleMessage> messageHistory = new List<ConsoleMessage>();
        private ConsoleMessage currentMessage;

        public bool IsActive { get; set; } = true;

        public void Render()
        {
           // ImGui.SetNextWindowScroll(new Vector2(0, 1000));
            windowName = windowName ?? "Console";
            ImGui.Begin(windowName);
           // ImGui.scroll
            foreach(var v in messageHistory.ToArray())
            {
                v.Draw();
                ImGui.NewLine();
            }
            if(ImGui.Button("write test"))
            {
                Write($"test txt N{messageHistory.Count}");
            }
            if (ImGui.Button("writeLn test"))
            {
                WriteLine($"tt N{messageHistory.Count}");
            }
            ImGui.End();
        }
        public void Write(string text)
        {
            Write(new MCText(text));
            Console.Write(text);
        }
        public void WriteLine(string text)
        {
            WriteLine(new MCText(text));
            Console.WriteLine(text);
        }
        private void WriteLine(MessageContent mc)
        {
            
            Write(mc);
            currentMessage = new ConsoleMessage();
            messageHistory.Add(currentMessage);
       
        }
        private void Write(MessageContent mc)
        {
            if (currentMessage == null) { currentMessage = new ConsoleMessage(); messageHistory.Add(currentMessage); }
            currentMessage.AddContent(mc);
            
        }
        public void Write(string text, Vector4 textColor)
        {
            Write(new McColoredText(text,textColor));
        }
        public void WriteLine(string text, Vector4 textColor)
        {
            WriteLine(new McColoredText(text, textColor));
        }

        public void Update()
        {
            Render();
        }

        public class ConsoleMessage
        {
            public List<MessageContent> content=new List<MessageContent>();
            public void Draw()
            {
                foreach(var c in content)
                {
                    if(c is MCText text)
                    {
                        ImGui.SameLine();
                        ImGui.Text(text.text);
                    }
                }
            }

            public void AddContent(MessageContent mt)
            {
                content.Add(mt);
            }

        }
        public class MessageContent
        {
            
        }
        public class MCText : MessageContent
        {
            public string text;

            public MCText(string text)
            {
                this.text = text;
            }
        }
        public class McColoredText(string text,Vector4 color) : MCText(text)
        {
            public Vector4 color = color;
        }
    }

}
