using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ConsoleApp1_Pet.Новая_папка
{
    public abstract class ImGuiTheme
    {
        public abstract void Apply();

    }

    public class DarkishRedImGuiTheme : ImGuiTheme
    {
        public override void Apply()
        {

            var style = ImGui.GetStyle();
            var colors = style.Colors;
           

  style.WindowRounding = 4.0f                    ;
  style.WindowTitleAlign = new Vector2(0.5f, 0.5f);
  style.ChildRounding = 2.0f;
  style.FrameRounding = 4.0f;
  style.ItemSpacing = new Vector2(10, 5);
  style.ScrollbarSize = 15;
  style.ScrollbarRounding = 0;
  style.GrabMinSize = 9.6f;
  style.GrabRounding = 1.0f;
  style.WindowPadding = new Vector2(10, 10);
  style.AntiAliasedLines = true;
  style.AntiAliasedFill = true;
  style.FramePadding = new Vector2(5, 4);
  style.DisplayWindowPadding = new Vector2(27, 27);
  style.DisplaySafeAreaPadding = new Vector2(5, 5);
  style.ButtonTextAlign = new Vector2(0.5f, 0.5f);
  style.IndentSpacing = 12.0f;
            style.Alpha = 1.0f;

  colors[(int)ImGuiCol.Text]                 = new Vector4(0.75f, 0.75f, 0.75f, 1.00f);
  colors[(int)ImGuiCol.TextDisabled]         = new Vector4(0.35f, 0.35f, 0.35f, 1.00f)                ;
  colors[(int)ImGuiCol.WindowBg]             = new Vector4(0.00f, 0.00f, 0.00f, 0.94f)                    ;
  colors[(int)ImGuiCol.PopupBg]              = new Vector4(0.08f, 0.08f, 0.08f, 0.94f)                     ;
  colors[(int)ImGuiCol.Border]               = new Vector4(0.00f, 0.00f, 0.00f, 0.50f)                      ;
  colors[(int)ImGuiCol.BorderShadow]         = new Vector4(0.00f, 0.00f, 0.00f, 0.00f)                ;
  colors[(int)ImGuiCol.FrameBg]              = new Vector4(0.00f, 0.00f, 0.00f, 0.54f)                     ;
  colors[(int)ImGuiCol.FrameBgHovered]       = new Vector4(0.37f, 0.14f, 0.14f, 0.67f)              ;
  colors[(int)ImGuiCol.FrameBgActive]        = new Vector4(0.39f, 0.20f, 0.20f, 0.67f)               ;
  colors[(int)ImGuiCol.TitleBg]              = new Vector4(0.04f, 0.04f, 0.04f, 1.00f)                     ;
  colors[(int)ImGuiCol.TitleBgActive]        = new Vector4(0.48f, 0.16f, 0.16f, 1.00f)               ;
  colors[(int)ImGuiCol.TitleBgCollapsed]     = new Vector4(0.48f, 0.16f, 0.16f, 1.00f)            ;
  colors[(int)ImGuiCol.MenuBarBg]            = new Vector4(0.14f, 0.14f, 0.14f, 1.00f)                   ;
  colors[(int)ImGuiCol.ScrollbarBg]          = new Vector4(0.02f, 0.02f, 0.02f, 0.53f)                 ;
  colors[(int)ImGuiCol.ScrollbarGrab]        = new Vector4(0.31f, 0.31f, 0.31f, 1.00f)               ;
  colors[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.41f, 0.41f, 0.41f, 1.00f)        ;
  colors[(int)ImGuiCol.ScrollbarGrabActive]  = new Vector4(0.51f, 0.51f, 0.51f, 1.00f)         ;
  colors[(int)ImGuiCol.CheckMark]            = new Vector4(0.56f, 0.10f, 0.10f, 1.00f)                   ;
  colors[(int)ImGuiCol.SliderGrab]           = new Vector4(1.00f, 0.19f, 0.19f, 0.40f)                  ;
  colors[(int)ImGuiCol.SliderGrabActive]     = new Vector4(0.89f, 0.00f, 0.19f, 1.00f)            ;
  colors[(int)ImGuiCol.Button]               = new Vector4(1.00f, 0.19f, 0.19f, 0.40f)                      ;
  colors[(int)ImGuiCol.ButtonHovered]        = new Vector4(0.80f, 0.17f, 0.00f, 1.00f)               ;
  colors[(int)ImGuiCol.ButtonActive]         = new Vector4(0.89f, 0.00f, 0.19f, 1.00f)                ;
  colors[(int)ImGuiCol.Header]               = new Vector4(0.33f, 0.35f, 0.36f, 0.53f)                      ;
  colors[(int)ImGuiCol.HeaderHovered]        = new Vector4(0.76f, 0.28f, 0.44f, 0.67f)               ;
  colors[(int)ImGuiCol.HeaderActive]         = new Vector4(0.47f, 0.47f, 0.47f, 0.67f)                ;
  colors[(int)ImGuiCol.Separator]            = new Vector4(0.32f, 0.32f, 0.32f, 1.00f)                   ;
  colors[(int)ImGuiCol.SeparatorHovered]     = new Vector4(0.32f, 0.32f, 0.32f, 1.00f)            ;
  colors[(int)ImGuiCol.SeparatorActive]      = new Vector4(0.32f, 0.32f, 0.32f, 1.00f)             ;
  colors[(int)ImGuiCol.ResizeGrip]           = new Vector4(1.00f, 1.00f, 1.00f, 0.85f)                  ;
  colors[(int)ImGuiCol.ResizeGripHovered]    = new Vector4(1.00f, 1.00f, 1.00f, 0.60f)           ;
  colors[(int)ImGuiCol.ResizeGripActive]     = new Vector4(1.00f, 1.00f, 1.00f, 0.90f)            ;
  colors[(int)ImGuiCol.PlotLines]            = new Vector4(0.61f, 0.61f, 0.61f, 1.00f)                   ;
  colors[(int)ImGuiCol.PlotLinesHovered]     = new Vector4(1.00f, 0.43f, 0.35f, 1.00f)            ;
  colors[(int)ImGuiCol.PlotHistogram]        = new Vector4(0.90f, 0.70f, 0.00f, 1.00f)               ;
  colors[(int)ImGuiCol.PlotHistogramHovered] = new Vector4(1.00f, 0.60f, 0.00f, 1.00f)        ;
  colors[(int)ImGuiCol.TextSelectedBg]       = new Vector4(0.26f, 0.59f, 0.98f, 0.35f)              ;
        }
    }
}
