using ConsoleApp1_Pet.Render;
using ConsoleApp1_Pet.Новая_папка;
using ImGuiNET;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1_Pet.Editor
{
    public class Editor
    {






        public static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[System.Random.Shared.Next(s.Length)]).ToArray());
        }

    }

    public class Inspector
    {
        private static Inspector _instance;
        public static Inspector instance { get => _instance ??= new Inspector(); }

        public Dictionary<Type, object> Drawers = new Dictionary<Type, object>()
        {
            {typeof(Transform),  new Etd_Transform() }
        };
     

        public void DrawWindow(object obj)
        {
            ImGui.PushID(obj.GetType().Name);
            ImGui.Begin($"I ({obj.ToString()})");
            Draw(obj,true);
           ImGui.End();
            ImGui.PopID();
        }
        public void Draw(object o,bool skipRootNode=false)
        {
            FieldInfo[] members = o.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance );
          //  Drawers.Add(typeof(Transform), new Etd_Transform());
            if (o is Transform)
            {
                FieldInfo[] members2 = o.GetType().GetFields();
            }
            var Title = o.ToString();
           //ImGui.PushID(o.GetType().Name);
            //var a = ImGui.GetItemID();
            if (skipRootNode ||ImGui.TreeNode(o.GetType().Name,Title))
            {
            
                foreach (var f in members)
                {
                    //TypedReference objRef = __makeref(o);

                    //f.GetValue(o);
                    // var rr = __makeref(o);

                    var fname = f.Name.ToLower();

                    object fieldValue = f.GetValue(o);

                    if(f.Name.ToLower() == "fov")
                    {

                    }
                    // Get the address of the field value using GCHandle


                    //val = "1234";

                    switch (fieldValue)
                    {
                        case null:
                            ImGui.BulletText("NULL");
                            break;
                        case string v:
                            //ImGui.GetStyle().ItemInnerSpacing = new System.Numerics.Vector2(5, 4);

                            if (ImGui.InputText(f.Name, ref v, 256))
                            {
                                f.SetValue(o, v);
                            }

                            break;

                        case int v:
                            //ImGui.GetStyle().ItemInnerSpacing = new System.Numerics.Vector2(5, 4);

                            if (ImGui.DragInt(f.Name, ref v, MathF.Sqrt(v) / 10f))
                            {
                                f.SetValue(o, v);
                            }

                            break;
                        case long v:
                            //ImGui.GetStyle().ItemInnerSpacing = new System.Numerics.Vector2(5, 4);

                            if (ImGui.DragScalar(f.Name, ImGuiDataType.S64, (nint)v))
                            {
                                f.SetValue(o, v);
                            }
                     
                            break;
                        case float v:
                            //ImGui.GetStyle().ItemInnerSpacing = new System.Numerics.Vector2(5, 4);

                            if (ImGui.DragFloat(f.Name, ref v, MathF.Sqrt(v) / 10f))
                            {
                                f.SetValue(o, v);
                            }

                            break;
                        case Enum e:
                            var val = Enum.GetName(e.GetType(), e); 
                            var arr=  Enum.GetNames(e.GetType());
                            var Current = Array.IndexOf(arr, val);
                            if (ImGui.Combo(f.Name,ref Current,arr,arr.Length))
                            {
                                f.SetValue(o, Current);
                            }
                            break;
                        case object:
                            
                            if (ImGui.TreeNodeEx(f.Name,ImGuiTreeNodeFlags.CollapsingHeader))
                            {
                                if (Drawers.TryGetValue(f.FieldType, out var drawer))
                                {
                                    drawer.GetType().GetMethod("Draw").Invoke(drawer,new object[] { f.GetValue(o) });
                                }
                                else
                                {
                                    this.Draw(f.GetValue(o),true);
                                }
                                ImGui.TreePop();
                            }
                            
                            break;
                            //case Rectangle s when (s.Length == s.Height):
                            //    WriteLine($"{s.Length} x {s.Height} square");
                            //    break;
                            //case Rectangle r:
                            //    WriteLine($"{r.Length} x {r.Height} rectangle");
                            //    break;
                            //default:
                            //    WriteLine("<unknown shape>");
                            //    break;
                            //case null:
                            //    throw new ArgumentNullException(nameof(shape));
                    }
                }
                if(!skipRootNode)ImGui.TreePop();
                //ImGui.PopID();
            }
            //a = ImGui.GetItemID();
        }
    }

    public static class Hierarchy
    {
        public static void Draw()
        {
            ImGui.Begin("Hierarchy");

            foreach(var r in Game.instance.renderer.renderObjects)
            {
                DrawElement(r.transform);
                //if (ImGui.TreeNodeEx(r.GetHashCode() + "s", ImGuiTreeNodeFlags.None, "No name object"))
                //{
                   
                //        DrawElement(r.transform);
                    
                //    ImGui.TreePop();
                //}
                
            }

            ImGui.End();
        }
        private static void DrawElement(Transform rr)
        {
           
            if (ImGui.TreeNodeEx(rr.GetHashCode() + "s",rr.childs.Count<=0? ImGuiTreeNodeFlags.Leaf:ImGuiTreeNodeFlags.None, "No name object"))
            {
                foreach (var c in rr.childs)
                {
                    ImGui.Text(" >");
                        ImGui.SameLine(25f);
                    DrawElement(c);
                }
                    ImGui.TreePop();
            }
            if (ImGui.IsItemClicked())
            {
                Console.WriteLine("Asd");
            }
        }
    }

    public abstract class EditorTypeDrawer<T>
    {
        public abstract void Draw (T toDraw);
    }
    public class Etd_Transform : EditorTypeDrawer<Transform>
    {
        public override void Draw (Transform toDraw)
        {
            var curVec3 = new System.Numerics.Vector3 (toDraw.LocalPosition.X, toDraw.LocalPosition.Y, toDraw.LocalPosition.Z);
            if(ImGui.DragFloat3 ("position",ref curVec3, MathF.Sqrt(curVec3.Length()) / 10.0f))
            {
                toDraw.LocalPosition = new OpenTK.Mathematics.Vector3(curVec3.X, curVec3.Y, curVec3.Z);
            }
            var qq = toDraw.LocalRotation.ToEulerAngles()* 57.29578f; // Radians to degree constant ratio
            curVec3  = new System.Numerics.Vector3(qq.X, qq.Y, qq.Z);
            if (ImGui.DragFloat3("rotation", ref curVec3))
            {
                toDraw.rotation = OpenTK.Mathematics.Quaternion.FromEulerAngles(new OpenTK.Mathematics.Vector3(curVec3.X, curVec3.Y, curVec3.Z) / 57.29578f) ;// Radians to degree constant ratio
            }
            curVec3 = new System.Numerics.Vector3(toDraw.LocalScale.X, toDraw.LocalScale.Y, toDraw.LocalScale.Z);
            if (ImGui.DragFloat3("scale", ref curVec3, MathF.Sqrt(curVec3.Length()) / 10.0f))
            {
                toDraw.LocalScale = new OpenTK.Mathematics.Vector3(curVec3.X, curVec3.Y, curVec3.Z);
            }
        }
    }
    //public class Inspector
}
