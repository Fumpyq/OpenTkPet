using ConsoleApp1_Pet.Новая_папка;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1_Pet.Editor
{
    public static class ResourceDrawer
    {
        public static void Draw()
        {
            ImGui.Begin("Resources");
            ImGui.Text("SceneRenderTexture");
            if (ImGui.BeginItemTooltip())
            {
               //ImGui.Text($"{v.Value.filePath}");
                ImGui.Image(MainGameWindow.instance.prePostProcessingBuffer.texture.id, new System.Numerics.Vector2(256, 256));
                ImGui.EndTooltip();
            }
            foreach (var v in Resources.resources)
            {
                ImGui.Text(v.Key);
                //if(ImGui.IsItemHovered())
                //{
                    if (v.Value is TextureResource tr) {
                    if (ImGui.BeginItemTooltip())
                        { 
                        ImGui.Text($"{v.Value.filePath}");
                        ImGui.Image(tr.texture.id,new System.Numerics.Vector2(256,256));
                        ImGui.EndTooltip();
                    }
                }
               // }
            }

            ImGui.End();
        }
    }
}
